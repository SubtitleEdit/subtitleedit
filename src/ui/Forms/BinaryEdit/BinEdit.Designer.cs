
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
            Nikse.SubtitleEdit.Core.Common.TimeCode timeCode3 = new Nikse.SubtitleEdit.Core.Common.TimeCode();
            Nikse.SubtitleEdit.Core.Common.TimeCode timeCode4 = new Nikse.SubtitleEdit.Core.Common.TimeCode();
            this.groupBoxCurrent = new System.Windows.Forms.GroupBox();
            this.labelSyntaxError = new System.Windows.Forms.Label();
            this.buttonSetText = new System.Windows.Forms.Button();
            this.labelCurrentSize = new System.Windows.Forms.Label();
            this.buttonExportImage = new System.Windows.Forms.Button();
            this.buttonImportImage = new System.Windows.Forms.Button();
            this.labelPositionComma = new System.Windows.Forms.Label();
            this.labelEndTime = new System.Windows.Forms.Label();
            this.labelStart = new System.Windows.Forms.Label();
            this.numericUpDownY = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownX = new System.Windows.Forms.NumericUpDown();
            this.checkBoxIsForced = new System.Windows.Forms.CheckBox();
            this.timeUpDownEndTime = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.timeUpDownStartTime = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labelFrameRate = new System.Windows.Forms.Label();
            this.comboBoxFrameRate = new System.Windows.Forms.ComboBox();
            this.labelX = new System.Windows.Forms.Label();
            this.labelVideoSize = new System.Windows.Forms.Label();
            this.numericUpDownScreenHeight = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownScreenWidth = new System.Windows.Forms.NumericUpDown();
            this.contextMenuStripListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.alignSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.centerSelectedLineshorizontallyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.topAlignSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bottomAlignSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.toggleforcedForSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectOnlyForcedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.resizeImagesForSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeBrightnessForSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeAlphaForSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.adjustAllTimesForSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adjustDisplayTimeForSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyDurationLimitsForSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorInsertSub = new System.Windows.Forms.ToolStripSeparator();
            this.insertSubtitleAfterThisLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.importTimeCodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemTools = new System.Windows.Forms.ToolStripMenuItem();
            this.adjustDisplayTimesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyDurationLimitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.appendSubtitleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.alignmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resizeBitmapsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeBrightnessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeAlphaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.sortByToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startTimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.quickOCRTextsforOverviewOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.videoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.synchronizationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adjustAllTimesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeFrameRateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeSpeedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.panelBackground = new System.Windows.Forms.Panel();
            this.contextMenuStripBackground = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setAspectRatio11ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.videoPlayerContainer1 = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            this.pictureBoxMovableImage = new System.Windows.Forms.PictureBox();
            this.contextMenuStripMovableImage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.centerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.undoChangesForThisElementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timerSubtitleOnVideo = new System.Windows.Forms.Timer(this.components);
            this.labelVideoInfo = new System.Windows.Forms.Label();
            this.subtitleListView1 = new System.Windows.Forms.ListView();
            this.columnHeaderForced = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStart = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDuration = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.timerSyntaxColor = new System.Windows.Forms.Timer(this.components);
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.saveImageAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxCurrent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenWidth)).BeginInit();
            this.contextMenuStripListView.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panelBackground.SuspendLayout();
            this.contextMenuStripBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMovableImage)).BeginInit();
            this.contextMenuStripMovableImage.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxCurrent
            // 
            this.groupBoxCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxCurrent.Controls.Add(this.labelSyntaxError);
            this.groupBoxCurrent.Controls.Add(this.buttonSetText);
            this.groupBoxCurrent.Controls.Add(this.labelCurrentSize);
            this.groupBoxCurrent.Controls.Add(this.buttonExportImage);
            this.groupBoxCurrent.Controls.Add(this.buttonImportImage);
            this.groupBoxCurrent.Controls.Add(this.labelPositionComma);
            this.groupBoxCurrent.Controls.Add(this.labelEndTime);
            this.groupBoxCurrent.Controls.Add(this.labelStart);
            this.groupBoxCurrent.Controls.Add(this.numericUpDownY);
            this.groupBoxCurrent.Controls.Add(this.numericUpDownX);
            this.groupBoxCurrent.Controls.Add(this.checkBoxIsForced);
            this.groupBoxCurrent.Controls.Add(this.timeUpDownEndTime);
            this.groupBoxCurrent.Controls.Add(this.timeUpDownStartTime);
            this.groupBoxCurrent.Controls.Add(this.label1);
            this.groupBoxCurrent.Location = new System.Drawing.Point(12, 391);
            this.groupBoxCurrent.Name = "groupBoxCurrent";
            this.groupBoxCurrent.Size = new System.Drawing.Size(493, 109);
            this.groupBoxCurrent.TabIndex = 2;
            this.groupBoxCurrent.TabStop = false;
            this.groupBoxCurrent.Text = "Current";
            // 
            // labelSyntaxError
            // 
            this.labelSyntaxError.AutoSize = true;
            this.labelSyntaxError.ForeColor = System.Drawing.Color.Red;
            this.labelSyntaxError.Location = new System.Drawing.Point(103, 85);
            this.labelSyntaxError.Name = "labelSyntaxError";
            this.labelSyntaxError.Size = new System.Drawing.Size(83, 13);
            this.labelSyntaxError.TabIndex = 12;
            this.labelSyntaxError.Text = "labelSyntaxError";
            // 
            // buttonSetText
            // 
            this.buttonSetText.Location = new System.Drawing.Point(376, 75);
            this.buttonSetText.Name = "buttonSetText";
            this.buttonSetText.Size = new System.Drawing.Size(111, 23);
            this.buttonSetText.TabIndex = 11;
            this.buttonSetText.Text = "Set text...";
            this.buttonSetText.UseVisualStyleBackColor = true;
            this.buttonSetText.Click += new System.EventHandler(this.buttonSetText_Click);
            // 
            // labelCurrentSize
            // 
            this.labelCurrentSize.AutoSize = true;
            this.labelCurrentSize.Location = new System.Drawing.Point(192, 46);
            this.labelCurrentSize.Name = "labelCurrentSize";
            this.labelCurrentSize.Size = new System.Drawing.Size(30, 13);
            this.labelCurrentSize.TabIndex = 8;
            this.labelCurrentSize.Text = "Size:";
            // 
            // buttonExportImage
            // 
            this.buttonExportImage.Location = new System.Drawing.Point(376, 17);
            this.buttonExportImage.Name = "buttonExportImage";
            this.buttonExportImage.Size = new System.Drawing.Size(111, 23);
            this.buttonExportImage.TabIndex = 9;
            this.buttonExportImage.Text = "Export image...";
            this.buttonExportImage.UseVisualStyleBackColor = true;
            this.buttonExportImage.Click += new System.EventHandler(this.buttonExportImage_Click);
            // 
            // buttonImportImage
            // 
            this.buttonImportImage.Location = new System.Drawing.Point(376, 46);
            this.buttonImportImage.Name = "buttonImportImage";
            this.buttonImportImage.Size = new System.Drawing.Size(111, 23);
            this.buttonImportImage.TabIndex = 10;
            this.buttonImportImage.Text = "Import image...";
            this.buttonImportImage.UseVisualStyleBackColor = true;
            this.buttonImportImage.Click += new System.EventHandler(this.buttonImportImage_Click);
            // 
            // labelPositionComma
            // 
            this.labelPositionComma.AutoSize = true;
            this.labelPositionComma.Location = new System.Drawing.Point(298, 23);
            this.labelPositionComma.Name = "labelPositionComma";
            this.labelPositionComma.Size = new System.Drawing.Size(10, 13);
            this.labelPositionComma.TabIndex = 6;
            this.labelPositionComma.Text = ",";
            // 
            // labelEndTime
            // 
            this.labelEndTime.AutoSize = true;
            this.labelEndTime.Location = new System.Drawing.Point(16, 55);
            this.labelEndTime.Name = "labelEndTime";
            this.labelEndTime.Size = new System.Drawing.Size(48, 13);
            this.labelEndTime.TabIndex = 2;
            this.labelEndTime.Text = "End time";
            // 
            // labelStart
            // 
            this.labelStart.AutoSize = true;
            this.labelStart.Location = new System.Drawing.Point(16, 27);
            this.labelStart.Name = "labelStart";
            this.labelStart.Size = new System.Drawing.Size(51, 13);
            this.labelStart.TabIndex = 0;
            this.labelStart.Text = "Start time";
            // 
            // numericUpDownY
            // 
            this.numericUpDownY.Location = new System.Drawing.Point(313, 20);
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
            this.numericUpDownY.Size = new System.Drawing.Size(50, 20);
            this.numericUpDownY.TabIndex = 7;
            this.numericUpDownY.ValueChanged += new System.EventHandler(this.numericUpDownY_ValueChanged);
            // 
            // numericUpDownX
            // 
            this.numericUpDownX.Location = new System.Drawing.Point(243, 20);
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
            this.numericUpDownX.Size = new System.Drawing.Size(50, 20);
            this.numericUpDownX.TabIndex = 5;
            this.numericUpDownX.ValueChanged += new System.EventHandler(this.numericUpDownX_ValueChanged);
            // 
            // checkBoxIsForced
            // 
            this.checkBoxIsForced.AutoSize = true;
            this.checkBoxIsForced.Location = new System.Drawing.Point(19, 84);
            this.checkBoxIsForced.Name = "checkBoxIsForced";
            this.checkBoxIsForced.Size = new System.Drawing.Size(59, 17);
            this.checkBoxIsForced.TabIndex = 4;
            this.checkBoxIsForced.Text = "Forced";
            this.checkBoxIsForced.UseVisualStyleBackColor = true;
            this.checkBoxIsForced.CheckedChanged += new System.EventHandler(this.checkBoxIsForced_CheckedChanged);
            // 
            // timeUpDownEndTime
            // 
            this.timeUpDownEndTime.AutoSize = true;
            this.timeUpDownEndTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownEndTime.BackColor = System.Drawing.SystemColors.Control;
            this.timeUpDownEndTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownEndTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
            this.timeUpDownEndTime.Location = new System.Drawing.Point(74, 50);
            this.timeUpDownEndTime.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownEndTime.Name = "timeUpDownEndTime";
            this.timeUpDownEndTime.Size = new System.Drawing.Size(111, 27);
            this.timeUpDownEndTime.TabIndex = 3;
            timeCode3.Hours = 0;
            timeCode3.Milliseconds = 0;
            timeCode3.Minutes = 0;
            timeCode3.Seconds = 0;
            timeCode3.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode3.TotalMilliseconds = 0D;
            timeCode3.TotalSeconds = 0D;
            this.timeUpDownEndTime.TimeCode = timeCode3;
            this.timeUpDownEndTime.UseVideoOffset = false;
            // 
            // timeUpDownStartTime
            // 
            this.timeUpDownStartTime.AutoSize = true;
            this.timeUpDownStartTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownStartTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
            this.timeUpDownStartTime.Location = new System.Drawing.Point(74, 18);
            this.timeUpDownStartTime.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownStartTime.Name = "timeUpDownStartTime";
            this.timeUpDownStartTime.Size = new System.Drawing.Size(111, 27);
            this.timeUpDownStartTime.TabIndex = 1;
            timeCode4.Hours = 0;
            timeCode4.Milliseconds = 0;
            timeCode4.Minutes = 0;
            timeCode4.Seconds = 0;
            timeCode4.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode4.TotalMilliseconds = 0D;
            timeCode4.TotalSeconds = 0D;
            this.timeUpDownStartTime.TimeCode = timeCode4;
            this.timeUpDownStartTime.UseVideoOffset = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(192, 22);
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
            this.groupBox2.Controls.Add(this.labelX);
            this.groupBox2.Controls.Add(this.labelVideoSize);
            this.groupBox2.Controls.Add(this.numericUpDownScreenHeight);
            this.groupBox2.Controls.Add(this.numericUpDownScreenWidth);
            this.groupBox2.Location = new System.Drawing.Point(13, 506);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(493, 51);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
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
            this.comboBoxFrameRate.Size = new System.Drawing.Size(74, 21);
            this.comboBoxFrameRate.TabIndex = 5;
            this.comboBoxFrameRate.SelectedIndexChanged += new System.EventHandler(this.comboBoxFrameRate_SelectedIndexChanged);
            this.comboBoxFrameRate.SelectedValueChanged += new System.EventHandler(this.comboBoxFrameRate_SelectedValueChanged);
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(131, 23);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(12, 13);
            this.labelX.TabIndex = 2;
            this.labelX.Text = "x";
            // 
            // labelVideoSize
            // 
            this.labelVideoSize.AutoSize = true;
            this.labelVideoSize.Location = new System.Drawing.Point(10, 23);
            this.labelVideoSize.Name = "labelVideoSize";
            this.labelVideoSize.Size = new System.Drawing.Size(55, 13);
            this.labelVideoSize.TabIndex = 0;
            this.labelVideoSize.Text = "Video size";
            // 
            // numericUpDownScreenHeight
            // 
            this.numericUpDownScreenHeight.Location = new System.Drawing.Point(152, 21);
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
            this.numericUpDownScreenHeight.Size = new System.Drawing.Size(55, 20);
            this.numericUpDownScreenHeight.TabIndex = 3;
            this.numericUpDownScreenHeight.ValueChanged += new System.EventHandler(this.numericUpDownScreenHeight_ValueChanged);
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
            this.numericUpDownScreenWidth.Size = new System.Drawing.Size(55, 20);
            this.numericUpDownScreenWidth.TabIndex = 1;
            this.numericUpDownScreenWidth.ValueChanged += new System.EventHandler(this.numericUpDownScreenWidth_ValueChanged);
            // 
            // contextMenuStripListView
            // 
            this.contextMenuStripListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.insertToolStripMenuItem,
            this.insertAfterToolStripMenuItem,
            this.toolStripSeparator3,
            this.alignSelectedLinesToolStripMenuItem,
            this.centerSelectedLineshorizontallyToolStripMenuItem,
            this.topAlignSelectedLinesToolStripMenuItem,
            this.bottomAlignSelectedLinesToolStripMenuItem,
            this.toolStripSeparator9,
            this.toggleforcedForSelectedLinesToolStripMenuItem,
            this.selectOnlyForcedLinesToolStripMenuItem,
            this.toolStripSeparator7,
            this.resizeImagesForSelectedLinesToolStripMenuItem,
            this.colorSelectedLinesToolStripMenuItem,
            this.changeBrightnessForSelectedLinesToolStripMenuItem,
            this.changeAlphaForSelectedLinesToolStripMenuItem,
            this.toolStripSeparator6,
            this.adjustAllTimesForSelectedLinesToolStripMenuItem,
            this.adjustDisplayTimeForSelectedLinesToolStripMenuItem,
            this.applyDurationLimitsForSelectedLinesToolStripMenuItem,
            this.toolStripSeparatorInsertSub,
            this.insertSubtitleAfterThisLineToolStripMenuItem});
            this.contextMenuStripListView.Name = "contextMenuStripListView";
            this.contextMenuStripListView.Size = new System.Drawing.Size(377, 408);
            this.contextMenuStripListView.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripListView_Opening);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // insertToolStripMenuItem
            // 
            this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            this.insertToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.insertToolStripMenuItem.Text = "Insert before";
            this.insertToolStripMenuItem.Click += new System.EventHandler(this.insertToolStripMenuItem_Click);
            // 
            // insertAfterToolStripMenuItem
            // 
            this.insertAfterToolStripMenuItem.Name = "insertAfterToolStripMenuItem";
            this.insertAfterToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.insertAfterToolStripMenuItem.Text = "Insert after";
            this.insertAfterToolStripMenuItem.Click += new System.EventHandler(this.insertAfterToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(373, 6);
            // 
            // alignSelectedLinesToolStripMenuItem
            // 
            this.alignSelectedLinesToolStripMenuItem.Name = "alignSelectedLinesToolStripMenuItem";
            this.alignSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.alignSelectedLinesToolStripMenuItem.Text = "Align selected lines...";
            this.alignSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.alignSelectedLinesToolStripMenuItem_Click);
            // 
            // centerSelectedLineshorizontallyToolStripMenuItem
            // 
            this.centerSelectedLineshorizontallyToolStripMenuItem.Name = "centerSelectedLineshorizontallyToolStripMenuItem";
            this.centerSelectedLineshorizontallyToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.centerSelectedLineshorizontallyToolStripMenuItem.Text = "Center selected lines (horizontally, keep vertical position) ";
            this.centerSelectedLineshorizontallyToolStripMenuItem.Click += new System.EventHandler(this.centerSelectedLinesHorizontallyToolStripMenuItem_Click);
            // 
            // topAlignSelectedLinesToolStripMenuItem
            // 
            this.topAlignSelectedLinesToolStripMenuItem.Name = "topAlignSelectedLinesToolStripMenuItem";
            this.topAlignSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.topAlignSelectedLinesToolStripMenuItem.Text = "Top align selected lines (keep horizontal position)";
            this.topAlignSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.topAlignSelectedLinesToolStripMenuItem_Click);
            // 
            // bottomAlignSelectedLinesToolStripMenuItem
            // 
            this.bottomAlignSelectedLinesToolStripMenuItem.Name = "bottomAlignSelectedLinesToolStripMenuItem";
            this.bottomAlignSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.bottomAlignSelectedLinesToolStripMenuItem.Text = "Bottom align selected lines (keep horizontal position)";
            this.bottomAlignSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.bottomAlignSelectedLinesToolStripMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(373, 6);
            // 
            // toggleforcedForSelectedLinesToolStripMenuItem
            // 
            this.toggleforcedForSelectedLinesToolStripMenuItem.Name = "toggleforcedForSelectedLinesToolStripMenuItem";
            this.toggleforcedForSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.toggleforcedForSelectedLinesToolStripMenuItem.Text = "Toggle \"forced\" for selected lines";
            this.toggleforcedForSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.toggleForcedForSelectedLinesToolStripMenuItem_Click);
            // 
            // selectOnlyForcedLinesToolStripMenuItem
            // 
            this.selectOnlyForcedLinesToolStripMenuItem.Name = "selectOnlyForcedLinesToolStripMenuItem";
            this.selectOnlyForcedLinesToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.selectOnlyForcedLinesToolStripMenuItem.Text = "Select only forced lines";
            this.selectOnlyForcedLinesToolStripMenuItem.Click += new System.EventHandler(this.selectOnlyForcedLinesToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(373, 6);
            // 
            // resizeImagesForSelectedLinesToolStripMenuItem
            // 
            this.resizeImagesForSelectedLinesToolStripMenuItem.Name = "resizeImagesForSelectedLinesToolStripMenuItem";
            this.resizeImagesForSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.resizeImagesForSelectedLinesToolStripMenuItem.Text = "Resize images for selected lines...";
            this.resizeImagesForSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.resizeImagesForSelectedLinesToolStripMenuItem_Click);
            // 
            // colorSelectedLinesToolStripMenuItem
            // 
            this.colorSelectedLinesToolStripMenuItem.Name = "colorSelectedLinesToolStripMenuItem";
            this.colorSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.colorSelectedLinesToolStripMenuItem.Text = "Change color for selected lines...";
            this.colorSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.colorSelectedLinesToolStripMenuItem_Click);
            // 
            // changeBrightnessForSelectedLinesToolStripMenuItem
            // 
            this.changeBrightnessForSelectedLinesToolStripMenuItem.Name = "changeBrightnessForSelectedLinesToolStripMenuItem";
            this.changeBrightnessForSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.changeBrightnessForSelectedLinesToolStripMenuItem.Text = "Change brightness for selected lines...";
            this.changeBrightnessForSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.changeBrightnessForSelectedLinesToolStripMenuItem_Click);
            // 
            // changeAlphaForSelectedLinesToolStripMenuItem
            // 
            this.changeAlphaForSelectedLinesToolStripMenuItem.Name = "changeAlphaForSelectedLinesToolStripMenuItem";
            this.changeAlphaForSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.changeAlphaForSelectedLinesToolStripMenuItem.Text = "Change alpha for selected lines...";
            this.changeAlphaForSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.changeAlphaForSelectedLinesToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(373, 6);
            // 
            // adjustAllTimesForSelectedLinesToolStripMenuItem
            // 
            this.adjustAllTimesForSelectedLinesToolStripMenuItem.Name = "adjustAllTimesForSelectedLinesToolStripMenuItem";
            this.adjustAllTimesForSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.adjustAllTimesForSelectedLinesToolStripMenuItem.Text = "Adjust all times for selected lines...";
            this.adjustAllTimesForSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.adjustAllTimesForSelectedLinesToolStripMenuItem_Click);
            // 
            // adjustDisplayTimeForSelectedLinesToolStripMenuItem
            // 
            this.adjustDisplayTimeForSelectedLinesToolStripMenuItem.Name = "adjustDisplayTimeForSelectedLinesToolStripMenuItem";
            this.adjustDisplayTimeForSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.adjustDisplayTimeForSelectedLinesToolStripMenuItem.Text = "Adjust display time for selected lines...";
            this.adjustDisplayTimeForSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.adjustDisplayTimeForSelectedLinesToolStripMenuItem_Click);
            // 
            // applyDurationLimitsForSelectedLinesToolStripMenuItem
            // 
            this.applyDurationLimitsForSelectedLinesToolStripMenuItem.Name = "applyDurationLimitsForSelectedLinesToolStripMenuItem";
            this.applyDurationLimitsForSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.applyDurationLimitsForSelectedLinesToolStripMenuItem.Text = "Apply duration limits for selected lines...";
            this.applyDurationLimitsForSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.applyDurationLimitsForSelectedLinesToolStripMenuItem_Click);
            // 
            // toolStripSeparatorInsertSub
            // 
            this.toolStripSeparatorInsertSub.Name = "toolStripSeparatorInsertSub";
            this.toolStripSeparatorInsertSub.Size = new System.Drawing.Size(373, 6);
            // 
            // insertSubtitleAfterThisLineToolStripMenuItem
            // 
            this.insertSubtitleAfterThisLineToolStripMenuItem.Name = "insertSubtitleAfterThisLineToolStripMenuItem";
            this.insertSubtitleAfterThisLineToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            this.insertSubtitleAfterThisLineToolStripMenuItem.Text = "Insert subtitle after this line...";
            this.insertSubtitleAfterThisLineToolStripMenuItem.Click += new System.EventHandler(this.insertSubtitleAfterThisLineToolStripMenuItem_Click_1);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolStripMenuItemTools,
            this.videoToolStripMenuItem,
            this.synchronizationToolStripMenuItem,
            this.optionsToolStripMenuItem});
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
            // toolStripMenuItemTools
            // 
            this.toolStripMenuItemTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.adjustDisplayTimesToolStripMenuItem,
            this.applyDurationLimitsToolStripMenuItem,
            this.appendSubtitleToolStripMenuItem,
            this.toolStripSeparator8,
            this.alignmentToolStripMenuItem,
            this.resizeBitmapsToolStripMenuItem,
            this.changeBrightnessToolStripMenuItem,
            this.changeAlphaToolStripMenuItem,
            this.toolStripSeparator5,
            this.sortByToolStripMenuItem,
            this.toolStripSeparator10,
            this.quickOCRTextsforOverviewOnlyToolStripMenuItem});
            this.toolStripMenuItemTools.Name = "toolStripMenuItemTools";
            this.toolStripMenuItemTools.Size = new System.Drawing.Size(46, 20);
            this.toolStripMenuItemTools.Text = "Tools";
            // 
            // adjustDisplayTimesToolStripMenuItem
            // 
            this.adjustDisplayTimesToolStripMenuItem.Name = "adjustDisplayTimesToolStripMenuItem";
            this.adjustDisplayTimesToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.adjustDisplayTimesToolStripMenuItem.Text = "Adjust display times...";
            this.adjustDisplayTimesToolStripMenuItem.Click += new System.EventHandler(this.adjustDisplayTimesToolStripMenuItem_Click);
            // 
            // applyDurationLimitsToolStripMenuItem
            // 
            this.applyDurationLimitsToolStripMenuItem.Name = "applyDurationLimitsToolStripMenuItem";
            this.applyDurationLimitsToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.applyDurationLimitsToolStripMenuItem.Text = "Apply duration limits";
            this.applyDurationLimitsToolStripMenuItem.Click += new System.EventHandler(this.applyDurationLimitsToolStripMenuItem_Click);
            // 
            // appendSubtitleToolStripMenuItem
            // 
            this.appendSubtitleToolStripMenuItem.Name = "appendSubtitleToolStripMenuItem";
            this.appendSubtitleToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.appendSubtitleToolStripMenuItem.Text = "Append subtitle...";
            this.appendSubtitleToolStripMenuItem.Click += new System.EventHandler(this.appendSubtitleToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(259, 6);
            // 
            // alignmentToolStripMenuItem
            // 
            this.alignmentToolStripMenuItem.Name = "alignmentToolStripMenuItem";
            this.alignmentToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.alignmentToolStripMenuItem.Text = "Alignment...";
            this.alignmentToolStripMenuItem.Click += new System.EventHandler(this.alignmentToolStripMenuItem_Click);
            // 
            // resizeBitmapsToolStripMenuItem
            // 
            this.resizeBitmapsToolStripMenuItem.Name = "resizeBitmapsToolStripMenuItem";
            this.resizeBitmapsToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.resizeBitmapsToolStripMenuItem.Text = "Resize bitmaps...";
            this.resizeBitmapsToolStripMenuItem.Click += new System.EventHandler(this.resizeBitmapsToolStripMenuItem_Click);
            // 
            // changeBrightnessToolStripMenuItem
            // 
            this.changeBrightnessToolStripMenuItem.Name = "changeBrightnessToolStripMenuItem";
            this.changeBrightnessToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.changeBrightnessToolStripMenuItem.Text = "Change brightness...";
            this.changeBrightnessToolStripMenuItem.Click += new System.EventHandler(this.changeBrightnessToolStripMenuItem_Click);
            // 
            // changeAlphaToolStripMenuItem
            // 
            this.changeAlphaToolStripMenuItem.Name = "changeAlphaToolStripMenuItem";
            this.changeAlphaToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.changeAlphaToolStripMenuItem.Text = "Change alpha...";
            this.changeAlphaToolStripMenuItem.Click += new System.EventHandler(this.changeAlphaToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(259, 6);
            // 
            // sortByToolStripMenuItem
            // 
            this.sortByToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startTimeToolStripMenuItem});
            this.sortByToolStripMenuItem.Name = "sortByToolStripMenuItem";
            this.sortByToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.sortByToolStripMenuItem.Text = "Sort by";
            // 
            // startTimeToolStripMenuItem
            // 
            this.startTimeToolStripMenuItem.Name = "startTimeToolStripMenuItem";
            this.startTimeToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.startTimeToolStripMenuItem.Text = "Start time";
            this.startTimeToolStripMenuItem.Click += new System.EventHandler(this.startTimeToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(259, 6);
            // 
            // quickOCRTextsforOverviewOnlyToolStripMenuItem
            // 
            this.quickOCRTextsforOverviewOnlyToolStripMenuItem.Name = "quickOCRTextsforOverviewOnlyToolStripMenuItem";
            this.quickOCRTextsforOverviewOnlyToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.quickOCRTextsforOverviewOnlyToolStripMenuItem.Text = "Quick OCR texts (for overview only)";
            this.quickOCRTextsforOverviewOnlyToolStripMenuItem.Click += new System.EventHandler(this.quickOcrTextsForOverviewOnlyToolStripMenuItem_Click);
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
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // panelBackground
            // 
            this.panelBackground.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBackground.BackColor = System.Drawing.Color.Black;
            this.panelBackground.ContextMenuStrip = this.contextMenuStripBackground;
            this.panelBackground.Controls.Add(this.videoPlayerContainer1);
            this.panelBackground.Controls.Add(this.pictureBoxMovableImage);
            this.panelBackground.Location = new System.Drawing.Point(513, 28);
            this.panelBackground.Name = "panelBackground";
            this.panelBackground.Size = new System.Drawing.Size(638, 529);
            this.panelBackground.TabIndex = 4;
            // 
            // contextMenuStripBackground
            // 
            this.contextMenuStripBackground.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setAspectRatio11ToolStripMenuItem});
            this.contextMenuStripBackground.Name = "contextMenuStripBackground";
            this.contextMenuStripBackground.Size = new System.Drawing.Size(213, 26);
            // 
            // setAspectRatio11ToolStripMenuItem
            // 
            this.setAspectRatio11ToolStripMenuItem.Name = "setAspectRatio11ToolStripMenuItem";
            this.setAspectRatio11ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D0)));
            this.setAspectRatio11ToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.setAspectRatio11ToolStripMenuItem.Text = "Set aspect ratio 1:1";
            this.setAspectRatio11ToolStripMenuItem.Click += new System.EventHandler(this.setAspectRatio11ToolStripMenuItem_Click);
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
            this.centerToolStripMenuItem,
            this.toolStripSeparator4,
            this.undoChangesForThisElementToolStripMenuItem,
            this.toolStripSeparator11,
            this.saveImageAsToolStripMenuItem});
            this.contextMenuStripMovableImage.Name = "contextMenuStripMovableImage";
            this.contextMenuStripMovableImage.Size = new System.Drawing.Size(237, 104);
            // 
            // centerToolStripMenuItem
            // 
            this.centerToolStripMenuItem.Name = "centerToolStripMenuItem";
            this.centerToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.centerToolStripMenuItem.Text = "Center horizontally";
            this.centerToolStripMenuItem.Click += new System.EventHandler(this.centerToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(233, 6);
            // 
            // undoChangesForThisElementToolStripMenuItem
            // 
            this.undoChangesForThisElementToolStripMenuItem.Name = "undoChangesForThisElementToolStripMenuItem";
            this.undoChangesForThisElementToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.undoChangesForThisElementToolStripMenuItem.Text = "Undo changes for this element";
            this.undoChangesForThisElementToolStripMenuItem.Click += new System.EventHandler(this.undoChangesForThisElementToolStripMenuItem_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 564);
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
            // labelVideoInfo
            // 
            this.labelVideoInfo.AutoSize = true;
            this.labelVideoInfo.Location = new System.Drawing.Point(513, 11);
            this.labelVideoInfo.Name = "labelVideoInfo";
            this.labelVideoInfo.Size = new System.Drawing.Size(54, 13);
            this.labelVideoInfo.TabIndex = 9;
            this.labelVideoInfo.Text = "Video info";
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.subtitleListView1.CheckBoxes = true;
            this.subtitleListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderForced,
            this.columnHeaderNumber,
            this.columnHeaderStart,
            this.columnHeaderDuration,
            this.columnHeaderText});
            this.subtitleListView1.ContextMenuStrip = this.contextMenuStripListView;
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListView1.HideSelection = false;
            this.subtitleListView1.Location = new System.Drawing.Point(13, 28);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.Size = new System.Drawing.Size(492, 357);
            this.subtitleListView1.TabIndex = 10;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            this.subtitleListView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.subtitleListView1_ItemChecked);
            this.subtitleListView1.SelectedIndexChanged += new System.EventHandler(this.subtitleListView1_SelectedIndexChanged);
            this.subtitleListView1.Click += new System.EventHandler(this.subtitleListView1_Click);
            this.subtitleListView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.subtitleListView1_KeyDown);
            // 
            // columnHeaderForced
            // 
            this.columnHeaderForced.Text = "Forced";
            // 
            // columnHeaderStart
            // 
            this.columnHeaderStart.Width = 105;
            // 
            // columnHeaderDuration
            // 
            this.columnHeaderDuration.Width = 65;
            // 
            // columnHeaderText
            // 
            this.columnHeaderText.Width = 208;
            // 
            // timerSyntaxColor
            // 
            this.timerSyntaxColor.Interval = 250;
            this.timerSyntaxColor.Tick += new System.EventHandler(this.timerSyntaxColor_Tick);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(233, 6);
            // 
            // saveImageAsToolStripMenuItem
            // 
            this.saveImageAsToolStripMenuItem.Name = "saveImageAsToolStripMenuItem";
            this.saveImageAsToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.saveImageAsToolStripMenuItem.Text = "Save image as...";
            this.saveImageAsToolStripMenuItem.Click += new System.EventHandler(this.saveImageAsToolStripMenuItem_Click);
            // 
            // BinEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1161, 582);
            this.Controls.Add(this.subtitleListView1);
            this.Controls.Add(this.labelVideoInfo);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.panelBackground);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBoxCurrent);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "BinEdit";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BinEdit_FormClosing);
            this.Shown += new System.EventHandler(this.BinEdit_Shown);
            this.ResizeEnd += new System.EventHandler(this.BinEdit_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BinEdit_KeyDown);
            this.Resize += new System.EventHandler(this.BinEdit_Resize);
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
            this.panelBackground.ResumeLayout(false);
            this.contextMenuStripBackground.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMovableImage)).EndInit();
            this.contextMenuStripMovableImage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBoxCurrent;
        private System.Windows.Forms.CheckBox checkBoxIsForced;
        private Controls.TimeUpDown timeUpDownEndTime;
        private Controls.TimeUpDown timeUpDownStartTime;
        private System.Windows.Forms.Button buttonImportImage;
        private System.Windows.Forms.Label labelPositionComma;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelEndTime;
        private System.Windows.Forms.Label labelStart;
        private System.Windows.Forms.NumericUpDown numericUpDownY;
        private System.Windows.Forms.NumericUpDown numericUpDownX;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.Label labelVideoSize;
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
        private System.Windows.Forms.Panel panelBackground;
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem undoChangesForThisElementToolStripMenuItem;
        private System.Windows.Forms.Label labelVideoInfo;
        private System.Windows.Forms.ToolStripMenuItem centerSelectedLineshorizontallyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem topAlignSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bottomAlignSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorInsertSub;
        private System.Windows.Forms.ToolStripMenuItem colorSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adjustDisplayTimeForSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTools;
        private System.Windows.Forms.ToolStripMenuItem adjustDisplayTimesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem applyDurationLimitsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem applyDurationLimitsForSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem appendSubtitleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alignSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem quickOCRTextsforOverviewOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resizeBitmapsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem resizeImagesForSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alignmentToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripBackground;
        private System.Windows.Forms.ToolStripMenuItem setAspectRatio11ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeBrightnessToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeBrightnessForSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeAlphaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeAlphaForSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleforcedForSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem sortByToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startTimeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem selectOnlyForcedLinesToolStripMenuItem;
        private System.Windows.Forms.ListView subtitleListView1;
        private System.Windows.Forms.ColumnHeader columnHeaderForced;
        private System.Windows.Forms.ColumnHeader columnHeaderNumber;
        private System.Windows.Forms.ColumnHeader columnHeaderStart;
        private System.Windows.Forms.ColumnHeader columnHeaderDuration;
        private System.Windows.Forms.ColumnHeader columnHeaderText;
        private System.Windows.Forms.Timer timerSyntaxColor;
        private System.Windows.Forms.Label labelSyntaxError;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem saveImageAsToolStripMenuItem;
    }
}