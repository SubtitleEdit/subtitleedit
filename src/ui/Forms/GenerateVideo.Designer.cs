﻿
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
            this.components = new System.ComponentModel.Container();
            this.panelColor = new System.Windows.Forms.Panel();
            this.buttonColor = new System.Windows.Forms.Button();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.contextMenuStripGenerate = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.promptParameterBeforeGenerateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelDuration = new System.Windows.Forms.Label();
            this.numericUpDownWidth = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownHeight = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelResolution = new System.Windows.Forms.Label();
            this.labelX = new System.Windows.Forms.Label();
            this.groupBoxBackground = new System.Windows.Forms.GroupBox();
            this.labelImageFileName = new System.Windows.Forms.Label();
            this.buttonChooseImageFile = new System.Windows.Forms.Button();
            this.radioButtonImage = new System.Windows.Forms.RadioButton();
            this.radioButtonColor = new System.Windows.Forms.RadioButton();
            this.radioButtonCheckeredImage = new System.Windows.Forms.RadioButton();
            this.numericUpDownDurationMinutes = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.labelFrameRate = new System.Windows.Forms.Label();
            this.comboBoxFrameRate = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelProgress = new System.Windows.Forms.Label();
            this.contextMenuStripRes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.x2160ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uHD3840x2160ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.k2048x1080ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dCI2KScope2048x858ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dCI2KFlat1998x1080ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.p1920x1080ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x1080ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.p1280x720ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x720ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.p848x480ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pAL720x576ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nTSC720x480ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x352ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x272ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemResBrowse = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonVideoChooseStandardRes = new System.Windows.Forms.Button();
            this.buttonChooseDuration = new System.Windows.Forms.Button();
            this.checkBoxAddTimeCode = new System.Windows.Forms.CheckBox();
            this.contextMenuStripGenerate.SuspendLayout();
            this.groupBoxBackground.SuspendLayout();
            this.contextMenuStripRes.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelColor
            // 
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(169, 81);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(21, 20);
            this.panelColor.TabIndex = 11;
            this.panelColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelColor_MouseClick);
            // 
            // buttonColor
            // 
            this.buttonColor.Location = new System.Drawing.Point(42, 80);
            this.buttonColor.Name = "buttonColor";
            this.buttonColor.Size = new System.Drawing.Size(121, 23);
            this.buttonColor.TabIndex = 10;
            this.buttonColor.Text = "Color";
            this.buttonColor.UseVisualStyleBackColor = true;
            this.buttonColor.Click += new System.EventHandler(this.buttonColor_Click);
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGenerate.ContextMenuStrip = this.contextMenuStripGenerate;
            this.buttonGenerate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenerate.Location = new System.Drawing.Point(549, 249);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(121, 23);
            this.buttonGenerate.TabIndex = 91;
            this.buttonGenerate.Text = "Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
            // 
            // contextMenuStripGenerate
            // 
            this.contextMenuStripGenerate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.promptParameterBeforeGenerateToolStripMenuItem});
            this.contextMenuStripGenerate.Name = "contextMenuStripGenerate";
            this.contextMenuStripGenerate.Size = new System.Drawing.Size(290, 26);
            // 
            // promptParameterBeforeGenerateToolStripMenuItem
            // 
            this.promptParameterBeforeGenerateToolStripMenuItem.Name = "promptParameterBeforeGenerateToolStripMenuItem";
            this.promptParameterBeforeGenerateToolStripMenuItem.Size = new System.Drawing.Size(289, 22);
            this.promptParameterBeforeGenerateToolStripMenuItem.Text = "Prompt FFmpeg parameter and generate";
            this.promptParameterBeforeGenerateToolStripMenuItem.Click += new System.EventHandler(this.promptParameterBeforeGenerateToolStripMenuItem_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(676, 249);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 92;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 249);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(531, 10);
            this.progressBar1.TabIndex = 8;
            this.progressBar1.Visible = false;
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(12, 29);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(97, 13);
            this.labelDuration.TabIndex = 0;
            this.labelDuration.Text = "Duration in minutes";
            // 
            // numericUpDownWidth
            // 
            this.numericUpDownWidth.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownWidth.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownWidth.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownWidth.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownWidth.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownWidth.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownWidth.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownWidth.DecimalPlaces = 0;
            this.numericUpDownWidth.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownWidth.Location = new System.Drawing.Point(165, 60);
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
            this.numericUpDownWidth.Size = new System.Drawing.Size(64, 23);
            this.numericUpDownWidth.TabIndex = 2;
            this.numericUpDownWidth.TabStop = false;
            this.numericUpDownWidth.ThousandsSeparator = false;
            this.numericUpDownWidth.Value = new decimal(new int[] {
            1280,
            0,
            0,
            0});
            this.numericUpDownWidth.ValueChanged += new System.EventHandler(this.numericUpDownWidth_ValueChanged);
            // 
            // numericUpDownHeight
            // 
            this.numericUpDownHeight.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownHeight.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownHeight.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownHeight.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownHeight.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownHeight.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownHeight.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownHeight.DecimalPlaces = 0;
            this.numericUpDownHeight.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownHeight.Location = new System.Drawing.Point(253, 60);
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
            this.numericUpDownHeight.Size = new System.Drawing.Size(64, 23);
            this.numericUpDownHeight.TabIndex = 4;
            this.numericUpDownHeight.TabStop = false;
            this.numericUpDownHeight.ThousandsSeparator = false;
            this.numericUpDownHeight.Value = new decimal(new int[] {
            720,
            0,
            0,
            0});
            this.numericUpDownHeight.ValueChanged += new System.EventHandler(this.numericUpDownHeight_ValueChanged);
            // 
            // labelResolution
            // 
            this.labelResolution.AutoSize = true;
            this.labelResolution.Location = new System.Drawing.Point(12, 62);
            this.labelResolution.Name = "labelResolution";
            this.labelResolution.Size = new System.Drawing.Size(57, 13);
            this.labelResolution.TabIndex = 2;
            this.labelResolution.Text = "Resolution";
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(235, 62);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(12, 13);
            this.labelX.TabIndex = 3;
            this.labelX.Text = "x";
            // 
            // groupBoxBackground
            // 
            this.groupBoxBackground.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBackground.Controls.Add(this.labelImageFileName);
            this.groupBoxBackground.Controls.Add(this.buttonChooseImageFile);
            this.groupBoxBackground.Controls.Add(this.radioButtonImage);
            this.groupBoxBackground.Controls.Add(this.radioButtonColor);
            this.groupBoxBackground.Controls.Add(this.radioButtonCheckeredImage);
            this.groupBoxBackground.Controls.Add(this.buttonColor);
            this.groupBoxBackground.Controls.Add(this.panelColor);
            this.groupBoxBackground.Location = new System.Drawing.Point(387, 12);
            this.groupBoxBackground.Name = "groupBoxBackground";
            this.groupBoxBackground.Size = new System.Drawing.Size(366, 162);
            this.groupBoxBackground.TabIndex = 7;
            this.groupBoxBackground.TabStop = false;
            this.groupBoxBackground.Text = "Background";
            // 
            // labelImageFileName
            // 
            this.labelImageFileName.AutoSize = true;
            this.labelImageFileName.Location = new System.Drawing.Point(39, 135);
            this.labelImageFileName.Name = "labelImageFileName";
            this.labelImageFileName.Size = new System.Drawing.Size(102, 13);
            this.labelImageFileName.TabIndex = 34;
            this.labelImageFileName.Text = "labelImageFileName";
            // 
            // buttonChooseImageFile
            // 
            this.buttonChooseImageFile.Location = new System.Drawing.Point(78, 109);
            this.buttonChooseImageFile.Name = "buttonChooseImageFile";
            this.buttonChooseImageFile.Size = new System.Drawing.Size(25, 23);
            this.buttonChooseImageFile.TabIndex = 33;
            this.buttonChooseImageFile.Text = "...";
            this.buttonChooseImageFile.UseVisualStyleBackColor = true;
            this.buttonChooseImageFile.Click += new System.EventHandler(this.buttonChooseImageFile_Click);
            // 
            // radioButtonImage
            // 
            this.radioButtonImage.AutoSize = true;
            this.radioButtonImage.Location = new System.Drawing.Point(18, 112);
            this.radioButtonImage.Name = "radioButtonImage";
            this.radioButtonImage.Size = new System.Drawing.Size(54, 17);
            this.radioButtonImage.TabIndex = 12;
            this.radioButtonImage.Text = "Image";
            this.radioButtonImage.UseVisualStyleBackColor = true;
            this.radioButtonImage.CheckedChanged += new System.EventHandler(this.radioButtonImage_CheckedChanged);
            // 
            // radioButtonColor
            // 
            this.radioButtonColor.AutoSize = true;
            this.radioButtonColor.Location = new System.Drawing.Point(18, 57);
            this.radioButtonColor.Name = "radioButtonColor";
            this.radioButtonColor.Size = new System.Drawing.Size(74, 17);
            this.radioButtonColor.TabIndex = 9;
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
            this.radioButtonCheckeredImage.TabIndex = 8;
            this.radioButtonCheckeredImage.TabStop = true;
            this.radioButtonCheckeredImage.Text = "Checkered image";
            this.radioButtonCheckeredImage.UseVisualStyleBackColor = true;
            // 
            // numericUpDownDurationMinutes
            // 
            this.numericUpDownDurationMinutes.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownDurationMinutes.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownDurationMinutes.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownDurationMinutes.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownDurationMinutes.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownDurationMinutes.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownDurationMinutes.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownDurationMinutes.DecimalPlaces = 0;
            this.numericUpDownDurationMinutes.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
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
            this.numericUpDownDurationMinutes.Size = new System.Drawing.Size(64, 23);
            this.numericUpDownDurationMinutes.TabIndex = 0;
            this.numericUpDownDurationMinutes.TabStop = false;
            this.numericUpDownDurationMinutes.ThousandsSeparator = false;
            this.numericUpDownDurationMinutes.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(12, 229);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPleaseWait.TabIndex = 7;
            this.labelPleaseWait.Text = "Please wait...";
            // 
            // labelFrameRate
            // 
            this.labelFrameRate.AutoSize = true;
            this.labelFrameRate.Location = new System.Drawing.Point(12, 89);
            this.labelFrameRate.Name = "labelFrameRate";
            this.labelFrameRate.Size = new System.Drawing.Size(57, 13);
            this.labelFrameRate.TabIndex = 11;
            this.labelFrameRate.Text = "Frame rate";
            // 
            // comboBoxFrameRate
            // 
            this.comboBoxFrameRate.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFrameRate.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFrameRate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFrameRate.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFrameRate.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFrameRate.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFrameRate.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFrameRate.DropDownHeight = 400;
            this.comboBoxFrameRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFrameRate.DropDownWidth = 121;
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Location = new System.Drawing.Point(165, 89);
            this.comboBoxFrameRate.MaxLength = 32767;
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.SelectedIndex = -1;
            this.comboBoxFrameRate.SelectedItem = null;
            this.comboBoxFrameRate.SelectedText = "";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFrameRate.TabIndex = 6;
            this.comboBoxFrameRate.UsePopupWindow = false;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // labelProgress
            // 
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(12, 262);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(88, 13);
            this.labelProgress.TabIndex = 30;
            this.labelProgress.Text = "Remaining time...";
            // 
            // contextMenuStripRes
            // 
            this.contextMenuStripRes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.x2160ToolStripMenuItem,
            this.uHD3840x2160ToolStripMenuItem,
            this.k2048x1080ToolStripMenuItem,
            this.dCI2KScope2048x858ToolStripMenuItem,
            this.dCI2KFlat1998x1080ToolStripMenuItem,
            this.p1920x1080ToolStripMenuItem,
            this.x1080ToolStripMenuItem,
            this.p1280x720ToolStripMenuItem,
            this.x720ToolStripMenuItem,
            this.p848x480ToolStripMenuItem,
            this.pAL720x576ToolStripMenuItem,
            this.nTSC720x480ToolStripMenuItem,
            this.x352ToolStripMenuItem,
            this.x272ToolStripMenuItem,
            this.toolStripMenuItemResBrowse});
            this.contextMenuStripRes.Name = "contextMenuStripRes";
            this.contextMenuStripRes.Size = new System.Drawing.Size(204, 334);
            // 
            // x2160ToolStripMenuItem
            // 
            this.x2160ToolStripMenuItem.Name = "x2160ToolStripMenuItem";
            this.x2160ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.x2160ToolStripMenuItem.Text = "4K (4096x2160)";
            this.x2160ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // uHD3840x2160ToolStripMenuItem
            // 
            this.uHD3840x2160ToolStripMenuItem.Name = "uHD3840x2160ToolStripMenuItem";
            this.uHD3840x2160ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.uHD3840x2160ToolStripMenuItem.Text = "UHD (3840x2160)";
            this.uHD3840x2160ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // k2048x1080ToolStripMenuItem
            // 
            this.k2048x1080ToolStripMenuItem.Name = "k2048x1080ToolStripMenuItem";
            this.k2048x1080ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.k2048x1080ToolStripMenuItem.Text = "2K (2048x1080)";
            this.k2048x1080ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // dCI2KScope2048x858ToolStripMenuItem
            // 
            this.dCI2KScope2048x858ToolStripMenuItem.Name = "dCI2KScope2048x858ToolStripMenuItem";
            this.dCI2KScope2048x858ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.dCI2KScope2048x858ToolStripMenuItem.Text = "DCI 2K Scope (2048x858)";
            this.dCI2KScope2048x858ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // dCI2KFlat1998x1080ToolStripMenuItem
            // 
            this.dCI2KFlat1998x1080ToolStripMenuItem.Name = "dCI2KFlat1998x1080ToolStripMenuItem";
            this.dCI2KFlat1998x1080ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.dCI2KFlat1998x1080ToolStripMenuItem.Text = "DCI 2K Flat (1998x1080)";
            this.dCI2KFlat1998x1080ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // p1920x1080ToolStripMenuItem
            // 
            this.p1920x1080ToolStripMenuItem.Name = "p1920x1080ToolStripMenuItem";
            this.p1920x1080ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.p1920x1080ToolStripMenuItem.Text = "1080p (1920x1080)";
            this.p1920x1080ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // x1080ToolStripMenuItem
            // 
            this.x1080ToolStripMenuItem.Name = "x1080ToolStripMenuItem";
            this.x1080ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.x1080ToolStripMenuItem.Text = "1440x1080";
            this.x1080ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // p1280x720ToolStripMenuItem
            // 
            this.p1280x720ToolStripMenuItem.Name = "p1280x720ToolStripMenuItem";
            this.p1280x720ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.p1280x720ToolStripMenuItem.Text = "720p (1280x720)";
            this.p1280x720ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // x720ToolStripMenuItem
            // 
            this.x720ToolStripMenuItem.Name = "x720ToolStripMenuItem";
            this.x720ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.x720ToolStripMenuItem.Text = "960x720";
            this.x720ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // p848x480ToolStripMenuItem
            // 
            this.p848x480ToolStripMenuItem.Name = "p848x480ToolStripMenuItem";
            this.p848x480ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.p848x480ToolStripMenuItem.Text = "480p (848x480)";
            this.p848x480ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // pAL720x576ToolStripMenuItem
            // 
            this.pAL720x576ToolStripMenuItem.Name = "pAL720x576ToolStripMenuItem";
            this.pAL720x576ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.pAL720x576ToolStripMenuItem.Text = "PAL (720x576)";
            this.pAL720x576ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // nTSC720x480ToolStripMenuItem
            // 
            this.nTSC720x480ToolStripMenuItem.Name = "nTSC720x480ToolStripMenuItem";
            this.nTSC720x480ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.nTSC720x480ToolStripMenuItem.Text = "NTSC (720x480)";
            this.nTSC720x480ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // x352ToolStripMenuItem
            // 
            this.x352ToolStripMenuItem.Name = "x352ToolStripMenuItem";
            this.x352ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.x352ToolStripMenuItem.Text = "640x352";
            this.x352ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // x272ToolStripMenuItem
            // 
            this.x272ToolStripMenuItem.Name = "x272ToolStripMenuItem";
            this.x272ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.x272ToolStripMenuItem.Text = "640x272";
            this.x272ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // toolStripMenuItemResBrowse
            // 
            this.toolStripMenuItemResBrowse.Name = "toolStripMenuItemResBrowse";
            this.toolStripMenuItemResBrowse.Size = new System.Drawing.Size(203, 22);
            this.toolStripMenuItemResBrowse.Text = "...";
            this.toolStripMenuItemResBrowse.Click += new System.EventHandler(this.toolStripMenuItemResBrowse_Click);
            // 
            // buttonVideoChooseStandardRes
            // 
            this.buttonVideoChooseStandardRes.Location = new System.Drawing.Point(323, 60);
            this.buttonVideoChooseStandardRes.Name = "buttonVideoChooseStandardRes";
            this.buttonVideoChooseStandardRes.Size = new System.Drawing.Size(25, 23);
            this.buttonVideoChooseStandardRes.TabIndex = 5;
            this.buttonVideoChooseStandardRes.Text = "...";
            this.buttonVideoChooseStandardRes.UseVisualStyleBackColor = true;
            this.buttonVideoChooseStandardRes.Click += new System.EventHandler(this.buttonVideoChooseStandardRes_Click);
            // 
            // buttonChooseDuration
            // 
            this.buttonChooseDuration.Location = new System.Drawing.Point(236, 29);
            this.buttonChooseDuration.Name = "buttonChooseDuration";
            this.buttonChooseDuration.Size = new System.Drawing.Size(25, 23);
            this.buttonChooseDuration.TabIndex = 1;
            this.buttonChooseDuration.Text = "...";
            this.buttonChooseDuration.UseVisualStyleBackColor = true;
            this.buttonChooseDuration.Click += new System.EventHandler(this.buttonChooseDuration_Click);
            // 
            // checkBoxAddTimeCode
            // 
            this.checkBoxAddTimeCode.AutoSize = true;
            this.checkBoxAddTimeCode.Location = new System.Drawing.Point(387, 181);
            this.checkBoxAddTimeCode.Name = "checkBoxAddTimeCode";
            this.checkBoxAddTimeCode.Size = new System.Drawing.Size(94, 17);
            this.checkBoxAddTimeCode.TabIndex = 9;
            this.checkBoxAddTimeCode.Text = "Add time code";
            this.checkBoxAddTimeCode.UseVisualStyleBackColor = true;
            // 
            // GenerateVideo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(763, 284);
            this.Controls.Add(this.checkBoxAddTimeCode);
            this.Controls.Add(this.buttonChooseDuration);
            this.Controls.Add(this.buttonVideoChooseStandardRes);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.comboBoxFrameRate);
            this.Controls.Add(this.labelFrameRate);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.numericUpDownDurationMinutes);
            this.Controls.Add(this.groupBoxBackground);
            this.Controls.Add(this.labelX);
            this.Controls.Add(this.labelResolution);
            this.Controls.Add(this.numericUpDownHeight);
            this.Controls.Add(this.numericUpDownWidth);
            this.Controls.Add(this.labelDuration);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonGenerate);
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
            this.Shown += new System.EventHandler(this.GenerateVideo_Shown);
            this.contextMenuStripGenerate.ResumeLayout(false);
            this.groupBoxBackground.ResumeLayout(false);
            this.groupBoxBackground.PerformLayout();
            this.contextMenuStripRes.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Button buttonColor;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelDuration;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownWidth;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownHeight;
        private System.Windows.Forms.Label labelResolution;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.GroupBox groupBoxBackground;
        private System.Windows.Forms.RadioButton radioButtonColor;
        private System.Windows.Forms.RadioButton radioButtonCheckeredImage;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownDurationMinutes;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.Label labelFrameRate;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFrameRate;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRes;
        private System.Windows.Forms.ToolStripMenuItem x2160ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uHD3840x2160ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem k2048x1080ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dCI2KScope2048x858ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dCI2KFlat1998x1080ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem p1920x1080ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x1080ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem p1280x720ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x720ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem p848x480ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pAL720x576ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nTSC720x480ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x352ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x272ToolStripMenuItem;
        private System.Windows.Forms.Button buttonVideoChooseStandardRes;
        private System.Windows.Forms.Label labelImageFileName;
        private System.Windows.Forms.Button buttonChooseImageFile;
        private System.Windows.Forms.RadioButton radioButtonImage;
        private System.Windows.Forms.Button buttonChooseDuration;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemResBrowse;
        private System.Windows.Forms.CheckBox checkBoxAddTimeCode;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripGenerate;
        private System.Windows.Forms.ToolStripMenuItem promptParameterBeforeGenerateToolStripMenuItem;
    }
}