﻿using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms.DCinema
{
    partial class DCinemaPropertiesSmpte
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
            Nikse.SubtitleEdit.Core.Common.TimeCode timeCode1 = new Nikse.SubtitleEdit.Core.Common.TimeCode();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxGenerateIdAuto = new System.Windows.Forms.CheckBox();
            this.timeUpDownStartTime = new Nikse.SubtitleEdit.Controls.NikseTimeUpDown();
            this.labelStartTime = new System.Windows.Forms.Label();
            this.comboBoxTimeCodeRate = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelTimeCodeRate = new System.Windows.Forms.Label();
            this.textBoxEditRate = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelEditRate = new System.Windows.Forms.Label();
            this.buttonToday = new System.Windows.Forms.Button();
            this.textBoxIssueDate = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelIssueDate = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.buttonGenerateID = new System.Windows.Forms.Button();
            this.numericUpDownReelNumber = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelFadeUpMs = new System.Windows.Forms.Label();
            this.numericUpDownFadeDown = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelFadeDownTime = new System.Windows.Forms.Label();
            this.numericUpDownFadeUp = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelFadeUpTime = new System.Windows.Forms.Label();
            this.buttonGenFontUri = new System.Windows.Forms.Button();
            this.numericUpDownTopBottomMargin = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.panelFontEffectColor = new System.Windows.Forms.Panel();
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
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxGenerateIdAuto);
            this.groupBox1.Controls.Add(this.timeUpDownStartTime);
            this.groupBox1.Controls.Add(this.labelStartTime);
            this.groupBox1.Controls.Add(this.comboBoxTimeCodeRate);
            this.groupBox1.Controls.Add(this.labelTimeCodeRate);
            this.groupBox1.Controls.Add(this.textBoxEditRate);
            this.groupBox1.Controls.Add(this.labelEditRate);
            this.groupBox1.Controls.Add(this.buttonToday);
            this.groupBox1.Controls.Add(this.textBoxIssueDate);
            this.groupBox1.Controls.Add(this.labelIssueDate);
            this.groupBox1.Controls.Add(this.comboBoxLanguage);
            this.groupBox1.Controls.Add(this.labelLanguage);
            this.groupBox1.Controls.Add(this.buttonGenerateID);
            this.groupBox1.Controls.Add(this.numericUpDownReelNumber);
            this.groupBox1.Controls.Add(this.groupBoxFont);
            this.groupBox1.Controls.Add(this.labelReelNumber);
            this.groupBox1.Controls.Add(this.textBoxMovieTitle);
            this.groupBox1.Controls.Add(this.labelMovieTitle);
            this.groupBox1.Controls.Add(this.textBoxSubtitleID);
            this.groupBox1.Controls.Add(this.labelSubtitleID);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(583, 571);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // checkBoxGenerateIdAuto
            // 
            this.checkBoxGenerateIdAuto.AutoSize = true;
            this.checkBoxGenerateIdAuto.Location = new System.Drawing.Point(181, 56);
            this.checkBoxGenerateIdAuto.Name = "checkBoxGenerateIdAuto";
            this.checkBoxGenerateIdAuto.Size = new System.Drawing.Size(148, 17);
            this.checkBoxGenerateIdAuto.TabIndex = 40;
            this.checkBoxGenerateIdAuto.Text = "Generate new ID on save";
            this.checkBoxGenerateIdAuto.UseVisualStyleBackColor = true;
            // 
            // timeUpDownStartTime
            // 
            this.timeUpDownStartTime.BackColor = System.Drawing.SystemColors.Window;
            this.timeUpDownStartTime.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.timeUpDownStartTime.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.timeUpDownStartTime.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.timeUpDownStartTime.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.timeUpDownStartTime.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.timeUpDownStartTime.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.timeUpDownStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownStartTime.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.timeUpDownStartTime.Location = new System.Drawing.Point(181, 248);
            this.timeUpDownStartTime.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.timeUpDownStartTime.Name = "timeUpDownStartTime";
            this.timeUpDownStartTime.Size = new System.Drawing.Size(113, 27);
            this.timeUpDownStartTime.TabIndex = 8;
            this.timeUpDownStartTime.TabStop = false;
            timeCode1.Hours = 0;
            timeCode1.Milliseconds = 0;
            timeCode1.Minutes = 0;
            timeCode1.Seconds = 0;
            timeCode1.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode1.TotalMilliseconds = 0D;
            timeCode1.TotalSeconds = 0D;
            this.timeUpDownStartTime.TimeCode = timeCode1;
            this.timeUpDownStartTime.UseVideoOffset = false;
            // 
            // labelStartTime
            // 
            this.labelStartTime.AutoSize = true;
            this.labelStartTime.Location = new System.Drawing.Point(15, 248);
            this.labelStartTime.Name = "labelStartTime";
            this.labelStartTime.Size = new System.Drawing.Size(51, 13);
            this.labelStartTime.TabIndex = 39;
            this.labelStartTime.Text = "Start time";
            // 
            // comboBoxTimeCodeRate
            // 
            this.comboBoxTimeCodeRate.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxTimeCodeRate.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxTimeCodeRate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxTimeCodeRate.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxTimeCodeRate.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxTimeCodeRate.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxTimeCodeRate.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxTimeCodeRate.DropDownHeight = 400;
            this.comboBoxTimeCodeRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxTimeCodeRate.DropDownWidth = 112;
            this.comboBoxTimeCodeRate.FormattingEnabled = true;
            this.comboBoxTimeCodeRate.Items.AddRange(new string[] {
            "24",
            "25",
            "30",
            "48"});
            this.comboBoxTimeCodeRate.Location = new System.Drawing.Point(181, 219);
            this.comboBoxTimeCodeRate.MaxLength = 32767;
            this.comboBoxTimeCodeRate.Name = "comboBoxTimeCodeRate";
            this.comboBoxTimeCodeRate.SelectedIndex = -1;
            this.comboBoxTimeCodeRate.SelectedItem = null;
            this.comboBoxTimeCodeRate.SelectedText = "";
            this.comboBoxTimeCodeRate.Size = new System.Drawing.Size(112, 21);
            this.comboBoxTimeCodeRate.TabIndex = 7;
            this.comboBoxTimeCodeRate.TabStop = false;
            this.comboBoxTimeCodeRate.UsePopupWindow = false;
            // 
            // labelTimeCodeRate
            // 
            this.labelTimeCodeRate.AutoSize = true;
            this.labelTimeCodeRate.Location = new System.Drawing.Point(15, 222);
            this.labelTimeCodeRate.Name = "labelTimeCodeRate";
            this.labelTimeCodeRate.Size = new System.Drawing.Size(78, 13);
            this.labelTimeCodeRate.TabIndex = 37;
            this.labelTimeCodeRate.Text = "Time code rate";
            // 
            // textBoxEditRate
            // 
            this.textBoxEditRate.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxEditRate.Location = new System.Drawing.Point(181, 195);
            this.textBoxEditRate.Name = "textBoxEditRate";
            this.textBoxEditRate.Size = new System.Drawing.Size(112, 20);
            this.textBoxEditRate.TabIndex = 6;
            this.textBoxEditRate.Text = "24 1";
            // 
            // labelEditRate
            // 
            this.labelEditRate.AutoSize = true;
            this.labelEditRate.Location = new System.Drawing.Point(15, 197);
            this.labelEditRate.Name = "labelEditRate";
            this.labelEditRate.Size = new System.Drawing.Size(46, 13);
            this.labelEditRate.TabIndex = 35;
            this.labelEditRate.Text = "Edit rate";
            // 
            // buttonToday
            // 
            this.buttonToday.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonToday.Location = new System.Drawing.Point(461, 168);
            this.buttonToday.Name = "buttonToday";
            this.buttonToday.Size = new System.Drawing.Size(100, 23);
            this.buttonToday.TabIndex = 5;
            this.buttonToday.Text = "Now";
            this.buttonToday.UseVisualStyleBackColor = true;
            this.buttonToday.Click += new System.EventHandler(this.buttonToday_Click);
            // 
            // textBoxIssueDate
            // 
            this.textBoxIssueDate.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxIssueDate.Location = new System.Drawing.Point(181, 169);
            this.textBoxIssueDate.Name = "textBoxIssueDate";
            this.textBoxIssueDate.Size = new System.Drawing.Size(274, 20);
            this.textBoxIssueDate.TabIndex = 4;
            this.textBoxIssueDate.Text = "2005-07-14T21:52:02";
            // 
            // labelIssueDate
            // 
            this.labelIssueDate.AutoSize = true;
            this.labelIssueDate.Location = new System.Drawing.Point(15, 172);
            this.labelIssueDate.Name = "labelIssueDate";
            this.labelIssueDate.Size = new System.Drawing.Size(56, 13);
            this.labelIssueDate.TabIndex = 32;
            this.labelIssueDate.Text = "Issue date";
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
            this.comboBoxLanguage.DropDownWidth = 112;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(181, 142);
            this.comboBoxLanguage.MaxLength = 32767;
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.SelectedIndex = -1;
            this.comboBoxLanguage.SelectedItem = null;
            this.comboBoxLanguage.SelectedText = "";
            this.comboBoxLanguage.Size = new System.Drawing.Size(112, 21);
            this.comboBoxLanguage.TabIndex = 3;
            this.comboBoxLanguage.TabStop = false;
            this.comboBoxLanguage.UsePopupWindow = false;
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(15, 145);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(55, 13);
            this.labelLanguage.TabIndex = 30;
            this.labelLanguage.Text = "Language";
            // 
            // buttonGenerateID
            // 
            this.buttonGenerateID.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenerateID.Location = new System.Drawing.Point(461, 29);
            this.buttonGenerateID.Name = "buttonGenerateID";
            this.buttonGenerateID.Size = new System.Drawing.Size(100, 23);
            this.buttonGenerateID.TabIndex = 1;
            this.buttonGenerateID.Text = "Generate ID";
            this.buttonGenerateID.UseVisualStyleBackColor = true;
            this.buttonGenerateID.Click += new System.EventHandler(this.buttonGenerateID_Click);
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
            this.numericUpDownReelNumber.Location = new System.Drawing.Point(181, 116);
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
            this.numericUpDownReelNumber.TabIndex = 2;
            this.numericUpDownReelNumber.TabStop = false;
            this.numericUpDownReelNumber.ThousandsSeparator = false;
            this.numericUpDownReelNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFont.Controls.Add(this.label1);
            this.groupBoxFont.Controls.Add(this.labelFadeUpMs);
            this.groupBoxFont.Controls.Add(this.numericUpDownFadeDown);
            this.groupBoxFont.Controls.Add(this.labelFadeDownTime);
            this.groupBoxFont.Controls.Add(this.numericUpDownFadeUp);
            this.groupBoxFont.Controls.Add(this.labelFadeUpTime);
            this.groupBoxFont.Controls.Add(this.buttonGenFontUri);
            this.groupBoxFont.Controls.Add(this.numericUpDownTopBottomMargin);
            this.groupBoxFont.Controls.Add(this.label2);
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
            this.groupBoxFont.Location = new System.Drawing.Point(6, 284);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(571, 281);
            this.groupBoxFont.TabIndex = 9;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "Font";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(294, 232);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 43;
            this.label1.Text = "Frames";
            // 
            // labelFadeUpMs
            // 
            this.labelFadeUpMs.AutoSize = true;
            this.labelFadeUpMs.Location = new System.Drawing.Point(294, 206);
            this.labelFadeUpMs.Name = "labelFadeUpMs";
            this.labelFadeUpMs.Size = new System.Drawing.Size(41, 13);
            this.labelFadeUpMs.TabIndex = 42;
            this.labelFadeUpMs.Text = "Frames";
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
            this.numericUpDownFadeDown.Location = new System.Drawing.Point(174, 230);
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
            this.numericUpDownFadeDown.TabIndex = 39;
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
            this.labelFadeDownTime.Location = new System.Drawing.Point(10, 232);
            this.labelFadeDownTime.Name = "labelFadeDownTime";
            this.labelFadeDownTime.Size = new System.Drawing.Size(82, 13);
            this.labelFadeDownTime.TabIndex = 41;
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
            this.numericUpDownFadeUp.Location = new System.Drawing.Point(174, 204);
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
            this.numericUpDownFadeUp.TabIndex = 38;
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
            this.labelFadeUpTime.Location = new System.Drawing.Point(10, 206);
            this.labelFadeUpTime.Name = "labelFadeUpTime";
            this.labelFadeUpTime.Size = new System.Drawing.Size(68, 13);
            this.labelFadeUpTime.TabIndex = 40;
            this.labelFadeUpTime.Text = "Fade up time";
            // 
            // buttonGenFontUri
            // 
            this.buttonGenFontUri.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenFontUri.Location = new System.Drawing.Point(454, 41);
            this.buttonGenFontUri.Name = "buttonGenFontUri";
            this.buttonGenFontUri.Size = new System.Drawing.Size(100, 23);
            this.buttonGenFontUri.TabIndex = 2;
            this.buttonGenFontUri.Text = "Generate";
            this.buttonGenFontUri.UseVisualStyleBackColor = true;
            this.buttonGenFontUri.Click += new System.EventHandler(this.button1_Click);
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
            this.numericUpDownTopBottomMargin.Location = new System.Drawing.Point(175, 176);
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
            this.numericUpDownTopBottomMargin.TabIndex = 9;
            this.numericUpDownTopBottomMargin.TabStop = false;
            this.numericUpDownTopBottomMargin.ThousandsSeparator = false;
            this.numericUpDownTopBottomMargin.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 178);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "Top/bottom margin";
            // 
            // panelFontEffectColor
            // 
            this.panelFontEffectColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFontEffectColor.Location = new System.Drawing.Point(297, 123);
            this.panelFontEffectColor.Name = "panelFontEffectColor";
            this.panelFontEffectColor.Size = new System.Drawing.Size(21, 20);
            this.panelFontEffectColor.TabIndex = 7;
            this.panelFontEffectColor.Click += new System.EventHandler(this.buttonFontEffectColor_Click);
            // 
            // buttonFontEffectColor
            // 
            this.buttonFontEffectColor.Location = new System.Drawing.Point(175, 123);
            this.buttonFontEffectColor.Name = "buttonFontEffectColor";
            this.buttonFontEffectColor.Size = new System.Drawing.Size(112, 21);
            this.buttonFontEffectColor.TabIndex = 6;
            this.buttonFontEffectColor.Text = "Choose color";
            this.buttonFontEffectColor.UseVisualStyleBackColor = true;
            this.buttonFontEffectColor.Click += new System.EventHandler(this.buttonFontEffectColor_Click);
            // 
            // panelFontColor
            // 
            this.panelFontColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFontColor.Location = new System.Drawing.Point(297, 70);
            this.panelFontColor.Name = "panelFontColor";
            this.panelFontColor.Size = new System.Drawing.Size(21, 20);
            this.panelFontColor.TabIndex = 4;
            this.panelFontColor.Click += new System.EventHandler(this.buttonFontColor_Click);
            // 
            // buttonFontColor
            // 
            this.buttonFontColor.Location = new System.Drawing.Point(175, 70);
            this.buttonFontColor.Name = "buttonFontColor";
            this.buttonFontColor.Size = new System.Drawing.Size(112, 21);
            this.buttonFontColor.TabIndex = 3;
            this.buttonFontColor.Text = "Choose color";
            this.buttonFontColor.UseVisualStyleBackColor = true;
            this.buttonFontColor.Click += new System.EventHandler(this.buttonFontColor_Click);
            // 
            // labelEffectColor
            // 
            this.labelEffectColor.AutoSize = true;
            this.labelEffectColor.Location = new System.Drawing.Point(10, 127);
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
            this.numericUpDownFontSize.Location = new System.Drawing.Point(175, 150);
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
            this.numericUpDownFontSize.TabIndex = 8;
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
            this.labelFontSize.Location = new System.Drawing.Point(10, 152);
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
            this.comboBoxFontEffect.Location = new System.Drawing.Point(175, 97);
            this.comboBoxFontEffect.MaxLength = 32767;
            this.comboBoxFontEffect.Name = "comboBoxFontEffect";
            this.comboBoxFontEffect.SelectedIndex = -1;
            this.comboBoxFontEffect.SelectedItem = null;
            this.comboBoxFontEffect.SelectedText = "";
            this.comboBoxFontEffect.Size = new System.Drawing.Size(112, 21);
            this.comboBoxFontEffect.TabIndex = 5;
            this.comboBoxFontEffect.UsePopupWindow = false;
            // 
            // labelEffect
            // 
            this.labelEffect.AutoSize = true;
            this.labelEffect.Location = new System.Drawing.Point(10, 100);
            this.labelEffect.Name = "labelEffect";
            this.labelEffect.Size = new System.Drawing.Size(35, 13);
            this.labelEffect.TabIndex = 22;
            this.labelEffect.Text = "Effect";
            // 
            // labelFontColor
            // 
            this.labelFontColor.AutoSize = true;
            this.labelFontColor.Location = new System.Drawing.Point(10, 74);
            this.labelFontColor.Name = "labelFontColor";
            this.labelFontColor.Size = new System.Drawing.Size(31, 13);
            this.labelFontColor.TabIndex = 18;
            this.labelFontColor.Text = "Color";
            // 
            // textBoxFontID
            // 
            this.textBoxFontID.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxFontID.Location = new System.Drawing.Point(175, 19);
            this.textBoxFontID.Name = "textBoxFontID";
            this.textBoxFontID.Size = new System.Drawing.Size(274, 20);
            this.textBoxFontID.TabIndex = 0;
            this.textBoxFontID.Text = "Freds_Font";
            // 
            // labelFontId
            // 
            this.labelFontId.AutoSize = true;
            this.labelFontId.Location = new System.Drawing.Point(10, 22);
            this.labelFontId.Name = "labelFontId";
            this.labelFontId.Size = new System.Drawing.Size(18, 13);
            this.labelFontId.TabIndex = 16;
            this.labelFontId.Text = "ID";
            // 
            // textBoxFontUri
            // 
            this.textBoxFontUri.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxFontUri.Location = new System.Drawing.Point(175, 45);
            this.textBoxFontUri.Name = "textBoxFontUri";
            this.textBoxFontUri.Size = new System.Drawing.Size(274, 20);
            this.textBoxFontUri.TabIndex = 1;
            this.textBoxFontUri.Text = "Fred.ttf";
            // 
            // labelFontUri
            // 
            this.labelFontUri.AutoSize = true;
            this.labelFontUri.Location = new System.Drawing.Point(10, 48);
            this.labelFontUri.Name = "labelFontUri";
            this.labelFontUri.Size = new System.Drawing.Size(26, 13);
            this.labelFontUri.TabIndex = 14;
            this.labelFontUri.Text = "URI";
            // 
            // labelReelNumber
            // 
            this.labelReelNumber.AutoSize = true;
            this.labelReelNumber.Location = new System.Drawing.Point(15, 119);
            this.labelReelNumber.Name = "labelReelNumber";
            this.labelReelNumber.Size = new System.Drawing.Size(67, 13);
            this.labelReelNumber.TabIndex = 4;
            this.labelReelNumber.Text = "Reel number";
            // 
            // textBoxMovieTitle
            // 
            this.textBoxMovieTitle.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxMovieTitle.Location = new System.Drawing.Point(181, 90);
            this.textBoxMovieTitle.Name = "textBoxMovieTitle";
            this.textBoxMovieTitle.Size = new System.Drawing.Size(274, 20);
            this.textBoxMovieTitle.TabIndex = 2;
            // 
            // labelMovieTitle
            // 
            this.labelMovieTitle.AutoSize = true;
            this.labelMovieTitle.Location = new System.Drawing.Point(15, 93);
            this.labelMovieTitle.Name = "labelMovieTitle";
            this.labelMovieTitle.Size = new System.Drawing.Size(55, 13);
            this.labelMovieTitle.TabIndex = 2;
            this.labelMovieTitle.Text = "Movie title";
            // 
            // textBoxSubtitleID
            // 
            this.textBoxSubtitleID.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxSubtitleID.Location = new System.Drawing.Point(181, 30);
            this.textBoxSubtitleID.Name = "textBoxSubtitleID";
            this.textBoxSubtitleID.Size = new System.Drawing.Size(274, 20);
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
            this.buttonCancel.Location = new System.Drawing.Point(521, 588);
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
            this.buttonOK.Location = new System.Drawing.Point(440, 588);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click_1);
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
            // DCinemaPropertiesSmpte
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(607, 622);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DCinemaPropertiesSmpte";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "D-Cinema properties (SMPTE)";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DCinemaPropertiesSmpte_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxFont.ResumeLayout(false);
            this.groupBoxFont.PerformLayout();
            this.contextMenuStripProfile.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBoxFont;
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
        private System.Windows.Forms.Label labelEffectColor;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownFontSize;
        private System.Windows.Forms.Label labelFontSize;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFontEffect;
        private System.Windows.Forms.Label labelEffect;
        private System.Windows.Forms.Panel panelFontEffectColor;
        private System.Windows.Forms.Button buttonFontEffectColor;
        private System.Windows.Forms.Panel panelFontColor;
        private System.Windows.Forms.Button buttonFontColor;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownReelNumber;
        private System.Windows.Forms.Button buttonGenerateID;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.Button buttonToday;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxIssueDate;
        private System.Windows.Forms.Label labelIssueDate;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxEditRate;
        private System.Windows.Forms.Label labelEditRate;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownTopBottomMargin;
        private System.Windows.Forms.Label label2;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxTimeCodeRate;
        private System.Windows.Forms.Label labelTimeCodeRate;
        private Controls.NikseTimeUpDown timeUpDownStartTime;
        private System.Windows.Forms.Label labelStartTime;
        private System.Windows.Forms.Button buttonGenFontUri;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownFadeDown;
        private System.Windows.Forms.Label labelFadeDownTime;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownFadeUp;
        private System.Windows.Forms.Label labelFadeUpTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelFadeUpMs;
        private System.Windows.Forms.CheckBox checkBoxGenerateIdAuto;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripProfile;
        private System.Windows.Forms.ToolStripMenuItem profilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
    }
}