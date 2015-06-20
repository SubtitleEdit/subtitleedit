namespace Nikse.SubtitleEdit.Forms
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
            this.panelFullFrameBackground = new System.Windows.Forms.Panel();
            this.comboBoxLeftRightMargin = new System.Windows.Forms.ComboBox();
            this.labelLeftRightMargin = new System.Windows.Forms.Label();
            this.checkBoxFullFrameImage = new System.Windows.Forms.CheckBox();
            this.checkBoxTransAntiAliase = new System.Windows.Forms.CheckBox();
            this.labelLineHeight = new System.Windows.Forms.Label();
            this.numericUpDownLineSpacing = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownShadowTransparency = new System.Windows.Forms.NumericUpDown();
            this.labelShadowTransparency = new System.Windows.Forms.Label();
            this.labelShadowWidth = new System.Windows.Forms.Label();
            this.comboBoxShadowWidth = new System.Windows.Forms.ComboBox();
            this.panelShadowColor = new System.Windows.Forms.Panel();
            this.buttonShadowColor = new System.Windows.Forms.Button();
            this.labelDepth = new System.Windows.Forms.Label();
            this.label3D = new System.Windows.Forms.Label();
            this.comboBox3D = new System.Windows.Forms.ComboBox();
            this.numericUpDownDepth3D = new System.Windows.Forms.NumericUpDown();
            this.buttonCustomResolution = new System.Windows.Forms.Button();
            this.comboBoxBottomMargin = new System.Windows.Forms.ComboBox();
            this.labelBottomMargin = new System.Windows.Forms.Label();
            this.labelFrameRate = new System.Windows.Forms.Label();
            this.comboBoxFrameRate = new System.Windows.Forms.ComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.labelImageFormat = new System.Windows.Forms.Label();
            this.comboBoxImageFormat = new System.Windows.Forms.ComboBox();
            this.checkBoxBold = new System.Windows.Forms.CheckBox();
            this.labelResolution = new System.Windows.Forms.Label();
            this.comboBoxResolution = new System.Windows.Forms.ComboBox();
            this.comboBoxHAlign = new System.Windows.Forms.ComboBox();
            this.labelHorizontalAlign = new System.Windows.Forms.Label();
            this.checkBoxSimpleRender = new System.Windows.Forms.CheckBox();
            this.labelSubtitleFontSize = new System.Windows.Forms.Label();
            this.comboBoxSubtitleFont = new System.Windows.Forms.ComboBox();
            this.comboBoxSubtitleFontSize = new System.Windows.Forms.ComboBox();
            this.labelSubtitleFont = new System.Windows.Forms.Label();
            this.labelBorderWidth = new System.Windows.Forms.Label();
            this.comboBoxBorderWidth = new System.Windows.Forms.ComboBox();
            this.panelBorderColor = new System.Windows.Forms.Panel();
            this.buttonBorderColor = new System.Windows.Forms.Button();
            this.panelColor = new System.Windows.Forms.Panel();
            this.buttonColor = new System.Windows.Forms.Button();
            this.checkBoxSkipEmptyFrameAtStart = new System.Windows.Forms.CheckBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBoxExportImage = new System.Windows.Forms.GroupBox();
            this.panelVlcTemp = new System.Windows.Forms.Panel();
            this.linkLabelPreview = new System.Windows.Forms.LinkLabel();
            this.timerPreview = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStripListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.normalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.italicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boxSingleLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boxMultiLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.subtitleListView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBoxImageSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLineSpacing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowTransparency)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDepth3D)).BeginInit();
            this.groupBoxExportImage.SuspendLayout();
            this.contextMenuStripListView.SuspendLayout();
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
            this.buttonExport.Location = new System.Drawing.Point(742, 602);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(126, 21);
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
            this.buttonCancel.Location = new System.Drawing.Point(874, 602);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 602);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(724, 21);
            this.progressBar1.TabIndex = 5;
            this.progressBar1.Visible = false;
            // 
            // groupBoxImageSettings
            // 
            this.groupBoxImageSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImageSettings.Controls.Add(this.panelFullFrameBackground);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxLeftRightMargin);
            this.groupBoxImageSettings.Controls.Add(this.labelLeftRightMargin);
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
            this.groupBoxImageSettings.Controls.Add(this.comboBoxResolution);
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
            this.groupBoxImageSettings.Location = new System.Drawing.Point(12, 218);
            this.groupBoxImageSettings.Name = "groupBoxImageSettings";
            this.groupBoxImageSettings.Size = new System.Drawing.Size(937, 191);
            this.groupBoxImageSettings.TabIndex = 3;
            this.groupBoxImageSettings.TabStop = false;
            this.groupBoxImageSettings.Text = "Image settings";
            // 
            // panelFullFrameBackground
            // 
            this.panelFullFrameBackground.BackColor = System.Drawing.Color.Transparent;
            this.panelFullFrameBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFullFrameBackground.Location = new System.Drawing.Point(659, 104);
            this.panelFullFrameBackground.Name = "panelFullFrameBackground";
            this.panelFullFrameBackground.Size = new System.Drawing.Size(21, 20);
            this.panelFullFrameBackground.TabIndex = 58;
            this.panelFullFrameBackground.Visible = false;
            this.panelFullFrameBackground.Click += new System.EventHandler(this.panelFullFrameBackground_Click);
            // 
            // comboBoxLeftRightMargin
            // 
            this.comboBoxLeftRightMargin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLeftRightMargin.FormattingEnabled = true;
            this.comboBoxLeftRightMargin.Items.AddRange(new object[] {
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
            "50"});
            this.comboBoxLeftRightMargin.Location = new System.Drawing.Point(100, 159);
            this.comboBoxLeftRightMargin.Name = "comboBoxLeftRightMargin";
            this.comboBoxLeftRightMargin.Size = new System.Drawing.Size(121, 21);
            this.comboBoxLeftRightMargin.TabIndex = 57;
            this.comboBoxLeftRightMargin.SelectedIndexChanged += new System.EventHandler(this.comboBoxLeftRightMargin_SelectedIndexChanged);
            // 
            // labelLeftRightMargin
            // 
            this.labelLeftRightMargin.AutoSize = true;
            this.labelLeftRightMargin.Location = new System.Drawing.Point(10, 162);
            this.labelLeftRightMargin.Name = "labelLeftRightMargin";
            this.labelLeftRightMargin.Size = new System.Drawing.Size(84, 13);
            this.labelLeftRightMargin.TabIndex = 56;
            this.labelLeftRightMargin.Text = "Left/right margin";
            // 
            // checkBoxFullFrameImage
            // 
            this.checkBoxFullFrameImage.AutoSize = true;
            this.checkBoxFullFrameImage.Location = new System.Drawing.Point(532, 104);
            this.checkBoxFullFrameImage.Name = "checkBoxFullFrameImage";
            this.checkBoxFullFrameImage.Size = new System.Drawing.Size(102, 17);
            this.checkBoxFullFrameImage.TabIndex = 23;
            this.checkBoxFullFrameImage.Text = "Full frame image";
            this.checkBoxFullFrameImage.UseVisualStyleBackColor = true;
            this.checkBoxFullFrameImage.CheckedChanged += new System.EventHandler(this.checkBoxFullFrameImage_CheckedChanged);
            // 
            // checkBoxTransAntiAliase
            // 
            this.checkBoxTransAntiAliase.AutoSize = true;
            this.checkBoxTransAntiAliase.Location = new System.Drawing.Point(276, 92);
            this.checkBoxTransAntiAliase.Name = "checkBoxTransAntiAliase";
            this.checkBoxTransAntiAliase.Size = new System.Drawing.Size(162, 17);
            this.checkBoxTransAntiAliase.TabIndex = 55;
            this.checkBoxTransAntiAliase.Text = "Anti-alising with transparency";
            this.checkBoxTransAntiAliase.UseVisualStyleBackColor = true;
            this.checkBoxTransAntiAliase.CheckedChanged += new System.EventHandler(this.checkBoxAntiAlias_CheckedChanged);
            // 
            // labelLineHeight
            // 
            this.labelLineHeight.Location = new System.Drawing.Point(662, 132);
            this.labelLineHeight.Name = "labelLineHeight";
            this.labelLineHeight.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelLineHeight.Size = new System.Drawing.Size(110, 13);
            this.labelLineHeight.TabIndex = 54;
            this.labelLineHeight.Text = "Line height";
            this.labelLineHeight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericUpDownLineSpacing
            // 
            this.numericUpDownLineSpacing.Location = new System.Drawing.Point(778, 130);
            this.numericUpDownLineSpacing.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.numericUpDownLineSpacing.Name = "numericUpDownLineSpacing";
            this.numericUpDownLineSpacing.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownLineSpacing.TabIndex = 53;
            this.numericUpDownLineSpacing.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownLineSpacing.ValueChanged += new System.EventHandler(this.numericUpDownLineSpacing_ValueChanged);
            // 
            // numericUpDownShadowTransparency
            // 
            this.numericUpDownShadowTransparency.Location = new System.Drawing.Point(778, 81);
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
            this.numericUpDownShadowTransparency.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownShadowTransparency.TabIndex = 40;
            this.numericUpDownShadowTransparency.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownShadowTransparency.ValueChanged += new System.EventHandler(this.numericUpDownShadowTransparency_ValueChanged);
            // 
            // labelShadowTransparency
            // 
            this.labelShadowTransparency.Location = new System.Drawing.Point(662, 83);
            this.labelShadowTransparency.Name = "labelShadowTransparency";
            this.labelShadowTransparency.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelShadowTransparency.Size = new System.Drawing.Size(110, 13);
            this.labelShadowTransparency.TabIndex = 39;
            this.labelShadowTransparency.Text = "Transparency";
            this.labelShadowTransparency.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelShadowWidth
            // 
            this.labelShadowWidth.Location = new System.Drawing.Point(662, 55);
            this.labelShadowWidth.Name = "labelShadowWidth";
            this.labelShadowWidth.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelShadowWidth.Size = new System.Drawing.Size(110, 13);
            this.labelShadowWidth.TabIndex = 35;
            this.labelShadowWidth.Text = "Shadow width";
            this.labelShadowWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxShadowWidth
            // 
            this.comboBoxShadowWidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
            this.comboBoxShadowWidth.Location = new System.Drawing.Point(778, 52);
            this.comboBoxShadowWidth.Name = "comboBoxShadowWidth";
            this.comboBoxShadowWidth.Size = new System.Drawing.Size(121, 21);
            this.comboBoxShadowWidth.TabIndex = 38;
            this.comboBoxShadowWidth.SelectedIndexChanged += new System.EventHandler(this.comboBoxShadowWidth_SelectedIndexChanged);
            // 
            // panelShadowColor
            // 
            this.panelShadowColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelShadowColor.Location = new System.Drawing.Point(905, 26);
            this.panelShadowColor.Name = "panelShadowColor";
            this.panelShadowColor.Size = new System.Drawing.Size(21, 20);
            this.panelShadowColor.TabIndex = 37;
            this.panelShadowColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelShadowColor_MouseClick);
            // 
            // buttonShadowColor
            // 
            this.buttonShadowColor.Location = new System.Drawing.Point(778, 25);
            this.buttonShadowColor.Name = "buttonShadowColor";
            this.buttonShadowColor.Size = new System.Drawing.Size(121, 21);
            this.buttonShadowColor.TabIndex = 36;
            this.buttonShadowColor.Text = "Shadow color";
            this.buttonShadowColor.UseVisualStyleBackColor = true;
            this.buttonShadowColor.Click += new System.EventHandler(this.buttonShadowColor_Click);
            // 
            // labelDepth
            // 
            this.labelDepth.Location = new System.Drawing.Point(236, 134);
            this.labelDepth.Name = "labelDepth";
            this.labelDepth.Size = new System.Drawing.Size(100, 18);
            this.labelDepth.TabIndex = 18;
            this.labelDepth.Text = "Depth";
            this.labelDepth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3D
            // 
            this.label3D.AutoSize = true;
            this.label3D.Location = new System.Drawing.Point(273, 113);
            this.label3D.Name = "label3D";
            this.label3D.Size = new System.Drawing.Size(21, 13);
            this.label3D.TabIndex = 16;
            this.label3D.Text = "3D";
            // 
            // comboBox3D
            // 
            this.comboBox3D.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3D.FormattingEnabled = true;
            this.comboBox3D.Location = new System.Drawing.Point(298, 110);
            this.comboBox3D.Name = "comboBox3D";
            this.comboBox3D.Size = new System.Drawing.Size(121, 21);
            this.comboBox3D.TabIndex = 17;
            this.comboBox3D.SelectedIndexChanged += new System.EventHandler(this.comboBox3D_SelectedIndexChanged);
            // 
            // numericUpDownDepth3D
            // 
            this.numericUpDownDepth3D.Location = new System.Drawing.Point(342, 135);
            this.numericUpDownDepth3D.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericUpDownDepth3D.Name = "numericUpDownDepth3D";
            this.numericUpDownDepth3D.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownDepth3D.TabIndex = 19;
            this.numericUpDownDepth3D.ValueChanged += new System.EventHandler(this.numericUpDownDepth3D_ValueChanged);
            // 
            // buttonCustomResolution
            // 
            this.buttonCustomResolution.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCustomResolution.Location = new System.Drawing.Point(224, 78);
            this.buttonCustomResolution.Name = "buttonCustomResolution";
            this.buttonCustomResolution.Size = new System.Drawing.Size(24, 21);
            this.buttonCustomResolution.TabIndex = 6;
            this.buttonCustomResolution.Text = "...";
            this.buttonCustomResolution.UseVisualStyleBackColor = true;
            this.buttonCustomResolution.Click += new System.EventHandler(this.buttonCustomResolution_Click);
            // 
            // comboBoxBottomMargin
            // 
            this.comboBoxBottomMargin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBottomMargin.FormattingEnabled = true;
            this.comboBoxBottomMargin.Location = new System.Drawing.Point(100, 132);
            this.comboBoxBottomMargin.Name = "comboBoxBottomMargin";
            this.comboBoxBottomMargin.Size = new System.Drawing.Size(121, 21);
            this.comboBoxBottomMargin.TabIndex = 10;
            this.comboBoxBottomMargin.SelectedIndexChanged += new System.EventHandler(this.comboBoxBottomMargin_SelectedIndexChanged);
            // 
            // labelBottomMargin
            // 
            this.labelBottomMargin.AutoSize = true;
            this.labelBottomMargin.Location = new System.Drawing.Point(10, 135);
            this.labelBottomMargin.Name = "labelBottomMargin";
            this.labelBottomMargin.Size = new System.Drawing.Size(74, 13);
            this.labelBottomMargin.TabIndex = 9;
            this.labelBottomMargin.Text = "Bottom margin";
            // 
            // labelFrameRate
            // 
            this.labelFrameRate.Location = new System.Drawing.Point(416, 137);
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
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Location = new System.Drawing.Point(532, 134);
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFrameRate.TabIndex = 25;
            // 
            // labelLanguage
            // 
            this.labelLanguage.Location = new System.Drawing.Point(416, 108);
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
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Items.AddRange(new object[] {
            "Bmp",
            "Exif",
            "Gif",
            "Jpg",
            "Png",
            "Tiff"});
            this.comboBoxLanguage.Location = new System.Drawing.Point(532, 105);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(121, 21);
            this.comboBoxLanguage.TabIndex = 24;
            this.comboBoxLanguage.Visible = false;
            // 
            // labelImageFormat
            // 
            this.labelImageFormat.Location = new System.Drawing.Point(416, 81);
            this.labelImageFormat.Name = "labelImageFormat";
            this.labelImageFormat.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelImageFormat.Size = new System.Drawing.Size(110, 13);
            this.labelImageFormat.TabIndex = 28;
            this.labelImageFormat.Text = "Image format";
            this.labelImageFormat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxImageFormat
            // 
            this.comboBoxImageFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxImageFormat.FormattingEnabled = true;
            this.comboBoxImageFormat.Items.AddRange(new object[] {
            "Bmp",
            "Exif",
            "Gif",
            "Jpg",
            "Png",
            "Tiff",
            "Tga"});
            this.comboBoxImageFormat.Location = new System.Drawing.Point(532, 78);
            this.comboBoxImageFormat.Name = "comboBoxImageFormat";
            this.comboBoxImageFormat.Size = new System.Drawing.Size(121, 21);
            this.comboBoxImageFormat.TabIndex = 23;
            // 
            // checkBoxBold
            // 
            this.checkBoxBold.AutoSize = true;
            this.checkBoxBold.Location = new System.Drawing.Point(276, 50);
            this.checkBoxBold.Name = "checkBoxBold";
            this.checkBoxBold.Size = new System.Drawing.Size(47, 17);
            this.checkBoxBold.TabIndex = 13;
            this.checkBoxBold.Text = "Bold";
            this.checkBoxBold.UseVisualStyleBackColor = true;
            this.checkBoxBold.CheckedChanged += new System.EventHandler(this.checkBoxBold_CheckedChanged);
            // 
            // labelResolution
            // 
            this.labelResolution.AutoSize = true;
            this.labelResolution.Location = new System.Drawing.Point(10, 81);
            this.labelResolution.Name = "labelResolution";
            this.labelResolution.Size = new System.Drawing.Size(51, 13);
            this.labelResolution.TabIndex = 4;
            this.labelResolution.Text = "Video res";
            // 
            // comboBoxResolution
            // 
            this.comboBoxResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxResolution.FormattingEnabled = true;
            this.comboBoxResolution.Items.AddRange(new object[] {
            "2K (2048x1080)",
            "DCI 2K Scope (2048x 858)",
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
            this.comboBoxResolution.Location = new System.Drawing.Point(100, 78);
            this.comboBoxResolution.Name = "comboBoxResolution";
            this.comboBoxResolution.Size = new System.Drawing.Size(121, 21);
            this.comboBoxResolution.TabIndex = 5;
            this.comboBoxResolution.SelectedIndexChanged += new System.EventHandler(this.comboBoxResolution_SelectedIndexChanged);
            // 
            // comboBoxHAlign
            // 
            this.comboBoxHAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHAlign.FormattingEnabled = true;
            this.comboBoxHAlign.Items.AddRange(new object[] {
            "Left",
            "Center",
            "Right"});
            this.comboBoxHAlign.Location = new System.Drawing.Point(100, 105);
            this.comboBoxHAlign.Name = "comboBoxHAlign";
            this.comboBoxHAlign.Size = new System.Drawing.Size(121, 21);
            this.comboBoxHAlign.TabIndex = 8;
            this.comboBoxHAlign.SelectedIndexChanged += new System.EventHandler(this.comboBoxHAlign_SelectedIndexChanged);
            // 
            // labelHorizontalAlign
            // 
            this.labelHorizontalAlign.AutoSize = true;
            this.labelHorizontalAlign.Location = new System.Drawing.Point(10, 108);
            this.labelHorizontalAlign.Name = "labelHorizontalAlign";
            this.labelHorizontalAlign.Size = new System.Drawing.Size(30, 13);
            this.labelHorizontalAlign.TabIndex = 7;
            this.labelHorizontalAlign.Text = "Align";
            // 
            // checkBoxSimpleRender
            // 
            this.checkBoxSimpleRender.AutoSize = true;
            this.checkBoxSimpleRender.Location = new System.Drawing.Point(276, 71);
            this.checkBoxSimpleRender.Name = "checkBoxSimpleRender";
            this.checkBoxSimpleRender.Size = new System.Drawing.Size(104, 17);
            this.checkBoxSimpleRender.TabIndex = 14;
            this.checkBoxSimpleRender.Text = "Simple rendering";
            this.checkBoxSimpleRender.UseVisualStyleBackColor = true;
            this.checkBoxSimpleRender.CheckedChanged += new System.EventHandler(this.checkBoxAntiAlias_CheckedChanged);
            // 
            // labelSubtitleFontSize
            // 
            this.labelSubtitleFontSize.AutoSize = true;
            this.labelSubtitleFontSize.Location = new System.Drawing.Point(10, 54);
            this.labelSubtitleFontSize.Name = "labelSubtitleFontSize";
            this.labelSubtitleFontSize.Size = new System.Drawing.Size(84, 13);
            this.labelSubtitleFontSize.TabIndex = 2;
            this.labelSubtitleFontSize.Text = "Subtitle font size";
            // 
            // comboBoxSubtitleFont
            // 
            this.comboBoxSubtitleFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFont.FormattingEnabled = true;
            this.comboBoxSubtitleFont.Location = new System.Drawing.Point(100, 24);
            this.comboBoxSubtitleFont.Name = "comboBoxSubtitleFont";
            this.comboBoxSubtitleFont.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSubtitleFont.TabIndex = 1;
            this.comboBoxSubtitleFont.SelectedIndexChanged += new System.EventHandler(this.comboBoxSubtitleFont_SelectedIndexChanged);
            this.comboBoxSubtitleFont.SelectedValueChanged += new System.EventHandler(this.comboBoxSubtitleFont_SelectedValueChanged);
            // 
            // comboBoxSubtitleFontSize
            // 
            this.comboBoxSubtitleFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
            "150"});
            this.comboBoxSubtitleFontSize.Location = new System.Drawing.Point(100, 51);
            this.comboBoxSubtitleFontSize.Name = "comboBoxSubtitleFontSize";
            this.comboBoxSubtitleFontSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSubtitleFontSize.TabIndex = 3;
            this.comboBoxSubtitleFontSize.SelectedIndexChanged += new System.EventHandler(this.comboBoxSubtitleFontSize_SelectedIndexChanged);
            // 
            // labelSubtitleFont
            // 
            this.labelSubtitleFont.AutoSize = true;
            this.labelSubtitleFont.Location = new System.Drawing.Point(10, 27);
            this.labelSubtitleFont.Name = "labelSubtitleFont";
            this.labelSubtitleFont.Size = new System.Drawing.Size(63, 13);
            this.labelSubtitleFont.TabIndex = 0;
            this.labelSubtitleFont.Text = "Subtitle font";
            // 
            // labelBorderWidth
            // 
            this.labelBorderWidth.Location = new System.Drawing.Point(416, 54);
            this.labelBorderWidth.Name = "labelBorderWidth";
            this.labelBorderWidth.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelBorderWidth.Size = new System.Drawing.Size(110, 13);
            this.labelBorderWidth.TabIndex = 16;
            this.labelBorderWidth.Text = "Border width";
            this.labelBorderWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxBorderWidth
            // 
            this.comboBoxBorderWidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
            this.comboBoxBorderWidth.Location = new System.Drawing.Point(532, 51);
            this.comboBoxBorderWidth.Name = "comboBoxBorderWidth";
            this.comboBoxBorderWidth.Size = new System.Drawing.Size(121, 21);
            this.comboBoxBorderWidth.TabIndex = 22;
            this.comboBoxBorderWidth.SelectedIndexChanged += new System.EventHandler(this.comboBoxBorderWidth_SelectedIndexChanged);
            // 
            // panelBorderColor
            // 
            this.panelBorderColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBorderColor.Location = new System.Drawing.Point(659, 25);
            this.panelBorderColor.Name = "panelBorderColor";
            this.panelBorderColor.Size = new System.Drawing.Size(21, 20);
            this.panelBorderColor.TabIndex = 21;
            this.panelBorderColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelBorderColor_MouseClick);
            // 
            // buttonBorderColor
            // 
            this.buttonBorderColor.Location = new System.Drawing.Point(532, 24);
            this.buttonBorderColor.Name = "buttonBorderColor";
            this.buttonBorderColor.Size = new System.Drawing.Size(121, 21);
            this.buttonBorderColor.TabIndex = 20;
            this.buttonBorderColor.Text = "Border color";
            this.buttonBorderColor.UseVisualStyleBackColor = true;
            this.buttonBorderColor.Click += new System.EventHandler(this.buttonBorderColor_Click);
            // 
            // panelColor
            // 
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(403, 25);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(21, 20);
            this.panelColor.TabIndex = 12;
            this.panelColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelColor_MouseClick);
            // 
            // buttonColor
            // 
            this.buttonColor.Location = new System.Drawing.Point(276, 24);
            this.buttonColor.Name = "buttonColor";
            this.buttonColor.Size = new System.Drawing.Size(121, 21);
            this.buttonColor.TabIndex = 11;
            this.buttonColor.Text = "Color";
            this.buttonColor.UseVisualStyleBackColor = true;
            this.buttonColor.Click += new System.EventHandler(this.buttonColor_Click);
            // 
            // checkBoxSkipEmptyFrameAtStart
            // 
            this.checkBoxSkipEmptyFrameAtStart.AutoSize = true;
            this.checkBoxSkipEmptyFrameAtStart.Checked = true;
            this.checkBoxSkipEmptyFrameAtStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSkipEmptyFrameAtStart.Location = new System.Drawing.Point(276, 92);
            this.checkBoxSkipEmptyFrameAtStart.Name = "checkBoxSkipEmptyFrameAtStart";
            this.checkBoxSkipEmptyFrameAtStart.Size = new System.Drawing.Size(147, 17);
            this.checkBoxSkipEmptyFrameAtStart.TabIndex = 15;
            this.checkBoxSkipEmptyFrameAtStart.Text = "Skip empty frames at start";
            this.checkBoxSkipEmptyFrameAtStart.UseVisualStyleBackColor = true;
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
            this.groupBoxExportImage.Size = new System.Drawing.Size(936, 181);
            this.groupBoxExportImage.TabIndex = 1;
            this.groupBoxExportImage.TabStop = false;
            // 
            // panelVlcTemp
            // 
            this.panelVlcTemp.Location = new System.Drawing.Point(642, 52);
            this.panelVlcTemp.Name = "panelVlcTemp";
            this.panelVlcTemp.Size = new System.Drawing.Size(200, 100);
            this.panelVlcTemp.TabIndex = 10;
            this.panelVlcTemp.Visible = false;
            // 
            // linkLabelPreview
            // 
            this.linkLabelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelPreview.AutoSize = true;
            this.linkLabelPreview.Location = new System.Drawing.Point(880, 16);
            this.linkLabelPreview.Name = "linkLabelPreview";
            this.linkLabelPreview.Size = new System.Drawing.Size(45, 13);
            this.linkLabelPreview.TabIndex = 9;
            this.linkLabelPreview.TabStop = true;
            this.linkLabelPreview.Text = "Preview";
            this.linkLabelPreview.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabelPreview.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelPreview_LinkClicked);
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
            this.boxMultiLineToolStripMenuItem});
            this.contextMenuStripListView.Name = "contextMenuStripListView";
            this.contextMenuStripListView.Size = new System.Drawing.Size(158, 92);
            // 
            // normalToolStripMenuItem
            // 
            this.normalToolStripMenuItem.Name = "normalToolStripMenuItem";
            this.normalToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.normalToolStripMenuItem.Text = "Normal";
            this.normalToolStripMenuItem.Click += new System.EventHandler(this.normalToolStripMenuItem_Click);
            // 
            // italicToolStripMenuItem
            // 
            this.italicToolStripMenuItem.Name = "italicToolStripMenuItem";
            this.italicToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.italicToolStripMenuItem.Text = "Italic";
            this.italicToolStripMenuItem.Click += new System.EventHandler(this.italicToolStripMenuItem_Click);
            // 
            // boxSingleLineToolStripMenuItem
            // 
            this.boxSingleLineToolStripMenuItem.Name = "boxSingleLineToolStripMenuItem";
            this.boxSingleLineToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.boxSingleLineToolStripMenuItem.Text = "Box - single line";
            this.boxSingleLineToolStripMenuItem.Click += new System.EventHandler(this.boxSingleLineToolStripMenuItem_Click);
            // 
            // boxMultiLineToolStripMenuItem
            // 
            this.boxMultiLineToolStripMenuItem.Name = "boxMultiLineToolStripMenuItem";
            this.boxMultiLineToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.boxMultiLineToolStripMenuItem.Text = "Box - multi line";
            this.boxMultiLineToolStripMenuItem.Click += new System.EventHandler(this.boxMultiLineToolStripMenuItem_Click);
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
            this.subtitleListView1.Size = new System.Drawing.Size(936, 199);
            this.subtitleListView1.TabIndex = 7;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            this.subtitleListView1.SelectedIndexChanged += new System.EventHandler(this.subtitleListView1_SelectedIndexChanged);
            this.subtitleListView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.subtitleListView1_KeyDown);
            // 
            // ExportPngXml
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(955, 628);
            this.Controls.Add(this.subtitleListView1);
            this.Controls.Add(this.groupBoxExportImage);
            this.Controls.Add(this.groupBoxImageSettings);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(730, 630);
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
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLineSpacing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowTransparency)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDepth3D)).EndInit();
            this.groupBoxExportImage.ResumeLayout(false);
            this.groupBoxExportImage.PerformLayout();
            this.contextMenuStripListView.ResumeLayout(false);
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
        private System.Windows.Forms.ComboBox comboBoxBorderWidth;
        private System.Windows.Forms.Panel panelBorderColor;
        private System.Windows.Forms.Button buttonBorderColor;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Button buttonColor;
        private System.Windows.Forms.Label labelSubtitleFontSize;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFont;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFontSize;
        private System.Windows.Forms.Label labelSubtitleFont;
        private System.Windows.Forms.CheckBox checkBoxSimpleRender;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ComboBox comboBoxHAlign;
        private System.Windows.Forms.Label labelHorizontalAlign;
        private System.Windows.Forms.Label labelResolution;
        private System.Windows.Forms.ComboBox comboBoxResolution;
        private System.Windows.Forms.CheckBox checkBoxBold;
        private System.Windows.Forms.Label labelImageFormat;
        private System.Windows.Forms.ComboBox comboBoxImageFormat;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelFrameRate;
        private System.Windows.Forms.ComboBox comboBoxFrameRate;
        private System.Windows.Forms.ComboBox comboBoxBottomMargin;
        private System.Windows.Forms.Label labelBottomMargin;
        private System.Windows.Forms.CheckBox checkBoxSkipEmptyFrameAtStart;
        private System.Windows.Forms.Button buttonCustomResolution;
        private System.Windows.Forms.GroupBox groupBoxExportImage;
        private System.Windows.Forms.NumericUpDown numericUpDownDepth3D;
        private System.Windows.Forms.Label labelDepth;
        private System.Windows.Forms.Label label3D;
        private System.Windows.Forms.ComboBox comboBox3D;
        private System.Windows.Forms.Timer timerPreview;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem saveImageAsToolStripMenuItem;
        private System.Windows.Forms.Label labelShadowWidth;
        private System.Windows.Forms.ComboBox comboBoxShadowWidth;
        private System.Windows.Forms.Panel panelShadowColor;
        private System.Windows.Forms.Button buttonShadowColor;
        private System.Windows.Forms.NumericUpDown numericUpDownShadowTransparency;
        private System.Windows.Forms.Label labelShadowTransparency;
        private System.Windows.Forms.Label labelLineHeight;
        private System.Windows.Forms.NumericUpDown numericUpDownLineSpacing;
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
        private System.Windows.Forms.ComboBox comboBoxLeftRightMargin;
        private System.Windows.Forms.Label labelLeftRightMargin;
        private System.Windows.Forms.Panel panelFullFrameBackground;
    }
}