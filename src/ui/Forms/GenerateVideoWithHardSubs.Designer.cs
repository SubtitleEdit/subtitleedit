
namespace Nikse.SubtitleEdit.Forms
{
    partial class GenerateVideoWithHardSubs
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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.contextMenuStripGenerate = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.promptParameterBeforeGenerateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBoxSettings = new System.Windows.Forms.GroupBox();
            this.checkBoxFontBold = new System.Windows.Forms.CheckBox();
            this.buttonClear = new System.Windows.Forms.Button();
            this.panelForeColor = new System.Windows.Forms.Panel();
            this.nikseLabelOutputFileFolder = new System.Windows.Forms.Label();
            this.buttonRemoveFile = new System.Windows.Forms.Button();
            this.buttonForeColor = new System.Windows.Forms.Button();
            this.buttonAddFile = new System.Windows.Forms.Button();
            this.panelOutlineColor = new System.Windows.Forms.Panel();
            this.buttonOutlineColor = new System.Windows.Forms.Button();
            this.groupBoxCut = new System.Windows.Forms.GroupBox();
            this.buttonCutTo = new System.Windows.Forms.Button();
            this.buttonCutFrom = new System.Windows.Forms.Button();
            this.checkBoxCut = new System.Windows.Forms.CheckBox();
            this.checkBoxBox = new System.Windows.Forms.CheckBox();
            this.checkBoxAlignRight = new System.Windows.Forms.CheckBox();
            this.checkBoxRightToLeft = new System.Windows.Forms.CheckBox();
            this.groupBoxVideo = new System.Windows.Forms.GroupBox();
            this.buttonVideoChooseStandardRes = new System.Windows.Forms.Button();
            this.groupBoxAudio = new System.Windows.Forms.GroupBox();
            this.listViewAudioTracks = new System.Windows.Forms.ListView();
            this.columnHeaderAudioTrack = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.checkBoxMakeStereo = new System.Windows.Forms.CheckBox();
            this.checkBoxTargetFileSize = new System.Windows.Forms.CheckBox();
            this.listViewBatch = new System.Windows.Forms.ListView();
            this.columnHeaderVideoFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderResolution = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSubtitleFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripBatch = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.pickSubtitleFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeSubtitleFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonOutputFileSettings = new System.Windows.Forms.Button();
            this.buttonPreview = new System.Windows.Forms.Button();
            this.linkLabelHelp = new System.Windows.Forms.LinkLabel();
            this.contextMenuStripRes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.useSourceResolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
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
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.youTubeShortsTikTok10801920ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.youTubeShortsTikTokAspectRatio9167201280ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aAspectRatio916540960ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bAspectRatio916360540ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aAspectRatio916270480ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bAspectRatio916180270ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonMode = new System.Windows.Forms.Button();
            this.nikseLabelOutline = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.numericUpDownOutline = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.videoPlayerContainer1 = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            this.numericUpDownCutToSeconds = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownCutToMinutes = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownCutToHours = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownCutFromSeconds = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownCutFromMinutes = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownCutFromHours = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelCutTo = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelCutFrom = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelVideoBitrate = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelInfo = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxSubtitleFont = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelSubtitleFont = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelCrfHint = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelResolution = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.numericUpDownWidth = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownHeight = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelX = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelPreset = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxTune = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxPreset = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelTune = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelCRF = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxVideoEncoding = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxCrf = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelVideoEncoding = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelAudioEnc = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxAudioBitRate = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxAudioEnc = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelAudioBitRate = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelAudioSampleRate = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxAudioSampleRate = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.numericUpDownTargetFileSize = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelFileSize = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelFileName = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.numericUpDownFontSize = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelFontSize = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelPreviewPleaseWait = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelPass = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelProgress = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelPleaseWait = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.textBoxLog = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.contextMenuStripGenerate.SuspendLayout();
            this.groupBoxSettings.SuspendLayout();
            this.groupBoxCut.SuspendLayout();
            this.groupBoxVideo.SuspendLayout();
            this.groupBoxAudio.SuspendLayout();
            this.contextMenuStripBatch.SuspendLayout();
            this.contextMenuStripRes.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 615);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(603, 11);
            this.progressBar1.TabIndex = 22;
            this.progressBar1.Visible = false;
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGenerate.ContextMenuStrip = this.contextMenuStripGenerate;
            this.buttonGenerate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenerate.Location = new System.Drawing.Point(621, 615);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(121, 23);
            this.buttonGenerate.TabIndex = 140;
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
            this.buttonCancel.Location = new System.Drawing.Point(748, 615);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(125, 23);
            this.buttonCancel.TabIndex = 141;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // groupBoxSettings
            // 
            this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSettings.Controls.Add(this.nikseLabelOutline);
            this.groupBoxSettings.Controls.Add(this.numericUpDownOutline);
            this.groupBoxSettings.Controls.Add(this.checkBoxFontBold);
            this.groupBoxSettings.Controls.Add(this.videoPlayerContainer1);
            this.groupBoxSettings.Controls.Add(this.buttonClear);
            this.groupBoxSettings.Controls.Add(this.panelForeColor);
            this.groupBoxSettings.Controls.Add(this.nikseLabelOutputFileFolder);
            this.groupBoxSettings.Controls.Add(this.buttonRemoveFile);
            this.groupBoxSettings.Controls.Add(this.buttonForeColor);
            this.groupBoxSettings.Controls.Add(this.buttonAddFile);
            this.groupBoxSettings.Controls.Add(this.panelOutlineColor);
            this.groupBoxSettings.Controls.Add(this.buttonOutlineColor);
            this.groupBoxSettings.Controls.Add(this.groupBoxCut);
            this.groupBoxSettings.Controls.Add(this.labelVideoBitrate);
            this.groupBoxSettings.Controls.Add(this.checkBoxBox);
            this.groupBoxSettings.Controls.Add(this.checkBoxAlignRight);
            this.groupBoxSettings.Controls.Add(this.labelInfo);
            this.groupBoxSettings.Controls.Add(this.checkBoxRightToLeft);
            this.groupBoxSettings.Controls.Add(this.comboBoxSubtitleFont);
            this.groupBoxSettings.Controls.Add(this.labelSubtitleFont);
            this.groupBoxSettings.Controls.Add(this.groupBoxVideo);
            this.groupBoxSettings.Controls.Add(this.groupBoxAudio);
            this.groupBoxSettings.Controls.Add(this.numericUpDownTargetFileSize);
            this.groupBoxSettings.Controls.Add(this.labelFileSize);
            this.groupBoxSettings.Controls.Add(this.checkBoxTargetFileSize);
            this.groupBoxSettings.Controls.Add(this.labelFileName);
            this.groupBoxSettings.Controls.Add(this.numericUpDownFontSize);
            this.groupBoxSettings.Controls.Add(this.labelFontSize);
            this.groupBoxSettings.Controls.Add(this.listViewBatch);
            this.groupBoxSettings.Controls.Add(this.buttonOutputFileSettings);
            this.groupBoxSettings.Location = new System.Drawing.Point(12, 12);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(861, 548);
            this.groupBoxSettings.TabIndex = 0;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "Settings";
            // 
            // checkBoxFontBold
            // 
            this.checkBoxFontBold.AutoSize = true;
            this.checkBoxFontBold.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxFontBold.Location = new System.Drawing.Point(187, 31);
            this.checkBoxFontBold.Name = "checkBoxFontBold";
            this.checkBoxFontBold.Size = new System.Drawing.Size(51, 17);
            this.checkBoxFontBold.TabIndex = 20;
            this.checkBoxFontBold.Text = "Bold";
            this.checkBoxFontBold.UseVisualStyleBackColor = true;
            this.checkBoxFontBold.CheckedChanged += new System.EventHandler(this.checkBoxFontBold_CheckedChanged);
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonClear.Location = new System.Drawing.Point(165, 514);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(74, 23);
            this.buttonClear.TabIndex = 146;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // panelForeColor
            // 
            this.panelForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelForeColor.Location = new System.Drawing.Point(369, 28);
            this.panelForeColor.Name = "panelForeColor";
            this.panelForeColor.Size = new System.Drawing.Size(21, 21);
            this.panelForeColor.TabIndex = 6;
            this.panelForeColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelForeColor_MouseClick);
            // 
            // nikseLabelOutputFileFolder
            // 
            this.nikseLabelOutputFileFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nikseLabelOutputFileFolder.AutoSize = true;
            this.nikseLabelOutputFileFolder.Location = new System.Drawing.Point(334, 519);
            this.nikseLabelOutputFileFolder.Name = "nikseLabelOutputFileFolder";
            this.nikseLabelOutputFileFolder.Size = new System.Drawing.Size(90, 13);
            this.nikseLabelOutputFileFolder.TabIndex = 144;
            this.nikseLabelOutputFileFolder.Text = "Use source folder";
            // 
            // buttonRemoveFile
            // 
            this.buttonRemoveFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemoveFile.Location = new System.Drawing.Point(85, 514);
            this.buttonRemoveFile.Name = "buttonRemoveFile";
            this.buttonRemoveFile.Size = new System.Drawing.Size(74, 23);
            this.buttonRemoveFile.TabIndex = 145;
            this.buttonRemoveFile.Text = "Remove";
            this.buttonRemoveFile.UseVisualStyleBackColor = true;
            this.buttonRemoveFile.Click += new System.EventHandler(this.buttonRemoveFile_Click);
            // 
            // buttonForeColor
            // 
            this.buttonForeColor.Location = new System.Drawing.Point(274, 25);
            this.buttonForeColor.Name = "buttonForeColor";
            this.buttonForeColor.Size = new System.Drawing.Size(89, 23);
            this.buttonForeColor.TabIndex = 30;
            this.buttonForeColor.Text = "Color";
            this.buttonForeColor.UseVisualStyleBackColor = true;
            this.buttonForeColor.Click += new System.EventHandler(this.buttonForeColor_Click);
            // 
            // buttonAddFile
            // 
            this.buttonAddFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddFile.Location = new System.Drawing.Point(6, 514);
            this.buttonAddFile.Name = "buttonAddFile";
            this.buttonAddFile.Size = new System.Drawing.Size(73, 23);
            this.buttonAddFile.TabIndex = 144;
            this.buttonAddFile.Text = "Add...";
            this.buttonAddFile.UseVisualStyleBackColor = true;
            this.buttonAddFile.Click += new System.EventHandler(this.buttonAddFile_Click);
            // 
            // panelOutlineColor
            // 
            this.panelOutlineColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelOutlineColor.Location = new System.Drawing.Point(369, 56);
            this.panelOutlineColor.Name = "panelOutlineColor";
            this.panelOutlineColor.Size = new System.Drawing.Size(21, 20);
            this.panelOutlineColor.TabIndex = 4;
            this.panelOutlineColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelOutlineColor_MouseClick);
            // 
            // buttonOutlineColor
            // 
            this.buttonOutlineColor.Location = new System.Drawing.Point(274, 54);
            this.buttonOutlineColor.Name = "buttonOutlineColor";
            this.buttonOutlineColor.Size = new System.Drawing.Size(89, 23);
            this.buttonOutlineColor.TabIndex = 60;
            this.buttonOutlineColor.Text = "Color";
            this.buttonOutlineColor.UseVisualStyleBackColor = true;
            this.buttonOutlineColor.Click += new System.EventHandler(this.buttonOutlineColor_Click);
            // 
            // groupBoxCut
            // 
            this.groupBoxCut.Controls.Add(this.buttonCutTo);
            this.groupBoxCut.Controls.Add(this.buttonCutFrom);
            this.groupBoxCut.Controls.Add(this.numericUpDownCutToSeconds);
            this.groupBoxCut.Controls.Add(this.numericUpDownCutToMinutes);
            this.groupBoxCut.Controls.Add(this.numericUpDownCutToHours);
            this.groupBoxCut.Controls.Add(this.numericUpDownCutFromSeconds);
            this.groupBoxCut.Controls.Add(this.numericUpDownCutFromMinutes);
            this.groupBoxCut.Controls.Add(this.numericUpDownCutFromHours);
            this.groupBoxCut.Controls.Add(this.labelCutTo);
            this.groupBoxCut.Controls.Add(this.labelCutFrom);
            this.groupBoxCut.Controls.Add(this.checkBoxCut);
            this.groupBoxCut.Location = new System.Drawing.Point(430, 21);
            this.groupBoxCut.Name = "groupBoxCut";
            this.groupBoxCut.Size = new System.Drawing.Size(425, 96);
            this.groupBoxCut.TabIndex = 50;
            this.groupBoxCut.TabStop = false;
            // 
            // buttonCutTo
            // 
            this.buttonCutTo.Location = new System.Drawing.Point(255, 62);
            this.buttonCutTo.Name = "buttonCutTo";
            this.buttonCutTo.Size = new System.Drawing.Size(33, 23);
            this.buttonCutTo.TabIndex = 24;
            this.buttonCutTo.Text = "...";
            this.buttonCutTo.UseVisualStyleBackColor = true;
            this.buttonCutTo.Click += new System.EventHandler(this.buttonCutTo_Click);
            // 
            // buttonCutFrom
            // 
            this.buttonCutFrom.Location = new System.Drawing.Point(97, 62);
            this.buttonCutFrom.Name = "buttonCutFrom";
            this.buttonCutFrom.Size = new System.Drawing.Size(33, 23);
            this.buttonCutFrom.TabIndex = 23;
            this.buttonCutFrom.Text = "...";
            this.buttonCutFrom.UseVisualStyleBackColor = true;
            this.buttonCutFrom.Click += new System.EventHandler(this.buttonCutFrom_Click);
            // 
            // checkBoxCut
            // 
            this.checkBoxCut.AutoSize = true;
            this.checkBoxCut.Location = new System.Drawing.Point(12, 36);
            this.checkBoxCut.Name = "checkBoxCut";
            this.checkBoxCut.Size = new System.Drawing.Size(42, 17);
            this.checkBoxCut.TabIndex = 0;
            this.checkBoxCut.Text = "Cut";
            this.checkBoxCut.UseVisualStyleBackColor = true;
            this.checkBoxCut.CheckedChanged += new System.EventHandler(this.checkBoxCut_CheckedChanged);
            // 
            // checkBoxBox
            // 
            this.checkBoxBox.AutoSize = true;
            this.checkBoxBox.Location = new System.Drawing.Point(187, 60);
            this.checkBoxBox.Name = "checkBoxBox";
            this.checkBoxBox.Size = new System.Drawing.Size(44, 17);
            this.checkBoxBox.TabIndex = 50;
            this.checkBoxBox.Text = "Box";
            this.checkBoxBox.UseVisualStyleBackColor = true;
            this.checkBoxBox.CheckedChanged += new System.EventHandler(this.checkBoxBox_CheckedChanged);
            // 
            // checkBoxAlignRight
            // 
            this.checkBoxAlignRight.AutoSize = true;
            this.checkBoxAlignRight.Location = new System.Drawing.Point(218, 114);
            this.checkBoxAlignRight.Name = "checkBoxAlignRight";
            this.checkBoxAlignRight.Size = new System.Drawing.Size(72, 17);
            this.checkBoxAlignRight.TabIndex = 76;
            this.checkBoxAlignRight.Text = "Align right";
            this.checkBoxAlignRight.UseVisualStyleBackColor = true;
            this.checkBoxAlignRight.CheckedChanged += new System.EventHandler(this.checkBoxAlignRight_CheckedChanged);
            // 
            // checkBoxRightToLeft
            // 
            this.checkBoxRightToLeft.AutoSize = true;
            this.checkBoxRightToLeft.Location = new System.Drawing.Point(120, 114);
            this.checkBoxRightToLeft.Name = "checkBoxRightToLeft";
            this.checkBoxRightToLeft.Size = new System.Drawing.Size(80, 17);
            this.checkBoxRightToLeft.TabIndex = 75;
            this.checkBoxRightToLeft.Text = "Right to left";
            this.checkBoxRightToLeft.UseVisualStyleBackColor = true;
            this.checkBoxRightToLeft.CheckedChanged += new System.EventHandler(this.checkBoxRightToLeft_CheckedChanged);
            // 
            // groupBoxVideo
            // 
            this.groupBoxVideo.Controls.Add(this.labelCrfHint);
            this.groupBoxVideo.Controls.Add(this.buttonVideoChooseStandardRes);
            this.groupBoxVideo.Controls.Add(this.labelResolution);
            this.groupBoxVideo.Controls.Add(this.numericUpDownWidth);
            this.groupBoxVideo.Controls.Add(this.numericUpDownHeight);
            this.groupBoxVideo.Controls.Add(this.labelX);
            this.groupBoxVideo.Controls.Add(this.labelPreset);
            this.groupBoxVideo.Controls.Add(this.comboBoxTune);
            this.groupBoxVideo.Controls.Add(this.comboBoxPreset);
            this.groupBoxVideo.Controls.Add(this.labelTune);
            this.groupBoxVideo.Controls.Add(this.labelCRF);
            this.groupBoxVideo.Controls.Add(this.comboBoxVideoEncoding);
            this.groupBoxVideo.Controls.Add(this.comboBoxCrf);
            this.groupBoxVideo.Controls.Add(this.labelVideoEncoding);
            this.groupBoxVideo.Location = new System.Drawing.Point(6, 141);
            this.groupBoxVideo.Name = "groupBoxVideo";
            this.groupBoxVideo.Size = new System.Drawing.Size(416, 166);
            this.groupBoxVideo.TabIndex = 70;
            this.groupBoxVideo.TabStop = false;
            this.groupBoxVideo.Text = "Video";
            // 
            // buttonVideoChooseStandardRes
            // 
            this.buttonVideoChooseStandardRes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonVideoChooseStandardRes.Location = new System.Drawing.Point(257, 15);
            this.buttonVideoChooseStandardRes.Name = "buttonVideoChooseStandardRes";
            this.buttonVideoChooseStandardRes.Size = new System.Drawing.Size(79, 23);
            this.buttonVideoChooseStandardRes.TabIndex = 3;
            this.buttonVideoChooseStandardRes.Text = "...";
            this.buttonVideoChooseStandardRes.UseVisualStyleBackColor = true;
            this.buttonVideoChooseStandardRes.Click += new System.EventHandler(this.buttonVideoChooseStandardRes_Click);
            // 
            // groupBoxAudio
            // 
            this.groupBoxAudio.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAudio.Controls.Add(this.listViewAudioTracks);
            this.groupBoxAudio.Controls.Add(this.labelAudioEnc);
            this.groupBoxAudio.Controls.Add(this.comboBoxAudioBitRate);
            this.groupBoxAudio.Controls.Add(this.comboBoxAudioEnc);
            this.groupBoxAudio.Controls.Add(this.labelAudioBitRate);
            this.groupBoxAudio.Controls.Add(this.checkBoxMakeStereo);
            this.groupBoxAudio.Controls.Add(this.labelAudioSampleRate);
            this.groupBoxAudio.Controls.Add(this.comboBoxAudioSampleRate);
            this.groupBoxAudio.Location = new System.Drawing.Point(430, 141);
            this.groupBoxAudio.Name = "groupBoxAudio";
            this.groupBoxAudio.Size = new System.Drawing.Size(425, 166);
            this.groupBoxAudio.TabIndex = 90;
            this.groupBoxAudio.TabStop = false;
            this.groupBoxAudio.Text = "Audio";
            // 
            // listViewAudioTracks
            // 
            this.listViewAudioTracks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewAudioTracks.CheckBoxes = true;
            this.listViewAudioTracks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAudioTrack});
            this.listViewAudioTracks.HideSelection = false;
            this.listViewAudioTracks.Location = new System.Drawing.Point(229, 29);
            this.listViewAudioTracks.Name = "listViewAudioTracks";
            this.listViewAudioTracks.Size = new System.Drawing.Size(190, 122);
            this.listViewAudioTracks.TabIndex = 45;
            this.listViewAudioTracks.UseCompatibleStateImageBehavior = false;
            this.listViewAudioTracks.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderAudioTrack
            // 
            this.columnHeaderAudioTrack.Text = "Audio tracks";
            this.columnHeaderAudioTrack.Width = 160;
            // 
            // checkBoxMakeStereo
            // 
            this.checkBoxMakeStereo.AutoSize = true;
            this.checkBoxMakeStereo.Checked = true;
            this.checkBoxMakeStereo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMakeStereo.Location = new System.Drawing.Point(91, 58);
            this.checkBoxMakeStereo.Name = "checkBoxMakeStereo";
            this.checkBoxMakeStereo.Size = new System.Drawing.Size(57, 17);
            this.checkBoxMakeStereo.TabIndex = 2;
            this.checkBoxMakeStereo.Text = "Stereo";
            this.checkBoxMakeStereo.UseVisualStyleBackColor = true;
            // 
            // checkBoxTargetFileSize
            // 
            this.checkBoxTargetFileSize.AutoSize = true;
            this.checkBoxTargetFileSize.Location = new System.Drawing.Point(22, 327);
            this.checkBoxTargetFileSize.Name = "checkBoxTargetFileSize";
            this.checkBoxTargetFileSize.Size = new System.Drawing.Size(192, 17);
            this.checkBoxTargetFileSize.TabIndex = 100;
            this.checkBoxTargetFileSize.Text = "Target file size (two pass encoding)";
            this.checkBoxTargetFileSize.UseVisualStyleBackColor = true;
            this.checkBoxTargetFileSize.CheckedChanged += new System.EventHandler(this.checkBoxTargetFileSize_CheckedChanged);
            // 
            // listViewBatch
            // 
            this.listViewBatch.AllowDrop = true;
            this.listViewBatch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewBatch.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderVideoFile,
            this.columnHeaderResolution,
            this.columnHeaderSize,
            this.columnHeaderSubtitleFile,
            this.columnHeaderStatus});
            this.listViewBatch.ContextMenuStrip = this.contextMenuStripBatch;
            this.listViewBatch.FullRowSelect = true;
            this.listViewBatch.HideSelection = false;
            this.listViewBatch.Location = new System.Drawing.Point(6, 316);
            this.listViewBatch.Name = "listViewBatch";
            this.listViewBatch.Size = new System.Drawing.Size(852, 192);
            this.listViewBatch.TabIndex = 3;
            this.listViewBatch.UseCompatibleStateImageBehavior = false;
            this.listViewBatch.View = System.Windows.Forms.View.Details;
            this.listViewBatch.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewBatch_ColumnClick);
            this.listViewBatch.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewBatch_DragDrop);
            this.listViewBatch.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewBatch_DragEnter);
            this.listViewBatch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewBatch_KeyDown);
            // 
            // columnHeaderVideoFile
            // 
            this.columnHeaderVideoFile.Text = "Video file";
            this.columnHeaderVideoFile.Width = 420;
            // 
            // columnHeaderResolution
            // 
            this.columnHeaderResolution.Text = "Resolution";
            this.columnHeaderResolution.Width = 80;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.Width = 80;
            // 
            // columnHeaderSubtitleFile
            // 
            this.columnHeaderSubtitleFile.Text = "Subtitle file";
            this.columnHeaderSubtitleFile.Width = 180;
            // 
            // columnHeaderStatus
            // 
            this.columnHeaderStatus.Text = "Status";
            this.columnHeaderStatus.Width = 80;
            // 
            // contextMenuStripBatch
            // 
            this.contextMenuStripBatch.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addFilesToolStripMenuItem,
            this.toolStripSeparator2,
            this.pickSubtitleFileToolStripMenuItem,
            this.removeSubtitleFileToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteToolStripMenuItem,
            this.clearToolStripMenuItem});
            this.contextMenuStripBatch.Name = "contextMenuStripBatch";
            this.contextMenuStripBatch.Size = new System.Drawing.Size(179, 126);
            this.contextMenuStripBatch.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripBatch_Opening);
            // 
            // addFilesToolStripMenuItem
            // 
            this.addFilesToolStripMenuItem.Name = "addFilesToolStripMenuItem";
            this.addFilesToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.addFilesToolStripMenuItem.Text = "Add video files...";
            this.addFilesToolStripMenuItem.Click += new System.EventHandler(this.addFilesToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(175, 6);
            // 
            // pickSubtitleFileToolStripMenuItem
            // 
            this.pickSubtitleFileToolStripMenuItem.Name = "pickSubtitleFileToolStripMenuItem";
            this.pickSubtitleFileToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.pickSubtitleFileToolStripMenuItem.Text = "Pick subtitle file...";
            this.pickSubtitleFileToolStripMenuItem.Click += new System.EventHandler(this.pickSubtitleFileToolStripMenuItem_Click);
            // 
            // removeSubtitleFileToolStripMenuItem
            // 
            this.removeSubtitleFileToolStripMenuItem.Name = "removeSubtitleFileToolStripMenuItem";
            this.removeSubtitleFileToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.removeSubtitleFileToolStripMenuItem.Text = "Remove subtitle file";
            this.removeSubtitleFileToolStripMenuItem.Click += new System.EventHandler(this.removeSubtitleFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(175, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // buttonOutputFileSettings
            // 
            this.buttonOutputFileSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOutputFileSettings.Location = new System.Drawing.Point(255, 514);
            this.buttonOutputFileSettings.Name = "buttonOutputFileSettings";
            this.buttonOutputFileSettings.Size = new System.Drawing.Size(167, 23);
            this.buttonOutputFileSettings.TabIndex = 148;
            this.buttonOutputFileSettings.Text = "Output file/folder...";
            this.buttonOutputFileSettings.UseVisualStyleBackColor = true;
            this.buttonOutputFileSettings.Click += new System.EventHandler(this.buttonOutputFileSettings_Click);
            // 
            // buttonPreview
            // 
            this.buttonPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPreview.Location = new System.Drawing.Point(621, 586);
            this.buttonPreview.Name = "buttonPreview";
            this.buttonPreview.Size = new System.Drawing.Size(121, 23);
            this.buttonPreview.TabIndex = 130;
            this.buttonPreview.Text = "Preview";
            this.buttonPreview.UseVisualStyleBackColor = true;
            this.buttonPreview.Click += new System.EventHandler(this.buttonPreview_Click);
            // 
            // linkLabelHelp
            // 
            this.linkLabelHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelHelp.AutoSize = true;
            this.linkLabelHelp.Location = new System.Drawing.Point(844, 563);
            this.linkLabelHelp.Name = "linkLabelHelp";
            this.linkLabelHelp.Size = new System.Drawing.Size(29, 13);
            this.linkLabelHelp.TabIndex = 120;
            this.linkLabelHelp.TabStop = true;
            this.linkLabelHelp.Text = "Help";
            this.linkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelp_LinkClicked);
            // 
            // contextMenuStripRes
            // 
            this.contextMenuStripRes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.useSourceResolutionToolStripMenuItem,
            this.toolStripSeparator3,
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
            this.toolStripSeparator4,
            this.youTubeShortsTikTok10801920ToolStripMenuItem,
            this.youTubeShortsTikTokAspectRatio9167201280ToolStripMenuItem,
            this.aAspectRatio916540960ToolStripMenuItem,
            this.bAspectRatio916360540ToolStripMenuItem,
            this.aAspectRatio916270480ToolStripMenuItem,
            this.bAspectRatio916180270ToolStripMenuItem});
            this.contextMenuStripRes.Name = "contextMenuStripRes";
            this.contextMenuStripRes.Size = new System.Drawing.Size(368, 412);
            this.contextMenuStripRes.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripRes_Opening);
            // 
            // useSourceResolutionToolStripMenuItem
            // 
            this.useSourceResolutionToolStripMenuItem.Name = "useSourceResolutionToolStripMenuItem";
            this.useSourceResolutionToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.useSourceResolutionToolStripMenuItem.Text = "Use source resolution";
            this.useSourceResolutionToolStripMenuItem.Click += new System.EventHandler(this.useSourceResolutionToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(364, 6);
            // 
            // x2160ToolStripMenuItem
            // 
            this.x2160ToolStripMenuItem.Name = "x2160ToolStripMenuItem";
            this.x2160ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.x2160ToolStripMenuItem.Text = "4K DCI - Aspect Ratio 16∶9 - (4096x2160)";
            this.x2160ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // uHD3840x2160ToolStripMenuItem
            // 
            this.uHD3840x2160ToolStripMenuItem.Name = "uHD3840x2160ToolStripMenuItem";
            this.uHD3840x2160ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.uHD3840x2160ToolStripMenuItem.Text = "4K UHD - Aspect Ratio 16∶9 - (3840x2160)";
            this.uHD3840x2160ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // k2048x1080ToolStripMenuItem
            // 
            this.k2048x1080ToolStripMenuItem.Name = "k2048x1080ToolStripMenuItem";
            this.k2048x1080ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.k2048x1080ToolStripMenuItem.Text = "2K WQHD - Aspect Ratio 16∶9 - (2560x1440)";
            this.k2048x1080ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // dCI2KScope2048x858ToolStripMenuItem
            // 
            this.dCI2KScope2048x858ToolStripMenuItem.Name = "dCI2KScope2048x858ToolStripMenuItem";
            this.dCI2KScope2048x858ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.dCI2KScope2048x858ToolStripMenuItem.Text = "2K DCI - Aspect Ratio 16∶9 - (2048x1080)";
            this.dCI2KScope2048x858ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // dCI2KFlat1998x1080ToolStripMenuItem
            // 
            this.dCI2KFlat1998x1080ToolStripMenuItem.Name = "dCI2KFlat1998x1080ToolStripMenuItem";
            this.dCI2KFlat1998x1080ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.dCI2KFlat1998x1080ToolStripMenuItem.Text = "Full HD 1080p - Aspect Ratio 16∶9 - (1920x1080)";
            this.dCI2KFlat1998x1080ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // p1920x1080ToolStripMenuItem
            // 
            this.p1920x1080ToolStripMenuItem.Name = "p1920x1080ToolStripMenuItem";
            this.p1920x1080ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.p1920x1080ToolStripMenuItem.Text = "HD 720p - Aspect Ratio 16∶9 - (1280x720)";
            this.p1920x1080ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // x1080ToolStripMenuItem
            // 
            this.x1080ToolStripMenuItem.Name = "x1080ToolStripMenuItem";
            this.x1080ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.x1080ToolStripMenuItem.Text = "540p - Aspect Ratio 16∶9 - (960x540)";
            this.x1080ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // p1280x720ToolStripMenuItem
            // 
            this.p1280x720ToolStripMenuItem.Name = "p1280x720ToolStripMenuItem";
            this.p1280x720ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.p1280x720ToolStripMenuItem.Text = "SD PAL - Aspect Ratio 4:3 - (720x576)";
            this.p1280x720ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // x720ToolStripMenuItem
            // 
            this.x720ToolStripMenuItem.Name = "x720ToolStripMenuItem";
            this.x720ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.x720ToolStripMenuItem.Text = "SD NTSC - Aspect Ratio 3:2 - (720x480)";
            this.x720ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // p848x480ToolStripMenuItem
            // 
            this.p848x480ToolStripMenuItem.Name = "p848x480ToolStripMenuItem";
            this.p848x480ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.p848x480ToolStripMenuItem.Text = "VGA - Aspect Ratio 4:3 - (640x480)";
            this.p848x480ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // pAL720x576ToolStripMenuItem
            // 
            this.pAL720x576ToolStripMenuItem.Name = "pAL720x576ToolStripMenuItem";
            this.pAL720x576ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.pAL720x576ToolStripMenuItem.Text = "360p - Aspect Ratio 16∶9 - (640x360)";
            this.pAL720x576ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(364, 6);
            // 
            // youTubeShortsTikTok10801920ToolStripMenuItem
            // 
            this.youTubeShortsTikTok10801920ToolStripMenuItem.Name = "youTubeShortsTikTok10801920ToolStripMenuItem";
            this.youTubeShortsTikTok10801920ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.youTubeShortsTikTok10801920ToolStripMenuItem.Text = "YouTube shorts/TikTok - Aspect Ratio 9∶16 - (1080x1920)";
            this.youTubeShortsTikTok10801920ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // youTubeShortsTikTokAspectRatio9167201280ToolStripMenuItem
            // 
            this.youTubeShortsTikTokAspectRatio9167201280ToolStripMenuItem.Name = "youTubeShortsTikTokAspectRatio9167201280ToolStripMenuItem";
            this.youTubeShortsTikTokAspectRatio9167201280ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.youTubeShortsTikTokAspectRatio9167201280ToolStripMenuItem.Text = "YouTube shorts/TikTok - Aspect Ratio 9∶16 - (720x1280)";
            this.youTubeShortsTikTokAspectRatio9167201280ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // aAspectRatio916540960ToolStripMenuItem
            // 
            this.aAspectRatio916540960ToolStripMenuItem.Name = "aAspectRatio916540960ToolStripMenuItem";
            this.aAspectRatio916540960ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.aAspectRatio916540960ToolStripMenuItem.Text = "1/2 A - Aspect Ratio 9∶16 - (540x960)";
            this.aAspectRatio916540960ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // bAspectRatio916360540ToolStripMenuItem
            // 
            this.bAspectRatio916360540ToolStripMenuItem.Name = "bAspectRatio916360540ToolStripMenuItem";
            this.bAspectRatio916360540ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.bAspectRatio916360540ToolStripMenuItem.Text = "1/2 B - Aspect Ratio 9∶16 - (360x540)";
            this.bAspectRatio916360540ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // aAspectRatio916270480ToolStripMenuItem
            // 
            this.aAspectRatio916270480ToolStripMenuItem.Name = "aAspectRatio916270480ToolStripMenuItem";
            this.aAspectRatio916270480ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.aAspectRatio916270480ToolStripMenuItem.Text = "1/4 A - Aspect Ratio 9∶16 - (270x480)";
            this.aAspectRatio916270480ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // bAspectRatio916180270ToolStripMenuItem
            // 
            this.bAspectRatio916180270ToolStripMenuItem.Name = "bAspectRatio916180270ToolStripMenuItem";
            this.bAspectRatio916180270ToolStripMenuItem.Size = new System.Drawing.Size(367, 22);
            this.bAspectRatio916180270ToolStripMenuItem.Text = "1/4 B - Aspect Ratio 9∶16 - (180x270)";
            this.bAspectRatio916180270ToolStripMenuItem.Click += new System.EventHandler(this.ResolutionPickClick);
            // 
            // buttonMode
            // 
            this.buttonMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMode.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonMode.Location = new System.Drawing.Point(748, 586);
            this.buttonMode.Name = "buttonMode";
            this.buttonMode.Size = new System.Drawing.Size(125, 23);
            this.buttonMode.TabIndex = 143;
            this.buttonMode.Text = "Batch mode";
            this.buttonMode.UseVisualStyleBackColor = true;
            this.buttonMode.Click += new System.EventHandler(this.buttonMode_Click);
            // 
            // nikseLabelOutline
            // 
            this.nikseLabelOutline.AutoSize = true;
            this.nikseLabelOutline.Location = new System.Drawing.Point(19, 58);
            this.nikseLabelOutline.Name = "nikseLabelOutline";
            this.nikseLabelOutline.Size = new System.Drawing.Size(40, 13);
            this.nikseLabelOutline.TabIndex = 151;
            this.nikseLabelOutline.Text = "Outline";
            // 
            // numericUpDownOutline
            // 
            this.numericUpDownOutline.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownOutline.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownOutline.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownOutline.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownOutline.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownOutline.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownOutline.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownOutline.DecimalPlaces = 1;
            this.numericUpDownOutline.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownOutline.Location = new System.Drawing.Point(119, 54);
            this.numericUpDownOutline.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownOutline.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownOutline.Name = "numericUpDownOutline";
            this.numericUpDownOutline.Size = new System.Drawing.Size(54, 23);
            this.numericUpDownOutline.TabIndex = 40;
            this.numericUpDownOutline.TabStop = false;
            this.numericUpDownOutline.ThousandsSeparator = false;
            this.numericUpDownOutline.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownOutline.ValueChanged += new System.EventHandler(this.numericUpDownOutline_ValueChanged);
            // 
            // videoPlayerContainer1
            // 
            this.videoPlayerContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.videoPlayerContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.videoPlayerContainer1.Chapters = null;
            this.videoPlayerContainer1.CurrentPosition = 0D;
            this.videoPlayerContainer1.FontSizeFactor = 1F;
            this.videoPlayerContainer1.LastParagraph = null;
            this.videoPlayerContainer1.Location = new System.Drawing.Point(430, 313);
            this.videoPlayerContainer1.Name = "videoPlayerContainer1";
            this.videoPlayerContainer1.ShowFullscreenButton = true;
            this.videoPlayerContainer1.ShowMuteButton = true;
            this.videoPlayerContainer1.ShowStopButton = true;
            this.videoPlayerContainer1.Size = new System.Drawing.Size(431, 227);
            this.videoPlayerContainer1.SubtitleText = "";
            this.videoPlayerContainer1.TabIndex = 110;
            this.videoPlayerContainer1.TextRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.videoPlayerContainer1.UsingFrontCenterAudioChannelOnly = false;
            this.videoPlayerContainer1.VideoHeight = 0;
            this.videoPlayerContainer1.VideoPlayer = null;
            this.videoPlayerContainer1.VideoWidth = 0;
            this.videoPlayerContainer1.Volume = 0D;
            // 
            // numericUpDownCutToSeconds
            // 
            this.numericUpDownCutToSeconds.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownCutToSeconds.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownCutToSeconds.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownCutToSeconds.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownCutToSeconds.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownCutToSeconds.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownCutToSeconds.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownCutToSeconds.DecimalPlaces = 0;
            this.numericUpDownCutToSeconds.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownCutToSeconds.Location = new System.Drawing.Point(327, 36);
            this.numericUpDownCutToSeconds.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numericUpDownCutToSeconds.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownCutToSeconds.Name = "numericUpDownCutToSeconds";
            this.numericUpDownCutToSeconds.Size = new System.Drawing.Size(40, 23);
            this.numericUpDownCutToSeconds.TabIndex = 22;
            this.numericUpDownCutToSeconds.TabStop = false;
            this.numericUpDownCutToSeconds.ThousandsSeparator = false;
            this.numericUpDownCutToSeconds.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // numericUpDownCutToMinutes
            // 
            this.numericUpDownCutToMinutes.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownCutToMinutes.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownCutToMinutes.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownCutToMinutes.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownCutToMinutes.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownCutToMinutes.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownCutToMinutes.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownCutToMinutes.DecimalPlaces = 0;
            this.numericUpDownCutToMinutes.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownCutToMinutes.Location = new System.Drawing.Point(291, 36);
            this.numericUpDownCutToMinutes.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numericUpDownCutToMinutes.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownCutToMinutes.Name = "numericUpDownCutToMinutes";
            this.numericUpDownCutToMinutes.Size = new System.Drawing.Size(40, 23);
            this.numericUpDownCutToMinutes.TabIndex = 21;
            this.numericUpDownCutToMinutes.TabStop = false;
            this.numericUpDownCutToMinutes.ThousandsSeparator = false;
            this.numericUpDownCutToMinutes.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // numericUpDownCutToHours
            // 
            this.numericUpDownCutToHours.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownCutToHours.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownCutToHours.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownCutToHours.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownCutToHours.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownCutToHours.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownCutToHours.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownCutToHours.DecimalPlaces = 0;
            this.numericUpDownCutToHours.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownCutToHours.Location = new System.Drawing.Point(255, 36);
            this.numericUpDownCutToHours.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDownCutToHours.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownCutToHours.Name = "numericUpDownCutToHours";
            this.numericUpDownCutToHours.Size = new System.Drawing.Size(40, 23);
            this.numericUpDownCutToHours.TabIndex = 20;
            this.numericUpDownCutToHours.TabStop = false;
            this.numericUpDownCutToHours.ThousandsSeparator = false;
            this.numericUpDownCutToHours.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // numericUpDownCutFromSeconds
            // 
            this.numericUpDownCutFromSeconds.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownCutFromSeconds.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownCutFromSeconds.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownCutFromSeconds.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownCutFromSeconds.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownCutFromSeconds.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownCutFromSeconds.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownCutFromSeconds.DecimalPlaces = 0;
            this.numericUpDownCutFromSeconds.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownCutFromSeconds.Location = new System.Drawing.Point(169, 36);
            this.numericUpDownCutFromSeconds.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numericUpDownCutFromSeconds.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownCutFromSeconds.Name = "numericUpDownCutFromSeconds";
            this.numericUpDownCutFromSeconds.Size = new System.Drawing.Size(40, 23);
            this.numericUpDownCutFromSeconds.TabIndex = 19;
            this.numericUpDownCutFromSeconds.TabStop = false;
            this.numericUpDownCutFromSeconds.ThousandsSeparator = false;
            this.numericUpDownCutFromSeconds.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // numericUpDownCutFromMinutes
            // 
            this.numericUpDownCutFromMinutes.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownCutFromMinutes.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownCutFromMinutes.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownCutFromMinutes.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownCutFromMinutes.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownCutFromMinutes.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownCutFromMinutes.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownCutFromMinutes.DecimalPlaces = 0;
            this.numericUpDownCutFromMinutes.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownCutFromMinutes.Location = new System.Drawing.Point(133, 36);
            this.numericUpDownCutFromMinutes.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numericUpDownCutFromMinutes.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownCutFromMinutes.Name = "numericUpDownCutFromMinutes";
            this.numericUpDownCutFromMinutes.Size = new System.Drawing.Size(40, 23);
            this.numericUpDownCutFromMinutes.TabIndex = 18;
            this.numericUpDownCutFromMinutes.TabStop = false;
            this.numericUpDownCutFromMinutes.ThousandsSeparator = false;
            this.numericUpDownCutFromMinutes.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // numericUpDownCutFromHours
            // 
            this.numericUpDownCutFromHours.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownCutFromHours.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownCutFromHours.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownCutFromHours.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownCutFromHours.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownCutFromHours.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownCutFromHours.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownCutFromHours.DecimalPlaces = 0;
            this.numericUpDownCutFromHours.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownCutFromHours.Location = new System.Drawing.Point(97, 36);
            this.numericUpDownCutFromHours.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDownCutFromHours.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownCutFromHours.Name = "numericUpDownCutFromHours";
            this.numericUpDownCutFromHours.Size = new System.Drawing.Size(40, 23);
            this.numericUpDownCutFromHours.TabIndex = 17;
            this.numericUpDownCutFromHours.TabStop = false;
            this.numericUpDownCutFromHours.ThousandsSeparator = false;
            this.numericUpDownCutFromHours.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // labelCutTo
            // 
            this.labelCutTo.AutoSize = true;
            this.labelCutTo.Location = new System.Drawing.Point(255, 15);
            this.labelCutTo.Name = "labelCutTo";
            this.labelCutTo.Size = new System.Drawing.Size(20, 13);
            this.labelCutTo.TabIndex = 16;
            this.labelCutTo.Text = "To";
            // 
            // labelCutFrom
            // 
            this.labelCutFrom.AutoSize = true;
            this.labelCutFrom.Location = new System.Drawing.Point(94, 15);
            this.labelCutFrom.Name = "labelCutFrom";
            this.labelCutFrom.Size = new System.Drawing.Size(30, 13);
            this.labelCutFrom.TabIndex = 14;
            this.labelCutFrom.Text = "From";
            // 
            // labelVideoBitrate
            // 
            this.labelVideoBitrate.AutoSize = true;
            this.labelVideoBitrate.Location = new System.Drawing.Point(193, 352);
            this.labelVideoBitrate.Name = "labelVideoBitrate";
            this.labelVideoBitrate.Size = new System.Drawing.Size(86, 13);
            this.labelVideoBitrate.TabIndex = 14;
            this.labelVideoBitrate.Text = "labelVideoBitrate";
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(5, 527);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(25, 13);
            this.labelInfo.TabIndex = 1;
            this.labelInfo.Text = "Info";
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
            this.comboBoxSubtitleFont.DropDownWidth = 250;
            this.comboBoxSubtitleFont.FormattingEnabled = true;
            this.comboBoxSubtitleFont.Location = new System.Drawing.Point(120, 87);
            this.comboBoxSubtitleFont.MaxLength = 32767;
            this.comboBoxSubtitleFont.Name = "comboBoxSubtitleFont";
            this.comboBoxSubtitleFont.SelectedIndex = -1;
            this.comboBoxSubtitleFont.SelectedItem = null;
            this.comboBoxSubtitleFont.SelectedText = "";
            this.comboBoxSubtitleFont.Size = new System.Drawing.Size(250, 21);
            this.comboBoxSubtitleFont.TabIndex = 70;
            this.comboBoxSubtitleFont.UsePopupWindow = false;
            this.comboBoxSubtitleFont.SelectedIndexChanged += new System.EventHandler(this.comboBoxSubtitleFont_SelectedValueChanged);
            // 
            // labelSubtitleFont
            // 
            this.labelSubtitleFont.AutoSize = true;
            this.labelSubtitleFont.Location = new System.Drawing.Point(19, 90);
            this.labelSubtitleFont.Name = "labelSubtitleFont";
            this.labelSubtitleFont.Size = new System.Drawing.Size(63, 13);
            this.labelSubtitleFont.TabIndex = 7;
            this.labelSubtitleFont.Text = "Subtitle font";
            // 
            // labelCrfHint
            // 
            this.labelCrfHint.AutoSize = true;
            this.labelCrfHint.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCrfHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelCrfHint.Location = new System.Drawing.Point(220, 106);
            this.labelCrfHint.Name = "labelCrfHint";
            this.labelCrfHint.Size = new System.Drawing.Size(22, 12);
            this.labelCrfHint.TabIndex = 32;
            this.labelCrfHint.Text = "Hint";
            // 
            // labelResolution
            // 
            this.labelResolution.AutoSize = true;
            this.labelResolution.Location = new System.Drawing.Point(10, 19);
            this.labelResolution.Name = "labelResolution";
            this.labelResolution.Size = new System.Drawing.Size(57, 13);
            this.labelResolution.TabIndex = 0;
            this.labelResolution.Text = "Resolution";
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
            this.numericUpDownWidth.Location = new System.Drawing.Point(96, 17);
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
            this.numericUpDownWidth.TabIndex = 1;
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
            this.numericUpDownHeight.Location = new System.Drawing.Point(184, 17);
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
            this.numericUpDownHeight.TabIndex = 2;
            this.numericUpDownHeight.TabStop = false;
            this.numericUpDownHeight.ThousandsSeparator = false;
            this.numericUpDownHeight.Value = new decimal(new int[] {
            720,
            0,
            0,
            0});
            this.numericUpDownHeight.ValueChanged += new System.EventHandler(this.numericUpDownHeight_ValueChanged);
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(166, 19);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(12, 13);
            this.labelX.TabIndex = 31;
            this.labelX.Text = "x";
            // 
            // labelPreset
            // 
            this.labelPreset.AutoSize = true;
            this.labelPreset.Location = new System.Drawing.Point(10, 78);
            this.labelPreset.Name = "labelPreset";
            this.labelPreset.Size = new System.Drawing.Size(37, 13);
            this.labelPreset.TabIndex = 5;
            this.labelPreset.Text = "Preset";
            // 
            // comboBoxTune
            // 
            this.comboBoxTune.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxTune.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxTune.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxTune.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxTune.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxTune.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxTune.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxTune.DropDownHeight = 400;
            this.comboBoxTune.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTune.DropDownWidth = 121;
            this.comboBoxTune.FormattingEnabled = true;
            this.comboBoxTune.Items.AddRange(new string[] {
            "",
            "film",
            "animation",
            "grain"});
            this.comboBoxTune.Location = new System.Drawing.Point(95, 130);
            this.comboBoxTune.MaxLength = 32767;
            this.comboBoxTune.Name = "comboBoxTune";
            this.comboBoxTune.SelectedIndex = 0;
            this.comboBoxTune.SelectedItem = "";
            this.comboBoxTune.SelectedText = "";
            this.comboBoxTune.Size = new System.Drawing.Size(121, 21);
            this.comboBoxTune.TabIndex = 10;
            this.comboBoxTune.UsePopupWindow = false;
            // 
            // comboBoxPreset
            // 
            this.comboBoxPreset.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxPreset.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxPreset.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxPreset.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxPreset.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxPreset.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxPreset.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxPreset.DropDownHeight = 400;
            this.comboBoxPreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPreset.DropDownWidth = 121;
            this.comboBoxPreset.FormattingEnabled = true;
            this.comboBoxPreset.Items.AddRange(new string[] {
            "ultrafast",
            "superfast",
            "veryfast",
            "faster",
            "fast",
            "medium",
            "slow",
            "slower",
            "veryslow "});
            this.comboBoxPreset.Location = new System.Drawing.Point(95, 76);
            this.comboBoxPreset.MaxLength = 32767;
            this.comboBoxPreset.Name = "comboBoxPreset";
            this.comboBoxPreset.SelectedIndex = -1;
            this.comboBoxPreset.SelectedItem = null;
            this.comboBoxPreset.SelectedText = "";
            this.comboBoxPreset.Size = new System.Drawing.Size(121, 21);
            this.comboBoxPreset.TabIndex = 6;
            this.comboBoxPreset.UsePopupWindow = false;
            // 
            // labelTune
            // 
            this.labelTune.AutoSize = true;
            this.labelTune.Location = new System.Drawing.Point(10, 132);
            this.labelTune.Name = "labelTune";
            this.labelTune.Size = new System.Drawing.Size(32, 13);
            this.labelTune.TabIndex = 9;
            this.labelTune.Text = "Tune";
            // 
            // labelCRF
            // 
            this.labelCRF.AutoSize = true;
            this.labelCRF.Location = new System.Drawing.Point(10, 105);
            this.labelCRF.Name = "labelCRF";
            this.labelCRF.Size = new System.Drawing.Size(28, 13);
            this.labelCRF.TabIndex = 7;
            this.labelCRF.Text = "CRF";
            // 
            // comboBoxVideoEncoding
            // 
            this.comboBoxVideoEncoding.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxVideoEncoding.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxVideoEncoding.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxVideoEncoding.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxVideoEncoding.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxVideoEncoding.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxVideoEncoding.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxVideoEncoding.DropDownHeight = 400;
            this.comboBoxVideoEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVideoEncoding.DropDownWidth = 121;
            this.comboBoxVideoEncoding.FormattingEnabled = true;
            this.comboBoxVideoEncoding.Items.AddRange(new string[] {
            "libx264",
            "libx265",
            "libvpx-vp9",
            "h264_nvenc",
            "hevc_nvenc",
            "h264_amf",
            "hevc_amf",
            "prores_ks"});
            this.comboBoxVideoEncoding.Location = new System.Drawing.Point(95, 49);
            this.comboBoxVideoEncoding.MaxLength = 32767;
            this.comboBoxVideoEncoding.Name = "comboBoxVideoEncoding";
            this.comboBoxVideoEncoding.SelectedIndex = -1;
            this.comboBoxVideoEncoding.SelectedItem = null;
            this.comboBoxVideoEncoding.SelectedText = "";
            this.comboBoxVideoEncoding.Size = new System.Drawing.Size(121, 21);
            this.comboBoxVideoEncoding.TabIndex = 4;
            this.comboBoxVideoEncoding.UsePopupWindow = false;
            this.comboBoxVideoEncoding.SelectedIndexChanged += new System.EventHandler(this.comboBoxVideoEncoding_SelectedIndexChanged);
            // 
            // comboBoxCrf
            // 
            this.comboBoxCrf.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxCrf.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxCrf.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxCrf.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxCrf.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxCrf.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxCrf.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxCrf.DropDownHeight = 400;
            this.comboBoxCrf.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCrf.DropDownWidth = 121;
            this.comboBoxCrf.FormattingEnabled = true;
            this.comboBoxCrf.Items.AddRange(new string[] {
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
            "28"});
            this.comboBoxCrf.Location = new System.Drawing.Point(95, 103);
            this.comboBoxCrf.MaxLength = 32767;
            this.comboBoxCrf.Name = "comboBoxCrf";
            this.comboBoxCrf.SelectedIndex = -1;
            this.comboBoxCrf.SelectedItem = null;
            this.comboBoxCrf.SelectedText = "";
            this.comboBoxCrf.Size = new System.Drawing.Size(121, 21);
            this.comboBoxCrf.TabIndex = 8;
            this.comboBoxCrf.UsePopupWindow = false;
            // 
            // labelVideoEncoding
            // 
            this.labelVideoEncoding.AutoSize = true;
            this.labelVideoEncoding.Location = new System.Drawing.Point(10, 51);
            this.labelVideoEncoding.Name = "labelVideoEncoding";
            this.labelVideoEncoding.Size = new System.Drawing.Size(55, 13);
            this.labelVideoEncoding.TabIndex = 3;
            this.labelVideoEncoding.Text = "Video enc";
            // 
            // labelAudioEnc
            // 
            this.labelAudioEnc.AutoSize = true;
            this.labelAudioEnc.Location = new System.Drawing.Point(14, 31);
            this.labelAudioEnc.Name = "labelAudioEnc";
            this.labelAudioEnc.Size = new System.Drawing.Size(52, 13);
            this.labelAudioEnc.TabIndex = 0;
            this.labelAudioEnc.Text = "Encoding";
            // 
            // comboBoxAudioBitRate
            // 
            this.comboBoxAudioBitRate.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxAudioBitRate.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxAudioBitRate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxAudioBitRate.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxAudioBitRate.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxAudioBitRate.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxAudioBitRate.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxAudioBitRate.DropDownHeight = 400;
            this.comboBoxAudioBitRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAudioBitRate.DropDownWidth = 121;
            this.comboBoxAudioBitRate.FormattingEnabled = true;
            this.comboBoxAudioBitRate.Items.AddRange(new string[] {
            "64k",
            "128k",
            "160k",
            "196k",
            "320k"});
            this.comboBoxAudioBitRate.Location = new System.Drawing.Point(91, 110);
            this.comboBoxAudioBitRate.MaxLength = 32767;
            this.comboBoxAudioBitRate.Name = "comboBoxAudioBitRate";
            this.comboBoxAudioBitRate.SelectedIndex = -1;
            this.comboBoxAudioBitRate.SelectedItem = null;
            this.comboBoxAudioBitRate.SelectedText = "";
            this.comboBoxAudioBitRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAudioBitRate.TabIndex = 5;
            this.comboBoxAudioBitRate.UsePopupWindow = false;
            this.comboBoxAudioBitRate.SelectedIndexChanged += new System.EventHandler(this.comboBoxAudioBitRate_SelectedValueChanged);
            // 
            // comboBoxAudioEnc
            // 
            this.comboBoxAudioEnc.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxAudioEnc.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxAudioEnc.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxAudioEnc.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxAudioEnc.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxAudioEnc.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxAudioEnc.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxAudioEnc.DropDownHeight = 400;
            this.comboBoxAudioEnc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAudioEnc.DropDownWidth = 121;
            this.comboBoxAudioEnc.FormattingEnabled = true;
            this.comboBoxAudioEnc.Items.AddRange(new string[] {
            "copy",
            "aac"});
            this.comboBoxAudioEnc.Location = new System.Drawing.Point(91, 29);
            this.comboBoxAudioEnc.MaxLength = 32767;
            this.comboBoxAudioEnc.Name = "comboBoxAudioEnc";
            this.comboBoxAudioEnc.SelectedIndex = -1;
            this.comboBoxAudioEnc.SelectedItem = null;
            this.comboBoxAudioEnc.SelectedText = "";
            this.comboBoxAudioEnc.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAudioEnc.TabIndex = 1;
            this.comboBoxAudioEnc.UsePopupWindow = false;
            this.comboBoxAudioEnc.SelectedIndexChanged += new System.EventHandler(this.comboBoxAudioEnc_SelectedIndexChanged);
            // 
            // labelAudioBitRate
            // 
            this.labelAudioBitRate.AutoSize = true;
            this.labelAudioBitRate.Location = new System.Drawing.Point(14, 112);
            this.labelAudioBitRate.Name = "labelAudioBitRate";
            this.labelAudioBitRate.Size = new System.Drawing.Size(40, 13);
            this.labelAudioBitRate.TabIndex = 4;
            this.labelAudioBitRate.Text = "Bit rate";
            // 
            // labelAudioSampleRate
            // 
            this.labelAudioSampleRate.AutoSize = true;
            this.labelAudioSampleRate.Location = new System.Drawing.Point(14, 85);
            this.labelAudioSampleRate.Name = "labelAudioSampleRate";
            this.labelAudioSampleRate.Size = new System.Drawing.Size(63, 13);
            this.labelAudioSampleRate.TabIndex = 44;
            this.labelAudioSampleRate.Text = "Sample rate";
            // 
            // comboBoxAudioSampleRate
            // 
            this.comboBoxAudioSampleRate.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxAudioSampleRate.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxAudioSampleRate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxAudioSampleRate.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxAudioSampleRate.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxAudioSampleRate.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxAudioSampleRate.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxAudioSampleRate.DropDownHeight = 400;
            this.comboBoxAudioSampleRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAudioSampleRate.DropDownWidth = 121;
            this.comboBoxAudioSampleRate.FormattingEnabled = true;
            this.comboBoxAudioSampleRate.Items.AddRange(new string[] {
            "44100 Hz",
            "48000 Hz",
            "88200 Hz",
            "96000 Hz",
            "192000 Hz"});
            this.comboBoxAudioSampleRate.Location = new System.Drawing.Point(91, 83);
            this.comboBoxAudioSampleRate.MaxLength = 32767;
            this.comboBoxAudioSampleRate.Name = "comboBoxAudioSampleRate";
            this.comboBoxAudioSampleRate.SelectedIndex = -1;
            this.comboBoxAudioSampleRate.SelectedItem = null;
            this.comboBoxAudioSampleRate.SelectedText = "";
            this.comboBoxAudioSampleRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAudioSampleRate.TabIndex = 3;
            this.comboBoxAudioSampleRate.UsePopupWindow = false;
            // 
            // numericUpDownTargetFileSize
            // 
            this.numericUpDownTargetFileSize.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownTargetFileSize.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownTargetFileSize.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownTargetFileSize.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownTargetFileSize.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownTargetFileSize.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownTargetFileSize.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownTargetFileSize.DecimalPlaces = 0;
            this.numericUpDownTargetFileSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTargetFileSize.Location = new System.Drawing.Point(120, 350);
            this.numericUpDownTargetFileSize.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDownTargetFileSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTargetFileSize.Name = "numericUpDownTargetFileSize";
            this.numericUpDownTargetFileSize.Size = new System.Drawing.Size(64, 23);
            this.numericUpDownTargetFileSize.TabIndex = 101;
            this.numericUpDownTargetFileSize.TabStop = false;
            this.numericUpDownTargetFileSize.ThousandsSeparator = false;
            this.numericUpDownTargetFileSize.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownTargetFileSize.ValueChanged += new System.EventHandler(this.numericUpDownTargetFileSize_ValueChanged);
            // 
            // labelFileSize
            // 
            this.labelFileSize.AutoSize = true;
            this.labelFileSize.Location = new System.Drawing.Point(35, 352);
            this.labelFileSize.Name = "labelFileSize";
            this.labelFileSize.Size = new System.Drawing.Size(74, 13);
            this.labelFileSize.TabIndex = 12;
            this.labelFileSize.Text = "File size in MB";
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(19, 398);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(52, 13);
            this.labelFileName.TabIndex = 0;
            this.labelFileName.Text = "File name";
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
            this.numericUpDownFontSize.Location = new System.Drawing.Point(120, 25);
            this.numericUpDownFontSize.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownFontSize.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericUpDownFontSize.Name = "numericUpDownFontSize";
            this.numericUpDownFontSize.Size = new System.Drawing.Size(54, 20);
            this.numericUpDownFontSize.TabIndex = 10;
            this.numericUpDownFontSize.TabStop = false;
            this.numericUpDownFontSize.ThousandsSeparator = false;
            this.numericUpDownFontSize.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownFontSize.ValueChanged += new System.EventHandler(this.numericUpDownFontSize_ValueChanged);
            // 
            // labelFontSize
            // 
            this.labelFontSize.AutoSize = true;
            this.labelFontSize.Location = new System.Drawing.Point(19, 27);
            this.labelFontSize.Name = "labelFontSize";
            this.labelFontSize.Size = new System.Drawing.Size(49, 13);
            this.labelFontSize.TabIndex = 0;
            this.labelFontSize.Text = "Font size";
            // 
            // labelPreviewPleaseWait
            // 
            this.labelPreviewPleaseWait.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelPreviewPleaseWait.AutoSize = true;
            this.labelPreviewPleaseWait.Location = new System.Drawing.Point(668, 570);
            this.labelPreviewPleaseWait.Name = "labelPreviewPleaseWait";
            this.labelPreviewPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPreviewPleaseWait.TabIndex = 48;
            this.labelPreviewPleaseWait.Text = "Please wait...";
            // 
            // labelPass
            // 
            this.labelPass.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPass.AutoSize = true;
            this.labelPass.Location = new System.Drawing.Point(564, 599);
            this.labelPass.Name = "labelPass";
            this.labelPass.Size = new System.Drawing.Size(51, 13);
            this.labelPass.TabIndex = 47;
            this.labelPass.Text = "Pass one";
            // 
            // labelProgress
            // 
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(12, 629);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(88, 13);
            this.labelProgress.TabIndex = 29;
            this.labelProgress.Text = "Remaining time...";
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(12, 599);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPleaseWait.TabIndex = 25;
            this.labelPleaseWait.Text = "Please wait...";
            // 
            // textBoxLog
            // 
            this.textBoxLog.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxLog.Location = new System.Drawing.Point(12, 13);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.Size = new System.Drawing.Size(188, 26);
            this.textBoxLog.TabIndex = 31;
            // 
            // GenerateVideoWithHardSubs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(885, 650);
            this.Controls.Add(this.groupBoxSettings);
            this.Controls.Add(this.buttonMode);
            this.Controls.Add(this.labelPreviewPleaseWait);
            this.Controls.Add(this.labelPass);
            this.Controls.Add(this.linkLabelHelp);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.buttonPreview);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.textBoxLog);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(890, 680);
            this.Name = "GenerateVideoWithHardSubs";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GenerateVideoWithHardSubs";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GenerateVideoWithHardSubs_FormClosing);
            this.Shown += new System.EventHandler(this.GenerateVideoWithHardSubs_Shown);
            this.ResizeEnd += new System.EventHandler(this.GenerateVideoWithHardSubs_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenerateVideoWithHardSubs_KeyDown);
            this.Resize += new System.EventHandler(this.GenerateVideoWithHardSubs_Resize);
            this.contextMenuStripGenerate.ResumeLayout(false);
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            this.groupBoxCut.ResumeLayout(false);
            this.groupBoxCut.PerformLayout();
            this.groupBoxVideo.ResumeLayout(false);
            this.groupBoxVideo.PerformLayout();
            this.groupBoxAudio.ResumeLayout(false);
            this.groupBoxAudio.PerformLayout();
            this.contextMenuStripBatch.ResumeLayout(false);
            this.contextMenuStripRes.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.Button buttonCancel;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelPleaseWait;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownFontSize;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelFontSize;
        private System.Windows.Forms.Timer timer1;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelProgress;
        private System.Windows.Forms.GroupBox groupBoxSettings;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelX;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelResolution;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownHeight;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownWidth;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelFileName;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxPreset;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelPreset;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxCrf;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelCRF;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxVideoEncoding;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelVideoEncoding;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxAudioEnc;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelAudioEnc;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxAudioSampleRate;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelAudioSampleRate;
        private System.Windows.Forms.CheckBox checkBoxMakeStereo;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxLog;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelInfo;
        private System.Windows.Forms.LinkLabel linkLabelHelp;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxTune;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelTune;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownTargetFileSize;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelFileSize;
        private System.Windows.Forms.CheckBox checkBoxTargetFileSize;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelPass;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxAudioBitRate;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelAudioBitRate;
        private System.Windows.Forms.GroupBox groupBoxAudio;
        private System.Windows.Forms.GroupBox groupBoxVideo;
        private System.Windows.Forms.Button buttonPreview;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSubtitleFont;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelSubtitleFont;
        private System.Windows.Forms.CheckBox checkBoxRightToLeft;
        private System.Windows.Forms.CheckBox checkBoxAlignRight;
        private System.Windows.Forms.CheckBox checkBoxBox;
        private System.Windows.Forms.Button buttonVideoChooseStandardRes;
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
        private Nikse.SubtitleEdit.Controls.NikseLabel labelVideoBitrate;
        private System.Windows.Forms.GroupBox groupBoxCut;
        private System.Windows.Forms.CheckBox checkBoxCut;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelCutTo;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelCutFrom;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownCutToSeconds;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownCutToMinutes;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownCutToHours;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownCutFromSeconds;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownCutFromMinutes;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownCutFromHours;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelCrfHint;
        private System.Windows.Forms.Button buttonCutTo;
        private System.Windows.Forms.Button buttonCutFrom;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripGenerate;
        private System.Windows.Forms.ToolStripMenuItem promptParameterBeforeGenerateToolStripMenuItem;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelPreviewPleaseWait;
        private Controls.VideoPlayerContainer videoPlayerContainer1;
        private System.Windows.Forms.Panel panelOutlineColor;
        private System.Windows.Forms.Button buttonOutlineColor;
        private System.Windows.Forms.Panel panelForeColor;
        private System.Windows.Forms.Button buttonForeColor;
        private System.Windows.Forms.ListView listViewBatch;
        private System.Windows.Forms.ColumnHeader columnHeaderVideoFile;
        private System.Windows.Forms.ColumnHeader columnHeaderSubtitleFile;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripBatch;
        private System.Windows.Forms.ToolStripMenuItem addFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pickSubtitleFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.Button buttonMode;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonRemoveFile;
        private System.Windows.Forms.Button buttonAddFile;
        private System.Windows.Forms.ListView listViewAudioTracks;
        private System.Windows.Forms.ColumnHeader columnHeaderAudioTrack;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem removeSubtitleFileToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader columnHeaderResolution;
        private System.Windows.Forms.ToolStripMenuItem useSourceResolutionToolStripMenuItem;
        private System.Windows.Forms.Button buttonOutputFileSettings;
        private System.Windows.Forms.Label nikseLabelOutputFileFolder;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem youTubeShortsTikTok10801920ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem youTubeShortsTikTokAspectRatio9167201280ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aAspectRatio916540960ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bAspectRatio916360540ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aAspectRatio916270480ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bAspectRatio916180270ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.CheckBox checkBoxFontBold;
        private Controls.NikseUpDown numericUpDownOutline;
        private Controls.NikseLabel nikseLabelOutline;
    }
}