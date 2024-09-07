﻿namespace Nikse.SubtitleEdit.Forms
{
    partial class ExportPngXml
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveImageAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBoxImageSettings = new System.Windows.Forms.GroupBox();
            this.labelImagePrefix = new System.Windows.Forms.Label();
            this.textBoxImagePrefix = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.comboBoxResolution = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelResize = new System.Windows.Forms.Label();
            this.comboBoxResizePercentage = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxBottomMarginUnit = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxLeftRightMarginUnit = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelLineHeightStyle = new System.Windows.Forms.Label();
            this.panelFullFrameBackground = new System.Windows.Forms.Panel();
            this.comboBoxLeftRightMargin = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxFullFrameImage = new System.Windows.Forms.CheckBox();
            this.checkBoxTransAntiAliase = new System.Windows.Forms.CheckBox();
            this.labelLineHeight = new System.Windows.Forms.Label();
            this.numericUpDownLineSpacing = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownShadowTransparency = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelShadowTransparency = new System.Windows.Forms.Label();
            this.labelShadowWidth = new System.Windows.Forms.Label();
            this.comboBoxShadowWidth = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.panelShadowColor = new System.Windows.Forms.Panel();
            this.buttonShadowColor = new System.Windows.Forms.Button();
            this.labelDepth = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.label3D = new System.Windows.Forms.Label();
            this.comboBox3D = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.numericUpDownDepth3D = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.buttonCustomResolution = new System.Windows.Forms.Button();
            this.comboBoxBottomMargin = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelBottomMargin = new System.Windows.Forms.Label();
            this.labelFrameRate = new System.Windows.Forms.Label();
            this.comboBoxFrameRate = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelImageFormat = new System.Windows.Forms.Label();
            this.comboBoxImageFormat = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxBold = new System.Windows.Forms.CheckBox();
            this.labelResolution = new System.Windows.Forms.Label();
            this.comboBoxHAlign = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelHorizontalAlign = new System.Windows.Forms.Label();
            this.checkBoxSimpleRender = new System.Windows.Forms.CheckBox();
            this.labelSubtitleFontSize = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxSubtitleFont = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxSubtitleFontSize = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelSubtitleFont = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelBorderWidth = new System.Windows.Forms.Label();
            this.comboBoxBorderWidth = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.panelBorderColor = new System.Windows.Forms.Panel();
            this.buttonBorderColor = new System.Windows.Forms.Button();
            this.panelColor = new System.Windows.Forms.Panel();
            this.buttonColor = new System.Windows.Forms.Button();
            this.checkBoxSkipEmptyFrameAtStart = new System.Windows.Forms.CheckBox();
            this.checkBoxFcpFullPathUrl = new System.Windows.Forms.CheckBox();
            this.labelLeftRightMargin = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBoxExportImage = new System.Windows.Forms.GroupBox();
            this.linkLabelPreview = new System.Windows.Forms.LinkLabel();
            this.panelVlcTemp = new System.Windows.Forms.Panel();
            this.timerPreview = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStripListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.normalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.italicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boxSingleLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boxMultiLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorAdjust = new System.Windows.Forms.ToolStripSeparator();
            this.adjustDisplayTimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adjustTimeCodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.subtitleListView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripProfile = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.profilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBoxImageSettings.SuspendLayout();
            this.groupBoxExportImage.SuspendLayout();
            this.contextMenuStripListView.SuspendLayout();
            this.contextMenuStripProfile.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBox1.Location = new System.Drawing.Point(6, 19);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(553, 152);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveImageAsToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(158, 26);
            // 
            // saveImageAsToolStripMenuItem
            // 
            this.saveImageAsToolStripMenuItem.Name = "saveImageAsToolStripMenuItem";
            this.saveImageAsToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.saveImageAsToolStripMenuItem.Text = "Save image as...";
            this.saveImageAsToolStripMenuItem.Click += new System.EventHandler(this.saveImageAsToolStripMenuItem_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonExport.Location = new System.Drawing.Point(762, 597);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(126, 23);
            this.buttonExport.TabIndex = 6;
            this.buttonExport.Text = "Export all lines...";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.ButtonExportClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(894, 597);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 599);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(744, 21);
            this.progressBar1.TabIndex = 5;
            this.progressBar1.Visible = false;
            // 
            // groupBoxImageSettings
            // 
            this.groupBoxImageSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImageSettings.Controls.Add(this.labelImagePrefix);
            this.groupBoxImageSettings.Controls.Add(this.textBoxImagePrefix);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxResolution);
            this.groupBoxImageSettings.Controls.Add(this.labelResize);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxResizePercentage);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxBottomMarginUnit);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxLeftRightMarginUnit);
            this.groupBoxImageSettings.Controls.Add(this.labelLineHeightStyle);
            this.groupBoxImageSettings.Controls.Add(this.panelFullFrameBackground);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxLeftRightMargin);
            this.groupBoxImageSettings.Controls.Add(this.checkBoxFullFrameImage);
            this.groupBoxImageSettings.Controls.Add(this.checkBoxTransAntiAliase);
            this.groupBoxImageSettings.Controls.Add(this.labelLineHeight);
            this.groupBoxImageSettings.Controls.Add(this.numericUpDownLineSpacing);
            this.groupBoxImageSettings.Controls.Add(this.numericUpDownShadowTransparency);
            this.groupBoxImageSettings.Controls.Add(this.labelShadowTransparency);
            this.groupBoxImageSettings.Controls.Add(this.labelShadowWidth);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxShadowWidth);
            this.groupBoxImageSettings.Controls.Add(this.panelShadowColor);
            this.groupBoxImageSettings.Controls.Add(this.buttonShadowColor);
            this.groupBoxImageSettings.Controls.Add(this.labelDepth);
            this.groupBoxImageSettings.Controls.Add(this.label3D);
            this.groupBoxImageSettings.Controls.Add(this.comboBox3D);
            this.groupBoxImageSettings.Controls.Add(this.numericUpDownDepth3D);
            this.groupBoxImageSettings.Controls.Add(this.buttonCustomResolution);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxBottomMargin);
            this.groupBoxImageSettings.Controls.Add(this.labelBottomMargin);
            this.groupBoxImageSettings.Controls.Add(this.labelFrameRate);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxFrameRate);
            this.groupBoxImageSettings.Controls.Add(this.labelLanguage);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxLanguage);
            this.groupBoxImageSettings.Controls.Add(this.labelImageFormat);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxImageFormat);
            this.groupBoxImageSettings.Controls.Add(this.checkBoxBold);
            this.groupBoxImageSettings.Controls.Add(this.labelResolution);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxHAlign);
            this.groupBoxImageSettings.Controls.Add(this.labelHorizontalAlign);
            this.groupBoxImageSettings.Controls.Add(this.checkBoxSimpleRender);
            this.groupBoxImageSettings.Controls.Add(this.labelSubtitleFontSize);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxSubtitleFont);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxSubtitleFontSize);
            this.groupBoxImageSettings.Controls.Add(this.labelSubtitleFont);
            this.groupBoxImageSettings.Controls.Add(this.labelBorderWidth);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxBorderWidth);
            this.groupBoxImageSettings.Controls.Add(this.panelBorderColor);
            this.groupBoxImageSettings.Controls.Add(this.buttonBorderColor);
            this.groupBoxImageSettings.Controls.Add(this.panelColor);
            this.groupBoxImageSettings.Controls.Add(this.buttonColor);
            this.groupBoxImageSettings.Controls.Add(this.checkBoxSkipEmptyFrameAtStart);
            this.groupBoxImageSettings.Controls.Add(this.checkBoxFcpFullPathUrl);
            this.groupBoxImageSettings.Controls.Add(this.labelLeftRightMargin);
            this.groupBoxImageSettings.Location = new System.Drawing.Point(12, 218);
            this.groupBoxImageSettings.Name = "groupBoxImageSettings";
            this.groupBoxImageSettings.Size = new System.Drawing.Size(963, 191);
            this.groupBoxImageSettings.TabIndex = 3;
            this.groupBoxImageSettings.TabStop = false;
            this.groupBoxImageSettings.Text = "Image settings";
            // 
            // labelImagePrefix
            // 
            this.labelImagePrefix.AutoSize = true;
            this.labelImagePrefix.Location = new System.Drawing.Point(284, 165);
            this.labelImagePrefix.Name = "labelImagePrefix";
            this.labelImagePrefix.Size = new System.Drawing.Size(64, 13);
            this.labelImagePrefix.TabIndex = 64;
            this.labelImagePrefix.Text = "Image prefix";
            // 
            // textBoxImagePrefix
            // 
            this.textBoxImagePrefix.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxImagePrefix.Location = new System.Drawing.Point(354, 162);
            this.textBoxImagePrefix.Name = "textBoxImagePrefix";
            this.textBoxImagePrefix.Size = new System.Drawing.Size(183, 20);
            this.textBoxImagePrefix.TabIndex = 63;
            // 
            // comboBoxResolution
            // 
            this.comboBoxResolution.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxResolution.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxResolution.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxResolution.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxResolution.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxResolution.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxResolution.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxResolution.DropDownHeight = 400;
            this.comboBoxResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxResolution.DropDownWidth = 160;
            this.comboBoxResolution.FormattingEnabled = true;
            this.comboBoxResolution.Items.AddRange(new object[] {
            "4K (4096x2160)",
            "UHD (3840x2160)",
            "2K (2048x1080)",
            "DCI 2K Scope (2048x858)",
            "DCI 2K Flat (1998x1080)",
            "1080p (1920x1080)",
            "1440x1080",
            "720p (1280x720)",
            "960x720",
            "480p (848x480)",
            "PAL (720x576)",
            "NTSC (720x480)",
            "640x352",
            "640x272"});
            this.comboBoxResolution.Location = new System.Drawing.Point(112, 78);
            this.comboBoxResolution.MaxLength = 32767;
            this.comboBoxResolution.Name = "comboBoxResolution";
            this.comboBoxResolution.SelectedIndex = -1;
            this.comboBoxResolution.SelectedItem = null;
            this.comboBoxResolution.SelectedText = "";
            this.comboBoxResolution.Size = new System.Drawing.Size(123, 21);
            this.comboBoxResolution.TabIndex = 5;
            this.comboBoxResolution.UsePopupWindow = false;
            this.comboBoxResolution.SelectedIndexChanged += new System.EventHandler(this.comboBoxResolution_SelectedIndexChanged);
            // 
            // labelResize
            // 
            this.labelResize.AutoSize = true;
            this.labelResize.Location = new System.Drawing.Point(263, 8);
            this.labelResize.Name = "labelResize";
            this.labelResize.Size = new System.Drawing.Size(39, 13);
            this.labelResize.TabIndex = 61;
            this.labelResize.Text = "Resize";
            this.labelResize.Visible = false;
            // 
            // comboBoxResizePercentage
            // 
            this.comboBoxResizePercentage.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxResizePercentage.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxResizePercentage.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxResizePercentage.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxResizePercentage.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxResizePercentage.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxResizePercentage.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxResizePercentage.DropDownHeight = 400;
            this.comboBoxResizePercentage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxResizePercentage.DropDownWidth = 77;
            this.comboBoxResizePercentage.FormattingEnabled = true;
            this.comboBoxResizePercentage.Location = new System.Drawing.Point(308, 5);
            this.comboBoxResizePercentage.MaxLength = 32767;
            this.comboBoxResizePercentage.Name = "comboBoxResizePercentage";
            this.comboBoxResizePercentage.SelectedIndex = -1;
            this.comboBoxResizePercentage.SelectedItem = null;
            this.comboBoxResizePercentage.SelectedText = "";
            this.comboBoxResizePercentage.Size = new System.Drawing.Size(77, 21);
            this.comboBoxResizePercentage.TabIndex = 18;
            this.comboBoxResizePercentage.UsePopupWindow = false;
            this.comboBoxResizePercentage.Visible = false;
            this.comboBoxResizePercentage.SelectedIndexChanged += new System.EventHandler(this.comboBoxResizePercentage_SelectedIndexChanged);
            // 
            // comboBoxBottomMarginUnit
            // 
            this.comboBoxBottomMarginUnit.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxBottomMarginUnit.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxBottomMarginUnit.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxBottomMarginUnit.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxBottomMarginUnit.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxBottomMarginUnit.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxBottomMarginUnit.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxBottomMarginUnit.DropDownHeight = 400;
            this.comboBoxBottomMarginUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBottomMarginUnit.DropDownWidth = 54;
            this.comboBoxBottomMarginUnit.FormattingEnabled = true;
            this.comboBoxBottomMarginUnit.Items.AddRange(new object[] {
            "%",
            "px"});
            this.comboBoxBottomMarginUnit.Location = new System.Drawing.Point(181, 132);
            this.comboBoxBottomMarginUnit.MaxLength = 32767;
            this.comboBoxBottomMarginUnit.Name = "comboBoxBottomMarginUnit";
            this.comboBoxBottomMarginUnit.SelectedIndex = -1;
            this.comboBoxBottomMarginUnit.SelectedItem = null;
            this.comboBoxBottomMarginUnit.SelectedText = "";
            this.comboBoxBottomMarginUnit.Size = new System.Drawing.Size(54, 21);
            this.comboBoxBottomMarginUnit.TabIndex = 14;
            this.comboBoxBottomMarginUnit.UsePopupWindow = false;
            this.comboBoxBottomMarginUnit.SelectedIndexChanged += new System.EventHandler(this.comboBoxBottomMarginUnit_SelectedIndexChanged);
            // 
            // comboBoxLeftRightMarginUnit
            // 
            this.comboBoxLeftRightMarginUnit.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxLeftRightMarginUnit.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxLeftRightMarginUnit.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxLeftRightMarginUnit.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxLeftRightMarginUnit.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxLeftRightMarginUnit.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxLeftRightMarginUnit.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxLeftRightMarginUnit.DropDownHeight = 400;
            this.comboBoxLeftRightMarginUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLeftRightMarginUnit.DropDownWidth = 54;
            this.comboBoxLeftRightMarginUnit.FormattingEnabled = true;
            this.comboBoxLeftRightMarginUnit.Items.AddRange(new object[] {
            "%",
            "px"});
            this.comboBoxLeftRightMarginUnit.Location = new System.Drawing.Point(181, 159);
            this.comboBoxLeftRightMarginUnit.MaxLength = 32767;
            this.comboBoxLeftRightMarginUnit.Name = "comboBoxLeftRightMarginUnit";
            this.comboBoxLeftRightMarginUnit.SelectedIndex = -1;
            this.comboBoxLeftRightMarginUnit.SelectedItem = null;
            this.comboBoxLeftRightMarginUnit.SelectedText = "";
            this.comboBoxLeftRightMarginUnit.Size = new System.Drawing.Size(54, 21);
            this.comboBoxLeftRightMarginUnit.TabIndex = 17;
            this.comboBoxLeftRightMarginUnit.UsePopupWindow = false;
            this.comboBoxLeftRightMarginUnit.SelectedIndexChanged += new System.EventHandler(this.comboBoxLeftRightMarginUnit_SelectedIndexChanged);
            // 
            // labelLineHeightStyle
            // 
            this.labelLineHeightStyle.AutoSize = true;
            this.labelLineHeightStyle.Location = new System.Drawing.Point(799, 155);
            this.labelLineHeightStyle.Name = "labelLineHeightStyle";
            this.labelLineHeightStyle.Size = new System.Drawing.Size(103, 13);
            this.labelLineHeightStyle.TabIndex = 59;
            this.labelLineHeightStyle.Text = "labelLineHeightStyle";
            // 
            // panelFullFrameBackground
            // 
            this.panelFullFrameBackground.BackColor = System.Drawing.Color.Transparent;
            this.panelFullFrameBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFullFrameBackground.Location = new System.Drawing.Point(677, 104);
            this.panelFullFrameBackground.Name = "panelFullFrameBackground";
            this.panelFullFrameBackground.Size = new System.Drawing.Size(21, 20);
            this.panelFullFrameBackground.TabIndex = 36;
            this.panelFullFrameBackground.Visible = false;
            this.panelFullFrameBackground.Click += new System.EventHandler(this.panelFullFrameBackground_Click);
            // 
            // comboBoxLeftRightMargin
            // 
            this.comboBoxLeftRightMargin.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxLeftRightMargin.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxLeftRightMargin.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxLeftRightMargin.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxLeftRightMargin.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxLeftRightMargin.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxLeftRightMargin.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxLeftRightMargin.DropDownHeight = 400;
            this.comboBoxLeftRightMargin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLeftRightMargin.DropDownWidth = 63;
            this.comboBoxLeftRightMargin.FormattingEnabled = true;
            this.comboBoxLeftRightMargin.Location = new System.Drawing.Point(112, 159);
            this.comboBoxLeftRightMargin.MaxLength = 32767;
            this.comboBoxLeftRightMargin.Name = "comboBoxLeftRightMargin";
            this.comboBoxLeftRightMargin.SelectedIndex = -1;
            this.comboBoxLeftRightMargin.SelectedItem = null;
            this.comboBoxLeftRightMargin.SelectedText = "";
            this.comboBoxLeftRightMargin.Size = new System.Drawing.Size(63, 21);
            this.comboBoxLeftRightMargin.TabIndex = 16;
            this.comboBoxLeftRightMargin.UsePopupWindow = false;
            this.comboBoxLeftRightMargin.SelectedIndexChanged += new System.EventHandler(this.comboBoxLeftRightMargin_SelectedIndexChanged);
            // 
            // checkBoxFullFrameImage
            // 
            this.checkBoxFullFrameImage.AutoSize = true;
            this.checkBoxFullFrameImage.Location = new System.Drawing.Point(543, 104);
            this.checkBoxFullFrameImage.Name = "checkBoxFullFrameImage";
            this.checkBoxFullFrameImage.Size = new System.Drawing.Size(102, 17);
            this.checkBoxFullFrameImage.TabIndex = 34;
            this.checkBoxFullFrameImage.Text = "Full frame image";
            this.checkBoxFullFrameImage.UseVisualStyleBackColor = true;
            this.checkBoxFullFrameImage.CheckedChanged += new System.EventHandler(this.checkBoxFullFrameImage_CheckedChanged);
            // 
            // checkBoxTransAntiAliase
            // 
            this.checkBoxTransAntiAliase.AutoSize = true;
            this.checkBoxTransAntiAliase.Location = new System.Drawing.Point(287, 92);
            this.checkBoxTransAntiAliase.Name = "checkBoxTransAntiAliase";
            this.checkBoxTransAntiAliase.Size = new System.Drawing.Size(162, 17);
            this.checkBoxTransAntiAliase.TabIndex = 21;
            this.checkBoxTransAntiAliase.Text = "Anti-alising with transparency";
            this.checkBoxTransAntiAliase.UseVisualStyleBackColor = true;
            this.checkBoxTransAntiAliase.CheckedChanged += new System.EventHandler(this.checkBoxAntiAlias_CheckedChanged);
            // 
            // labelLineHeight
            // 
            this.labelLineHeight.Location = new System.Drawing.Point(686, 132);
            this.labelLineHeight.Name = "labelLineHeight";
            this.labelLineHeight.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelLineHeight.Size = new System.Drawing.Size(110, 16);
            this.labelLineHeight.TabIndex = 54;
            this.labelLineHeight.Text = "Line height";
            this.labelLineHeight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericUpDownLineSpacing
            // 
            this.numericUpDownLineSpacing.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownLineSpacing.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownLineSpacing.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownLineSpacing.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownLineSpacing.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownLineSpacing.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownLineSpacing.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownLineSpacing.DecimalPlaces = 0;
            this.numericUpDownLineSpacing.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownLineSpacing.Location = new System.Drawing.Point(802, 130);
            this.numericUpDownLineSpacing.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDownLineSpacing.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownLineSpacing.Name = "numericUpDownLineSpacing";
            this.numericUpDownLineSpacing.Size = new System.Drawing.Size(47, 23);
            this.numericUpDownLineSpacing.TabIndex = 44;
            this.numericUpDownLineSpacing.TabStop = false;
            this.numericUpDownLineSpacing.ThousandsSeparator = false;
            this.numericUpDownLineSpacing.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownLineSpacing.ValueChanged += new System.EventHandler(this.numericUpDownLineSpacing_ValueChanged);
            this.numericUpDownLineSpacing.KeyUp += new System.Windows.Forms.KeyEventHandler(this.numericUpDownLineSpacing_KeyUp);
            // 
            // numericUpDownShadowTransparency
            // 
            this.numericUpDownShadowTransparency.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownShadowTransparency.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownShadowTransparency.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownShadowTransparency.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownShadowTransparency.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownShadowTransparency.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownShadowTransparency.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownShadowTransparency.DecimalPlaces = 0;
            this.numericUpDownShadowTransparency.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownShadowTransparency.Location = new System.Drawing.Point(802, 81);
            this.numericUpDownShadowTransparency.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownShadowTransparency.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownShadowTransparency.Name = "numericUpDownShadowTransparency";
            this.numericUpDownShadowTransparency.Size = new System.Drawing.Size(47, 23);
            this.numericUpDownShadowTransparency.TabIndex = 43;
            this.numericUpDownShadowTransparency.TabStop = false;
            this.numericUpDownShadowTransparency.ThousandsSeparator = false;
            this.numericUpDownShadowTransparency.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownShadowTransparency.ValueChanged += new System.EventHandler(this.numericUpDownShadowTransparency_ValueChanged);
            // 
            // labelShadowTransparency
            // 
            this.labelShadowTransparency.Location = new System.Drawing.Point(686, 83);
            this.labelShadowTransparency.Name = "labelShadowTransparency";
            this.labelShadowTransparency.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelShadowTransparency.Size = new System.Drawing.Size(110, 13);
            this.labelShadowTransparency.TabIndex = 39;
            this.labelShadowTransparency.Text = "Transparency";
            this.labelShadowTransparency.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelShadowWidth
            // 
            this.labelShadowWidth.Location = new System.Drawing.Point(686, 55);
            this.labelShadowWidth.Name = "labelShadowWidth";
            this.labelShadowWidth.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelShadowWidth.Size = new System.Drawing.Size(110, 13);
            this.labelShadowWidth.TabIndex = 35;
            this.labelShadowWidth.Text = "Shadow width";
            this.labelShadowWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxShadowWidth
            // 
            this.comboBoxShadowWidth.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxShadowWidth.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxShadowWidth.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxShadowWidth.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxShadowWidth.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxShadowWidth.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxShadowWidth.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxShadowWidth.DropDownHeight = 400;
            this.comboBoxShadowWidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShadowWidth.DropDownWidth = 121;
            this.comboBoxShadowWidth.FormattingEnabled = true;
            this.comboBoxShadowWidth.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15"});
            this.comboBoxShadowWidth.Location = new System.Drawing.Point(802, 52);
            this.comboBoxShadowWidth.MaxLength = 32767;
            this.comboBoxShadowWidth.Name = "comboBoxShadowWidth";
            this.comboBoxShadowWidth.SelectedIndex = -1;
            this.comboBoxShadowWidth.SelectedItem = null;
            this.comboBoxShadowWidth.SelectedText = "";
            this.comboBoxShadowWidth.Size = new System.Drawing.Size(121, 21);
            this.comboBoxShadowWidth.TabIndex = 42;
            this.comboBoxShadowWidth.UsePopupWindow = false;
            this.comboBoxShadowWidth.SelectedIndexChanged += new System.EventHandler(this.comboBoxShadowWidth_SelectedIndexChanged);
            // 
            // panelShadowColor
            // 
            this.panelShadowColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelShadowColor.Location = new System.Drawing.Point(929, 26);
            this.panelShadowColor.Name = "panelShadowColor";
            this.panelShadowColor.Size = new System.Drawing.Size(21, 20);
            this.panelShadowColor.TabIndex = 41;
            this.panelShadowColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelShadowColor_MouseClick);
            // 
            // buttonShadowColor
            // 
            this.buttonShadowColor.Location = new System.Drawing.Point(802, 25);
            this.buttonShadowColor.Name = "buttonShadowColor";
            this.buttonShadowColor.Size = new System.Drawing.Size(121, 23);
            this.buttonShadowColor.TabIndex = 40;
            this.buttonShadowColor.Text = "Shadow color";
            this.buttonShadowColor.UseVisualStyleBackColor = true;
            this.buttonShadowColor.Click += new System.EventHandler(this.buttonShadowColor_Click);
            // 
            // labelDepth
            // 
            this.labelDepth.Location = new System.Drawing.Point(247, 134);
            this.labelDepth.Name = "labelDepth";
            this.labelDepth.Size = new System.Drawing.Size(100, 18);
            this.labelDepth.TabIndex = 18;
            this.labelDepth.Text = "Depth";
            this.labelDepth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3D
            // 
            this.label3D.AutoSize = true;
            this.label3D.Location = new System.Drawing.Point(284, 113);
            this.label3D.Name = "label3D";
            this.label3D.Size = new System.Drawing.Size(21, 13);
            this.label3D.TabIndex = 16;
            this.label3D.Text = "3D";
            // 
            // comboBox3D
            // 
            this.comboBox3D.BackColor = System.Drawing.SystemColors.Window;
            this.comboBox3D.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBox3D.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBox3D.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBox3D.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBox3D.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBox3D.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBox3D.DropDownHeight = 400;
            this.comboBox3D.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3D.DropDownWidth = 150;
            this.comboBox3D.FormattingEnabled = true;
            this.comboBox3D.Location = new System.Drawing.Point(309, 110);
            this.comboBox3D.MaxLength = 32767;
            this.comboBox3D.Name = "comboBox3D";
            this.comboBox3D.SelectedIndex = -1;
            this.comboBox3D.SelectedItem = null;
            this.comboBox3D.SelectedText = "";
            this.comboBox3D.Size = new System.Drawing.Size(112, 21);
            this.comboBox3D.TabIndex = 22;
            this.comboBox3D.UsePopupWindow = false;
            this.comboBox3D.SelectedIndexChanged += new System.EventHandler(this.comboBox3D_SelectedIndexChanged);
            // 
            // numericUpDownDepth3D
            // 
            this.numericUpDownDepth3D.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownDepth3D.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownDepth3D.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownDepth3D.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownDepth3D.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownDepth3D.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownDepth3D.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownDepth3D.DecimalPlaces = 0;
            this.numericUpDownDepth3D.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownDepth3D.Location = new System.Drawing.Point(353, 135);
            this.numericUpDownDepth3D.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDepth3D.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericUpDownDepth3D.Name = "numericUpDownDepth3D";
            this.numericUpDownDepth3D.Size = new System.Drawing.Size(47, 23);
            this.numericUpDownDepth3D.TabIndex = 23;
            this.numericUpDownDepth3D.TabStop = false;
            this.numericUpDownDepth3D.ThousandsSeparator = false;
            this.numericUpDownDepth3D.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownDepth3D.ValueChanged += new System.EventHandler(this.numericUpDownDepth3D_ValueChanged);
            // 
            // buttonCustomResolution
            // 
            this.buttonCustomResolution.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCustomResolution.Location = new System.Drawing.Point(238, 78);
            this.buttonCustomResolution.Name = "buttonCustomResolution";
            this.buttonCustomResolution.Size = new System.Drawing.Size(24, 21);
            this.buttonCustomResolution.TabIndex = 6;
            this.buttonCustomResolution.Text = "...";
            this.buttonCustomResolution.UseVisualStyleBackColor = true;
            this.buttonCustomResolution.Click += new System.EventHandler(this.buttonCustomResolution_Click);
            // 
            // comboBoxBottomMargin
            // 
            this.comboBoxBottomMargin.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxBottomMargin.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxBottomMargin.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxBottomMargin.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxBottomMargin.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxBottomMargin.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxBottomMargin.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxBottomMargin.DropDownHeight = 400;
            this.comboBoxBottomMargin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBottomMargin.DropDownWidth = 63;
            this.comboBoxBottomMargin.FormattingEnabled = true;
            this.comboBoxBottomMargin.Location = new System.Drawing.Point(112, 132);
            this.comboBoxBottomMargin.MaxLength = 32767;
            this.comboBoxBottomMargin.Name = "comboBoxBottomMargin";
            this.comboBoxBottomMargin.SelectedIndex = -1;
            this.comboBoxBottomMargin.SelectedItem = null;
            this.comboBoxBottomMargin.SelectedText = "";
            this.comboBoxBottomMargin.Size = new System.Drawing.Size(63, 21);
            this.comboBoxBottomMargin.TabIndex = 10;
            this.comboBoxBottomMargin.UsePopupWindow = false;
            this.comboBoxBottomMargin.SelectedIndexChanged += new System.EventHandler(this.comboBoxBottomMargin_SelectedIndexChanged);
            // 
            // labelBottomMargin
            // 
            this.labelBottomMargin.AutoSize = true;
            this.labelBottomMargin.Location = new System.Drawing.Point(5, 135);
            this.labelBottomMargin.Name = "labelBottomMargin";
            this.labelBottomMargin.Size = new System.Drawing.Size(74, 13);
            this.labelBottomMargin.TabIndex = 9;
            this.labelBottomMargin.Text = "Bottom margin";
            // 
            // labelFrameRate
            // 
            this.labelFrameRate.Location = new System.Drawing.Point(427, 137);
            this.labelFrameRate.Name = "labelFrameRate";
            this.labelFrameRate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelFrameRate.Size = new System.Drawing.Size(110, 13);
            this.labelFrameRate.TabIndex = 34;
            this.labelFrameRate.Text = "Frame rate";
            this.labelFrameRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.labelFrameRate.Visible = false;
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
            this.comboBoxFrameRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxFrameRate.DropDownWidth = 128;
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Location = new System.Drawing.Point(543, 134);
            this.comboBoxFrameRate.MaxLength = 32767;
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.SelectedIndex = -1;
            this.comboBoxFrameRate.SelectedItem = null;
            this.comboBoxFrameRate.SelectedText = "";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(128, 21);
            this.comboBoxFrameRate.TabIndex = 37;
            this.comboBoxFrameRate.TabStop = false;
            this.comboBoxFrameRate.UsePopupWindow = false;
            // 
            // labelLanguage
            // 
            this.labelLanguage.Location = new System.Drawing.Point(427, 108);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelLanguage.Size = new System.Drawing.Size(110, 13);
            this.labelLanguage.TabIndex = 30;
            this.labelLanguage.Text = "Language";
            this.labelLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.labelLanguage.Visible = false;
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
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.DropDownWidth = 128;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Items.AddRange(new object[] {
            "Bmp",
            "Exif",
            "Gif",
            "Jpg",
            "Png",
            "Tiff"});
            this.comboBoxLanguage.Location = new System.Drawing.Point(543, 105);
            this.comboBoxLanguage.MaxLength = 32767;
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.SelectedIndex = -1;
            this.comboBoxLanguage.SelectedItem = null;
            this.comboBoxLanguage.SelectedText = "";
            this.comboBoxLanguage.Size = new System.Drawing.Size(128, 21);
            this.comboBoxLanguage.TabIndex = 35;
            this.comboBoxLanguage.UsePopupWindow = false;
            this.comboBoxLanguage.Visible = false;
            // 
            // labelImageFormat
            // 
            this.labelImageFormat.Location = new System.Drawing.Point(427, 81);
            this.labelImageFormat.Name = "labelImageFormat";
            this.labelImageFormat.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelImageFormat.Size = new System.Drawing.Size(110, 13);
            this.labelImageFormat.TabIndex = 28;
            this.labelImageFormat.Text = "Image format";
            this.labelImageFormat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxImageFormat
            // 
            this.comboBoxImageFormat.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxImageFormat.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxImageFormat.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxImageFormat.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxImageFormat.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxImageFormat.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxImageFormat.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxImageFormat.DropDownHeight = 400;
            this.comboBoxImageFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxImageFormat.DropDownWidth = 128;
            this.comboBoxImageFormat.FormattingEnabled = true;
            this.comboBoxImageFormat.Items.AddRange(new object[] {
            "Bmp",
            "Exif",
            "Gif",
            "Jpg",
            "Png",
            "Tiff",
            "Tga"});
            this.comboBoxImageFormat.Location = new System.Drawing.Point(543, 78);
            this.comboBoxImageFormat.MaxLength = 32767;
            this.comboBoxImageFormat.Name = "comboBoxImageFormat";
            this.comboBoxImageFormat.SelectedIndex = -1;
            this.comboBoxImageFormat.SelectedItem = null;
            this.comboBoxImageFormat.SelectedText = "";
            this.comboBoxImageFormat.Size = new System.Drawing.Size(128, 21);
            this.comboBoxImageFormat.TabIndex = 33;
            this.comboBoxImageFormat.UsePopupWindow = false;
            // 
            // checkBoxBold
            // 
            this.checkBoxBold.AutoSize = true;
            this.checkBoxBold.Location = new System.Drawing.Point(287, 50);
            this.checkBoxBold.Name = "checkBoxBold";
            this.checkBoxBold.Size = new System.Drawing.Size(47, 17);
            this.checkBoxBold.TabIndex = 19;
            this.checkBoxBold.Text = "Bold";
            this.checkBoxBold.UseVisualStyleBackColor = true;
            this.checkBoxBold.CheckedChanged += new System.EventHandler(this.checkBoxBold_CheckedChanged);
            // 
            // labelResolution
            // 
            this.labelResolution.AutoSize = true;
            this.labelResolution.Location = new System.Drawing.Point(5, 81);
            this.labelResolution.Name = "labelResolution";
            this.labelResolution.Size = new System.Drawing.Size(51, 13);
            this.labelResolution.TabIndex = 4;
            this.labelResolution.Text = "Video res";
            // 
            // comboBoxHAlign
            // 
            this.comboBoxHAlign.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxHAlign.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxHAlign.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxHAlign.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxHAlign.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxHAlign.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxHAlign.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxHAlign.DropDownHeight = 400;
            this.comboBoxHAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHAlign.DropDownWidth = 200;
            this.comboBoxHAlign.FormattingEnabled = true;
            this.comboBoxHAlign.Items.AddRange(new object[] {
            "Left",
            "Center",
            "Right",
            "Center - Left justify"});
            this.comboBoxHAlign.Location = new System.Drawing.Point(112, 105);
            this.comboBoxHAlign.MaxLength = 32767;
            this.comboBoxHAlign.Name = "comboBoxHAlign";
            this.comboBoxHAlign.SelectedIndex = -1;
            this.comboBoxHAlign.SelectedItem = null;
            this.comboBoxHAlign.SelectedText = "";
            this.comboBoxHAlign.Size = new System.Drawing.Size(123, 21);
            this.comboBoxHAlign.TabIndex = 8;
            this.comboBoxHAlign.UsePopupWindow = false;
            this.comboBoxHAlign.SelectedIndexChanged += new System.EventHandler(this.comboBoxHAlign_SelectedIndexChanged);
            // 
            // labelHorizontalAlign
            // 
            this.labelHorizontalAlign.AutoSize = true;
            this.labelHorizontalAlign.Location = new System.Drawing.Point(5, 108);
            this.labelHorizontalAlign.Name = "labelHorizontalAlign";
            this.labelHorizontalAlign.Size = new System.Drawing.Size(30, 13);
            this.labelHorizontalAlign.TabIndex = 7;
            this.labelHorizontalAlign.Text = "Align";
            // 
            // checkBoxSimpleRender
            // 
            this.checkBoxSimpleRender.AutoSize = true;
            this.checkBoxSimpleRender.Location = new System.Drawing.Point(287, 71);
            this.checkBoxSimpleRender.Name = "checkBoxSimpleRender";
            this.checkBoxSimpleRender.Size = new System.Drawing.Size(104, 17);
            this.checkBoxSimpleRender.TabIndex = 20;
            this.checkBoxSimpleRender.Text = "Simple rendering";
            this.checkBoxSimpleRender.UseVisualStyleBackColor = true;
            this.checkBoxSimpleRender.CheckedChanged += new System.EventHandler(this.checkBoxAntiAlias_CheckedChanged);
            // 
            // labelSubtitleFontSize
            // 
            this.labelSubtitleFontSize.AutoSize = true;
            this.labelSubtitleFontSize.Location = new System.Drawing.Point(5, 54);
            this.labelSubtitleFontSize.Name = "labelSubtitleFontSize";
            this.labelSubtitleFontSize.Size = new System.Drawing.Size(84, 13);
            this.labelSubtitleFontSize.TabIndex = 2;
            this.labelSubtitleFontSize.Text = "Subtitle font size";
            // 
            // comboBoxSubtitleFont
            // 
            this.comboBoxSubtitleFont.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSubtitleFont.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSubtitleFont.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSubtitleFont.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSubtitleFont.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSubtitleFont.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSubtitleFont.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSubtitleFont.DropDownHeight = 400;
            this.comboBoxSubtitleFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFont.DropDownWidth = 210;
            this.comboBoxSubtitleFont.FormattingEnabled = true;
            this.comboBoxSubtitleFont.Location = new System.Drawing.Point(112, 24);
            this.comboBoxSubtitleFont.MaxLength = 32767;
            this.comboBoxSubtitleFont.Name = "comboBoxSubtitleFont";
            this.comboBoxSubtitleFont.SelectedIndex = -1;
            this.comboBoxSubtitleFont.SelectedItem = null;
            this.comboBoxSubtitleFont.SelectedText = "";
            this.comboBoxSubtitleFont.Size = new System.Drawing.Size(123, 21);
            this.comboBoxSubtitleFont.TabIndex = 1;
            this.comboBoxSubtitleFont.UsePopupWindow = false;
            this.comboBoxSubtitleFont.SelectedIndexChanged += new System.EventHandler(this.comboBoxSubtitleFont_SelectedIndexChanged);
            this.comboBoxSubtitleFont.SelectedValueChanged += new System.EventHandler(this.comboBoxSubtitleFont_SelectedValueChanged);
            // 
            // comboBoxSubtitleFontSize
            // 
            this.comboBoxSubtitleFontSize.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSubtitleFontSize.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSubtitleFontSize.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSubtitleFontSize.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSubtitleFontSize.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSubtitleFontSize.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSubtitleFontSize.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSubtitleFontSize.DropDownHeight = 400;
            this.comboBoxSubtitleFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFontSize.DropDownWidth = 123;
            this.comboBoxSubtitleFontSize.FormattingEnabled = true;
            this.comboBoxSubtitleFontSize.Items.AddRange(new object[] {
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32",
            "33",
            "34",
            "35",
            "36",
            "37",
            "38",
            "39",
            "40",
            "41",
            "42",
            "43",
            "44",
            "45",
            "46",
            "47",
            "48",
            "49",
            "50",
            "51",
            "52",
            "53",
            "54",
            "55",
            "56",
            "57",
            "58",
            "59",
            "60",
            "61",
            "62",
            "63",
            "64",
            "65",
            "66",
            "67",
            "68",
            "69",
            "70",
            "71",
            "72",
            "73",
            "74",
            "75",
            "76",
            "77",
            "78",
            "79",
            "80",
            "81",
            "82",
            "83",
            "84",
            "85",
            "86",
            "87",
            "88",
            "89",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "100",
            "110",
            "120",
            "130",
            "140",
            "150",
            "160",
            "170",
            "180",
            "190",
            "200",
            "225",
            "250",
            "275",
            "300",
            "325",
            "350",
            "375",
            "400",
            "425",
            "450",
            "475",
            "500"});
            this.comboBoxSubtitleFontSize.Location = new System.Drawing.Point(112, 51);
            this.comboBoxSubtitleFontSize.MaxLength = 32767;
            this.comboBoxSubtitleFontSize.Name = "comboBoxSubtitleFontSize";
            this.comboBoxSubtitleFontSize.SelectedIndex = -1;
            this.comboBoxSubtitleFontSize.SelectedItem = null;
            this.comboBoxSubtitleFontSize.SelectedText = "";
            this.comboBoxSubtitleFontSize.Size = new System.Drawing.Size(123, 21);
            this.comboBoxSubtitleFontSize.TabIndex = 3;
            this.comboBoxSubtitleFontSize.UsePopupWindow = false;
            this.comboBoxSubtitleFontSize.SelectedIndexChanged += new System.EventHandler(this.comboBoxSubtitleFontSize_SelectedIndexChanged);
            // 
            // labelSubtitleFont
            // 
            this.labelSubtitleFont.AutoSize = true;
            this.labelSubtitleFont.Location = new System.Drawing.Point(5, 29);
            this.labelSubtitleFont.Name = "labelSubtitleFont";
            this.labelSubtitleFont.Size = new System.Drawing.Size(63, 13);
            this.labelSubtitleFont.TabIndex = 0;
            this.labelSubtitleFont.Text = "Subtitle font";
            // 
            // labelBorderWidth
            // 
            this.labelBorderWidth.Location = new System.Drawing.Point(427, 54);
            this.labelBorderWidth.Name = "labelBorderWidth";
            this.labelBorderWidth.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelBorderWidth.Size = new System.Drawing.Size(110, 13);
            this.labelBorderWidth.TabIndex = 16;
            this.labelBorderWidth.Text = "Border width";
            this.labelBorderWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxBorderWidth
            // 
            this.comboBoxBorderWidth.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxBorderWidth.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxBorderWidth.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxBorderWidth.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxBorderWidth.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxBorderWidth.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxBorderWidth.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxBorderWidth.DropDownHeight = 400;
            this.comboBoxBorderWidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBorderWidth.DropDownWidth = 128;
            this.comboBoxBorderWidth.FormattingEnabled = true;
            this.comboBoxBorderWidth.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15"});
            this.comboBoxBorderWidth.Location = new System.Drawing.Point(543, 51);
            this.comboBoxBorderWidth.MaxLength = 32767;
            this.comboBoxBorderWidth.Name = "comboBoxBorderWidth";
            this.comboBoxBorderWidth.SelectedIndex = -1;
            this.comboBoxBorderWidth.SelectedItem = null;
            this.comboBoxBorderWidth.SelectedText = "";
            this.comboBoxBorderWidth.Size = new System.Drawing.Size(128, 21);
            this.comboBoxBorderWidth.TabIndex = 32;
            this.comboBoxBorderWidth.UsePopupWindow = false;
            this.comboBoxBorderWidth.SelectedIndexChanged += new System.EventHandler(this.comboBoxBorderWidth_SelectedIndexChanged);
            // 
            // panelBorderColor
            // 
            this.panelBorderColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBorderColor.Location = new System.Drawing.Point(677, 25);
            this.panelBorderColor.Name = "panelBorderColor";
            this.panelBorderColor.Size = new System.Drawing.Size(21, 20);
            this.panelBorderColor.TabIndex = 31;
            this.panelBorderColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelBorderColor_MouseClick);
            // 
            // buttonBorderColor
            // 
            this.buttonBorderColor.Location = new System.Drawing.Point(543, 24);
            this.buttonBorderColor.Name = "buttonBorderColor";
            this.buttonBorderColor.Size = new System.Drawing.Size(128, 23);
            this.buttonBorderColor.TabIndex = 30;
            this.buttonBorderColor.Text = "Border color";
            this.buttonBorderColor.UseVisualStyleBackColor = true;
            this.buttonBorderColor.Click += new System.EventHandler(this.buttonBorderColor_Click);
            // 
            // panelColor
            // 
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(414, 25);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(21, 20);
            this.panelColor.TabIndex = 12;
            this.panelColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelColor_MouseClick);
            // 
            // buttonColor
            // 
            this.buttonColor.Location = new System.Drawing.Point(287, 24);
            this.buttonColor.Name = "buttonColor";
            this.buttonColor.Size = new System.Drawing.Size(121, 23);
            this.buttonColor.TabIndex = 18;
            this.buttonColor.Text = "Color";
            this.buttonColor.UseVisualStyleBackColor = true;
            this.buttonColor.Click += new System.EventHandler(this.buttonColor_Click);
            // 
            // checkBoxSkipEmptyFrameAtStart
            // 
            this.checkBoxSkipEmptyFrameAtStart.AutoSize = true;
            this.checkBoxSkipEmptyFrameAtStart.Checked = true;
            this.checkBoxSkipEmptyFrameAtStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSkipEmptyFrameAtStart.Location = new System.Drawing.Point(287, 92);
            this.checkBoxSkipEmptyFrameAtStart.Name = "checkBoxSkipEmptyFrameAtStart";
            this.checkBoxSkipEmptyFrameAtStart.Size = new System.Drawing.Size(147, 17);
            this.checkBoxSkipEmptyFrameAtStart.TabIndex = 15;
            this.checkBoxSkipEmptyFrameAtStart.Text = "Skip empty frames at start";
            this.checkBoxSkipEmptyFrameAtStart.UseVisualStyleBackColor = true;
            // 
            // checkBoxFcpFullPathUrl
            // 
            this.checkBoxFcpFullPathUrl.AutoSize = true;
            this.checkBoxFcpFullPathUrl.Location = new System.Drawing.Point(5, 161);
            this.checkBoxFcpFullPathUrl.Name = "checkBoxFcpFullPathUrl";
            this.checkBoxFcpFullPathUrl.Size = new System.Drawing.Size(182, 17);
            this.checkBoxFcpFullPathUrl.TabIndex = 62;
            this.checkBoxFcpFullPathUrl.Text = "Use full image path url in FCP xml";
            this.checkBoxFcpFullPathUrl.UseVisualStyleBackColor = true;
            // 
            // labelLeftRightMargin
            // 
            this.labelLeftRightMargin.AutoSize = true;
            this.labelLeftRightMargin.Location = new System.Drawing.Point(5, 161);
            this.labelLeftRightMargin.Name = "labelLeftRightMargin";
            this.labelLeftRightMargin.Size = new System.Drawing.Size(84, 13);
            this.labelLeftRightMargin.TabIndex = 56;
            this.labelLeftRightMargin.Text = "Left/right margin";
            // 
            // groupBoxExportImage
            // 
            this.groupBoxExportImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxExportImage.Controls.Add(this.linkLabelPreview);
            this.groupBoxExportImage.Controls.Add(this.panelVlcTemp);
            this.groupBoxExportImage.Controls.Add(this.pictureBox1);
            this.groupBoxExportImage.Location = new System.Drawing.Point(13, 415);
            this.groupBoxExportImage.Name = "groupBoxExportImage";
            this.groupBoxExportImage.Size = new System.Drawing.Size(962, 178);
            this.groupBoxExportImage.TabIndex = 1;
            this.groupBoxExportImage.TabStop = false;
            // 
            // linkLabelPreview
            // 
            this.linkLabelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelPreview.AutoSize = true;
            this.linkLabelPreview.Location = new System.Drawing.Point(906, 16);
            this.linkLabelPreview.Name = "linkLabelPreview";
            this.linkLabelPreview.Size = new System.Drawing.Size(45, 13);
            this.linkLabelPreview.TabIndex = 9;
            this.linkLabelPreview.TabStop = true;
            this.linkLabelPreview.Text = "Preview";
            this.linkLabelPreview.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabelPreview.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelPreview_LinkClicked);
            // 
            // panelVlcTemp
            // 
            this.panelVlcTemp.Location = new System.Drawing.Point(642, 52);
            this.panelVlcTemp.Name = "panelVlcTemp";
            this.panelVlcTemp.Size = new System.Drawing.Size(200, 100);
            this.panelVlcTemp.TabIndex = 10;
            this.panelVlcTemp.Visible = false;
            // 
            // timerPreview
            // 
            this.timerPreview.Interval = 500;
            this.timerPreview.Tick += new System.EventHandler(this.timerPreview_Tick);
            // 
            // contextMenuStripListView
            // 
            this.contextMenuStripListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.normalToolStripMenuItem,
            this.italicToolStripMenuItem,
            this.boxSingleLineToolStripMenuItem,
            this.boxMultiLineToolStripMenuItem,
            this.toolStripSeparatorAdjust,
            this.adjustDisplayTimeToolStripMenuItem,
            this.adjustTimeCodesToolStripMenuItem});
            this.contextMenuStripListView.Name = "contextMenuStripListView";
            this.contextMenuStripListView.Size = new System.Drawing.Size(185, 142);
            this.contextMenuStripListView.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripListView_Opening);
            // 
            // normalToolStripMenuItem
            // 
            this.normalToolStripMenuItem.Name = "normalToolStripMenuItem";
            this.normalToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.normalToolStripMenuItem.Text = "Normal";
            this.normalToolStripMenuItem.Click += new System.EventHandler(this.normalToolStripMenuItem_Click);
            // 
            // italicToolStripMenuItem
            // 
            this.italicToolStripMenuItem.Name = "italicToolStripMenuItem";
            this.italicToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.italicToolStripMenuItem.Text = "Italic";
            this.italicToolStripMenuItem.Click += new System.EventHandler(this.italicToolStripMenuItem_Click);
            // 
            // boxSingleLineToolStripMenuItem
            // 
            this.boxSingleLineToolStripMenuItem.Name = "boxSingleLineToolStripMenuItem";
            this.boxSingleLineToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.boxSingleLineToolStripMenuItem.Text = "Box - single line";
            this.boxSingleLineToolStripMenuItem.Click += new System.EventHandler(this.boxSingleLineToolStripMenuItem_Click);
            // 
            // boxMultiLineToolStripMenuItem
            // 
            this.boxMultiLineToolStripMenuItem.Name = "boxMultiLineToolStripMenuItem";
            this.boxMultiLineToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.boxMultiLineToolStripMenuItem.Text = "Box - multi line";
            this.boxMultiLineToolStripMenuItem.Click += new System.EventHandler(this.boxMultiLineToolStripMenuItem_Click);
            // 
            // toolStripSeparatorAdjust
            // 
            this.toolStripSeparatorAdjust.Name = "toolStripSeparatorAdjust";
            this.toolStripSeparatorAdjust.Size = new System.Drawing.Size(181, 6);
            // 
            // adjustDisplayTimeToolStripMenuItem
            // 
            this.adjustDisplayTimeToolStripMenuItem.Name = "adjustDisplayTimeToolStripMenuItem";
            this.adjustDisplayTimeToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.adjustDisplayTimeToolStripMenuItem.Text = "Adjust display time...";
            this.adjustDisplayTimeToolStripMenuItem.Click += new System.EventHandler(this.adjustDisplayTimeToolStripMenuItem_Click);
            // 
            // adjustTimeCodesToolStripMenuItem
            // 
            this.adjustTimeCodesToolStripMenuItem.Name = "adjustTimeCodesToolStripMenuItem";
            this.adjustTimeCodesToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.adjustTimeCodesToolStripMenuItem.Text = "Adjust time codes...";
            this.adjustTimeCodesToolStripMenuItem.Click += new System.EventHandler(this.adjustTimeCodesToolStripMenuItem_Click);
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.subtitleListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.subtitleListView1.ContextMenuStrip = this.contextMenuStripListView;
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.HideSelection = false;
            this.subtitleListView1.Location = new System.Drawing.Point(13, 13);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.Size = new System.Drawing.Size(962, 199);
            this.subtitleListView1.TabIndex = 7;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            this.subtitleListView1.SelectedIndexChanged += new System.EventHandler(this.subtitleListView1_SelectedIndexChanged);
            this.subtitleListView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.subtitleListView1_KeyDown);
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
            // ExportPngXml
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(981, 632);
            this.ContextMenuStrip = this.contextMenuStripProfile;
            this.Controls.Add(this.subtitleListView1);
            this.Controls.Add(this.groupBoxExportImage);
            this.Controls.Add(this.groupBoxImageSettings);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(890, 667);
            this.Name = "ExportPngXml";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ExportPngXml";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExportPngXml_FormClosing);
            this.Shown += new System.EventHandler(this.ExportPngXml_Shown);
            this.ResizeEnd += new System.EventHandler(this.ExportPngXml_ResizeEnd);
            this.SizeChanged += new System.EventHandler(this.ExportPngXml_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExportPngXml_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBoxImageSettings.ResumeLayout(false);
            this.groupBoxImageSettings.PerformLayout();
            this.groupBoxExportImage.ResumeLayout(false);
            this.groupBoxExportImage.PerformLayout();
            this.contextMenuStripListView.ResumeLayout(false);
            this.contextMenuStripProfile.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.GroupBox groupBoxImageSettings;
        private System.Windows.Forms.Label labelBorderWidth;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxBorderWidth;
        private System.Windows.Forms.Panel panelBorderColor;
        private System.Windows.Forms.Button buttonBorderColor;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Button buttonColor;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelSubtitleFontSize;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSubtitleFont;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSubtitleFontSize;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelSubtitleFont;
        private System.Windows.Forms.CheckBox checkBoxSimpleRender;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxHAlign;
        private System.Windows.Forms.Label labelHorizontalAlign;
        private System.Windows.Forms.Label labelResolution;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxResolution;
        private System.Windows.Forms.CheckBox checkBoxBold;
        private System.Windows.Forms.Label labelImageFormat;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxImageFormat;
        private System.Windows.Forms.Label labelLanguage;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelFrameRate;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFrameRate;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxBottomMargin;
        private System.Windows.Forms.Label labelBottomMargin;
        private System.Windows.Forms.CheckBox checkBoxSkipEmptyFrameAtStart;
        private System.Windows.Forms.Button buttonCustomResolution;
        private System.Windows.Forms.GroupBox groupBoxExportImage;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownDepth3D;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelDepth;
        private System.Windows.Forms.Label label3D;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBox3D;
        private System.Windows.Forms.Timer timerPreview;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem saveImageAsToolStripMenuItem;
        private System.Windows.Forms.Label labelShadowWidth;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxShadowWidth;
        private System.Windows.Forms.Panel panelShadowColor;
        private System.Windows.Forms.Button buttonShadowColor;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownShadowTransparency;
        private System.Windows.Forms.Label labelShadowTransparency;
        private System.Windows.Forms.Label labelLineHeight;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownLineSpacing;
        private System.Windows.Forms.CheckBox checkBoxTransAntiAliase;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListView;
        private System.Windows.Forms.ToolStripMenuItem boxSingleLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boxMultiLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem normalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem italicToolStripMenuItem;
        private System.Windows.Forms.ListView subtitleListView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.LinkLabel linkLabelPreview;
        private System.Windows.Forms.Panel panelVlcTemp;
        private System.Windows.Forms.CheckBox checkBoxFullFrameImage;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLeftRightMargin;
        private System.Windows.Forms.Label labelLeftRightMargin;
        private System.Windows.Forms.Panel panelFullFrameBackground;
        private System.Windows.Forms.Label labelLineHeightStyle;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxBottomMarginUnit;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLeftRightMarginUnit;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxResizePercentage;
        private System.Windows.Forms.Label labelResize;
        private System.Windows.Forms.ToolStripMenuItem adjustTimeCodesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorAdjust;
        private System.Windows.Forms.ToolStripMenuItem adjustDisplayTimeToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxFcpFullPathUrl;
        private System.Windows.Forms.Label labelImagePrefix;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxImagePrefix;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripProfile;
        private System.Windows.Forms.ToolStripMenuItem profilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
    }
}