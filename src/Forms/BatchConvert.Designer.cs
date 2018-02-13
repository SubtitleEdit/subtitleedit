namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class BatchConvert
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
            Nikse.SubtitleEdit.Core.TimeCode timeCode1 = new Nikse.SubtitleEdit.Core.TimeCode();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxConvertOptions = new System.Windows.Forms.GroupBox();
            this.buttonBridgeGapsSettings = new System.Windows.Forms.Button();
            this.checkBoxBridgeGaps = new System.Windows.Forms.CheckBox();
            this.groupBoxSpeed = new System.Windows.Forms.GroupBox();
            this.radioButtonToDropFrame = new System.Windows.Forms.RadioButton();
            this.radioButtonSpeedFromDropFrame = new System.Windows.Forms.RadioButton();
            this.radioButtonSpeedCustom = new System.Windows.Forms.RadioButton();
            this.numericUpDownPercent = new System.Windows.Forms.NumericUpDown();
            this.labelPercent = new System.Windows.Forms.Label();
            this.checkBoxSplitLongLines = new System.Windows.Forms.CheckBox();
            this.buttonRemoveTextForHiSettings = new System.Windows.Forms.Button();
            this.checkBoxSetMinimumDisplayTimeBetweenSubs = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoBalance = new System.Windows.Forms.CheckBox();
            this.buttonMultipleReplaceSettings = new System.Windows.Forms.Button();
            this.checkBoxMultipleReplace = new System.Windows.Forms.CheckBox();
            this.checkBoxFixCommonErrors = new System.Windows.Forms.CheckBox();
            this.buttonFixCommonErrorSettings = new System.Windows.Forms.Button();
            this.groupBoxChangeFrameRate = new System.Windows.Forms.GroupBox();
            this.comboBoxFrameRateTo = new System.Windows.Forms.ComboBox();
            this.labelToFrameRate = new System.Windows.Forms.Label();
            this.comboBoxFrameRateFrom = new System.Windows.Forms.ComboBox();
            this.labelFromFrameRate = new System.Windows.Forms.Label();
            this.groupBoxOffsetTimeCodes = new System.Windows.Forms.GroupBox();
            this.radioButtonShowLater = new System.Windows.Forms.RadioButton();
            this.radioButtonShowEarlier = new System.Windows.Forms.RadioButton();
            this.timeUpDownAdjust = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.labelHourMinSecMilliSecond = new System.Windows.Forms.Label();
            this.checkBoxFixCasing = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveTextForHI = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveFormatting = new System.Windows.Forms.CheckBox();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.checkBoxUseStyleFromSource = new System.Windows.Forms.CheckBox();
            this.checkBoxOverwriteOriginalFiles = new System.Windows.Forms.CheckBox();
            this.linkLabelOpenOutputFolder = new System.Windows.Forms.LinkLabel();
            this.buttonStyles = new System.Windows.Forms.Button();
            this.checkBoxOverwrite = new System.Windows.Forms.CheckBox();
            this.comboBoxSubtitleFormats = new System.Windows.Forms.ComboBox();
            this.labelEncoding = new System.Windows.Forms.Label();
            this.comboBoxEncoding = new System.Windows.Forms.ComboBox();
            this.labelOutputFormat = new System.Windows.Forms.Label();
            this.labelChooseOutputFolder = new System.Windows.Forms.Label();
            this.buttonChooseFolder = new System.Windows.Forms.Button();
            this.textBoxOutputFolder = new System.Windows.Forms.TextBox();
            this.groupBoxInput = new System.Windows.Forms.GroupBox();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.labelFilter = new System.Windows.Forms.Label();
            this.comboBoxFilter = new System.Windows.Forms.ComboBox();
            this.checkBoxScanFolderRecursive = new System.Windows.Forms.CheckBox();
            this.buttonSearchFolder = new System.Windows.Forms.Button();
            this.buttonInputBrowse = new System.Windows.Forms.Button();
            this.labelChooseInputFiles = new System.Windows.Forms.Label();
            this.listViewInputFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            this.groupBoxConvertOptions.SuspendLayout();
            this.groupBoxSpeed.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPercent)).BeginInit();
            this.groupBoxChangeFrameRate.SuspendLayout();
            this.groupBoxOffsetTimeCodes.SuspendLayout();
            this.groupBoxOutput.SuspendLayout();
            this.groupBoxInput.SuspendLayout();
            this.contextMenuStripFiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonConvert
            // 
            this.buttonConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConvert.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonConvert.Location = new System.Drawing.Point(844, 598);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(98, 21);
            this.buttonConvert.TabIndex = 2;
            this.buttonConvert.Text = "&Convert";
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.buttonConvert_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(948, 598);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "&Done";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBoxConvertOptions
            // 
            this.groupBoxConvertOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxConvertOptions.Controls.Add(this.buttonBridgeGapsSettings);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxBridgeGaps);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxSpeed);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxSplitLongLines);
            this.groupBoxConvertOptions.Controls.Add(this.buttonRemoveTextForHiSettings);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxSetMinimumDisplayTimeBetweenSubs);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxAutoBalance);
            this.groupBoxConvertOptions.Controls.Add(this.buttonMultipleReplaceSettings);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxMultipleReplace);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxFixCommonErrors);
            this.groupBoxConvertOptions.Controls.Add(this.buttonFixCommonErrorSettings);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxChangeFrameRate);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxOffsetTimeCodes);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxFixCasing);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxRemoveTextForHI);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxRemoveFormatting);
            this.groupBoxConvertOptions.Location = new System.Drawing.Point(422, 19);
            this.groupBoxConvertOptions.Name = "groupBoxConvertOptions";
            this.groupBoxConvertOptions.Size = new System.Drawing.Size(583, 234);
            this.groupBoxConvertOptions.TabIndex = 11;
            this.groupBoxConvertOptions.TabStop = false;
            this.groupBoxConvertOptions.Text = "Convert options";
            // 
            // buttonBridgeGapsSettings
            // 
            this.buttonBridgeGapsSettings.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonBridgeGapsSettings.Location = new System.Drawing.Point(183, 92);
            this.buttonBridgeGapsSettings.Name = "buttonBridgeGapsSettings";
            this.buttonBridgeGapsSettings.Size = new System.Drawing.Size(104, 21);
            this.buttonBridgeGapsSettings.TabIndex = 10;
            this.buttonBridgeGapsSettings.Text = "Settings...";
            this.buttonBridgeGapsSettings.UseVisualStyleBackColor = true;
            this.buttonBridgeGapsSettings.Click += new System.EventHandler(this.buttonBridgeGapsSettings_Click);
            // 
            // checkBoxBridgeGaps
            // 
            this.checkBoxBridgeGaps.AutoSize = true;
            this.checkBoxBridgeGaps.Location = new System.Drawing.Point(15, 96);
            this.checkBoxBridgeGaps.Name = "checkBoxBridgeGaps";
            this.checkBoxBridgeGaps.Size = new System.Drawing.Size(139, 17);
            this.checkBoxBridgeGaps.TabIndex = 7;
            this.checkBoxBridgeGaps.Text = "Bridge gaps in durations";
            this.checkBoxBridgeGaps.UseVisualStyleBackColor = true;
            // 
            // groupBoxSpeed
            // 
            this.groupBoxSpeed.Controls.Add(this.radioButtonToDropFrame);
            this.groupBoxSpeed.Controls.Add(this.radioButtonSpeedFromDropFrame);
            this.groupBoxSpeed.Controls.Add(this.radioButtonSpeedCustom);
            this.groupBoxSpeed.Controls.Add(this.numericUpDownPercent);
            this.groupBoxSpeed.Controls.Add(this.labelPercent);
            this.groupBoxSpeed.Location = new System.Drawing.Point(444, 89);
            this.groupBoxSpeed.Name = "groupBoxSpeed";
            this.groupBoxSpeed.Size = new System.Drawing.Size(132, 115);
            this.groupBoxSpeed.TabIndex = 12;
            this.groupBoxSpeed.TabStop = false;
            this.groupBoxSpeed.Text = "Change speed";
            // 
            // radioButtonToDropFrame
            // 
            this.radioButtonToDropFrame.AutoSize = true;
            this.radioButtonToDropFrame.Location = new System.Drawing.Point(6, 91);
            this.radioButtonToDropFrame.Name = "radioButtonToDropFrame";
            this.radioButtonToDropFrame.Size = new System.Drawing.Size(91, 17);
            this.radioButtonToDropFrame.TabIndex = 3;
            this.radioButtonToDropFrame.Text = "To drop frame";
            this.radioButtonToDropFrame.UseVisualStyleBackColor = true;
            this.radioButtonToDropFrame.CheckedChanged += new System.EventHandler(this.radioButtonToDropFrame_CheckedChanged);
            // 
            // radioButtonSpeedFromDropFrame
            // 
            this.radioButtonSpeedFromDropFrame.AutoSize = true;
            this.radioButtonSpeedFromDropFrame.Location = new System.Drawing.Point(6, 68);
            this.radioButtonSpeedFromDropFrame.Name = "radioButtonSpeedFromDropFrame";
            this.radioButtonSpeedFromDropFrame.Size = new System.Drawing.Size(101, 17);
            this.radioButtonSpeedFromDropFrame.TabIndex = 2;
            this.radioButtonSpeedFromDropFrame.Text = "From drop frame";
            this.radioButtonSpeedFromDropFrame.UseVisualStyleBackColor = true;
            this.radioButtonSpeedFromDropFrame.CheckedChanged += new System.EventHandler(this.radioButtonSpeedFromDropFrame_CheckedChanged);
            // 
            // radioButtonSpeedCustom
            // 
            this.radioButtonSpeedCustom.AutoSize = true;
            this.radioButtonSpeedCustom.Checked = true;
            this.radioButtonSpeedCustom.Location = new System.Drawing.Point(6, 45);
            this.radioButtonSpeedCustom.Name = "radioButtonSpeedCustom";
            this.radioButtonSpeedCustom.Size = new System.Drawing.Size(60, 17);
            this.radioButtonSpeedCustom.TabIndex = 1;
            this.radioButtonSpeedCustom.TabStop = true;
            this.radioButtonSpeedCustom.Text = "Custom";
            this.radioButtonSpeedCustom.UseVisualStyleBackColor = true;
            this.radioButtonSpeedCustom.CheckedChanged += new System.EventHandler(this.radioButtonSpeedCustom_CheckedChanged);
            // 
            // numericUpDownPercent
            // 
            this.numericUpDownPercent.DecimalPlaces = 5;
            this.numericUpDownPercent.Location = new System.Drawing.Point(6, 19);
            this.numericUpDownPercent.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownPercent.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownPercent.Name = "numericUpDownPercent";
            this.numericUpDownPercent.Size = new System.Drawing.Size(81, 20);
            this.numericUpDownPercent.TabIndex = 0;
            this.numericUpDownPercent.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // labelPercent
            // 
            this.labelPercent.AutoSize = true;
            this.labelPercent.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelPercent.Location = new System.Drawing.Point(87, 22);
            this.labelPercent.Name = "labelPercent";
            this.labelPercent.Size = new System.Drawing.Size(15, 13);
            this.labelPercent.TabIndex = 12;
            this.labelPercent.Text = "%";
            // 
            // checkBoxSplitLongLines
            // 
            this.checkBoxSplitLongLines.AutoSize = true;
            this.checkBoxSplitLongLines.Location = new System.Drawing.Point(15, 167);
            this.checkBoxSplitLongLines.Name = "checkBoxSplitLongLines";
            this.checkBoxSplitLongLines.Size = new System.Drawing.Size(93, 17);
            this.checkBoxSplitLongLines.TabIndex = 40;
            this.checkBoxSplitLongLines.Text = "Split long lines";
            this.checkBoxSplitLongLines.UseVisualStyleBackColor = true;
            // 
            // buttonRemoveTextForHiSettings
            // 
            this.buttonRemoveTextForHiSettings.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemoveTextForHiSettings.Location = new System.Drawing.Point(183, 68);
            this.buttonRemoveTextForHiSettings.Name = "buttonRemoveTextForHiSettings";
            this.buttonRemoveTextForHiSettings.Size = new System.Drawing.Size(104, 21);
            this.buttonRemoveTextForHiSettings.TabIndex = 3;
            this.buttonRemoveTextForHiSettings.Text = "Settings...";
            this.buttonRemoveTextForHiSettings.UseVisualStyleBackColor = true;
            this.buttonRemoveTextForHiSettings.Click += new System.EventHandler(this.buttonRemoveTextForHiSettings_Click);
            // 
            // checkBoxSetMinimumDisplayTimeBetweenSubs
            // 
            this.checkBoxSetMinimumDisplayTimeBetweenSubs.AutoSize = true;
            this.checkBoxSetMinimumDisplayTimeBetweenSubs.Location = new System.Drawing.Point(15, 213);
            this.checkBoxSetMinimumDisplayTimeBetweenSubs.Name = "checkBoxSetMinimumDisplayTimeBetweenSubs";
            this.checkBoxSetMinimumDisplayTimeBetweenSubs.Size = new System.Drawing.Size(208, 17);
            this.checkBoxSetMinimumDisplayTimeBetweenSubs.TabIndex = 50;
            this.checkBoxSetMinimumDisplayTimeBetweenSubs.Text = "Set min. milliseconds between subtitles";
            this.checkBoxSetMinimumDisplayTimeBetweenSubs.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoBalance
            // 
            this.checkBoxAutoBalance.AutoSize = true;
            this.checkBoxAutoBalance.Location = new System.Drawing.Point(15, 190);
            this.checkBoxAutoBalance.Name = "checkBoxAutoBalance";
            this.checkBoxAutoBalance.Size = new System.Drawing.Size(113, 17);
            this.checkBoxAutoBalance.TabIndex = 45;
            this.checkBoxAutoBalance.Text = "Auto balance lines";
            this.checkBoxAutoBalance.UseVisualStyleBackColor = true;
            // 
            // buttonMultipleReplaceSettings
            // 
            this.buttonMultipleReplaceSettings.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonMultipleReplaceSettings.Location = new System.Drawing.Point(183, 140);
            this.buttonMultipleReplaceSettings.Name = "buttonMultipleReplaceSettings";
            this.buttonMultipleReplaceSettings.Size = new System.Drawing.Size(104, 21);
            this.buttonMultipleReplaceSettings.TabIndex = 30;
            this.buttonMultipleReplaceSettings.Text = "Settings...";
            this.buttonMultipleReplaceSettings.UseVisualStyleBackColor = true;
            this.buttonMultipleReplaceSettings.Click += new System.EventHandler(this.buttonMultipleReplaceSettings_Click);
            // 
            // checkBoxMultipleReplace
            // 
            this.checkBoxMultipleReplace.AutoSize = true;
            this.checkBoxMultipleReplace.Location = new System.Drawing.Point(15, 144);
            this.checkBoxMultipleReplace.Name = "checkBoxMultipleReplace";
            this.checkBoxMultipleReplace.Size = new System.Drawing.Size(100, 17);
            this.checkBoxMultipleReplace.TabIndex = 25;
            this.checkBoxMultipleReplace.Text = "Multiple replace";
            this.checkBoxMultipleReplace.UseVisualStyleBackColor = true;
            // 
            // checkBoxFixCommonErrors
            // 
            this.checkBoxFixCommonErrors.AutoSize = true;
            this.checkBoxFixCommonErrors.Location = new System.Drawing.Point(15, 119);
            this.checkBoxFixCommonErrors.Name = "checkBoxFixCommonErrors";
            this.checkBoxFixCommonErrors.Size = new System.Drawing.Size(111, 17);
            this.checkBoxFixCommonErrors.TabIndex = 15;
            this.checkBoxFixCommonErrors.Text = "Fix common errors";
            this.checkBoxFixCommonErrors.UseVisualStyleBackColor = true;
            // 
            // buttonFixCommonErrorSettings
            // 
            this.buttonFixCommonErrorSettings.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonFixCommonErrorSettings.Location = new System.Drawing.Point(183, 116);
            this.buttonFixCommonErrorSettings.Name = "buttonFixCommonErrorSettings";
            this.buttonFixCommonErrorSettings.Size = new System.Drawing.Size(104, 21);
            this.buttonFixCommonErrorSettings.TabIndex = 20;
            this.buttonFixCommonErrorSettings.Text = "Settings...";
            this.buttonFixCommonErrorSettings.UseVisualStyleBackColor = true;
            this.buttonFixCommonErrorSettings.Click += new System.EventHandler(this.buttonFixCommonErrorSettings_Click);
            // 
            // groupBoxChangeFrameRate
            // 
            this.groupBoxChangeFrameRate.Controls.Add(this.comboBoxFrameRateTo);
            this.groupBoxChangeFrameRate.Controls.Add(this.labelToFrameRate);
            this.groupBoxChangeFrameRate.Controls.Add(this.comboBoxFrameRateFrom);
            this.groupBoxChangeFrameRate.Controls.Add(this.labelFromFrameRate);
            this.groupBoxChangeFrameRate.Location = new System.Drawing.Point(295, 12);
            this.groupBoxChangeFrameRate.Name = "groupBoxChangeFrameRate";
            this.groupBoxChangeFrameRate.Size = new System.Drawing.Size(281, 73);
            this.groupBoxChangeFrameRate.TabIndex = 10;
            this.groupBoxChangeFrameRate.TabStop = false;
            this.groupBoxChangeFrameRate.Text = "Change frame rate";
            // 
            // comboBoxFrameRateTo
            // 
            this.comboBoxFrameRateTo.FormattingEnabled = true;
            this.comboBoxFrameRateTo.Location = new System.Drawing.Point(130, 46);
            this.comboBoxFrameRateTo.Name = "comboBoxFrameRateTo";
            this.comboBoxFrameRateTo.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFrameRateTo.TabIndex = 3;
            // 
            // labelToFrameRate
            // 
            this.labelToFrameRate.AutoSize = true;
            this.labelToFrameRate.Location = new System.Drawing.Point(6, 50);
            this.labelToFrameRate.Name = "labelToFrameRate";
            this.labelToFrameRate.Size = new System.Drawing.Size(70, 13);
            this.labelToFrameRate.TabIndex = 2;
            this.labelToFrameRate.Text = "To frame rate";
            // 
            // comboBoxFrameRateFrom
            // 
            this.comboBoxFrameRateFrom.FormattingEnabled = true;
            this.comboBoxFrameRateFrom.Location = new System.Drawing.Point(130, 17);
            this.comboBoxFrameRateFrom.Name = "comboBoxFrameRateFrom";
            this.comboBoxFrameRateFrom.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFrameRateFrom.TabIndex = 1;
            // 
            // labelFromFrameRate
            // 
            this.labelFromFrameRate.AutoSize = true;
            this.labelFromFrameRate.Location = new System.Drawing.Point(6, 21);
            this.labelFromFrameRate.Name = "labelFromFrameRate";
            this.labelFromFrameRate.Size = new System.Drawing.Size(80, 13);
            this.labelFromFrameRate.TabIndex = 0;
            this.labelFromFrameRate.Text = "From frame rate";
            // 
            // groupBoxOffsetTimeCodes
            // 
            this.groupBoxOffsetTimeCodes.Controls.Add(this.radioButtonShowLater);
            this.groupBoxOffsetTimeCodes.Controls.Add(this.radioButtonShowEarlier);
            this.groupBoxOffsetTimeCodes.Controls.Add(this.timeUpDownAdjust);
            this.groupBoxOffsetTimeCodes.Controls.Add(this.labelHourMinSecMilliSecond);
            this.groupBoxOffsetTimeCodes.Location = new System.Drawing.Point(295, 89);
            this.groupBoxOffsetTimeCodes.Name = "groupBoxOffsetTimeCodes";
            this.groupBoxOffsetTimeCodes.Size = new System.Drawing.Size(143, 119);
            this.groupBoxOffsetTimeCodes.TabIndex = 11;
            this.groupBoxOffsetTimeCodes.TabStop = false;
            this.groupBoxOffsetTimeCodes.Text = "Offset time codes";
            // 
            // radioButtonShowLater
            // 
            this.radioButtonShowLater.AutoSize = true;
            this.radioButtonShowLater.Checked = true;
            this.radioButtonShowLater.Location = new System.Drawing.Point(9, 89);
            this.radioButtonShowLater.Name = "radioButtonShowLater";
            this.radioButtonShowLater.Size = new System.Drawing.Size(75, 17);
            this.radioButtonShowLater.TabIndex = 3;
            this.radioButtonShowLater.TabStop = true;
            this.radioButtonShowLater.Text = "Show later";
            this.radioButtonShowLater.UseVisualStyleBackColor = true;
            // 
            // radioButtonShowEarlier
            // 
            this.radioButtonShowEarlier.AutoSize = true;
            this.radioButtonShowEarlier.Location = new System.Drawing.Point(9, 66);
            this.radioButtonShowEarlier.Name = "radioButtonShowEarlier";
            this.radioButtonShowEarlier.Size = new System.Drawing.Size(83, 17);
            this.radioButtonShowEarlier.TabIndex = 2;
            this.radioButtonShowEarlier.Text = "Show earlier";
            this.radioButtonShowEarlier.UseVisualStyleBackColor = true;
            // 
            // timeUpDownAdjust
            // 
            this.timeUpDownAdjust.AutoSize = true;
            this.timeUpDownAdjust.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownAdjust.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.timeUpDownAdjust.Location = new System.Drawing.Point(7, 37);
            this.timeUpDownAdjust.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownAdjust.Name = "timeUpDownAdjust";
            this.timeUpDownAdjust.Size = new System.Drawing.Size(96, 27);
            this.timeUpDownAdjust.TabIndex = 1;
            timeCode1.Hours = 0;
            timeCode1.Milliseconds = 0;
            timeCode1.Minutes = 0;
            timeCode1.Seconds = 0;
            timeCode1.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode1.TotalMilliseconds = 0D;
            timeCode1.TotalSeconds = 0D;
            this.timeUpDownAdjust.TimeCode = timeCode1;
            this.timeUpDownAdjust.UseVideoOffset = false;
            // 
            // labelHourMinSecMilliSecond
            // 
            this.labelHourMinSecMilliSecond.AutoSize = true;
            this.labelHourMinSecMilliSecond.Location = new System.Drawing.Point(6, 20);
            this.labelHourMinSecMilliSecond.Name = "labelHourMinSecMilliSecond";
            this.labelHourMinSecMilliSecond.Size = new System.Drawing.Size(90, 13);
            this.labelHourMinSecMilliSecond.TabIndex = 0;
            this.labelHourMinSecMilliSecond.Text = "Hours:min:sec.ms";
            // 
            // checkBoxFixCasing
            // 
            this.checkBoxFixCasing.AutoSize = true;
            this.checkBoxFixCasing.Location = new System.Drawing.Point(15, 47);
            this.checkBoxFixCasing.Name = "checkBoxFixCasing";
            this.checkBoxFixCasing.Size = new System.Drawing.Size(95, 17);
            this.checkBoxFixCasing.TabIndex = 1;
            this.checkBoxFixCasing.Text = "Auto-fix casing";
            this.checkBoxFixCasing.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveTextForHI
            // 
            this.checkBoxRemoveTextForHI.AutoSize = true;
            this.checkBoxRemoveTextForHI.Location = new System.Drawing.Point(15, 72);
            this.checkBoxRemoveTextForHI.Name = "checkBoxRemoveTextForHI";
            this.checkBoxRemoveTextForHI.Size = new System.Drawing.Size(115, 17);
            this.checkBoxRemoveTextForHI.TabIndex = 2;
            this.checkBoxRemoveTextForHI.Text = "Remove text for HI";
            this.checkBoxRemoveTextForHI.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveFormatting
            // 
            this.checkBoxRemoveFormatting.AutoSize = true;
            this.checkBoxRemoveFormatting.Location = new System.Drawing.Point(15, 22);
            this.checkBoxRemoveFormatting.Name = "checkBoxRemoveFormatting";
            this.checkBoxRemoveFormatting.Size = new System.Drawing.Size(115, 17);
            this.checkBoxRemoveFormatting.TabIndex = 0;
            this.checkBoxRemoveFormatting.Text = "Remove formatting";
            this.checkBoxRemoveFormatting.UseVisualStyleBackColor = true;
            // 
            // groupBoxOutput
            // 
            this.groupBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOutput.Controls.Add(this.checkBoxUseStyleFromSource);
            this.groupBoxOutput.Controls.Add(this.checkBoxOverwriteOriginalFiles);
            this.groupBoxOutput.Controls.Add(this.linkLabelOpenOutputFolder);
            this.groupBoxOutput.Controls.Add(this.buttonStyles);
            this.groupBoxOutput.Controls.Add(this.checkBoxOverwrite);
            this.groupBoxOutput.Controls.Add(this.comboBoxSubtitleFormats);
            this.groupBoxOutput.Controls.Add(this.labelEncoding);
            this.groupBoxOutput.Controls.Add(this.comboBoxEncoding);
            this.groupBoxOutput.Controls.Add(this.labelOutputFormat);
            this.groupBoxOutput.Controls.Add(this.groupBoxConvertOptions);
            this.groupBoxOutput.Controls.Add(this.labelChooseOutputFolder);
            this.groupBoxOutput.Controls.Add(this.buttonChooseFolder);
            this.groupBoxOutput.Controls.Add(this.textBoxOutputFolder);
            this.groupBoxOutput.Location = new System.Drawing.Point(12, 326);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(1014, 259);
            this.groupBoxOutput.TabIndex = 1;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "Output";
            // 
            // checkBoxUseStyleFromSource
            // 
            this.checkBoxUseStyleFromSource.AutoSize = true;
            this.checkBoxUseStyleFromSource.Location = new System.Drawing.Point(80, 170);
            this.checkBoxUseStyleFromSource.Name = "checkBoxUseStyleFromSource";
            this.checkBoxUseStyleFromSource.Size = new System.Drawing.Size(127, 17);
            this.checkBoxUseStyleFromSource.TabIndex = 9;
            this.checkBoxUseStyleFromSource.Text = "Use style from source";
            this.checkBoxUseStyleFromSource.UseVisualStyleBackColor = true;
            this.checkBoxUseStyleFromSource.Visible = false;
            // 
            // checkBoxOverwriteOriginalFiles
            // 
            this.checkBoxOverwriteOriginalFiles.AutoSize = true;
            this.checkBoxOverwriteOriginalFiles.Location = new System.Drawing.Point(9, 19);
            this.checkBoxOverwriteOriginalFiles.Name = "checkBoxOverwriteOriginalFiles";
            this.checkBoxOverwriteOriginalFiles.Size = new System.Drawing.Size(300, 17);
            this.checkBoxOverwriteOriginalFiles.TabIndex = 0;
            this.checkBoxOverwriteOriginalFiles.Text = "Overwrite original files (new extension if format is changed)";
            this.checkBoxOverwriteOriginalFiles.UseVisualStyleBackColor = true;
            this.checkBoxOverwriteOriginalFiles.CheckedChanged += new System.EventHandler(this.checkBoxOverwriteOriginalFiles_CheckedChanged);
            // 
            // linkLabelOpenOutputFolder
            // 
            this.linkLabelOpenOutputFolder.AutoSize = true;
            this.linkLabelOpenOutputFolder.Location = new System.Drawing.Point(368, 74);
            this.linkLabelOpenOutputFolder.Name = "linkLabelOpenOutputFolder";
            this.linkLabelOpenOutputFolder.Size = new System.Drawing.Size(42, 13);
            this.linkLabelOpenOutputFolder.TabIndex = 4;
            this.linkLabelOpenOutputFolder.TabStop = true;
            this.linkLabelOpenOutputFolder.Text = "Open...";
            this.linkLabelOpenOutputFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelOpenOutputFolderLinkClicked);
            // 
            // buttonStyles
            // 
            this.buttonStyles.Location = new System.Drawing.Point(300, 141);
            this.buttonStyles.Name = "buttonStyles";
            this.buttonStyles.Size = new System.Drawing.Size(116, 23);
            this.buttonStyles.TabIndex = 8;
            this.buttonStyles.Text = "Style...";
            this.buttonStyles.UseVisualStyleBackColor = true;
            this.buttonStyles.Visible = false;
            this.buttonStyles.Click += new System.EventHandler(this.ButtonStylesClick);
            // 
            // checkBoxOverwrite
            // 
            this.checkBoxOverwrite.AutoSize = true;
            this.checkBoxOverwrite.Location = new System.Drawing.Point(13, 97);
            this.checkBoxOverwrite.Name = "checkBoxOverwrite";
            this.checkBoxOverwrite.Size = new System.Drawing.Size(125, 17);
            this.checkBoxOverwrite.TabIndex = 5;
            this.checkBoxOverwrite.Text = "Overwrite exiting files";
            this.checkBoxOverwrite.UseVisualStyleBackColor = true;
            // 
            // comboBoxSubtitleFormats
            // 
            this.comboBoxSubtitleFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFormats.FormattingEnabled = true;
            this.comboBoxSubtitleFormats.Location = new System.Drawing.Point(80, 143);
            this.comboBoxSubtitleFormats.Name = "comboBoxSubtitleFormats";
            this.comboBoxSubtitleFormats.Size = new System.Drawing.Size(214, 21);
            this.comboBoxSubtitleFormats.TabIndex = 7;
            this.comboBoxSubtitleFormats.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSubtitleFormatsSelectedIndexChanged);
            // 
            // labelEncoding
            // 
            this.labelEncoding.AutoSize = true;
            this.labelEncoding.Location = new System.Drawing.Point(10, 201);
            this.labelEncoding.Name = "labelEncoding";
            this.labelEncoding.Size = new System.Drawing.Size(52, 13);
            this.labelEncoding.TabIndex = 9;
            this.labelEncoding.Text = "Encoding";
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncoding.FormattingEnabled = true;
            this.comboBoxEncoding.Location = new System.Drawing.Point(80, 198);
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.Size = new System.Drawing.Size(214, 21);
            this.comboBoxEncoding.TabIndex = 10;
            // 
            // labelOutputFormat
            // 
            this.labelOutputFormat.AutoSize = true;
            this.labelOutputFormat.Location = new System.Drawing.Point(10, 146);
            this.labelOutputFormat.Name = "labelOutputFormat";
            this.labelOutputFormat.Size = new System.Drawing.Size(39, 13);
            this.labelOutputFormat.TabIndex = 6;
            this.labelOutputFormat.Text = "Format";
            // 
            // labelChooseOutputFolder
            // 
            this.labelChooseOutputFolder.AutoSize = true;
            this.labelChooseOutputFolder.Location = new System.Drawing.Point(10, 55);
            this.labelChooseOutputFolder.Name = "labelChooseOutputFolder";
            this.labelChooseOutputFolder.Size = new System.Drawing.Size(105, 13);
            this.labelChooseOutputFolder.TabIndex = 1;
            this.labelChooseOutputFolder.Text = "Choose output folder";
            // 
            // buttonChooseFolder
            // 
            this.buttonChooseFolder.Location = new System.Drawing.Point(338, 69);
            this.buttonChooseFolder.Name = "buttonChooseFolder";
            this.buttonChooseFolder.Size = new System.Drawing.Size(26, 23);
            this.buttonChooseFolder.TabIndex = 3;
            this.buttonChooseFolder.Text = "...";
            this.buttonChooseFolder.UseVisualStyleBackColor = true;
            this.buttonChooseFolder.Click += new System.EventHandler(this.buttonChooseFolder_Click);
            // 
            // textBoxOutputFolder
            // 
            this.textBoxOutputFolder.Location = new System.Drawing.Point(11, 71);
            this.textBoxOutputFolder.Name = "textBoxOutputFolder";
            this.textBoxOutputFolder.Size = new System.Drawing.Size(322, 20);
            this.textBoxOutputFolder.TabIndex = 2;
            // 
            // groupBoxInput
            // 
            this.groupBoxInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxInput.Controls.Add(this.textBoxFilter);
            this.groupBoxInput.Controls.Add(this.labelFilter);
            this.groupBoxInput.Controls.Add(this.comboBoxFilter);
            this.groupBoxInput.Controls.Add(this.checkBoxScanFolderRecursive);
            this.groupBoxInput.Controls.Add(this.buttonSearchFolder);
            this.groupBoxInput.Controls.Add(this.buttonInputBrowse);
            this.groupBoxInput.Controls.Add(this.labelChooseInputFiles);
            this.groupBoxInput.Controls.Add(this.listViewInputFiles);
            this.groupBoxInput.Location = new System.Drawing.Point(15, 12);
            this.groupBoxInput.Name = "groupBoxInput";
            this.groupBoxInput.Size = new System.Drawing.Size(1011, 308);
            this.groupBoxInput.TabIndex = 0;
            this.groupBoxInput.TabStop = false;
            this.groupBoxInput.Text = "Input";
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxFilter.Location = new System.Drawing.Point(422, 279);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(158, 20);
            this.textBoxFilter.TabIndex = 13;
            // 
            // labelFilter
            // 
            this.labelFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFilter.AutoSize = true;
            this.labelFilter.Location = new System.Drawing.Point(11, 282);
            this.labelFilter.Name = "labelFilter";
            this.labelFilter.Size = new System.Drawing.Size(29, 13);
            this.labelFilter.TabIndex = 11;
            this.labelFilter.Text = "Filter";
            // 
            // comboBoxFilter
            // 
            this.comboBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFilter.FormattingEnabled = true;
            this.comboBoxFilter.Items.AddRange(new object[] {
            "All files",
            "SubRip .srt files without BOM header",
            "Files with subtitle with more than two lines",
            "Files that contains..."});
            this.comboBoxFilter.Location = new System.Drawing.Point(81, 279);
            this.comboBoxFilter.Name = "comboBoxFilter";
            this.comboBoxFilter.Size = new System.Drawing.Size(335, 21);
            this.comboBoxFilter.TabIndex = 12;
            this.comboBoxFilter.SelectedIndexChanged += new System.EventHandler(this.comboBoxFilter_SelectedIndexChanged);
            // 
            // checkBoxScanFolderRecursive
            // 
            this.checkBoxScanFolderRecursive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxScanFolderRecursive.AutoSize = true;
            this.checkBoxScanFolderRecursive.Checked = true;
            this.checkBoxScanFolderRecursive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxScanFolderRecursive.Location = new System.Drawing.Point(775, 16);
            this.checkBoxScanFolderRecursive.Name = "checkBoxScanFolderRecursive";
            this.checkBoxScanFolderRecursive.Size = new System.Drawing.Size(74, 17);
            this.checkBoxScanFolderRecursive.TabIndex = 0;
            this.checkBoxScanFolderRecursive.Text = "Recursive";
            this.checkBoxScanFolderRecursive.UseVisualStyleBackColor = true;
            // 
            // buttonSearchFolder
            // 
            this.buttonSearchFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSearchFolder.Location = new System.Drawing.Point(855, 12);
            this.buttonSearchFolder.Name = "buttonSearchFolder";
            this.buttonSearchFolder.Size = new System.Drawing.Size(116, 23);
            this.buttonSearchFolder.TabIndex = 1;
            this.buttonSearchFolder.Text = "Search folder...";
            this.buttonSearchFolder.UseVisualStyleBackColor = true;
            this.buttonSearchFolder.Click += new System.EventHandler(this.buttonSearchFolder_Click);
            // 
            // buttonInputBrowse
            // 
            this.buttonInputBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInputBrowse.Location = new System.Drawing.Point(976, 41);
            this.buttonInputBrowse.Name = "buttonInputBrowse";
            this.buttonInputBrowse.Size = new System.Drawing.Size(26, 23);
            this.buttonInputBrowse.TabIndex = 3;
            this.buttonInputBrowse.Text = "...";
            this.buttonInputBrowse.UseVisualStyleBackColor = true;
            this.buttonInputBrowse.Click += new System.EventHandler(this.buttonInputBrowse_Click);
            // 
            // labelChooseInputFiles
            // 
            this.labelChooseInputFiles.AutoSize = true;
            this.labelChooseInputFiles.Location = new System.Drawing.Point(5, 25);
            this.labelChooseInputFiles.Name = "labelChooseInputFiles";
            this.labelChooseInputFiles.Size = new System.Drawing.Size(202, 13);
            this.labelChooseInputFiles.TabIndex = 0;
            this.labelChooseInputFiles.Text = "Choose input files (browse or drag-n-drop)";
            // 
            // listViewInputFiles
            // 
            this.listViewInputFiles.AllowDrop = true;
            this.listViewInputFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewInputFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFName,
            this.columnHeaderSize,
            this.columnHeaderFormat,
            this.columnHeaderStatus});
            this.listViewInputFiles.ContextMenuStrip = this.contextMenuStripFiles;
            this.listViewInputFiles.FullRowSelect = true;
            this.listViewInputFiles.HideSelection = false;
            this.listViewInputFiles.Location = new System.Drawing.Point(6, 41);
            this.listViewInputFiles.Name = "listViewInputFiles";
            this.listViewInputFiles.Size = new System.Drawing.Size(965, 232);
            this.listViewInputFiles.TabIndex = 2;
            this.listViewInputFiles.UseCompatibleStateImageBehavior = false;
            this.listViewInputFiles.View = System.Windows.Forms.View.Details;
            this.listViewInputFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewInputFiles_DragDrop);
            this.listViewInputFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewInputFiles_DragEnter);
            this.listViewInputFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListViewInputFilesKeyDown);
            // 
            // columnHeaderFName
            // 
            this.columnHeaderFName.Text = "File name";
            this.columnHeaderFName.Width = 500;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.Width = 75;
            // 
            // columnHeaderFormat
            // 
            this.columnHeaderFormat.Text = "Format";
            this.columnHeaderFormat.Width = 200;
            // 
            // columnHeaderStatus
            // 
            this.columnHeaderStatus.Text = "Status";
            this.columnHeaderStatus.Width = 124;
            // 
            // contextMenuStripFiles
            // 
            this.contextMenuStripFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeToolStripMenuItem,
            this.removeAllToolStripMenuItem});
            this.contextMenuStripFiles.Name = "contextMenuStripStyles";
            this.contextMenuStripFiles.Size = new System.Drawing.Size(133, 48);
            this.contextMenuStripFiles.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripFilesOpening);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.RemoveToolStripMenuItemClick);
            // 
            // removeAllToolStripMenuItem
            // 
            this.removeAllToolStripMenuItem.Name = "removeAllToolStripMenuItem";
            this.removeAllToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.removeAllToolStripMenuItem.Text = "Remove all";
            this.removeAllToolStripMenuItem.Click += new System.EventHandler(this.RemoveAllToolStripMenuItemClick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(15, 607);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(820, 10);
            this.progressBar1.TabIndex = 8;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 591);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 9;
            this.labelStatus.Text = "labelStatus";
            // 
            // BatchConvert
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1035, 631);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.groupBoxOutput);
            this.Controls.Add(this.groupBoxInput);
            this.Controls.Add(this.buttonConvert);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1024, 578);
            this.Name = "BatchConvert";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Batch convert";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BatchConvert_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BatchConvert_KeyDown);
            this.groupBoxConvertOptions.ResumeLayout(false);
            this.groupBoxConvertOptions.PerformLayout();
            this.groupBoxSpeed.ResumeLayout(false);
            this.groupBoxSpeed.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPercent)).EndInit();
            this.groupBoxChangeFrameRate.ResumeLayout(false);
            this.groupBoxChangeFrameRate.PerformLayout();
            this.groupBoxOffsetTimeCodes.ResumeLayout(false);
            this.groupBoxOffsetTimeCodes.PerformLayout();
            this.groupBoxOutput.ResumeLayout(false);
            this.groupBoxOutput.PerformLayout();
            this.groupBoxInput.ResumeLayout(false);
            this.groupBoxInput.PerformLayout();
            this.contextMenuStripFiles.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonConvert;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxConvertOptions;
        private System.Windows.Forms.CheckBox checkBoxFixCasing;
        private System.Windows.Forms.CheckBox checkBoxRemoveTextForHI;
        private System.Windows.Forms.CheckBox checkBoxRemoveFormatting;
        private System.Windows.Forms.GroupBox groupBoxOutput;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFormats;
        private System.Windows.Forms.Label labelEncoding;
        private System.Windows.Forms.ComboBox comboBoxEncoding;
        private System.Windows.Forms.Label labelOutputFormat;
        private System.Windows.Forms.Label labelChooseOutputFolder;
        private System.Windows.Forms.Button buttonChooseFolder;
        private System.Windows.Forms.TextBox textBoxOutputFolder;
        private System.Windows.Forms.GroupBox groupBoxInput;
        private System.Windows.Forms.Button buttonInputBrowse;
        private System.Windows.Forms.Label labelChooseInputFiles;
        private System.Windows.Forms.ListView listViewInputFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFName;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderFormat;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private System.Windows.Forms.CheckBox checkBoxOverwrite;
        private System.Windows.Forms.Button buttonStyles;
        private System.Windows.Forms.GroupBox groupBoxOffsetTimeCodes;
        private System.Windows.Forms.GroupBox groupBoxChangeFrameRate;
        private Controls.TimeUpDown timeUpDownAdjust;
        private System.Windows.Forms.Label labelHourMinSecMilliSecond;
        private System.Windows.Forms.ComboBox comboBoxFrameRateTo;
        private System.Windows.Forms.Label labelToFrameRate;
        private System.Windows.Forms.ComboBox comboBoxFrameRateFrom;
        private System.Windows.Forms.Label labelFromFrameRate;
        private System.Windows.Forms.LinkLabel linkLabelOpenOutputFolder;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFiles;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAllToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxFixCommonErrors;
        private System.Windows.Forms.Button buttonFixCommonErrorSettings;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.RadioButton radioButtonShowLater;
        private System.Windows.Forms.RadioButton radioButtonShowEarlier;
        private System.Windows.Forms.Button buttonMultipleReplaceSettings;
        private System.Windows.Forms.CheckBox checkBoxMultipleReplace;
        private System.Windows.Forms.CheckBox checkBoxAutoBalance;
        private System.Windows.Forms.CheckBox checkBoxOverwriteOriginalFiles;
        private System.Windows.Forms.Button buttonSearchFolder;
        private System.Windows.Forms.CheckBox checkBoxSetMinimumDisplayTimeBetweenSubs;
        private System.Windows.Forms.CheckBox checkBoxScanFolderRecursive;
        private System.Windows.Forms.Button buttonRemoveTextForHiSettings;
        private System.Windows.Forms.CheckBox checkBoxSplitLongLines;
        private System.Windows.Forms.Label labelFilter;
        private System.Windows.Forms.ComboBox comboBoxFilter;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.GroupBox groupBoxSpeed;
        private System.Windows.Forms.NumericUpDown numericUpDownPercent;
        private System.Windows.Forms.Label labelPercent;
        private System.Windows.Forms.RadioButton radioButtonToDropFrame;
        private System.Windows.Forms.RadioButton radioButtonSpeedFromDropFrame;
        private System.Windows.Forms.RadioButton radioButtonSpeedCustom;
        private System.Windows.Forms.CheckBox checkBoxUseStyleFromSource;
        private System.Windows.Forms.CheckBox checkBoxBridgeGaps;
        private System.Windows.Forms.Button buttonBridgeGapsSettings;
    }
}