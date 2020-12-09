
namespace Nikse.SubtitleEdit.Forms.BinaryEdit
{
    partial class BinEdit
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
            Nikse.SubtitleEdit.Core.Common.TimeCode timeCode2 = new Nikse.SubtitleEdit.Core.Common.TimeCode();
            this.groupBoxCurrent = new System.Windows.Forms.GroupBox();
            this.labelCurrentSize = new System.Windows.Forms.Label();
            this.buttonExportImage = new System.Windows.Forms.Button();
            this.buttonImportImage = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.labelEndTime = new System.Windows.Forms.Label();
            this.labelStart = new System.Windows.Forms.Label();
            this.numericUpDownY = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownX = new System.Windows.Forms.NumericUpDown();
            this.checkBoxIsForced = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labelFrameRate = new System.Windows.Forms.Label();
            this.comboBoxFrameRate = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownScreenHeight = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownScreenWidth = new System.Windows.Forms.NumericUpDown();
            this.contextMenuStripListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.insertSubtitleAfterThisLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adjustAllTimesForSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.importTimeCodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.videoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.synchronizationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adjustAllTimesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeFrameRateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeSpeedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBoxMovableImage = new System.Windows.Forms.PictureBox();
            this.contextMenuStripMovableImage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.centerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBoxScreen = new System.Windows.Forms.PictureBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timerSubtitleOnVideo = new System.Windows.Forms.Timer(this.components);
            this.buttonSetText = new System.Windows.Forms.Button();
            this.videoPlayerContainer1 = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            this.timeUpDownEndTime = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.timeUpDownStartTime = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.subtitleListView1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.groupBoxCurrent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenWidth)).BeginInit();
            this.contextMenuStripListView.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMovableImage)).BeginInit();
            this.contextMenuStripMovableImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxScreen)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxCurrent
            // 
            this.groupBoxCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxCurrent.Controls.Add(this.buttonSetText);
            this.groupBoxCurrent.Controls.Add(this.labelCurrentSize);
            this.groupBoxCurrent.Controls.Add(this.buttonExportImage);
            this.groupBoxCurrent.Controls.Add(this.buttonImportImage);
            this.groupBoxCurrent.Controls.Add(this.label2);
            this.groupBoxCurrent.Controls.Add(this.labelEndTime);
            this.groupBoxCurrent.Controls.Add(this.labelStart);
            this.groupBoxCurrent.Controls.Add(this.numericUpDownY);
            this.groupBoxCurrent.Controls.Add(this.numericUpDownX);
            this.groupBoxCurrent.Controls.Add(this.checkBoxIsForced);
            this.groupBoxCurrent.Controls.Add(this.timeUpDownEndTime);
            this.groupBoxCurrent.Controls.Add(this.timeUpDownStartTime);
            this.groupBoxCurrent.Controls.Add(this.label1);
            this.groupBoxCurrent.Location = new System.Drawing.Point(12, 405);
            this.groupBoxCurrent.Name = "groupBoxCurrent";
            this.groupBoxCurrent.Size = new System.Drawing.Size(493, 109);
            this.groupBoxCurrent.TabIndex = 2;
            this.groupBoxCurrent.TabStop = false;
            this.groupBoxCurrent.Text = "Current";
            // 
            // labelCurrentSize
            // 
            this.labelCurrentSize.AutoSize = true;
            this.labelCurrentSize.Location = new System.Drawing.Point(192, 41);
            this.labelCurrentSize.Name = "labelCurrentSize";
            this.labelCurrentSize.Size = new System.Drawing.Size(30, 13);
            this.labelCurrentSize.TabIndex = 8;
            this.labelCurrentSize.Text = "Size:";
            // 
            // buttonExportImage
            // 
            this.buttonExportImage.Location = new System.Drawing.Point(376, 12);
            this.buttonExportImage.Name = "buttonExportImage";
            this.buttonExportImage.Size = new System.Drawing.Size(111, 23);
            this.buttonExportImage.TabIndex = 9;
            this.buttonExportImage.Text = "Export image...";
            this.buttonExportImage.UseVisualStyleBackColor = true;
            this.buttonExportImage.Click += new System.EventHandler(this.buttonExportImage_Click);
            // 
            // buttonImportImage
            // 
            this.buttonImportImage.Location = new System.Drawing.Point(376, 41);
            this.buttonImportImage.Name = "buttonImportImage";
            this.buttonImportImage.Size = new System.Drawing.Size(111, 23);
            this.buttonImportImage.TabIndex = 10;
            this.buttonImportImage.Text = "Import image...";
            this.buttonImportImage.UseVisualStyleBackColor = true;
            this.buttonImportImage.Click += new System.EventHandler(this.buttonImportImage_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(298, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "x";
            // 
            // labelEndTime
            // 
            this.labelEndTime.AutoSize = true;
            this.labelEndTime.Location = new System.Drawing.Point(16, 50);
            this.labelEndTime.Name = "labelEndTime";
            this.labelEndTime.Size = new System.Drawing.Size(48, 13);
            this.labelEndTime.TabIndex = 2;
            this.labelEndTime.Text = "End time";
            // 
            // labelStart
            // 
            this.labelStart.AutoSize = true;
            this.labelStart.Location = new System.Drawing.Point(16, 22);
            this.labelStart.Name = "labelStart";
            this.labelStart.Size = new System.Drawing.Size(51, 13);
            this.labelStart.TabIndex = 0;
            this.labelStart.Text = "Start time";
            // 
            // numericUpDownY
            // 
            this.numericUpDownY.Location = new System.Drawing.Point(314, 15);
            this.numericUpDownY.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.numericUpDownY.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this.numericUpDownY.Name = "numericUpDownY";
            this.numericUpDownY.Size = new System.Drawing.Size(45, 20);
            this.numericUpDownY.TabIndex = 7;
            this.numericUpDownY.ValueChanged += new System.EventHandler(this.numericUpDownY_ValueChanged);
            // 
            // numericUpDownX
            // 
            this.numericUpDownX.Location = new System.Drawing.Point(251, 15);
            this.numericUpDownX.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.numericUpDownX.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this.numericUpDownX.Name = "numericUpDownX";
            this.numericUpDownX.Size = new System.Drawing.Size(45, 20);
            this.numericUpDownX.TabIndex = 5;
            this.numericUpDownX.ValueChanged += new System.EventHandler(this.numericUpDownX_ValueChanged);
            // 
            // checkBoxIsForced
            // 
            this.checkBoxIsForced.AutoSize = true;
            this.checkBoxIsForced.Location = new System.Drawing.Point(19, 79);
            this.checkBoxIsForced.Name = "checkBoxIsForced";
            this.checkBoxIsForced.Size = new System.Drawing.Size(59, 17);
            this.checkBoxIsForced.TabIndex = 4;
            this.checkBoxIsForced.Text = "Forced";
            this.checkBoxIsForced.UseVisualStyleBackColor = true;
            this.checkBoxIsForced.CheckedChanged += new System.EventHandler(this.checkBoxIsForced_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(192, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Position";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.labelFrameRate);
            this.groupBox2.Controls.Add(this.comboBoxFrameRate);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.numericUpDownScreenHeight);
            this.groupBox2.Controls.Add(this.numericUpDownScreenWidth);
            this.groupBox2.Location = new System.Drawing.Point(13, 520);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(493, 51);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Global settings";
            // 
            // labelFrameRate
            // 
            this.labelFrameRate.AutoSize = true;
            this.labelFrameRate.Location = new System.Drawing.Point(259, 23);
            this.labelFrameRate.Name = "labelFrameRate";
            this.labelFrameRate.Size = new System.Drawing.Size(57, 13);
            this.labelFrameRate.TabIndex = 4;
            this.labelFrameRate.Text = "Frame rate";
            // 
            // comboBoxFrameRate
            // 
            this.comboBoxFrameRate.DropDownWidth = 200;
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Location = new System.Drawing.Point(322, 20);
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(102, 21);
            this.comboBoxFrameRate.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(120, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(12, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "x";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Video size";
            // 
            // numericUpDownScreenHeight
            // 
            this.numericUpDownScreenHeight.Location = new System.Drawing.Point(137, 21);
            this.numericUpDownScreenHeight.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.numericUpDownScreenHeight.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this.numericUpDownScreenHeight.Name = "numericUpDownScreenHeight";
            this.numericUpDownScreenHeight.Size = new System.Drawing.Size(45, 20);
            this.numericUpDownScreenHeight.TabIndex = 3;
            // 
            // numericUpDownScreenWidth
            // 
            this.numericUpDownScreenWidth.Location = new System.Drawing.Point(71, 21);
            this.numericUpDownScreenWidth.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.numericUpDownScreenWidth.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this.numericUpDownScreenWidth.Name = "numericUpDownScreenWidth";
            this.numericUpDownScreenWidth.Size = new System.Drawing.Size(45, 20);
            this.numericUpDownScreenWidth.TabIndex = 1;
            // 
            // contextMenuStripListView
            // 
            this.contextMenuStripListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.insertToolStripMenuItem,
            this.insertAfterToolStripMenuItem,
            this.toolStripSeparator3,
            this.insertSubtitleAfterThisLineToolStripMenuItem,
            this.adjustAllTimesForSelectedLinesToolStripMenuItem});
            this.contextMenuStripListView.Name = "contextMenuStripListView";
            this.contextMenuStripListView.Size = new System.Drawing.Size(256, 120);
            this.contextMenuStripListView.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripListView_Opening);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // insertToolStripMenuItem
            // 
            this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            this.insertToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.insertToolStripMenuItem.Text = "Insert before";
            this.insertToolStripMenuItem.Click += new System.EventHandler(this.insertToolStripMenuItem_Click);
            // 
            // insertAfterToolStripMenuItem
            // 
            this.insertAfterToolStripMenuItem.Name = "insertAfterToolStripMenuItem";
            this.insertAfterToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.insertAfterToolStripMenuItem.Text = "Insert after";
            this.insertAfterToolStripMenuItem.Click += new System.EventHandler(this.insertAfterToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(252, 6);
            // 
            // insertSubtitleAfterThisLineToolStripMenuItem
            // 
            this.insertSubtitleAfterThisLineToolStripMenuItem.Name = "insertSubtitleAfterThisLineToolStripMenuItem";
            this.insertSubtitleAfterThisLineToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.insertSubtitleAfterThisLineToolStripMenuItem.Text = "Insert subtitle after this line...";
            this.insertSubtitleAfterThisLineToolStripMenuItem.Click += new System.EventHandler(this.insertSubtitleAfterThisLineToolStripMenuItem_Click_1);
            // 
            // adjustAllTimesForSelectedLinesToolStripMenuItem
            // 
            this.adjustAllTimesForSelectedLinesToolStripMenuItem.Name = "adjustAllTimesForSelectedLinesToolStripMenuItem";
            this.adjustAllTimesForSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.adjustAllTimesForSelectedLinesToolStripMenuItem.Text = "Adjust all times for selected lines...";
            this.adjustAllTimesForSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.adjustAllTimesForSelectedLinesToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.videoToolStripMenuItem,
            this.synchronizationToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1161, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripMenuItem,
            this.saveFileAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.importTimeCodesToolStripMenuItem,
            this.toolStripSeparator2,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openFileToolStripMenuItem.Text = "Open file...";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // saveFileAsToolStripMenuItem
            // 
            this.saveFileAsToolStripMenuItem.Name = "saveFileAsToolStripMenuItem";
            this.saveFileAsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveFileAsToolStripMenuItem.Text = "Save file as...";
            this.saveFileAsToolStripMenuItem.Click += new System.EventHandler(this.saveFileAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // importTimeCodesToolStripMenuItem
            // 
            this.importTimeCodesToolStripMenuItem.Name = "importTimeCodesToolStripMenuItem";
            this.importTimeCodesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.importTimeCodesToolStripMenuItem.Text = "Import time codes...";
            this.importTimeCodesToolStripMenuItem.Click += new System.EventHandler(this.importTimeCodesToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // videoToolStripMenuItem
            // 
            this.videoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openVideoToolStripMenuItem,
            this.closeVideoToolStripMenuItem});
            this.videoToolStripMenuItem.Name = "videoToolStripMenuItem";
            this.videoToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
            this.videoToolStripMenuItem.Text = "Video";
            this.videoToolStripMenuItem.DropDownOpening += new System.EventHandler(this.videoToolStripMenuItem_DropDownOpening);
            // 
            // openVideoToolStripMenuItem
            // 
            this.openVideoToolStripMenuItem.Name = "openVideoToolStripMenuItem";
            this.openVideoToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.openVideoToolStripMenuItem.Text = "Open video...";
            this.openVideoToolStripMenuItem.Click += new System.EventHandler(this.openVideoToolStripMenuItem_Click);
            // 
            // closeVideoToolStripMenuItem
            // 
            this.closeVideoToolStripMenuItem.Name = "closeVideoToolStripMenuItem";
            this.closeVideoToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.closeVideoToolStripMenuItem.Text = "Close video";
            this.closeVideoToolStripMenuItem.Click += new System.EventHandler(this.closeVideoToolStripMenuItem_Click);
            // 
            // synchronizationToolStripMenuItem
            // 
            this.synchronizationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.adjustAllTimesToolStripMenuItem,
            this.changeFrameRateToolStripMenuItem,
            this.changeSpeedToolStripMenuItem});
            this.synchronizationToolStripMenuItem.Name = "synchronizationToolStripMenuItem";
            this.synchronizationToolStripMenuItem.Size = new System.Drawing.Size(104, 20);
            this.synchronizationToolStripMenuItem.Text = "Synchronization";
            // 
            // adjustAllTimesToolStripMenuItem
            // 
            this.adjustAllTimesToolStripMenuItem.Name = "adjustAllTimesToolStripMenuItem";
            this.adjustAllTimesToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.adjustAllTimesToolStripMenuItem.Text = "Adjust all times";
            this.adjustAllTimesToolStripMenuItem.Click += new System.EventHandler(this.adjustAllTimesToolStripMenuItem_Click);
            // 
            // changeFrameRateToolStripMenuItem
            // 
            this.changeFrameRateToolStripMenuItem.Name = "changeFrameRateToolStripMenuItem";
            this.changeFrameRateToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.changeFrameRateToolStripMenuItem.Text = "Change frame rate";
            this.changeFrameRateToolStripMenuItem.Click += new System.EventHandler(this.changeFrameRateToolStripMenuItem_Click);
            // 
            // changeSpeedToolStripMenuItem
            // 
            this.changeSpeedToolStripMenuItem.Name = "changeSpeedToolStripMenuItem";
            this.changeSpeedToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.changeSpeedToolStripMenuItem.Text = "Change speed";
            this.changeSpeedToolStripMenuItem.Click += new System.EventHandler(this.changeSpeedToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.videoPlayerContainer1);
            this.panel1.Controls.Add(this.pictureBoxMovableImage);
            this.panel1.Controls.Add(this.pictureBoxScreen);
            this.panel1.Location = new System.Drawing.Point(513, 28);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(638, 543);
            this.panel1.TabIndex = 4;
            // 
            // pictureBoxMovableImage
            // 
            this.pictureBoxMovableImage.ContextMenuStrip = this.contextMenuStripMovableImage;
            this.pictureBoxMovableImage.Location = new System.Drawing.Point(31, 21);
            this.pictureBoxMovableImage.Name = "pictureBoxMovableImage";
            this.pictureBoxMovableImage.Size = new System.Drawing.Size(100, 50);
            this.pictureBoxMovableImage.TabIndex = 15;
            this.pictureBoxMovableImage.TabStop = false;
            this.pictureBoxMovableImage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBoxMovableImage_MouseDown);
            this.pictureBoxMovableImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxMovableImage_MouseMove);
            // 
            // contextMenuStripMovableImage
            // 
            this.contextMenuStripMovableImage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.centerToolStripMenuItem});
            this.contextMenuStripMovableImage.Name = "contextMenuStripMovableImage";
            this.contextMenuStripMovableImage.Size = new System.Drawing.Size(175, 26);
            // 
            // centerToolStripMenuItem
            // 
            this.centerToolStripMenuItem.Name = "centerToolStripMenuItem";
            this.centerToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.centerToolStripMenuItem.Text = "Center horizontally";
            this.centerToolStripMenuItem.Click += new System.EventHandler(this.centerToolStripMenuItem_Click);
            // 
            // pictureBoxScreen
            // 
            this.pictureBoxScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxScreen.Location = new System.Drawing.Point(3, 3);
            this.pictureBoxScreen.Name = "pictureBoxScreen";
            this.pictureBoxScreen.Size = new System.Drawing.Size(633, 537);
            this.pictureBoxScreen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxScreen.TabIndex = 14;
            this.pictureBoxScreen.TabStop = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 578);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1139, 10);
            this.progressBar1.TabIndex = 8;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // timerSubtitleOnVideo
            // 
            this.timerSubtitleOnVideo.Interval = 25;
            this.timerSubtitleOnVideo.Tick += new System.EventHandler(this.timerSubtitleOnVideo_Tick);
            // 
            // buttonSetText
            // 
            this.buttonSetText.Location = new System.Drawing.Point(376, 70);
            this.buttonSetText.Name = "buttonSetText";
            this.buttonSetText.Size = new System.Drawing.Size(111, 23);
            this.buttonSetText.TabIndex = 11;
            this.buttonSetText.Text = "Set text...";
            this.buttonSetText.UseVisualStyleBackColor = true;
            this.buttonSetText.Click += new System.EventHandler(this.buttonSetText_Click);
            // 
            // videoPlayerContainer1
            // 
            this.videoPlayerContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.videoPlayerContainer1.Chapters = null;
            this.videoPlayerContainer1.CurrentPosition = 0D;
            this.videoPlayerContainer1.FontSizeFactor = 1F;
            this.videoPlayerContainer1.LastParagraph = null;
            this.videoPlayerContainer1.Location = new System.Drawing.Point(31, 107);
            this.videoPlayerContainer1.Name = "videoPlayerContainer1";
            this.videoPlayerContainer1.ShowFullscreenButton = true;
            this.videoPlayerContainer1.ShowMuteButton = true;
            this.videoPlayerContainer1.ShowStopButton = true;
            this.videoPlayerContainer1.Size = new System.Drawing.Size(584, 333);
            this.videoPlayerContainer1.SmpteMode = false;
            this.videoPlayerContainer1.SubtitleText = "";
            this.videoPlayerContainer1.TabIndex = 16;
            this.videoPlayerContainer1.TextRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.videoPlayerContainer1.VideoHeight = 0;
            this.videoPlayerContainer1.VideoPlayer = null;
            this.videoPlayerContainer1.VideoWidth = 0;
            this.videoPlayerContainer1.Volume = 0D;
            // 
            // timeUpDownEndTime
            // 
            this.timeUpDownEndTime.AutoSize = true;
            this.timeUpDownEndTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownEndTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownEndTime.Location = new System.Drawing.Point(74, 45);
            this.timeUpDownEndTime.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownEndTime.Name = "timeUpDownEndTime";
            this.timeUpDownEndTime.Size = new System.Drawing.Size(111, 27);
            this.timeUpDownEndTime.TabIndex = 3;
            timeCode1.Hours = 0;
            timeCode1.Milliseconds = 0;
            timeCode1.Minutes = 0;
            timeCode1.Seconds = 0;
            timeCode1.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode1.TotalMilliseconds = 0D;
            timeCode1.TotalSeconds = 0D;
            this.timeUpDownEndTime.TimeCode = timeCode1;
            this.timeUpDownEndTime.UseVideoOffset = false;
            // 
            // timeUpDownStartTime
            // 
            this.timeUpDownStartTime.AutoSize = true;
            this.timeUpDownStartTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownStartTime.Location = new System.Drawing.Point(74, 13);
            this.timeUpDownStartTime.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownStartTime.Name = "timeUpDownStartTime";
            this.timeUpDownStartTime.Size = new System.Drawing.Size(111, 27);
            this.timeUpDownStartTime.TabIndex = 1;
            timeCode2.Hours = 0;
            timeCode2.Milliseconds = 0;
            timeCode2.Minutes = 0;
            timeCode2.Seconds = 0;
            timeCode2.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode2.TotalMilliseconds = 0D;
            timeCode2.TotalSeconds = 0D;
            this.timeUpDownStartTime.TimeCode = timeCode2;
            this.timeUpDownStartTime.UseVideoOffset = false;
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.AllowColumnReorder = true;
            this.subtitleListView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.subtitleListView1.ContextMenuStrip = this.contextMenuStripListView;
            this.subtitleListView1.FirstVisibleIndex = -1;
            this.subtitleListView1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListView1.HideSelection = false;
            this.subtitleListView1.Location = new System.Drawing.Point(12, 27);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.OwnerDraw = true;
            this.subtitleListView1.Size = new System.Drawing.Size(494, 372);
            this.subtitleListView1.SubtitleFontBold = false;
            this.subtitleListView1.SubtitleFontName = "Tahoma";
            this.subtitleListView1.SubtitleFontSize = 8;
            this.subtitleListView1.TabIndex = 1;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.UseSyntaxColoring = true;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            this.subtitleListView1.SelectedIndexChanged += new System.EventHandler(this.subtitleListView1_SelectedIndexChanged);
            this.subtitleListView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.subtitleListView1_KeyDown);
            // 
            // BinEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1161, 596);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBoxCurrent);
            this.Controls.Add(this.subtitleListView1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "BinEdit";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BinEdit_FormClosing);
            this.Shown += new System.EventHandler(this.BinEdit_Shown);
            this.ResizeEnd += new System.EventHandler(this.BinEdit_ResizeEnd);
            this.groupBoxCurrent.ResumeLayout(false);
            this.groupBoxCurrent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenWidth)).EndInit();
            this.contextMenuStripListView.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMovableImage)).EndInit();
            this.contextMenuStripMovableImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxScreen)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.SubtitleListView subtitleListView1;
        private System.Windows.Forms.GroupBox groupBoxCurrent;
        private System.Windows.Forms.CheckBox checkBoxIsForced;
        private Controls.TimeUpDown timeUpDownEndTime;
        private Controls.TimeUpDown timeUpDownStartTime;
        private System.Windows.Forms.Button buttonImportImage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelEndTime;
        private System.Windows.Forms.Label labelStart;
        private System.Windows.Forms.NumericUpDown numericUpDownY;
        private System.Windows.Forms.NumericUpDown numericUpDownX;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownScreenHeight;
        private System.Windows.Forms.NumericUpDown numericUpDownScreenWidth;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListView;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
        private System.Windows.Forms.Button buttonExportImage;
        private System.Windows.Forms.Label labelCurrentSize;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveFileAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem videoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openVideoToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label labelFrameRate;
        private System.Windows.Forms.ComboBox comboBoxFrameRate;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBoxScreen;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem synchronizationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adjustAllTimesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeFrameRateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeSpeedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importTimeCodesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.PictureBox pictureBoxMovableImage;
        private System.Windows.Forms.ToolStripMenuItem insertAfterToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMovableImage;
        private System.Windows.Forms.ToolStripMenuItem centerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem insertSubtitleAfterThisLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adjustAllTimesForSelectedLinesToolStripMenuItem;
        private Controls.VideoPlayerContainer videoPlayerContainer1;
        private System.Windows.Forms.Timer timerSubtitleOnVideo;
        private System.Windows.Forms.ToolStripMenuItem closeVideoToolStripMenuItem;
        private System.Windows.Forms.Button buttonSetText;
    }
}