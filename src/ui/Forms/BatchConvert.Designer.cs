﻿using Nikse.SubtitleEdit.Core.Common;

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
            Nikse.SubtitleEdit.Core.Common.TimeCode timeCode1 = new Nikse.SubtitleEdit.Core.Common.TimeCode();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxConvertOptions = new System.Windows.Forms.GroupBox();
            this.groupBoxAutoTranslate = new System.Windows.Forms.GroupBox();
            this.labelTarget = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxTarget = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelSource = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxSource = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.linkLabelPoweredBy = new System.Windows.Forms.LinkLabel();
            this.nikseComboBoxEngine = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.listViewConvertOptions = new System.Windows.Forms.ListView();
            this.ActionCheckBox = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Action = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.inverseSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonConvertOptionsSettings = new System.Windows.Forms.Button();
            this.groupBoxDeleteLines = new System.Windows.Forms.GroupBox();
            this.textBoxDeleteContains = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelDeleteLinesContaining = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.numericUpDownDeleteLast = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelDeleteLastLines = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.numericUpDownDeleteFirst = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelDeleteFirstLines = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.groupBoxRemoveStyle = new System.Windows.Forms.GroupBox();
            this.textBoxRemoveStyle = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelStyleActor = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.groupBoxOffsetTimeCodes = new System.Windows.Forms.GroupBox();
            this.radioButtonShowLater = new System.Windows.Forms.RadioButton();
            this.radioButtonShowEarlier = new System.Windows.Forms.RadioButton();
            this.timeUpDownAdjust = new Nikse.SubtitleEdit.Controls.NikseTimeUpDown();
            this.labelHourMinSecMilliSecond = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.groupBoxChangeFrameRate = new System.Windows.Forms.GroupBox();
            this.buttonSwapFrameRate = new System.Windows.Forms.Button();
            this.comboBoxFrameRateTo = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelToFrameRate = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxFrameRateFrom = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelFromFrameRate = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.groupBoxFixRtl = new System.Windows.Forms.GroupBox();
            this.radioButtonReverseStartEnd = new System.Windows.Forms.RadioButton();
            this.radioButtonRemoveUnicode = new System.Windows.Forms.RadioButton();
            this.radioButtonAddUnicode = new System.Windows.Forms.RadioButton();
            this.groupBoxSpeed = new System.Windows.Forms.GroupBox();
            this.radioButtonToDropFrame = new System.Windows.Forms.RadioButton();
            this.radioButtonSpeedFromDropFrame = new System.Windows.Forms.RadioButton();
            this.radioButtonSpeedCustom = new System.Windows.Forms.RadioButton();
            this.numericUpDownPercent = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelPercent = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.groupBoxRemoveFormatting = new System.Windows.Forms.GroupBox();
            this.checkBoxRemoveAllFormatting = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveAlignment = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveFontName = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveColor = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveUnderline = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveItalic = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveBold = new System.Windows.Forms.CheckBox();
            this.groupBoxApplyDurationLimits = new System.Windows.Forms.GroupBox();
            this.checkBoxApplyDurationLimitsMaxDuration = new System.Windows.Forms.CheckBox();
            this.numericUpDownApplyDurationLimitsMaxDuration = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.checkBoxApplyDurationLimitsCheckShotChanges = new System.Windows.Forms.CheckBox();
            this.numericUpDownApplyDurationLimitsMinDuration = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.checkBoxApplyDurationLimitsMinDuration = new System.Windows.Forms.CheckBox();
            this.groupBoxAdjustDuration = new System.Windows.Forms.GroupBox();
            this.checkBoxAdjustDurationCheckShotChanges = new System.Windows.Forms.CheckBox();
            this.checkBoxEnforceDurationLimits = new System.Windows.Forms.CheckBox();
            this.comboBoxAdjustDurationVia = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelAdjustDurationVia = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.panelAdjustDurationFixed = new System.Windows.Forms.Panel();
            this.numericUpDownFixedMilliseconds = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMillisecondsFixed = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.panelAdjustDurationAddPercent = new System.Windows.Forms.Panel();
            this.label1 = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.numericUpDownAdjustViaPercent = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelAdjustViaPercent = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.panelAdjustDurationAddSeconds = new System.Windows.Forms.Panel();
            this.numericUpDownSeconds = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelAddSeconds = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.panelAdjustDurationRecalc = new System.Windows.Forms.Panel();
            this.checkBoxExtendOnly = new System.Windows.Forms.CheckBox();
            this.numericUpDownOptimalCharsSec = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelOptimalCharsSec = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.numericUpDownMaxCharsSec = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMaxCharsPerSecond = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.groupBoxBeautifyTimeCodes = new System.Windows.Forms.GroupBox();
            this.buttonBeautifyTimeCodesEditProfile = new System.Windows.Forms.Button();
            this.checkBoxBeautifyTimeCodesSnapToShotChanges = new System.Windows.Forms.CheckBox();
            this.checkBoxBeautifyTimeCodesUseExactTimeCodes = new System.Windows.Forms.CheckBox();
            this.checkBoxBeautifyTimeCodesAlignTimeCodes = new System.Windows.Forms.CheckBox();
            this.groupBoxChangeCasing = new System.Windows.Forms.GroupBox();
            this.checkBoxProperCaseOnlyUpper = new System.Windows.Forms.CheckBox();
            this.radioButtonProperCase = new System.Windows.Forms.RadioButton();
            this.checkBoxOnlyAllUpper = new System.Windows.Forms.CheckBox();
            this.checkBoxFixNames = new System.Windows.Forms.CheckBox();
            this.radioButtonFixOnlyNames = new System.Windows.Forms.RadioButton();
            this.radioButtonLowercase = new System.Windows.Forms.RadioButton();
            this.radioButtonUppercase = new System.Windows.Forms.RadioButton();
            this.radioButtonNormal = new System.Windows.Forms.RadioButton();
            this.groupBoxMergeShortLines = new System.Windows.Forms.GroupBox();
            this.checkBoxOnlyContinuationLines = new System.Windows.Forms.CheckBox();
            this.numericUpDownMaxCharacters = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownMaxMillisecondsBetweenLines = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMaxMillisecondsBetweenLines = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelMaxCharacters = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.groupBoxAssaChangeRes = new System.Windows.Forms.GroupBox();
            this.checkBoxDrawing = new System.Windows.Forms.CheckBox();
            this.checkBoxPosition = new System.Windows.Forms.CheckBox();
            this.checkBoxFontSize = new System.Windows.Forms.CheckBox();
            this.checkBoxMargins = new System.Windows.Forms.CheckBox();
            this.labelTargetRes = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.numericUpDownTargetHeight = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.buttonGetResolutionFromVideo = new System.Windows.Forms.Button();
            this.labelX = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.numericUpDownTargetWidth = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.groupBoxSortBy = new System.Windows.Forms.GroupBox();
            this.comboBoxSortBy = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.groupBoxMergeSameTimeCodes = new System.Windows.Forms.GroupBox();
            this.checkBoxMergeSameTimeCodesReBreakLines = new System.Windows.Forms.CheckBox();
            this.checkBoxMergeSameTimeCodesMakeDialog = new System.Windows.Forms.CheckBox();
            this.numericUpDownMergeSameTimeCodesMaxDifference = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMergeSameTimeCodesMaxDifference = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.groupBoxConvertColorsToDialog = new System.Windows.Forms.GroupBox();
            this.checkBoxConvertColorsToDialogReBreakLines = new System.Windows.Forms.CheckBox();
            this.checkBoxConvertColorsToDialogAddNewLines = new System.Windows.Forms.CheckBox();
            this.checkBoxConvertColorsToDialogRemoveColorTags = new System.Windows.Forms.CheckBox();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.buttonBrowseEncoding = new System.Windows.Forms.Button();
            this.radioButtonSaveInOutputFolder = new System.Windows.Forms.RadioButton();
            this.buttonTransportStreamSettings = new System.Windows.Forms.Button();
            this.linkLabelOpenOutputFolder = new System.Windows.Forms.LinkLabel();
            this.checkBoxUseStyleFromSource = new System.Windows.Forms.CheckBox();
            this.checkBoxOverwrite = new System.Windows.Forms.CheckBox();
            this.buttonStyles = new System.Windows.Forms.Button();
            this.buttonChooseFolder = new System.Windows.Forms.Button();
            this.comboBoxSubtitleFormats = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.textBoxOutputFolder = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelEncoding = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.radioButtonSaveInSourceFolder = new System.Windows.Forms.RadioButton();
            this.comboBoxEncoding = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelOutputFormat = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.groupBoxInput = new System.Windows.Forms.GroupBox();
            this.labelNumberOfFiles = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.textBoxFilter = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelFilter = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxFilter = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxScanFolderRecursive = new System.Windows.Forms.CheckBox();
            this.buttonSearchFolder = new System.Windows.Forms.Button();
            this.buttonInputBrowse = new System.Windows.Forms.Button();
            this.labelChooseInputFiles = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.listViewInputFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.convertMkvSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOcrEngine = new System.Windows.Forms.ToolStripMenuItem();
            this.alsoScanVideoFilesInSearchFolderslowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelError = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.nikseComboBoxTranslateModel = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.nikseLabelModel = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.groupBoxConvertOptions.SuspendLayout();
            this.groupBoxAutoTranslate.SuspendLayout();
            this.contextMenuStripOptions.SuspendLayout();
            this.groupBoxDeleteLines.SuspendLayout();
            this.groupBoxRemoveStyle.SuspendLayout();
            this.groupBoxOffsetTimeCodes.SuspendLayout();
            this.groupBoxChangeFrameRate.SuspendLayout();
            this.groupBoxFixRtl.SuspendLayout();
            this.groupBoxSpeed.SuspendLayout();
            this.groupBoxRemoveFormatting.SuspendLayout();
            this.groupBoxApplyDurationLimits.SuspendLayout();
            this.groupBoxAdjustDuration.SuspendLayout();
            this.panelAdjustDurationFixed.SuspendLayout();
            this.panelAdjustDurationAddPercent.SuspendLayout();
            this.panelAdjustDurationAddSeconds.SuspendLayout();
            this.panelAdjustDurationRecalc.SuspendLayout();
            this.groupBoxBeautifyTimeCodes.SuspendLayout();
            this.groupBoxChangeCasing.SuspendLayout();
            this.groupBoxMergeShortLines.SuspendLayout();
            this.groupBoxAssaChangeRes.SuspendLayout();
            this.groupBoxSortBy.SuspendLayout();
            this.groupBoxMergeSameTimeCodes.SuspendLayout();
            this.groupBoxConvertColorsToDialog.SuspendLayout();
            this.groupBoxOutput.SuspendLayout();
            this.groupBoxInput.SuspendLayout();
            this.contextMenuStripFiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonConvert
            // 
            this.buttonConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConvert.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonConvert.Location = new System.Drawing.Point(844, 618);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(98, 23);
            this.buttonConvert.TabIndex = 20;
            this.buttonConvert.Text = "&Convert";
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.buttonConvert_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(948, 618);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 21;
            this.buttonCancel.Text = "&Done";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBoxConvertOptions
            // 
            this.groupBoxConvertOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxAutoTranslate);
            this.groupBoxConvertOptions.Controls.Add(this.listViewConvertOptions);
            this.groupBoxConvertOptions.Controls.Add(this.buttonConvertOptionsSettings);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxDeleteLines);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxRemoveStyle);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxOffsetTimeCodes);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxChangeFrameRate);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxFixRtl);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxSpeed);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxRemoveFormatting);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxApplyDurationLimits);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxAdjustDuration);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxBeautifyTimeCodes);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxChangeCasing);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxMergeShortLines);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxAssaChangeRes);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxSortBy);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxMergeSameTimeCodes);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxConvertColorsToDialog);
            this.groupBoxConvertOptions.Location = new System.Drawing.Point(422, 19);
            this.groupBoxConvertOptions.Name = "groupBoxConvertOptions";
            this.groupBoxConvertOptions.Size = new System.Drawing.Size(583, 275);
            this.groupBoxConvertOptions.TabIndex = 15;
            this.groupBoxConvertOptions.TabStop = false;
            this.groupBoxConvertOptions.Text = "Convert options";
            // 
            // groupBoxAutoTranslate
            // 
            this.groupBoxAutoTranslate.Controls.Add(this.nikseLabelModel);
            this.groupBoxAutoTranslate.Controls.Add(this.nikseComboBoxTranslateModel);
            this.groupBoxAutoTranslate.Controls.Add(this.labelTarget);
            this.groupBoxAutoTranslate.Controls.Add(this.comboBoxTarget);
            this.groupBoxAutoTranslate.Controls.Add(this.labelSource);
            this.groupBoxAutoTranslate.Controls.Add(this.comboBoxSource);
            this.groupBoxAutoTranslate.Controls.Add(this.linkLabelPoweredBy);
            this.groupBoxAutoTranslate.Controls.Add(this.nikseComboBoxEngine);
            this.groupBoxAutoTranslate.Location = new System.Drawing.Point(306, 14);
            this.groupBoxAutoTranslate.Name = "groupBoxAutoTranslate";
            this.groupBoxAutoTranslate.Size = new System.Drawing.Size(271, 220);
            this.groupBoxAutoTranslate.TabIndex = 309;
            this.groupBoxAutoTranslate.TabStop = false;
            this.groupBoxAutoTranslate.Text = "Translate";
            this.groupBoxAutoTranslate.Visible = false;
            // 
            // labelTarget
            // 
            this.labelTarget.AutoSize = true;
            this.labelTarget.Location = new System.Drawing.Point(82, 124);
            this.labelTarget.Name = "labelTarget";
            this.labelTarget.Size = new System.Drawing.Size(23, 13);
            this.labelTarget.TabIndex = 115;
            this.labelTarget.Text = "To:";
            // 
            // comboBoxTarget
            // 
            this.comboBoxTarget.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxTarget.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxTarget.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxTarget.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxTarget.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxTarget.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxTarget.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxTarget.DropDownHeight = 400;
            this.comboBoxTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTarget.DropDownWidth = 140;
            this.comboBoxTarget.FormattingEnabled = true;
            this.comboBoxTarget.Location = new System.Drawing.Point(111, 120);
            this.comboBoxTarget.MaxLength = 32767;
            this.comboBoxTarget.Name = "comboBoxTarget";
            this.comboBoxTarget.SelectedIndex = -1;
            this.comboBoxTarget.SelectedItem = null;
            this.comboBoxTarget.SelectedText = "";
            this.comboBoxTarget.Size = new System.Drawing.Size(121, 21);
            this.comboBoxTarget.TabIndex = 114;
            this.comboBoxTarget.UsePopupWindow = false;
            // 
            // labelSource
            // 
            this.labelSource.AutoSize = true;
            this.labelSource.Location = new System.Drawing.Point(69, 95);
            this.labelSource.Name = "labelSource";
            this.labelSource.Size = new System.Drawing.Size(33, 13);
            this.labelSource.TabIndex = 113;
            this.labelSource.Text = "From:";
            // 
            // comboBoxSource
            // 
            this.comboBoxSource.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSource.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSource.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSource.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSource.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSource.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSource.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSource.DropDownHeight = 400;
            this.comboBoxSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSource.DropDownWidth = 140;
            this.comboBoxSource.FormattingEnabled = true;
            this.comboBoxSource.Location = new System.Drawing.Point(111, 91);
            this.comboBoxSource.MaxLength = 32767;
            this.comboBoxSource.Name = "comboBoxSource";
            this.comboBoxSource.SelectedIndex = -1;
            this.comboBoxSource.SelectedItem = null;
            this.comboBoxSource.SelectedText = "";
            this.comboBoxSource.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSource.TabIndex = 112;
            this.comboBoxSource.UsePopupWindow = false;
            // 
            // linkLabelPoweredBy
            // 
            this.linkLabelPoweredBy.AutoSize = true;
            this.linkLabelPoweredBy.Location = new System.Drawing.Point(13, 28);
            this.linkLabelPoweredBy.Name = "linkLabelPoweredBy";
            this.linkLabelPoweredBy.Size = new System.Drawing.Size(73, 13);
            this.linkLabelPoweredBy.TabIndex = 111;
            this.linkLabelPoweredBy.TabStop = true;
            this.linkLabelPoweredBy.Text = "Powered by X";
            this.linkLabelPoweredBy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelPoweredBy_LinkClicked);
            // 
            // nikseComboBoxEngine
            // 
            this.nikseComboBoxEngine.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxEngine.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxEngine.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxEngine.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxEngine.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxEngine.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxEngine.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxEngine.DropDownHeight = 400;
            this.nikseComboBoxEngine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nikseComboBoxEngine.DropDownWidth = 221;
            this.nikseComboBoxEngine.FormattingEnabled = true;
            this.nikseComboBoxEngine.Location = new System.Drawing.Point(11, 52);
            this.nikseComboBoxEngine.MaxLength = 32767;
            this.nikseComboBoxEngine.Name = "nikseComboBoxEngine";
            this.nikseComboBoxEngine.SelectedIndex = -1;
            this.nikseComboBoxEngine.SelectedItem = null;
            this.nikseComboBoxEngine.SelectedText = "";
            this.nikseComboBoxEngine.Size = new System.Drawing.Size(221, 21);
            this.nikseComboBoxEngine.TabIndex = 110;
            this.nikseComboBoxEngine.UsePopupWindow = false;
            this.nikseComboBoxEngine.SelectedIndexChanged += new System.EventHandler(this.nikseComboBoxEngine_SelectedIndexChanged);
            // 
            // listViewConvertOptions
            // 
            this.listViewConvertOptions.CheckBoxes = true;
            this.listViewConvertOptions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ActionCheckBox,
            this.Action});
            this.listViewConvertOptions.ContextMenuStrip = this.contextMenuStripOptions;
            this.listViewConvertOptions.FullRowSelect = true;
            this.listViewConvertOptions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewConvertOptions.HideSelection = false;
            this.listViewConvertOptions.Location = new System.Drawing.Point(6, 17);
            this.listViewConvertOptions.MultiSelect = false;
            this.listViewConvertOptions.Name = "listViewConvertOptions";
            this.listViewConvertOptions.Size = new System.Drawing.Size(293, 252);
            this.listViewConvertOptions.TabIndex = 301;
            this.listViewConvertOptions.UseCompatibleStateImageBehavior = false;
            this.listViewConvertOptions.View = System.Windows.Forms.View.Details;
            this.listViewConvertOptions.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewConvertOptions_ItemChecked);
            this.listViewConvertOptions.SelectedIndexChanged += new System.EventHandler(this.listViewConvertOptions_SelectedIndexChanged);
            // 
            // ActionCheckBox
            // 
            this.ActionCheckBox.Width = 30;
            // 
            // Action
            // 
            this.Action.Width = 400;
            // 
            // contextMenuStripOptions
            // 
            this.contextMenuStripOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSelectAll,
            this.inverseSelectionToolStripMenuItem});
            this.contextMenuStripOptions.Name = "contextMenuStripOptions";
            this.contextMenuStripOptions.Size = new System.Drawing.Size(162, 48);
            // 
            // toolStripMenuItemSelectAll
            // 
            this.toolStripMenuItemSelectAll.Name = "toolStripMenuItemSelectAll";
            this.toolStripMenuItemSelectAll.Size = new System.Drawing.Size(161, 22);
            this.toolStripMenuItemSelectAll.Text = "Select all";
            this.toolStripMenuItemSelectAll.Click += new System.EventHandler(this.toolStripMenuItemSelectAll_Click);
            // 
            // inverseSelectionToolStripMenuItem
            // 
            this.inverseSelectionToolStripMenuItem.Name = "inverseSelectionToolStripMenuItem";
            this.inverseSelectionToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.inverseSelectionToolStripMenuItem.Text = "Inverse selection";
            this.inverseSelectionToolStripMenuItem.Click += new System.EventHandler(this.inverseSelectionToolStripMenuItem_Click);
            // 
            // buttonConvertOptionsSettings
            // 
            this.buttonConvertOptionsSettings.Location = new System.Drawing.Point(305, 144);
            this.buttonConvertOptionsSettings.Name = "buttonConvertOptionsSettings";
            this.buttonConvertOptionsSettings.Size = new System.Drawing.Size(116, 23);
            this.buttonConvertOptionsSettings.TabIndex = 302;
            this.buttonConvertOptionsSettings.Text = "Settings...";
            this.buttonConvertOptionsSettings.UseVisualStyleBackColor = true;
            this.buttonConvertOptionsSettings.Visible = false;
            this.buttonConvertOptionsSettings.Click += new System.EventHandler(this.ButtonOptionConvertSettings);
            // 
            // groupBoxDeleteLines
            // 
            this.groupBoxDeleteLines.Controls.Add(this.textBoxDeleteContains);
            this.groupBoxDeleteLines.Controls.Add(this.labelDeleteLinesContaining);
            this.groupBoxDeleteLines.Controls.Add(this.numericUpDownDeleteLast);
            this.groupBoxDeleteLines.Controls.Add(this.labelDeleteLastLines);
            this.groupBoxDeleteLines.Controls.Add(this.numericUpDownDeleteFirst);
            this.groupBoxDeleteLines.Controls.Add(this.labelDeleteFirstLines);
            this.groupBoxDeleteLines.Location = new System.Drawing.Point(305, 94);
            this.groupBoxDeleteLines.Name = "groupBoxDeleteLines";
            this.groupBoxDeleteLines.Size = new System.Drawing.Size(271, 140);
            this.groupBoxDeleteLines.TabIndex = 308;
            this.groupBoxDeleteLines.TabStop = false;
            this.groupBoxDeleteLines.Text = "Delete lines";
            this.groupBoxDeleteLines.Visible = false;
            // 
            // textBoxDeleteContains
            // 
            this.textBoxDeleteContains.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxDeleteContains.Location = new System.Drawing.Point(10, 110);
            this.textBoxDeleteContains.Name = "textBoxDeleteContains";
            this.textBoxDeleteContains.Size = new System.Drawing.Size(237, 20);
            this.textBoxDeleteContains.TabIndex = 5;
            // 
            // labelDeleteLinesContaining
            // 
            this.labelDeleteLinesContaining.AutoSize = true;
            this.labelDeleteLinesContaining.Location = new System.Drawing.Point(9, 89);
            this.labelDeleteLinesContaining.Name = "labelDeleteLinesContaining";
            this.labelDeleteLinesContaining.Size = new System.Drawing.Size(114, 13);
            this.labelDeleteLinesContaining.TabIndex = 4;
            this.labelDeleteLinesContaining.Text = "Delete lines containing";
            // 
            // numericUpDownDeleteLast
            // 
            this.numericUpDownDeleteLast.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownDeleteLast.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownDeleteLast.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownDeleteLast.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownDeleteLast.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownDeleteLast.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownDeleteLast.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownDeleteLast.DecimalPlaces = 0;
            this.numericUpDownDeleteLast.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownDeleteLast.Location = new System.Drawing.Point(92, 45);
            this.numericUpDownDeleteLast.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDeleteLast.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownDeleteLast.Name = "numericUpDownDeleteLast";
            this.numericUpDownDeleteLast.Size = new System.Drawing.Size(43, 20);
            this.numericUpDownDeleteLast.TabIndex = 3;
            this.numericUpDownDeleteLast.TabStop = false;
            this.numericUpDownDeleteLast.ThousandsSeparator = false;
            this.numericUpDownDeleteLast.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // labelDeleteLastLines
            // 
            this.labelDeleteLastLines.AutoSize = true;
            this.labelDeleteLastLines.Location = new System.Drawing.Point(5, 47);
            this.labelDeleteLastLines.Name = "labelDeleteLastLines";
            this.labelDeleteLastLines.Size = new System.Drawing.Size(81, 13);
            this.labelDeleteLastLines.TabIndex = 2;
            this.labelDeleteLastLines.Text = "Delete last lines";
            // 
            // numericUpDownDeleteFirst
            // 
            this.numericUpDownDeleteFirst.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownDeleteFirst.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownDeleteFirst.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownDeleteFirst.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownDeleteFirst.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownDeleteFirst.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownDeleteFirst.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownDeleteFirst.DecimalPlaces = 0;
            this.numericUpDownDeleteFirst.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownDeleteFirst.Location = new System.Drawing.Point(93, 19);
            this.numericUpDownDeleteFirst.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDeleteFirst.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownDeleteFirst.Name = "numericUpDownDeleteFirst";
            this.numericUpDownDeleteFirst.Size = new System.Drawing.Size(43, 20);
            this.numericUpDownDeleteFirst.TabIndex = 1;
            this.numericUpDownDeleteFirst.TabStop = false;
            this.numericUpDownDeleteFirst.ThousandsSeparator = false;
            this.numericUpDownDeleteFirst.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // labelDeleteFirstLines
            // 
            this.labelDeleteFirstLines.AutoSize = true;
            this.labelDeleteFirstLines.Location = new System.Drawing.Point(6, 20);
            this.labelDeleteFirstLines.Name = "labelDeleteFirstLines";
            this.labelDeleteFirstLines.Size = new System.Drawing.Size(81, 13);
            this.labelDeleteFirstLines.TabIndex = 0;
            this.labelDeleteFirstLines.Text = "Delete first lines";
            // 
            // groupBoxRemoveStyle
            // 
            this.groupBoxRemoveStyle.Controls.Add(this.textBoxRemoveStyle);
            this.groupBoxRemoveStyle.Controls.Add(this.labelStyleActor);
            this.groupBoxRemoveStyle.Location = new System.Drawing.Point(307, 12);
            this.groupBoxRemoveStyle.Name = "groupBoxRemoveStyle";
            this.groupBoxRemoveStyle.Size = new System.Drawing.Size(271, 76);
            this.groupBoxRemoveStyle.TabIndex = 307;
            this.groupBoxRemoveStyle.TabStop = false;
            this.groupBoxRemoveStyle.Text = "Remove style/actor";
            this.groupBoxRemoveStyle.Visible = false;
            // 
            // textBoxRemoveStyle
            // 
            this.textBoxRemoveStyle.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxRemoveStyle.Location = new System.Drawing.Point(6, 35);
            this.textBoxRemoveStyle.Name = "textBoxRemoveStyle";
            this.textBoxRemoveStyle.Size = new System.Drawing.Size(257, 20);
            this.textBoxRemoveStyle.TabIndex = 8;
            // 
            // labelStyleActor
            // 
            this.labelStyleActor.AutoSize = true;
            this.labelStyleActor.Location = new System.Drawing.Point(6, 20);
            this.labelStyleActor.Name = "labelStyleActor";
            this.labelStyleActor.Size = new System.Drawing.Size(59, 13);
            this.labelStyleActor.TabIndex = 0;
            this.labelStyleActor.Text = "Style/actor";
            // 
            // groupBoxOffsetTimeCodes
            // 
            this.groupBoxOffsetTimeCodes.Controls.Add(this.radioButtonShowLater);
            this.groupBoxOffsetTimeCodes.Controls.Add(this.radioButtonShowEarlier);
            this.groupBoxOffsetTimeCodes.Controls.Add(this.timeUpDownAdjust);
            this.groupBoxOffsetTimeCodes.Controls.Add(this.labelHourMinSecMilliSecond);
            this.groupBoxOffsetTimeCodes.Location = new System.Drawing.Point(305, 19);
            this.groupBoxOffsetTimeCodes.Name = "groupBoxOffsetTimeCodes";
            this.groupBoxOffsetTimeCodes.Size = new System.Drawing.Size(271, 119);
            this.groupBoxOffsetTimeCodes.TabIndex = 306;
            this.groupBoxOffsetTimeCodes.TabStop = false;
            this.groupBoxOffsetTimeCodes.Text = "Offset time codes";
            this.groupBoxOffsetTimeCodes.Visible = false;
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
            this.timeUpDownAdjust.BackColor = System.Drawing.SystemColors.Window;
            this.timeUpDownAdjust.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.timeUpDownAdjust.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.timeUpDownAdjust.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.timeUpDownAdjust.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.timeUpDownAdjust.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.timeUpDownAdjust.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.timeUpDownAdjust.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownAdjust.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.timeUpDownAdjust.Location = new System.Drawing.Point(7, 37);
            this.timeUpDownAdjust.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownAdjust.Name = "timeUpDownAdjust";
            this.timeUpDownAdjust.Size = new System.Drawing.Size(113, 23);
            this.timeUpDownAdjust.TabIndex = 1;
            this.timeUpDownAdjust.TabStop = false;
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
            // groupBoxChangeFrameRate
            // 
            this.groupBoxChangeFrameRate.Controls.Add(this.buttonSwapFrameRate);
            this.groupBoxChangeFrameRate.Controls.Add(this.comboBoxFrameRateTo);
            this.groupBoxChangeFrameRate.Controls.Add(this.labelToFrameRate);
            this.groupBoxChangeFrameRate.Controls.Add(this.comboBoxFrameRateFrom);
            this.groupBoxChangeFrameRate.Controls.Add(this.labelFromFrameRate);
            this.groupBoxChangeFrameRate.Location = new System.Drawing.Point(307, 12);
            this.groupBoxChangeFrameRate.Name = "groupBoxChangeFrameRate";
            this.groupBoxChangeFrameRate.Size = new System.Drawing.Size(269, 90);
            this.groupBoxChangeFrameRate.TabIndex = 305;
            this.groupBoxChangeFrameRate.TabStop = false;
            this.groupBoxChangeFrameRate.Text = "Change frame rate";
            this.groupBoxChangeFrameRate.Visible = false;
            // 
            // buttonSwapFrameRate
            // 
            this.buttonSwapFrameRate.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSwapFrameRate.Location = new System.Drawing.Point(207, 28);
            this.buttonSwapFrameRate.Name = "buttonSwapFrameRate";
            this.buttonSwapFrameRate.Size = new System.Drawing.Size(27, 28);
            this.buttonSwapFrameRate.TabIndex = 9;
            this.buttonSwapFrameRate.Text = "<->";
            this.buttonSwapFrameRate.UseVisualStyleBackColor = true;
            this.buttonSwapFrameRate.Click += new System.EventHandler(this.buttonSwapFrameRate_Click);
            // 
            // comboBoxFrameRateTo
            // 
            this.comboBoxFrameRateTo.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFrameRateTo.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFrameRateTo.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFrameRateTo.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFrameRateTo.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFrameRateTo.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFrameRateTo.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFrameRateTo.DropDownHeight = 400;
            this.comboBoxFrameRateTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxFrameRateTo.DropDownWidth = 71;
            this.comboBoxFrameRateTo.FormattingEnabled = true;
            this.comboBoxFrameRateTo.Location = new System.Drawing.Point(130, 46);
            this.comboBoxFrameRateTo.MaxLength = 32767;
            this.comboBoxFrameRateTo.Name = "comboBoxFrameRateTo";
            this.comboBoxFrameRateTo.SelectedIndex = -1;
            this.comboBoxFrameRateTo.SelectedItem = null;
            this.comboBoxFrameRateTo.SelectedText = "";
            this.comboBoxFrameRateTo.Size = new System.Drawing.Size(71, 21);
            this.comboBoxFrameRateTo.TabIndex = 3;
            this.comboBoxFrameRateTo.TabStop = false;
            this.comboBoxFrameRateTo.UsePopupWindow = false;
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
            this.comboBoxFrameRateFrom.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFrameRateFrom.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFrameRateFrom.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFrameRateFrom.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFrameRateFrom.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFrameRateFrom.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFrameRateFrom.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFrameRateFrom.DropDownHeight = 400;
            this.comboBoxFrameRateFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxFrameRateFrom.DropDownWidth = 71;
            this.comboBoxFrameRateFrom.FormattingEnabled = true;
            this.comboBoxFrameRateFrom.Location = new System.Drawing.Point(130, 17);
            this.comboBoxFrameRateFrom.MaxLength = 32767;
            this.comboBoxFrameRateFrom.Name = "comboBoxFrameRateFrom";
            this.comboBoxFrameRateFrom.SelectedIndex = -1;
            this.comboBoxFrameRateFrom.SelectedItem = null;
            this.comboBoxFrameRateFrom.SelectedText = "";
            this.comboBoxFrameRateFrom.Size = new System.Drawing.Size(71, 21);
            this.comboBoxFrameRateFrom.TabIndex = 1;
            this.comboBoxFrameRateFrom.TabStop = false;
            this.comboBoxFrameRateFrom.UsePopupWindow = false;
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
            // groupBoxFixRtl
            // 
            this.groupBoxFixRtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFixRtl.Controls.Add(this.radioButtonReverseStartEnd);
            this.groupBoxFixRtl.Controls.Add(this.radioButtonRemoveUnicode);
            this.groupBoxFixRtl.Controls.Add(this.radioButtonAddUnicode);
            this.groupBoxFixRtl.Location = new System.Drawing.Point(305, 17);
            this.groupBoxFixRtl.Name = "groupBoxFixRtl";
            this.groupBoxFixRtl.Size = new System.Drawing.Size(271, 115);
            this.groupBoxFixRtl.TabIndex = 303;
            this.groupBoxFixRtl.TabStop = false;
            this.groupBoxFixRtl.Text = "Settings";
            this.groupBoxFixRtl.Visible = false;
            // 
            // radioButtonReverseStartEnd
            // 
            this.radioButtonReverseStartEnd.AutoSize = true;
            this.radioButtonReverseStartEnd.Location = new System.Drawing.Point(19, 77);
            this.radioButtonReverseStartEnd.Name = "radioButtonReverseStartEnd";
            this.radioButtonReverseStartEnd.Size = new System.Drawing.Size(135, 17);
            this.radioButtonReverseStartEnd.TabIndex = 2;
            this.radioButtonReverseStartEnd.TabStop = true;
            this.radioButtonReverseStartEnd.Text = "Reverse RTL start/end";
            this.radioButtonReverseStartEnd.UseVisualStyleBackColor = true;
            // 
            // radioButtonRemoveUnicode
            // 
            this.radioButtonRemoveUnicode.AutoSize = true;
            this.radioButtonRemoveUnicode.Location = new System.Drawing.Point(19, 54);
            this.radioButtonRemoveUnicode.Name = "radioButtonRemoveUnicode";
            this.radioButtonRemoveUnicode.Size = new System.Drawing.Size(153, 17);
            this.radioButtonRemoveUnicode.TabIndex = 1;
            this.radioButtonRemoveUnicode.TabStop = true;
            this.radioButtonRemoveUnicode.Text = "Remove RTL unicode tags";
            this.radioButtonRemoveUnicode.UseVisualStyleBackColor = true;
            // 
            // radioButtonAddUnicode
            // 
            this.radioButtonAddUnicode.AutoSize = true;
            this.radioButtonAddUnicode.Location = new System.Drawing.Point(19, 31);
            this.radioButtonAddUnicode.Name = "radioButtonAddUnicode";
            this.radioButtonAddUnicode.Size = new System.Drawing.Size(145, 17);
            this.radioButtonAddUnicode.TabIndex = 0;
            this.radioButtonAddUnicode.TabStop = true;
            this.radioButtonAddUnicode.Text = "Fix RTL via Unicode tags";
            this.radioButtonAddUnicode.UseVisualStyleBackColor = true;
            // 
            // groupBoxSpeed
            // 
            this.groupBoxSpeed.Controls.Add(this.radioButtonToDropFrame);
            this.groupBoxSpeed.Controls.Add(this.radioButtonSpeedFromDropFrame);
            this.groupBoxSpeed.Controls.Add(this.radioButtonSpeedCustom);
            this.groupBoxSpeed.Controls.Add(this.numericUpDownPercent);
            this.groupBoxSpeed.Controls.Add(this.labelPercent);
            this.groupBoxSpeed.Location = new System.Drawing.Point(305, 17);
            this.groupBoxSpeed.Name = "groupBoxSpeed";
            this.groupBoxSpeed.Size = new System.Drawing.Size(271, 129);
            this.groupBoxSpeed.TabIndex = 307;
            this.groupBoxSpeed.TabStop = false;
            this.groupBoxSpeed.Text = "Change speed";
            this.groupBoxSpeed.Visible = false;
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
            this.numericUpDownPercent.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownPercent.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownPercent.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownPercent.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownPercent.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownPercent.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownPercent.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownPercent.DecimalPlaces = 4;
            this.numericUpDownPercent.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
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
            this.numericUpDownPercent.TabStop = false;
            this.numericUpDownPercent.ThousandsSeparator = false;
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
            // groupBoxRemoveFormatting
            // 
            this.groupBoxRemoveFormatting.Controls.Add(this.checkBoxRemoveAllFormatting);
            this.groupBoxRemoveFormatting.Controls.Add(this.checkBoxRemoveAlignment);
            this.groupBoxRemoveFormatting.Controls.Add(this.checkBoxRemoveFontName);
            this.groupBoxRemoveFormatting.Controls.Add(this.checkBoxRemoveColor);
            this.groupBoxRemoveFormatting.Controls.Add(this.checkBoxRemoveUnderline);
            this.groupBoxRemoveFormatting.Controls.Add(this.checkBoxRemoveItalic);
            this.groupBoxRemoveFormatting.Controls.Add(this.checkBoxRemoveBold);
            this.groupBoxRemoveFormatting.Location = new System.Drawing.Point(314, 12);
            this.groupBoxRemoveFormatting.Name = "groupBoxRemoveFormatting";
            this.groupBoxRemoveFormatting.Size = new System.Drawing.Size(268, 210);
            this.groupBoxRemoveFormatting.TabIndex = 314;
            this.groupBoxRemoveFormatting.TabStop = false;
            this.groupBoxRemoveFormatting.Text = "Remove formatting";
            this.groupBoxRemoveFormatting.Visible = false;
            // 
            // checkBoxRemoveAllFormatting
            // 
            this.checkBoxRemoveAllFormatting.AutoSize = true;
            this.checkBoxRemoveAllFormatting.Location = new System.Drawing.Point(14, 20);
            this.checkBoxRemoveAllFormatting.Name = "checkBoxRemoveAllFormatting";
            this.checkBoxRemoveAllFormatting.Size = new System.Drawing.Size(79, 17);
            this.checkBoxRemoveAllFormatting.TabIndex = 8;
            this.checkBoxRemoveAllFormatting.Text = "Remove all";
            this.checkBoxRemoveAllFormatting.UseVisualStyleBackColor = true;
            this.checkBoxRemoveAllFormatting.CheckedChanged += new System.EventHandler(this.checkBoxRemoveAllFormatting_CheckedChanged);
            // 
            // checkBoxRemoveAlignment
            // 
            this.checkBoxRemoveAlignment.AutoSize = true;
            this.checkBoxRemoveAlignment.Location = new System.Drawing.Point(23, 155);
            this.checkBoxRemoveAlignment.Name = "checkBoxRemoveAlignment";
            this.checkBoxRemoveAlignment.Size = new System.Drawing.Size(114, 17);
            this.checkBoxRemoveAlignment.TabIndex = 7;
            this.checkBoxRemoveAlignment.Text = "Remove alignment";
            this.checkBoxRemoveAlignment.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveFontName
            // 
            this.checkBoxRemoveFontName.AutoSize = true;
            this.checkBoxRemoveFontName.Location = new System.Drawing.Point(23, 132);
            this.checkBoxRemoveFontName.Name = "checkBoxRemoveFontName";
            this.checkBoxRemoveFontName.Size = new System.Drawing.Size(116, 17);
            this.checkBoxRemoveFontName.TabIndex = 6;
            this.checkBoxRemoveFontName.Text = "Remove font name";
            this.checkBoxRemoveFontName.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveColor
            // 
            this.checkBoxRemoveColor.AutoSize = true;
            this.checkBoxRemoveColor.Location = new System.Drawing.Point(23, 109);
            this.checkBoxRemoveColor.Name = "checkBoxRemoveColor";
            this.checkBoxRemoveColor.Size = new System.Drawing.Size(92, 17);
            this.checkBoxRemoveColor.TabIndex = 5;
            this.checkBoxRemoveColor.Text = "Remove color";
            this.checkBoxRemoveColor.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveUnderline
            // 
            this.checkBoxRemoveUnderline.AutoSize = true;
            this.checkBoxRemoveUnderline.Location = new System.Drawing.Point(23, 88);
            this.checkBoxRemoveUnderline.Name = "checkBoxRemoveUnderline";
            this.checkBoxRemoveUnderline.Size = new System.Drawing.Size(112, 17);
            this.checkBoxRemoveUnderline.TabIndex = 4;
            this.checkBoxRemoveUnderline.Text = "Remove underline";
            this.checkBoxRemoveUnderline.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveItalic
            // 
            this.checkBoxRemoveItalic.AutoSize = true;
            this.checkBoxRemoveItalic.Location = new System.Drawing.Point(23, 64);
            this.checkBoxRemoveItalic.Name = "checkBoxRemoveItalic";
            this.checkBoxRemoveItalic.Size = new System.Drawing.Size(90, 17);
            this.checkBoxRemoveItalic.TabIndex = 3;
            this.checkBoxRemoveItalic.Text = "Remove italic";
            this.checkBoxRemoveItalic.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveBold
            // 
            this.checkBoxRemoveBold.AutoSize = true;
            this.checkBoxRemoveBold.Location = new System.Drawing.Point(23, 43);
            this.checkBoxRemoveBold.Name = "checkBoxRemoveBold";
            this.checkBoxRemoveBold.Size = new System.Drawing.Size(89, 17);
            this.checkBoxRemoveBold.TabIndex = 1;
            this.checkBoxRemoveBold.Text = "Remove bold";
            this.checkBoxRemoveBold.UseVisualStyleBackColor = true;
            // 
            // groupBoxApplyDurationLimits
            // 
            this.groupBoxApplyDurationLimits.Controls.Add(this.checkBoxApplyDurationLimitsMaxDuration);
            this.groupBoxApplyDurationLimits.Controls.Add(this.numericUpDownApplyDurationLimitsMaxDuration);
            this.groupBoxApplyDurationLimits.Controls.Add(this.checkBoxApplyDurationLimitsCheckShotChanges);
            this.groupBoxApplyDurationLimits.Controls.Add(this.numericUpDownApplyDurationLimitsMinDuration);
            this.groupBoxApplyDurationLimits.Controls.Add(this.checkBoxApplyDurationLimitsMinDuration);
            this.groupBoxApplyDurationLimits.Location = new System.Drawing.Point(309, 16);
            this.groupBoxApplyDurationLimits.Name = "groupBoxApplyDurationLimits";
            this.groupBoxApplyDurationLimits.Size = new System.Drawing.Size(268, 116);
            this.groupBoxApplyDurationLimits.TabIndex = 313;
            this.groupBoxApplyDurationLimits.TabStop = false;
            this.groupBoxApplyDurationLimits.Text = "Apply duration limits";
            this.groupBoxApplyDurationLimits.Visible = false;
            // 
            // checkBoxApplyDurationLimitsMaxDuration
            // 
            this.checkBoxApplyDurationLimitsMaxDuration.AutoSize = true;
            this.checkBoxApplyDurationLimitsMaxDuration.Location = new System.Drawing.Point(9, 71);
            this.checkBoxApplyDurationLimitsMaxDuration.Name = "checkBoxApplyDurationLimitsMaxDuration";
            this.checkBoxApplyDurationLimitsMaxDuration.Size = new System.Drawing.Size(152, 17);
            this.checkBoxApplyDurationLimitsMaxDuration.TabIndex = 4;
            this.checkBoxApplyDurationLimitsMaxDuration.Text = "Max. duration, milliseconds";
            this.checkBoxApplyDurationLimitsMaxDuration.UseVisualStyleBackColor = true;
            // 
            // numericUpDownApplyDurationLimitsMaxDuration
            // 
            this.numericUpDownApplyDurationLimitsMaxDuration.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownApplyDurationLimitsMaxDuration.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownApplyDurationLimitsMaxDuration.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownApplyDurationLimitsMaxDuration.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownApplyDurationLimitsMaxDuration.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownApplyDurationLimitsMaxDuration.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownApplyDurationLimitsMaxDuration.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownApplyDurationLimitsMaxDuration.DecimalPlaces = 0;
            this.numericUpDownApplyDurationLimitsMaxDuration.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownApplyDurationLimitsMaxDuration.Location = new System.Drawing.Point(184, 69);
            this.numericUpDownApplyDurationLimitsMaxDuration.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownApplyDurationLimitsMaxDuration.Minimum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.numericUpDownApplyDurationLimitsMaxDuration.Name = "numericUpDownApplyDurationLimitsMaxDuration";
            this.numericUpDownApplyDurationLimitsMaxDuration.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownApplyDurationLimitsMaxDuration.TabIndex = 5;
            this.numericUpDownApplyDurationLimitsMaxDuration.TabStop = false;
            this.numericUpDownApplyDurationLimitsMaxDuration.ThousandsSeparator = false;
            this.numericUpDownApplyDurationLimitsMaxDuration.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            // 
            // checkBoxApplyDurationLimitsCheckShotChanges
            // 
            this.checkBoxApplyDurationLimitsCheckShotChanges.AutoSize = true;
            this.checkBoxApplyDurationLimitsCheckShotChanges.Location = new System.Drawing.Point(17, 46);
            this.checkBoxApplyDurationLimitsCheckShotChanges.Name = "checkBoxApplyDurationLimitsCheckShotChanges";
            this.checkBoxApplyDurationLimitsCheckShotChanges.Size = new System.Drawing.Size(124, 17);
            this.checkBoxApplyDurationLimitsCheckShotChanges.TabIndex = 3;
            this.checkBoxApplyDurationLimitsCheckShotChanges.Text = "Check shot changes";
            this.checkBoxApplyDurationLimitsCheckShotChanges.UseVisualStyleBackColor = true;
            // 
            // numericUpDownApplyDurationLimitsMinDuration
            // 
            this.numericUpDownApplyDurationLimitsMinDuration.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownApplyDurationLimitsMinDuration.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownApplyDurationLimitsMinDuration.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownApplyDurationLimitsMinDuration.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownApplyDurationLimitsMinDuration.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownApplyDurationLimitsMinDuration.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownApplyDurationLimitsMinDuration.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownApplyDurationLimitsMinDuration.DecimalPlaces = 0;
            this.numericUpDownApplyDurationLimitsMinDuration.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownApplyDurationLimitsMinDuration.Location = new System.Drawing.Point(184, 21);
            this.numericUpDownApplyDurationLimitsMinDuration.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.numericUpDownApplyDurationLimitsMinDuration.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownApplyDurationLimitsMinDuration.Name = "numericUpDownApplyDurationLimitsMinDuration";
            this.numericUpDownApplyDurationLimitsMinDuration.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownApplyDurationLimitsMinDuration.TabIndex = 2;
            this.numericUpDownApplyDurationLimitsMinDuration.TabStop = false;
            this.numericUpDownApplyDurationLimitsMinDuration.ThousandsSeparator = false;
            this.numericUpDownApplyDurationLimitsMinDuration.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // checkBoxApplyDurationLimitsMinDuration
            // 
            this.checkBoxApplyDurationLimitsMinDuration.AutoSize = true;
            this.checkBoxApplyDurationLimitsMinDuration.Location = new System.Drawing.Point(9, 23);
            this.checkBoxApplyDurationLimitsMinDuration.Name = "checkBoxApplyDurationLimitsMinDuration";
            this.checkBoxApplyDurationLimitsMinDuration.Size = new System.Drawing.Size(149, 17);
            this.checkBoxApplyDurationLimitsMinDuration.TabIndex = 1;
            this.checkBoxApplyDurationLimitsMinDuration.Text = "Min. duration, milliseconds";
            this.checkBoxApplyDurationLimitsMinDuration.UseVisualStyleBackColor = true;
            // 
            // groupBoxAdjustDuration
            // 
            this.groupBoxAdjustDuration.Controls.Add(this.checkBoxAdjustDurationCheckShotChanges);
            this.groupBoxAdjustDuration.Controls.Add(this.checkBoxEnforceDurationLimits);
            this.groupBoxAdjustDuration.Controls.Add(this.comboBoxAdjustDurationVia);
            this.groupBoxAdjustDuration.Controls.Add(this.labelAdjustDurationVia);
            this.groupBoxAdjustDuration.Controls.Add(this.panelAdjustDurationFixed);
            this.groupBoxAdjustDuration.Controls.Add(this.panelAdjustDurationAddPercent);
            this.groupBoxAdjustDuration.Controls.Add(this.panelAdjustDurationAddSeconds);
            this.groupBoxAdjustDuration.Controls.Add(this.panelAdjustDurationRecalc);
            this.groupBoxAdjustDuration.Location = new System.Drawing.Point(305, 16);
            this.groupBoxAdjustDuration.Name = "groupBoxAdjustDuration";
            this.groupBoxAdjustDuration.Size = new System.Drawing.Size(271, 251);
            this.groupBoxAdjustDuration.TabIndex = 308;
            this.groupBoxAdjustDuration.TabStop = false;
            this.groupBoxAdjustDuration.Text = "Adjust duration";
            this.groupBoxAdjustDuration.Visible = false;
            // 
            // checkBoxAdjustDurationCheckShotChanges
            // 
            this.checkBoxAdjustDurationCheckShotChanges.AutoSize = true;
            this.checkBoxAdjustDurationCheckShotChanges.Location = new System.Drawing.Point(10, 222);
            this.checkBoxAdjustDurationCheckShotChanges.Name = "checkBoxAdjustDurationCheckShotChanges";
            this.checkBoxAdjustDurationCheckShotChanges.Size = new System.Drawing.Size(124, 17);
            this.checkBoxAdjustDurationCheckShotChanges.TabIndex = 18;
            this.checkBoxAdjustDurationCheckShotChanges.Text = "Check shot changes";
            this.checkBoxAdjustDurationCheckShotChanges.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnforceDurationLimits
            // 
            this.checkBoxEnforceDurationLimits.AutoSize = true;
            this.checkBoxEnforceDurationLimits.Location = new System.Drawing.Point(10, 199);
            this.checkBoxEnforceDurationLimits.Name = "checkBoxEnforceDurationLimits";
            this.checkBoxEnforceDurationLimits.Size = new System.Drawing.Size(214, 17);
            this.checkBoxEnforceDurationLimits.TabIndex = 17;
            this.checkBoxEnforceDurationLimits.Text = "Enforce minimum and maximum duration";
            this.checkBoxEnforceDurationLimits.UseVisualStyleBackColor = true;
            // 
            // comboBoxAdjustDurationVia
            // 
            this.comboBoxAdjustDurationVia.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxAdjustDurationVia.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxAdjustDurationVia.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxAdjustDurationVia.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxAdjustDurationVia.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxAdjustDurationVia.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxAdjustDurationVia.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxAdjustDurationVia.DropDownHeight = 400;
            this.comboBoxAdjustDurationVia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdjustDurationVia.DropDownWidth = 121;
            this.comboBoxAdjustDurationVia.FormattingEnabled = true;
            this.comboBoxAdjustDurationVia.Location = new System.Drawing.Point(65, 19);
            this.comboBoxAdjustDurationVia.MaxLength = 32767;
            this.comboBoxAdjustDurationVia.Name = "comboBoxAdjustDurationVia";
            this.comboBoxAdjustDurationVia.SelectedIndex = -1;
            this.comboBoxAdjustDurationVia.SelectedItem = null;
            this.comboBoxAdjustDurationVia.SelectedText = "";
            this.comboBoxAdjustDurationVia.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAdjustDurationVia.TabIndex = 8;
            this.comboBoxAdjustDurationVia.UsePopupWindow = false;
            this.comboBoxAdjustDurationVia.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdjustDurationVia_SelectedIndexChanged);
            // 
            // labelAdjustDurationVia
            // 
            this.labelAdjustDurationVia.AutoSize = true;
            this.labelAdjustDurationVia.Location = new System.Drawing.Point(8, 22);
            this.labelAdjustDurationVia.Name = "labelAdjustDurationVia";
            this.labelAdjustDurationVia.Size = new System.Drawing.Size(51, 13);
            this.labelAdjustDurationVia.TabIndex = 7;
            this.labelAdjustDurationVia.Text = "AdjustVia";
            // 
            // panelAdjustDurationFixed
            // 
            this.panelAdjustDurationFixed.Controls.Add(this.numericUpDownFixedMilliseconds);
            this.panelAdjustDurationFixed.Controls.Add(this.labelMillisecondsFixed);
            this.panelAdjustDurationFixed.Location = new System.Drawing.Point(6, 123);
            this.panelAdjustDurationFixed.Name = "panelAdjustDurationFixed";
            this.panelAdjustDurationFixed.Size = new System.Drawing.Size(257, 57);
            this.panelAdjustDurationFixed.TabIndex = 15;
            // 
            // numericUpDownFixedMilliseconds
            // 
            this.numericUpDownFixedMilliseconds.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownFixedMilliseconds.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownFixedMilliseconds.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownFixedMilliseconds.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownFixedMilliseconds.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownFixedMilliseconds.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownFixedMilliseconds.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownFixedMilliseconds.DecimalPlaces = 0;
            this.numericUpDownFixedMilliseconds.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownFixedMilliseconds.Location = new System.Drawing.Point(8, 27);
            this.numericUpDownFixedMilliseconds.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numericUpDownFixedMilliseconds.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownFixedMilliseconds.Name = "numericUpDownFixedMilliseconds";
            this.numericUpDownFixedMilliseconds.Size = new System.Drawing.Size(80, 20);
            this.numericUpDownFixedMilliseconds.TabIndex = 13;
            this.numericUpDownFixedMilliseconds.TabStop = false;
            this.numericUpDownFixedMilliseconds.ThousandsSeparator = false;
            this.numericUpDownFixedMilliseconds.Value = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            // 
            // labelMillisecondsFixed
            // 
            this.labelMillisecondsFixed.AutoSize = true;
            this.labelMillisecondsFixed.Location = new System.Drawing.Point(5, 8);
            this.labelMillisecondsFixed.Name = "labelMillisecondsFixed";
            this.labelMillisecondsFixed.Size = new System.Drawing.Size(64, 13);
            this.labelMillisecondsFixed.TabIndex = 14;
            this.labelMillisecondsFixed.Text = "Milliseconds";
            // 
            // panelAdjustDurationAddPercent
            // 
            this.panelAdjustDurationAddPercent.Controls.Add(this.label1);
            this.panelAdjustDurationAddPercent.Controls.Add(this.numericUpDownAdjustViaPercent);
            this.panelAdjustDurationAddPercent.Controls.Add(this.labelAdjustViaPercent);
            this.panelAdjustDurationAddPercent.Location = new System.Drawing.Point(7, 79);
            this.panelAdjustDurationAddPercent.Name = "panelAdjustDurationAddPercent";
            this.panelAdjustDurationAddPercent.Size = new System.Drawing.Size(257, 63);
            this.panelAdjustDurationAddPercent.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(90, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "%";
            // 
            // numericUpDownAdjustViaPercent
            // 
            this.numericUpDownAdjustViaPercent.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownAdjustViaPercent.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownAdjustViaPercent.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownAdjustViaPercent.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownAdjustViaPercent.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownAdjustViaPercent.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownAdjustViaPercent.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownAdjustViaPercent.DecimalPlaces = 0;
            this.numericUpDownAdjustViaPercent.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownAdjustViaPercent.Location = new System.Drawing.Point(8, 27);
            this.numericUpDownAdjustViaPercent.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.numericUpDownAdjustViaPercent.Minimum = new decimal(new int[] {
            75,
            0,
            0,
            0});
            this.numericUpDownAdjustViaPercent.Name = "numericUpDownAdjustViaPercent";
            this.numericUpDownAdjustViaPercent.Size = new System.Drawing.Size(80, 20);
            this.numericUpDownAdjustViaPercent.TabIndex = 24;
            this.numericUpDownAdjustViaPercent.TabStop = false;
            this.numericUpDownAdjustViaPercent.ThousandsSeparator = false;
            this.numericUpDownAdjustViaPercent.Value = new decimal(new int[] {
            110,
            0,
            0,
            0});
            // 
            // labelAdjustViaPercent
            // 
            this.labelAdjustViaPercent.AutoSize = true;
            this.labelAdjustViaPercent.Location = new System.Drawing.Point(5, 8);
            this.labelAdjustViaPercent.Name = "labelAdjustViaPercent";
            this.labelAdjustViaPercent.Size = new System.Drawing.Size(86, 13);
            this.labelAdjustViaPercent.TabIndex = 23;
            this.labelAdjustViaPercent.Text = "Adjust in percent";
            // 
            // panelAdjustDurationAddSeconds
            // 
            this.panelAdjustDurationAddSeconds.Controls.Add(this.numericUpDownSeconds);
            this.panelAdjustDurationAddSeconds.Controls.Add(this.labelAddSeconds);
            this.panelAdjustDurationAddSeconds.Location = new System.Drawing.Point(8, 47);
            this.panelAdjustDurationAddSeconds.Name = "panelAdjustDurationAddSeconds";
            this.panelAdjustDurationAddSeconds.Size = new System.Drawing.Size(257, 55);
            this.panelAdjustDurationAddSeconds.TabIndex = 13;
            // 
            // numericUpDownSeconds
            // 
            this.numericUpDownSeconds.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownSeconds.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownSeconds.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownSeconds.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownSeconds.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownSeconds.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownSeconds.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownSeconds.DecimalPlaces = 3;
            this.numericUpDownSeconds.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDownSeconds.Location = new System.Drawing.Point(8, 27);
            this.numericUpDownSeconds.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownSeconds.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.numericUpDownSeconds.Name = "numericUpDownSeconds";
            this.numericUpDownSeconds.Size = new System.Drawing.Size(80, 20);
            this.numericUpDownSeconds.TabIndex = 12;
            this.numericUpDownSeconds.TabStop = false;
            this.numericUpDownSeconds.ThousandsSeparator = false;
            this.numericUpDownSeconds.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // labelAddSeconds
            // 
            this.labelAddSeconds.AutoSize = true;
            this.labelAddSeconds.Location = new System.Drawing.Point(5, 8);
            this.labelAddSeconds.Name = "labelAddSeconds";
            this.labelAddSeconds.Size = new System.Drawing.Size(69, 13);
            this.labelAddSeconds.TabIndex = 11;
            this.labelAddSeconds.Text = "Add seconds";
            // 
            // panelAdjustDurationRecalc
            // 
            this.panelAdjustDurationRecalc.Controls.Add(this.checkBoxExtendOnly);
            this.panelAdjustDurationRecalc.Controls.Add(this.numericUpDownOptimalCharsSec);
            this.panelAdjustDurationRecalc.Controls.Add(this.labelOptimalCharsSec);
            this.panelAdjustDurationRecalc.Controls.Add(this.numericUpDownMaxCharsSec);
            this.panelAdjustDurationRecalc.Controls.Add(this.labelMaxCharsPerSecond);
            this.panelAdjustDurationRecalc.Location = new System.Drawing.Point(6, 66);
            this.panelAdjustDurationRecalc.Name = "panelAdjustDurationRecalc";
            this.panelAdjustDurationRecalc.Size = new System.Drawing.Size(257, 170);
            this.panelAdjustDurationRecalc.TabIndex = 16;
            // 
            // checkBoxExtendOnly
            // 
            this.checkBoxExtendOnly.AutoSize = true;
            this.checkBoxExtendOnly.Location = new System.Drawing.Point(8, 122);
            this.checkBoxExtendOnly.Name = "checkBoxExtendOnly";
            this.checkBoxExtendOnly.Size = new System.Drawing.Size(81, 17);
            this.checkBoxExtendOnly.TabIndex = 14;
            this.checkBoxExtendOnly.Text = "Extend only";
            this.checkBoxExtendOnly.UseVisualStyleBackColor = true;
            // 
            // numericUpDownOptimalCharsSec
            // 
            this.numericUpDownOptimalCharsSec.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownOptimalCharsSec.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownOptimalCharsSec.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownOptimalCharsSec.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownOptimalCharsSec.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownOptimalCharsSec.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownOptimalCharsSec.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownOptimalCharsSec.DecimalPlaces = 1;
            this.numericUpDownOptimalCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownOptimalCharsSec.Location = new System.Drawing.Point(8, 85);
            this.numericUpDownOptimalCharsSec.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.Name = "numericUpDownOptimalCharsSec";
            this.numericUpDownOptimalCharsSec.Size = new System.Drawing.Size(80, 20);
            this.numericUpDownOptimalCharsSec.TabIndex = 13;
            this.numericUpDownOptimalCharsSec.TabStop = false;
            this.numericUpDownOptimalCharsSec.ThousandsSeparator = false;
            this.numericUpDownOptimalCharsSec.Value = new decimal(new int[] {
            17,
            0,
            0,
            0});
            // 
            // labelOptimalCharsSec
            // 
            this.labelOptimalCharsSec.AutoSize = true;
            this.labelOptimalCharsSec.Location = new System.Drawing.Point(5, 66);
            this.labelOptimalCharsSec.Name = "labelOptimalCharsSec";
            this.labelOptimalCharsSec.Size = new System.Drawing.Size(93, 13);
            this.labelOptimalCharsSec.TabIndex = 15;
            this.labelOptimalCharsSec.Text = "Optimal chars/sec";
            // 
            // numericUpDownMaxCharsSec
            // 
            this.numericUpDownMaxCharsSec.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMaxCharsSec.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMaxCharsSec.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMaxCharsSec.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMaxCharsSec.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMaxCharsSec.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMaxCharsSec.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMaxCharsSec.DecimalPlaces = 1;
            this.numericUpDownMaxCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownMaxCharsSec.Location = new System.Drawing.Point(8, 27);
            this.numericUpDownMaxCharsSec.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.Name = "numericUpDownMaxCharsSec";
            this.numericUpDownMaxCharsSec.Size = new System.Drawing.Size(80, 20);
            this.numericUpDownMaxCharsSec.TabIndex = 12;
            this.numericUpDownMaxCharsSec.TabStop = false;
            this.numericUpDownMaxCharsSec.ThousandsSeparator = false;
            this.numericUpDownMaxCharsSec.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // labelMaxCharsPerSecond
            // 
            this.labelMaxCharsPerSecond.AutoSize = true;
            this.labelMaxCharsPerSecond.Location = new System.Drawing.Point(5, 8);
            this.labelMaxCharsPerSecond.Name = "labelMaxCharsPerSecond";
            this.labelMaxCharsPerSecond.Size = new System.Drawing.Size(81, 13);
            this.labelMaxCharsPerSecond.TabIndex = 11;
            this.labelMaxCharsPerSecond.Text = "Max. chars/sec";
            // 
            // groupBoxBeautifyTimeCodes
            // 
            this.groupBoxBeautifyTimeCodes.Controls.Add(this.buttonBeautifyTimeCodesEditProfile);
            this.groupBoxBeautifyTimeCodes.Controls.Add(this.checkBoxBeautifyTimeCodesSnapToShotChanges);
            this.groupBoxBeautifyTimeCodes.Controls.Add(this.checkBoxBeautifyTimeCodesUseExactTimeCodes);
            this.groupBoxBeautifyTimeCodes.Controls.Add(this.checkBoxBeautifyTimeCodesAlignTimeCodes);
            this.groupBoxBeautifyTimeCodes.Location = new System.Drawing.Point(304, 11);
            this.groupBoxBeautifyTimeCodes.Name = "groupBoxBeautifyTimeCodes";
            this.groupBoxBeautifyTimeCodes.Size = new System.Drawing.Size(268, 169);
            this.groupBoxBeautifyTimeCodes.TabIndex = 312;
            this.groupBoxBeautifyTimeCodes.TabStop = false;
            this.groupBoxBeautifyTimeCodes.Text = "Beautify time codes";
            this.groupBoxBeautifyTimeCodes.Visible = false;
            // 
            // buttonBeautifyTimeCodesEditProfile
            // 
            this.buttonBeautifyTimeCodesEditProfile.Location = new System.Drawing.Point(8, 97);
            this.buttonBeautifyTimeCodesEditProfile.Name = "buttonBeautifyTimeCodesEditProfile";
            this.buttonBeautifyTimeCodesEditProfile.Size = new System.Drawing.Size(132, 26);
            this.buttonBeautifyTimeCodesEditProfile.TabIndex = 4;
            this.buttonBeautifyTimeCodesEditProfile.Text = "Edit profile...";
            this.buttonBeautifyTimeCodesEditProfile.UseVisualStyleBackColor = true;
            this.buttonBeautifyTimeCodesEditProfile.Click += new System.EventHandler(this.buttonBeautifyTimeCodesEditProfile_Click);
            // 
            // checkBoxBeautifyTimeCodesSnapToShotChanges
            // 
            this.checkBoxBeautifyTimeCodesSnapToShotChanges.AutoSize = true;
            this.checkBoxBeautifyTimeCodesSnapToShotChanges.Location = new System.Drawing.Point(9, 69);
            this.checkBoxBeautifyTimeCodesSnapToShotChanges.Name = "checkBoxBeautifyTimeCodesSnapToShotChanges";
            this.checkBoxBeautifyTimeCodesSnapToShotChanges.Size = new System.Drawing.Size(215, 17);
            this.checkBoxBeautifyTimeCodesSnapToShotChanges.TabIndex = 3;
            this.checkBoxBeautifyTimeCodesSnapToShotChanges.Text = "Snap cues to shot changes (if available)";
            this.checkBoxBeautifyTimeCodesSnapToShotChanges.UseVisualStyleBackColor = true;
            // 
            // checkBoxBeautifyTimeCodesUseExactTimeCodes
            // 
            this.checkBoxBeautifyTimeCodesUseExactTimeCodes.AutoSize = true;
            this.checkBoxBeautifyTimeCodesUseExactTimeCodes.Location = new System.Drawing.Point(9, 46);
            this.checkBoxBeautifyTimeCodesUseExactTimeCodes.Name = "checkBoxBeautifyTimeCodesUseExactTimeCodes";
            this.checkBoxBeautifyTimeCodesUseExactTimeCodes.Size = new System.Drawing.Size(187, 17);
            this.checkBoxBeautifyTimeCodesUseExactTimeCodes.TabIndex = 2;
            this.checkBoxBeautifyTimeCodesUseExactTimeCodes.Text = "Use exact time codes (if available)";
            this.checkBoxBeautifyTimeCodesUseExactTimeCodes.UseVisualStyleBackColor = true;
            // 
            // checkBoxBeautifyTimeCodesAlignTimeCodes
            // 
            this.checkBoxBeautifyTimeCodesAlignTimeCodes.AutoSize = true;
            this.checkBoxBeautifyTimeCodesAlignTimeCodes.Enabled = false;
            this.checkBoxBeautifyTimeCodesAlignTimeCodes.Location = new System.Drawing.Point(9, 23);
            this.checkBoxBeautifyTimeCodesAlignTimeCodes.Name = "checkBoxBeautifyTimeCodesAlignTimeCodes";
            this.checkBoxBeautifyTimeCodesAlignTimeCodes.Size = new System.Drawing.Size(198, 17);
            this.checkBoxBeautifyTimeCodesAlignTimeCodes.TabIndex = 1;
            this.checkBoxBeautifyTimeCodesAlignTimeCodes.Text = "Align time codes to frame time codes";
            this.checkBoxBeautifyTimeCodesAlignTimeCodes.UseVisualStyleBackColor = true;
            // 
            // groupBoxChangeCasing
            // 
            this.groupBoxChangeCasing.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxChangeCasing.Controls.Add(this.checkBoxProperCaseOnlyUpper);
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonProperCase);
            this.groupBoxChangeCasing.Controls.Add(this.checkBoxOnlyAllUpper);
            this.groupBoxChangeCasing.Controls.Add(this.checkBoxFixNames);
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonFixOnlyNames);
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonLowercase);
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonUppercase);
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonNormal);
            this.groupBoxChangeCasing.Location = new System.Drawing.Point(308, 16);
            this.groupBoxChangeCasing.Name = "groupBoxChangeCasing";
            this.groupBoxChangeCasing.Size = new System.Drawing.Size(268, 212);
            this.groupBoxChangeCasing.TabIndex = 308;
            this.groupBoxChangeCasing.TabStop = false;
            this.groupBoxChangeCasing.Text = "Change casing to";
            // 
            // checkBoxProperCaseOnlyUpper
            // 
            this.checkBoxProperCaseOnlyUpper.AutoSize = true;
            this.checkBoxProperCaseOnlyUpper.Location = new System.Drawing.Point(29, 176);
            this.checkBoxProperCaseOnlyUpper.Name = "checkBoxProperCaseOnlyUpper";
            this.checkBoxProperCaseOnlyUpper.Size = new System.Drawing.Size(182, 17);
            this.checkBoxProperCaseOnlyUpper.TabIndex = 16;
            this.checkBoxProperCaseOnlyUpper.Text = "Only change all upper case lines.";
            this.checkBoxProperCaseOnlyUpper.UseVisualStyleBackColor = true;
            // 
            // radioButtonProperCase
            // 
            this.radioButtonProperCase.AutoSize = true;
            this.radioButtonProperCase.Location = new System.Drawing.Point(12, 154);
            this.radioButtonProperCase.Name = "radioButtonProperCase";
            this.radioButtonProperCase.Size = new System.Drawing.Size(79, 17);
            this.radioButtonProperCase.TabIndex = 13;
            this.radioButtonProperCase.Text = "Propercase";
            this.radioButtonProperCase.UseVisualStyleBackColor = true;
            // 
            // checkBoxOnlyAllUpper
            // 
            this.checkBoxOnlyAllUpper.AutoSize = true;
            this.checkBoxOnlyAllUpper.Location = new System.Drawing.Point(29, 60);
            this.checkBoxOnlyAllUpper.Name = "checkBoxOnlyAllUpper";
            this.checkBoxOnlyAllUpper.Size = new System.Drawing.Size(182, 17);
            this.checkBoxOnlyAllUpper.TabIndex = 12;
            this.checkBoxOnlyAllUpper.Text = "Only change all upper case lines.";
            this.checkBoxOnlyAllUpper.UseVisualStyleBackColor = true;
            // 
            // checkBoxFixNames
            // 
            this.checkBoxFixNames.AutoSize = true;
            this.checkBoxFixNames.Location = new System.Drawing.Point(29, 39);
            this.checkBoxFixNames.Name = "checkBoxFixNames";
            this.checkBoxFixNames.Size = new System.Drawing.Size(107, 17);
            this.checkBoxFixNames.TabIndex = 11;
            this.checkBoxFixNames.Text = "Fix names casing";
            this.checkBoxFixNames.UseVisualStyleBackColor = true;
            // 
            // radioButtonFixOnlyNames
            // 
            this.radioButtonFixOnlyNames.AutoSize = true;
            this.radioButtonFixOnlyNames.Location = new System.Drawing.Point(11, 85);
            this.radioButtonFixOnlyNames.Name = "radioButtonFixOnlyNames";
            this.radioButtonFixOnlyNames.Size = new System.Drawing.Size(263, 17);
            this.radioButtonFixOnlyNames.TabIndex = 6;
            this.radioButtonFixOnlyNames.Text = "Fix only names casing (via Dictionaries\\names.xml)";
            this.radioButtonFixOnlyNames.UseVisualStyleBackColor = true;
            // 
            // radioButtonLowercase
            // 
            this.radioButtonLowercase.AutoSize = true;
            this.radioButtonLowercase.Location = new System.Drawing.Point(11, 131);
            this.radioButtonLowercase.Name = "radioButtonLowercase";
            this.radioButtonLowercase.Size = new System.Drawing.Size(86, 17);
            this.radioButtonLowercase.TabIndex = 10;
            this.radioButtonLowercase.Text = "all lowercase";
            this.radioButtonLowercase.UseVisualStyleBackColor = true;
            // 
            // radioButtonUppercase
            // 
            this.radioButtonUppercase.AutoSize = true;
            this.radioButtonUppercase.Location = new System.Drawing.Point(11, 108);
            this.radioButtonUppercase.Name = "radioButtonUppercase";
            this.radioButtonUppercase.Size = new System.Drawing.Size(112, 17);
            this.radioButtonUppercase.TabIndex = 8;
            this.radioButtonUppercase.Text = "ALL UPPERCASE";
            this.radioButtonUppercase.UseVisualStyleBackColor = true;
            // 
            // radioButtonNormal
            // 
            this.radioButtonNormal.AutoSize = true;
            this.radioButtonNormal.Checked = true;
            this.radioButtonNormal.Location = new System.Drawing.Point(11, 18);
            this.radioButtonNormal.Name = "radioButtonNormal";
            this.radioButtonNormal.Size = new System.Drawing.Size(282, 17);
            this.radioButtonNormal.TabIndex = 0;
            this.radioButtonNormal.TabStop = true;
            this.radioButtonNormal.Text = "Normal casing. Sentences begin with uppercase letter.";
            this.radioButtonNormal.UseVisualStyleBackColor = true;
            // 
            // groupBoxMergeShortLines
            // 
            this.groupBoxMergeShortLines.Controls.Add(this.checkBoxOnlyContinuationLines);
            this.groupBoxMergeShortLines.Controls.Add(this.numericUpDownMaxCharacters);
            this.groupBoxMergeShortLines.Controls.Add(this.numericUpDownMaxMillisecondsBetweenLines);
            this.groupBoxMergeShortLines.Controls.Add(this.labelMaxMillisecondsBetweenLines);
            this.groupBoxMergeShortLines.Controls.Add(this.labelMaxCharacters);
            this.groupBoxMergeShortLines.Location = new System.Drawing.Point(308, 23);
            this.groupBoxMergeShortLines.Name = "groupBoxMergeShortLines";
            this.groupBoxMergeShortLines.Size = new System.Drawing.Size(268, 149);
            this.groupBoxMergeShortLines.TabIndex = 304;
            this.groupBoxMergeShortLines.TabStop = false;
            this.groupBoxMergeShortLines.Text = "Merge short lines";
            this.groupBoxMergeShortLines.Visible = false;
            // 
            // checkBoxOnlyContinuationLines
            // 
            this.checkBoxOnlyContinuationLines.AutoSize = true;
            this.checkBoxOnlyContinuationLines.Checked = true;
            this.checkBoxOnlyContinuationLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxOnlyContinuationLines.Location = new System.Drawing.Point(16, 127);
            this.checkBoxOnlyContinuationLines.Name = "checkBoxOnlyContinuationLines";
            this.checkBoxOnlyContinuationLines.Size = new System.Drawing.Size(164, 17);
            this.checkBoxOnlyContinuationLines.TabIndex = 42;
            this.checkBoxOnlyContinuationLines.Text = "Only merge continuation lines";
            this.checkBoxOnlyContinuationLines.UseVisualStyleBackColor = true;
            // 
            // numericUpDownMaxCharacters
            // 
            this.numericUpDownMaxCharacters.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMaxCharacters.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMaxCharacters.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMaxCharacters.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMaxCharacters.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMaxCharacters.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMaxCharacters.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMaxCharacters.DecimalPlaces = 0;
            this.numericUpDownMaxCharacters.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxCharacters.Location = new System.Drawing.Point(15, 41);
            this.numericUpDownMaxCharacters.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericUpDownMaxCharacters.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownMaxCharacters.Name = "numericUpDownMaxCharacters";
            this.numericUpDownMaxCharacters.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownMaxCharacters.TabIndex = 38;
            this.numericUpDownMaxCharacters.TabStop = false;
            this.numericUpDownMaxCharacters.ThousandsSeparator = false;
            this.numericUpDownMaxCharacters.Value = new decimal(new int[] {
            65,
            0,
            0,
            0});
            // 
            // numericUpDownMaxMillisecondsBetweenLines
            // 
            this.numericUpDownMaxMillisecondsBetweenLines.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMaxMillisecondsBetweenLines.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMaxMillisecondsBetweenLines.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMaxMillisecondsBetweenLines.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMaxMillisecondsBetweenLines.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMaxMillisecondsBetweenLines.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMaxMillisecondsBetweenLines.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMaxMillisecondsBetweenLines.DecimalPlaces = 0;
            this.numericUpDownMaxMillisecondsBetweenLines.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxMillisecondsBetweenLines.Location = new System.Drawing.Point(15, 90);
            this.numericUpDownMaxMillisecondsBetweenLines.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownMaxMillisecondsBetweenLines.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxMillisecondsBetweenLines.Name = "numericUpDownMaxMillisecondsBetweenLines";
            this.numericUpDownMaxMillisecondsBetweenLines.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownMaxMillisecondsBetweenLines.TabIndex = 39;
            this.numericUpDownMaxMillisecondsBetweenLines.TabStop = false;
            this.numericUpDownMaxMillisecondsBetweenLines.ThousandsSeparator = false;
            this.numericUpDownMaxMillisecondsBetweenLines.Value = new decimal(new int[] {
            250,
            0,
            0,
            0});
            // 
            // labelMaxMillisecondsBetweenLines
            // 
            this.labelMaxMillisecondsBetweenLines.AutoSize = true;
            this.labelMaxMillisecondsBetweenLines.Location = new System.Drawing.Point(12, 73);
            this.labelMaxMillisecondsBetweenLines.Name = "labelMaxMillisecondsBetweenLines";
            this.labelMaxMillisecondsBetweenLines.Size = new System.Drawing.Size(178, 13);
            this.labelMaxMillisecondsBetweenLines.TabIndex = 41;
            this.labelMaxMillisecondsBetweenLines.Text = "Maximum milliseconds between lines";
            // 
            // labelMaxCharacters
            // 
            this.labelMaxCharacters.AutoSize = true;
            this.labelMaxCharacters.Location = new System.Drawing.Point(12, 23);
            this.labelMaxCharacters.Name = "labelMaxCharacters";
            this.labelMaxCharacters.Size = new System.Drawing.Size(187, 13);
            this.labelMaxCharacters.TabIndex = 40;
            this.labelMaxCharacters.Text = "Maximum characters in one paragraph";
            // 
            // groupBoxAssaChangeRes
            // 
            this.groupBoxAssaChangeRes.Controls.Add(this.checkBoxDrawing);
            this.groupBoxAssaChangeRes.Controls.Add(this.checkBoxPosition);
            this.groupBoxAssaChangeRes.Controls.Add(this.checkBoxFontSize);
            this.groupBoxAssaChangeRes.Controls.Add(this.checkBoxMargins);
            this.groupBoxAssaChangeRes.Controls.Add(this.labelTargetRes);
            this.groupBoxAssaChangeRes.Controls.Add(this.numericUpDownTargetHeight);
            this.groupBoxAssaChangeRes.Controls.Add(this.buttonGetResolutionFromVideo);
            this.groupBoxAssaChangeRes.Controls.Add(this.labelX);
            this.groupBoxAssaChangeRes.Controls.Add(this.numericUpDownTargetWidth);
            this.groupBoxAssaChangeRes.Location = new System.Drawing.Point(301, 12);
            this.groupBoxAssaChangeRes.Name = "groupBoxAssaChangeRes";
            this.groupBoxAssaChangeRes.Size = new System.Drawing.Size(271, 240);
            this.groupBoxAssaChangeRes.TabIndex = 309;
            this.groupBoxAssaChangeRes.TabStop = false;
            this.groupBoxAssaChangeRes.Text = "ASSA: change resolution";
            this.groupBoxAssaChangeRes.Visible = false;
            // 
            // checkBoxDrawing
            // 
            this.checkBoxDrawing.AutoSize = true;
            this.checkBoxDrawing.Checked = true;
            this.checkBoxDrawing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDrawing.Location = new System.Drawing.Point(12, 163);
            this.checkBoxDrawing.Name = "checkBoxDrawing";
            this.checkBoxDrawing.Size = new System.Drawing.Size(166, 17);
            this.checkBoxDrawing.TabIndex = 25;
            this.checkBoxDrawing.Text = "Change resolution for drawing";
            this.checkBoxDrawing.UseVisualStyleBackColor = true;
            // 
            // checkBoxPosition
            // 
            this.checkBoxPosition.AutoSize = true;
            this.checkBoxPosition.Checked = true;
            this.checkBoxPosition.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPosition.Location = new System.Drawing.Point(13, 140);
            this.checkBoxPosition.Name = "checkBoxPosition";
            this.checkBoxPosition.Size = new System.Drawing.Size(165, 17);
            this.checkBoxPosition.TabIndex = 24;
            this.checkBoxPosition.Text = "Change resolution for position";
            this.checkBoxPosition.UseVisualStyleBackColor = true;
            // 
            // checkBoxFontSize
            // 
            this.checkBoxFontSize.AutoSize = true;
            this.checkBoxFontSize.Checked = true;
            this.checkBoxFontSize.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFontSize.Location = new System.Drawing.Point(13, 117);
            this.checkBoxFontSize.Name = "checkBoxFontSize";
            this.checkBoxFontSize.Size = new System.Drawing.Size(168, 17);
            this.checkBoxFontSize.TabIndex = 23;
            this.checkBoxFontSize.Text = "Change resolution for font size";
            this.checkBoxFontSize.UseVisualStyleBackColor = true;
            // 
            // checkBoxMargins
            // 
            this.checkBoxMargins.AutoSize = true;
            this.checkBoxMargins.Location = new System.Drawing.Point(13, 94);
            this.checkBoxMargins.Name = "checkBoxMargins";
            this.checkBoxMargins.Size = new System.Drawing.Size(165, 17);
            this.checkBoxMargins.TabIndex = 22;
            this.checkBoxMargins.Text = "Change resolution for margins";
            this.checkBoxMargins.UseVisualStyleBackColor = true;
            // 
            // labelTargetRes
            // 
            this.labelTargetRes.AutoSize = true;
            this.labelTargetRes.Location = new System.Drawing.Point(9, 29);
            this.labelTargetRes.Name = "labelTargetRes";
            this.labelTargetRes.Size = new System.Drawing.Size(38, 13);
            this.labelTargetRes.TabIndex = 17;
            this.labelTargetRes.Text = "Target";
            // 
            // numericUpDownTargetHeight
            // 
            this.numericUpDownTargetHeight.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownTargetHeight.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownTargetHeight.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownTargetHeight.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownTargetHeight.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownTargetHeight.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownTargetHeight.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownTargetHeight.DecimalPlaces = 0;
            this.numericUpDownTargetHeight.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTargetHeight.Location = new System.Drawing.Point(82, 46);
            this.numericUpDownTargetHeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownTargetHeight.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownTargetHeight.Name = "numericUpDownTargetHeight";
            this.numericUpDownTargetHeight.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownTargetHeight.TabIndex = 20;
            this.numericUpDownTargetHeight.TabStop = false;
            this.numericUpDownTargetHeight.ThousandsSeparator = false;
            this.numericUpDownTargetHeight.Value = new decimal(new int[] {
            288,
            0,
            0,
            0});
            // 
            // buttonGetResolutionFromVideo
            // 
            this.buttonGetResolutionFromVideo.Location = new System.Drawing.Point(133, 46);
            this.buttonGetResolutionFromVideo.Name = "buttonGetResolutionFromVideo";
            this.buttonGetResolutionFromVideo.Size = new System.Drawing.Size(27, 23);
            this.buttonGetResolutionFromVideo.TabIndex = 21;
            this.buttonGetResolutionFromVideo.Text = "...";
            this.buttonGetResolutionFromVideo.UseVisualStyleBackColor = true;
            this.buttonGetResolutionFromVideo.Click += new System.EventHandler(this.buttonGetResolutionFromVideo_Click);
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(64, 49);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(14, 13);
            this.labelX.TabIndex = 19;
            this.labelX.Text = "X";
            // 
            // numericUpDownTargetWidth
            // 
            this.numericUpDownTargetWidth.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownTargetWidth.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownTargetWidth.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownTargetWidth.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownTargetWidth.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownTargetWidth.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownTargetWidth.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownTargetWidth.DecimalPlaces = 0;
            this.numericUpDownTargetWidth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTargetWidth.Location = new System.Drawing.Point(13, 46);
            this.numericUpDownTargetWidth.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownTargetWidth.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownTargetWidth.Name = "numericUpDownTargetWidth";
            this.numericUpDownTargetWidth.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownTargetWidth.TabIndex = 18;
            this.numericUpDownTargetWidth.TabStop = false;
            this.numericUpDownTargetWidth.ThousandsSeparator = false;
            this.numericUpDownTargetWidth.Value = new decimal(new int[] {
            384,
            0,
            0,
            0});
            // 
            // groupBoxSortBy
            // 
            this.groupBoxSortBy.Controls.Add(this.comboBoxSortBy);
            this.groupBoxSortBy.Location = new System.Drawing.Point(301, 11);
            this.groupBoxSortBy.Name = "groupBoxSortBy";
            this.groupBoxSortBy.Size = new System.Drawing.Size(268, 149);
            this.groupBoxSortBy.TabIndex = 311;
            this.groupBoxSortBy.TabStop = false;
            this.groupBoxSortBy.Text = "Sort by";
            this.groupBoxSortBy.Visible = false;
            // 
            // comboBoxSortBy
            // 
            this.comboBoxSortBy.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSortBy.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSortBy.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSortBy.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSortBy.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSortBy.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSortBy.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSortBy.DropDownHeight = 400;
            this.comboBoxSortBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSortBy.DropDownWidth = 228;
            this.comboBoxSortBy.FormattingEnabled = true;
            this.comboBoxSortBy.Location = new System.Drawing.Point(8, 30);
            this.comboBoxSortBy.MaxLength = 32767;
            this.comboBoxSortBy.Name = "comboBoxSortBy";
            this.comboBoxSortBy.SelectedIndex = -1;
            this.comboBoxSortBy.SelectedItem = null;
            this.comboBoxSortBy.SelectedText = "";
            this.comboBoxSortBy.Size = new System.Drawing.Size(228, 21);
            this.comboBoxSortBy.TabIndex = 0;
            this.comboBoxSortBy.UsePopupWindow = false;
            // 
            // groupBoxMergeSameTimeCodes
            // 
            this.groupBoxMergeSameTimeCodes.Controls.Add(this.checkBoxMergeSameTimeCodesReBreakLines);
            this.groupBoxMergeSameTimeCodes.Controls.Add(this.checkBoxMergeSameTimeCodesMakeDialog);
            this.groupBoxMergeSameTimeCodes.Controls.Add(this.numericUpDownMergeSameTimeCodesMaxDifference);
            this.groupBoxMergeSameTimeCodes.Controls.Add(this.labelMergeSameTimeCodesMaxDifference);
            this.groupBoxMergeSameTimeCodes.Location = new System.Drawing.Point(308, 17);
            this.groupBoxMergeSameTimeCodes.Name = "groupBoxMergeSameTimeCodes";
            this.groupBoxMergeSameTimeCodes.Size = new System.Drawing.Size(268, 149);
            this.groupBoxMergeSameTimeCodes.TabIndex = 310;
            this.groupBoxMergeSameTimeCodes.TabStop = false;
            this.groupBoxMergeSameTimeCodes.Text = "Merge lines with same time codes";
            this.groupBoxMergeSameTimeCodes.Visible = false;
            // 
            // checkBoxMergeSameTimeCodesReBreakLines
            // 
            this.checkBoxMergeSameTimeCodesReBreakLines.AutoSize = true;
            this.checkBoxMergeSameTimeCodesReBreakLines.Checked = true;
            this.checkBoxMergeSameTimeCodesReBreakLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMergeSameTimeCodesReBreakLines.Location = new System.Drawing.Point(15, 102);
            this.checkBoxMergeSameTimeCodesReBreakLines.Name = "checkBoxMergeSameTimeCodesReBreakLines";
            this.checkBoxMergeSameTimeCodesReBreakLines.Size = new System.Drawing.Size(94, 17);
            this.checkBoxMergeSameTimeCodesReBreakLines.TabIndex = 43;
            this.checkBoxMergeSameTimeCodesReBreakLines.Text = "Re-break lines";
            this.checkBoxMergeSameTimeCodesReBreakLines.UseVisualStyleBackColor = true;
            // 
            // checkBoxMergeSameTimeCodesMakeDialog
            // 
            this.checkBoxMergeSameTimeCodesMakeDialog.AutoSize = true;
            this.checkBoxMergeSameTimeCodesMakeDialog.Checked = true;
            this.checkBoxMergeSameTimeCodesMakeDialog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMergeSameTimeCodesMakeDialog.Location = new System.Drawing.Point(15, 79);
            this.checkBoxMergeSameTimeCodesMakeDialog.Name = "checkBoxMergeSameTimeCodesMakeDialog";
            this.checkBoxMergeSameTimeCodesMakeDialog.Size = new System.Drawing.Size(84, 17);
            this.checkBoxMergeSameTimeCodesMakeDialog.TabIndex = 42;
            this.checkBoxMergeSameTimeCodesMakeDialog.Text = "Make dialog";
            this.checkBoxMergeSameTimeCodesMakeDialog.UseVisualStyleBackColor = true;
            // 
            // numericUpDownMergeSameTimeCodesMaxDifference
            // 
            this.numericUpDownMergeSameTimeCodesMaxDifference.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMergeSameTimeCodesMaxDifference.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMergeSameTimeCodesMaxDifference.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMergeSameTimeCodesMaxDifference.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMergeSameTimeCodesMaxDifference.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMergeSameTimeCodesMaxDifference.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMergeSameTimeCodesMaxDifference.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMergeSameTimeCodesMaxDifference.DecimalPlaces = 0;
            this.numericUpDownMergeSameTimeCodesMaxDifference.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMergeSameTimeCodesMaxDifference.Location = new System.Drawing.Point(15, 41);
            this.numericUpDownMergeSameTimeCodesMaxDifference.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownMergeSameTimeCodesMaxDifference.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownMergeSameTimeCodesMaxDifference.Name = "numericUpDownMergeSameTimeCodesMaxDifference";
            this.numericUpDownMergeSameTimeCodesMaxDifference.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownMergeSameTimeCodesMaxDifference.TabIndex = 38;
            this.numericUpDownMergeSameTimeCodesMaxDifference.TabStop = false;
            this.numericUpDownMergeSameTimeCodesMaxDifference.ThousandsSeparator = false;
            this.numericUpDownMergeSameTimeCodesMaxDifference.Value = new decimal(new int[] {
            250,
            0,
            0,
            0});
            // 
            // labelMergeSameTimeCodesMaxDifference
            // 
            this.labelMergeSameTimeCodesMaxDifference.AutoSize = true;
            this.labelMergeSameTimeCodesMaxDifference.Location = new System.Drawing.Point(12, 23);
            this.labelMergeSameTimeCodesMaxDifference.Name = "labelMergeSameTimeCodesMaxDifference";
            this.labelMergeSameTimeCodesMaxDifference.Size = new System.Drawing.Size(139, 13);
            this.labelMergeSameTimeCodesMaxDifference.TabIndex = 40;
            this.labelMergeSameTimeCodesMaxDifference.Text = "Max. milliseconds difference";
            // 
            // groupBoxConvertColorsToDialog
            // 
            this.groupBoxConvertColorsToDialog.Controls.Add(this.checkBoxConvertColorsToDialogReBreakLines);
            this.groupBoxConvertColorsToDialog.Controls.Add(this.checkBoxConvertColorsToDialogAddNewLines);
            this.groupBoxConvertColorsToDialog.Controls.Add(this.checkBoxConvertColorsToDialogRemoveColorTags);
            this.groupBoxConvertColorsToDialog.Location = new System.Drawing.Point(305, 15);
            this.groupBoxConvertColorsToDialog.Name = "groupBoxConvertColorsToDialog";
            this.groupBoxConvertColorsToDialog.Size = new System.Drawing.Size(268, 149);
            this.groupBoxConvertColorsToDialog.TabIndex = 310;
            this.groupBoxConvertColorsToDialog.TabStop = false;
            this.groupBoxConvertColorsToDialog.Text = "Convert colors to dialog";
            this.groupBoxConvertColorsToDialog.Visible = false;
            // 
            // checkBoxConvertColorsToDialogReBreakLines
            // 
            this.checkBoxConvertColorsToDialogReBreakLines.AutoSize = true;
            this.checkBoxConvertColorsToDialogReBreakLines.Location = new System.Drawing.Point(9, 69);
            this.checkBoxConvertColorsToDialogReBreakLines.Name = "checkBoxConvertColorsToDialogReBreakLines";
            this.checkBoxConvertColorsToDialogReBreakLines.Size = new System.Drawing.Size(94, 17);
            this.checkBoxConvertColorsToDialogReBreakLines.TabIndex = 6;
            this.checkBoxConvertColorsToDialogReBreakLines.Text = "Re-break lines";
            this.checkBoxConvertColorsToDialogReBreakLines.UseVisualStyleBackColor = true;
            // 
            // checkBoxConvertColorsToDialogAddNewLines
            // 
            this.checkBoxConvertColorsToDialogAddNewLines.AutoSize = true;
            this.checkBoxConvertColorsToDialogAddNewLines.Location = new System.Drawing.Point(9, 46);
            this.checkBoxConvertColorsToDialogAddNewLines.Name = "checkBoxConvertColorsToDialogAddNewLines";
            this.checkBoxConvertColorsToDialogAddNewLines.Size = new System.Drawing.Size(165, 17);
            this.checkBoxConvertColorsToDialogAddNewLines.TabIndex = 5;
            this.checkBoxConvertColorsToDialogAddNewLines.Text = "Place every dash on new line";
            this.checkBoxConvertColorsToDialogAddNewLines.UseVisualStyleBackColor = true;
            // 
            // checkBoxConvertColorsToDialogRemoveColorTags
            // 
            this.checkBoxConvertColorsToDialogRemoveColorTags.AutoSize = true;
            this.checkBoxConvertColorsToDialogRemoveColorTags.Location = new System.Drawing.Point(9, 23);
            this.checkBoxConvertColorsToDialogRemoveColorTags.Name = "checkBoxConvertColorsToDialogRemoveColorTags";
            this.checkBoxConvertColorsToDialogRemoveColorTags.Size = new System.Drawing.Size(115, 17);
            this.checkBoxConvertColorsToDialogRemoveColorTags.TabIndex = 4;
            this.checkBoxConvertColorsToDialogRemoveColorTags.Text = "Remove color tags";
            this.checkBoxConvertColorsToDialogRemoveColorTags.UseVisualStyleBackColor = true;
            // 
            // groupBoxOutput
            // 
            this.groupBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOutput.Controls.Add(this.buttonBrowseEncoding);
            this.groupBoxOutput.Controls.Add(this.radioButtonSaveInOutputFolder);
            this.groupBoxOutput.Controls.Add(this.buttonTransportStreamSettings);
            this.groupBoxOutput.Controls.Add(this.linkLabelOpenOutputFolder);
            this.groupBoxOutput.Controls.Add(this.checkBoxUseStyleFromSource);
            this.groupBoxOutput.Controls.Add(this.checkBoxOverwrite);
            this.groupBoxOutput.Controls.Add(this.buttonStyles);
            this.groupBoxOutput.Controls.Add(this.buttonChooseFolder);
            this.groupBoxOutput.Controls.Add(this.comboBoxSubtitleFormats);
            this.groupBoxOutput.Controls.Add(this.textBoxOutputFolder);
            this.groupBoxOutput.Controls.Add(this.labelEncoding);
            this.groupBoxOutput.Controls.Add(this.radioButtonSaveInSourceFolder);
            this.groupBoxOutput.Controls.Add(this.comboBoxEncoding);
            this.groupBoxOutput.Controls.Add(this.labelOutputFormat);
            this.groupBoxOutput.Controls.Add(this.groupBoxConvertOptions);
            this.groupBoxOutput.Location = new System.Drawing.Point(12, 305);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(1014, 300);
            this.groupBoxOutput.TabIndex = 1;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "Output";
            // 
            // buttonBrowseEncoding
            // 
            this.buttonBrowseEncoding.Location = new System.Drawing.Point(300, 263);
            this.buttonBrowseEncoding.Name = "buttonBrowseEncoding";
            this.buttonBrowseEncoding.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowseEncoding.TabIndex = 12;
            this.buttonBrowseEncoding.Text = "...";
            this.buttonBrowseEncoding.UseVisualStyleBackColor = true;
            this.buttonBrowseEncoding.Click += new System.EventHandler(this.buttonBrowseEncoding_Click);
            // 
            // radioButtonSaveInOutputFolder
            // 
            this.radioButtonSaveInOutputFolder.AutoSize = true;
            this.radioButtonSaveInOutputFolder.Location = new System.Drawing.Point(10, 56);
            this.radioButtonSaveInOutputFolder.Name = "radioButtonSaveInOutputFolder";
            this.radioButtonSaveInOutputFolder.Size = new System.Drawing.Size(154, 17);
            this.radioButtonSaveInOutputFolder.TabIndex = 1;
            this.radioButtonSaveInOutputFolder.Text = "Save in output folder below";
            this.radioButtonSaveInOutputFolder.UseVisualStyleBackColor = true;
            this.radioButtonSaveInOutputFolder.CheckedChanged += new System.EventHandler(this.radioButtonSaveInOutputFolder_CheckedChanged);
            // 
            // buttonTransportStreamSettings
            // 
            this.buttonTransportStreamSettings.Location = new System.Drawing.Point(300, 236);
            this.buttonTransportStreamSettings.Name = "buttonTransportStreamSettings";
            this.buttonTransportStreamSettings.Size = new System.Drawing.Size(116, 23);
            this.buttonTransportStreamSettings.TabIndex = 10;
            this.buttonTransportStreamSettings.Text = "TS settings...";
            this.buttonTransportStreamSettings.UseVisualStyleBackColor = true;
            this.buttonTransportStreamSettings.Visible = false;
            this.buttonTransportStreamSettings.Click += new System.EventHandler(this.buttonTransportStreamSettings_Click);
            // 
            // linkLabelOpenOutputFolder
            // 
            this.linkLabelOpenOutputFolder.AutoSize = true;
            this.linkLabelOpenOutputFolder.Location = new System.Drawing.Point(357, 81);
            this.linkLabelOpenOutputFolder.Name = "linkLabelOpenOutputFolder";
            this.linkLabelOpenOutputFolder.Size = new System.Drawing.Size(42, 13);
            this.linkLabelOpenOutputFolder.TabIndex = 4;
            this.linkLabelOpenOutputFolder.TabStop = true;
            this.linkLabelOpenOutputFolder.Text = "Open...";
            this.linkLabelOpenOutputFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenOutputFolder_LinkClicked);
            // 
            // checkBoxUseStyleFromSource
            // 
            this.checkBoxUseStyleFromSource.AutoSize = true;
            this.checkBoxUseStyleFromSource.Location = new System.Drawing.Point(80, 236);
            this.checkBoxUseStyleFromSource.Name = "checkBoxUseStyleFromSource";
            this.checkBoxUseStyleFromSource.Size = new System.Drawing.Size(127, 17);
            this.checkBoxUseStyleFromSource.TabIndex = 9;
            this.checkBoxUseStyleFromSource.Text = "Use style from source";
            this.checkBoxUseStyleFromSource.UseVisualStyleBackColor = true;
            this.checkBoxUseStyleFromSource.Visible = false;
            // 
            // checkBoxOverwrite
            // 
            this.checkBoxOverwrite.AutoSize = true;
            this.checkBoxOverwrite.Location = new System.Drawing.Point(10, 122);
            this.checkBoxOverwrite.Name = "checkBoxOverwrite";
            this.checkBoxOverwrite.Size = new System.Drawing.Size(125, 17);
            this.checkBoxOverwrite.TabIndex = 5;
            this.checkBoxOverwrite.Text = "Overwrite exiting files";
            this.checkBoxOverwrite.UseVisualStyleBackColor = true;
            // 
            // buttonStyles
            // 
            this.buttonStyles.Location = new System.Drawing.Point(300, 207);
            this.buttonStyles.Name = "buttonStyles";
            this.buttonStyles.Size = new System.Drawing.Size(116, 23);
            this.buttonStyles.TabIndex = 8;
            this.buttonStyles.Text = "Style...";
            this.buttonStyles.UseVisualStyleBackColor = true;
            this.buttonStyles.Visible = false;
            this.buttonStyles.Click += new System.EventHandler(this.ButtonStylesClick);
            // 
            // buttonChooseFolder
            // 
            this.buttonChooseFolder.Enabled = false;
            this.buttonChooseFolder.Location = new System.Drawing.Point(325, 76);
            this.buttonChooseFolder.Name = "buttonChooseFolder";
            this.buttonChooseFolder.Size = new System.Drawing.Size(26, 23);
            this.buttonChooseFolder.TabIndex = 3;
            this.buttonChooseFolder.Text = "...";
            this.buttonChooseFolder.UseVisualStyleBackColor = true;
            this.buttonChooseFolder.Click += new System.EventHandler(this.buttonChooseFolder_Click);
            // 
            // comboBoxSubtitleFormats
            // 
            this.comboBoxSubtitleFormats.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSubtitleFormats.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSubtitleFormats.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSubtitleFormats.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSubtitleFormats.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSubtitleFormats.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSubtitleFormats.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSubtitleFormats.DropDownHeight = 400;
            this.comboBoxSubtitleFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFormats.DropDownWidth = 214;
            this.comboBoxSubtitleFormats.FormattingEnabled = true;
            this.comboBoxSubtitleFormats.Location = new System.Drawing.Point(80, 209);
            this.comboBoxSubtitleFormats.MaxLength = 32767;
            this.comboBoxSubtitleFormats.Name = "comboBoxSubtitleFormats";
            this.comboBoxSubtitleFormats.SelectedIndex = -1;
            this.comboBoxSubtitleFormats.SelectedItem = null;
            this.comboBoxSubtitleFormats.SelectedText = "";
            this.comboBoxSubtitleFormats.Size = new System.Drawing.Size(214, 21);
            this.comboBoxSubtitleFormats.TabIndex = 7;
            this.comboBoxSubtitleFormats.UsePopupWindow = false;
            this.comboBoxSubtitleFormats.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSubtitleFormatsSelectedIndexChanged);
            // 
            // textBoxOutputFolder
            // 
            this.textBoxOutputFolder.Enabled = false;
            this.textBoxOutputFolder.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxOutputFolder.Location = new System.Drawing.Point(17, 79);
            this.textBoxOutputFolder.Name = "textBoxOutputFolder";
            this.textBoxOutputFolder.Size = new System.Drawing.Size(302, 20);
            this.textBoxOutputFolder.TabIndex = 2;
            // 
            // labelEncoding
            // 
            this.labelEncoding.AutoSize = true;
            this.labelEncoding.Location = new System.Drawing.Point(10, 267);
            this.labelEncoding.Name = "labelEncoding";
            this.labelEncoding.Size = new System.Drawing.Size(52, 13);
            this.labelEncoding.TabIndex = 9;
            this.labelEncoding.Text = "Encoding";
            // 
            // radioButtonSaveInSourceFolder
            // 
            this.radioButtonSaveInSourceFolder.AutoSize = true;
            this.radioButtonSaveInSourceFolder.Checked = true;
            this.radioButtonSaveInSourceFolder.Location = new System.Drawing.Point(10, 27);
            this.radioButtonSaveInSourceFolder.Name = "radioButtonSaveInSourceFolder";
            this.radioButtonSaveInSourceFolder.Size = new System.Drawing.Size(141, 17);
            this.radioButtonSaveInSourceFolder.TabIndex = 0;
            this.radioButtonSaveInSourceFolder.TabStop = true;
            this.radioButtonSaveInSourceFolder.Text = "Save in source file folder";
            this.radioButtonSaveInSourceFolder.UseVisualStyleBackColor = true;
            this.radioButtonSaveInSourceFolder.CheckedChanged += new System.EventHandler(this.radioButtonSaveInSourceFolder_CheckedChanged);
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxEncoding.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxEncoding.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxEncoding.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxEncoding.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxEncoding.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxEncoding.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxEncoding.DropDownHeight = 400;
            this.comboBoxEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncoding.DropDownWidth = 214;
            this.comboBoxEncoding.FormattingEnabled = true;
            this.comboBoxEncoding.Location = new System.Drawing.Point(80, 264);
            this.comboBoxEncoding.MaxLength = 32767;
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.SelectedIndex = -1;
            this.comboBoxEncoding.SelectedItem = null;
            this.comboBoxEncoding.SelectedText = "";
            this.comboBoxEncoding.Size = new System.Drawing.Size(214, 21);
            this.comboBoxEncoding.TabIndex = 11;
            this.comboBoxEncoding.UsePopupWindow = false;
            // 
            // labelOutputFormat
            // 
            this.labelOutputFormat.AutoSize = true;
            this.labelOutputFormat.Location = new System.Drawing.Point(10, 212);
            this.labelOutputFormat.Name = "labelOutputFormat";
            this.labelOutputFormat.Size = new System.Drawing.Size(39, 13);
            this.labelOutputFormat.TabIndex = 6;
            this.labelOutputFormat.Text = "Format";
            // 
            // groupBoxInput
            // 
            this.groupBoxInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxInput.Controls.Add(this.labelNumberOfFiles);
            this.groupBoxInput.Controls.Add(this.textBoxFilter);
            this.groupBoxInput.Controls.Add(this.labelFilter);
            this.groupBoxInput.Controls.Add(this.comboBoxFilter);
            this.groupBoxInput.Controls.Add(this.checkBoxScanFolderRecursive);
            this.groupBoxInput.Controls.Add(this.buttonSearchFolder);
            this.groupBoxInput.Controls.Add(this.buttonInputBrowse);
            this.groupBoxInput.Controls.Add(this.labelChooseInputFiles);
            this.groupBoxInput.Controls.Add(this.listViewInputFiles);
            this.groupBoxInput.Location = new System.Drawing.Point(12, 12);
            this.groupBoxInput.Name = "groupBoxInput";
            this.groupBoxInput.Size = new System.Drawing.Size(1014, 287);
            this.groupBoxInput.TabIndex = 0;
            this.groupBoxInput.TabStop = false;
            this.groupBoxInput.Text = "Input";
            // 
            // labelNumberOfFiles
            // 
            this.labelNumberOfFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelNumberOfFiles.Location = new System.Drawing.Point(872, 258);
            this.labelNumberOfFiles.Name = "labelNumberOfFiles";
            this.labelNumberOfFiles.Size = new System.Drawing.Size(100, 16);
            this.labelNumberOfFiles.TabIndex = 14;
            this.labelNumberOfFiles.Text = "labelNumberOfFiles";
            this.labelNumberOfFiles.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxFilter.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxFilter.Location = new System.Drawing.Point(422, 258);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(158, 20);
            this.textBoxFilter.TabIndex = 13;
            this.textBoxFilter.TextChanged += new System.EventHandler(this.textBoxFilter_TextChanged);
            // 
            // labelFilter
            // 
            this.labelFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFilter.AutoSize = true;
            this.labelFilter.Location = new System.Drawing.Point(11, 261);
            this.labelFilter.Name = "labelFilter";
            this.labelFilter.Size = new System.Drawing.Size(29, 13);
            this.labelFilter.TabIndex = 11;
            this.labelFilter.Text = "Filter";
            // 
            // comboBoxFilter
            // 
            this.comboBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxFilter.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFilter.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFilter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFilter.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFilter.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFilter.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFilter.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFilter.DropDownHeight = 400;
            this.comboBoxFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFilter.DropDownWidth = 335;
            this.comboBoxFilter.FormattingEnabled = true;
            this.comboBoxFilter.Items.AddRange(new string[] {
            "All files",
            "SubRip .srt files without BOM header",
            "Files with subtitle with more than two lines",
            "Files that contains...",
            "File name cotains...",
            "Mkv language code contains..."});
            this.comboBoxFilter.Location = new System.Drawing.Point(81, 258);
            this.comboBoxFilter.MaxLength = 32767;
            this.comboBoxFilter.Name = "comboBoxFilter";
            this.comboBoxFilter.SelectedIndex = -1;
            this.comboBoxFilter.SelectedItem = null;
            this.comboBoxFilter.SelectedText = "";
            this.comboBoxFilter.Size = new System.Drawing.Size(335, 21);
            this.comboBoxFilter.TabIndex = 3;
            this.comboBoxFilter.UsePopupWindow = false;
            this.comboBoxFilter.SelectedIndexChanged += new System.EventHandler(this.comboBoxFilter_SelectedIndexChanged);
            // 
            // checkBoxScanFolderRecursive
            // 
            this.checkBoxScanFolderRecursive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxScanFolderRecursive.AutoSize = true;
            this.checkBoxScanFolderRecursive.Checked = true;
            this.checkBoxScanFolderRecursive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxScanFolderRecursive.Location = new System.Drawing.Point(778, 16);
            this.checkBoxScanFolderRecursive.Name = "checkBoxScanFolderRecursive";
            this.checkBoxScanFolderRecursive.Size = new System.Drawing.Size(74, 17);
            this.checkBoxScanFolderRecursive.TabIndex = 0;
            this.checkBoxScanFolderRecursive.Text = "Recursive";
            this.checkBoxScanFolderRecursive.UseVisualStyleBackColor = true;
            // 
            // buttonSearchFolder
            // 
            this.buttonSearchFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSearchFolder.Location = new System.Drawing.Point(858, 12);
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
            this.buttonInputBrowse.Location = new System.Drawing.Point(979, 41);
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
            this.listViewInputFiles.Size = new System.Drawing.Size(968, 211);
            this.listViewInputFiles.TabIndex = 2;
            this.listViewInputFiles.UseCompatibleStateImageBehavior = false;
            this.listViewInputFiles.View = System.Windows.Forms.View.Details;
            this.listViewInputFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewInputFiles_ColumnClick);
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
            this.addFilesToolStripMenuItem,
            this.toolStripSeparator2,
            this.removeToolStripMenuItem,
            this.removeAllToolStripMenuItem,
            this.openContainingFolderToolStripMenuItem,
            this.toolStripSeparator1,
            this.convertMkvSettingsToolStripMenuItem,
            this.toolStripMenuItemOcrEngine,
            this.alsoScanVideoFilesInSearchFolderslowToolStripMenuItem});
            this.contextMenuStripFiles.Name = "contextMenuStripStyles";
            this.contextMenuStripFiles.Size = new System.Drawing.Size(400, 170);
            this.contextMenuStripFiles.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripFilesOpening);
            // 
            // addFilesToolStripMenuItem
            // 
            this.addFilesToolStripMenuItem.Name = "addFilesToolStripMenuItem";
            this.addFilesToolStripMenuItem.Size = new System.Drawing.Size(399, 22);
            this.addFilesToolStripMenuItem.Text = "Add files";
            this.addFilesToolStripMenuItem.Click += new System.EventHandler(this.addFilesToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(396, 6);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(399, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.RemoveToolStripMenuItemClick);
            // 
            // removeAllToolStripMenuItem
            // 
            this.removeAllToolStripMenuItem.Name = "removeAllToolStripMenuItem";
            this.removeAllToolStripMenuItem.Size = new System.Drawing.Size(399, 22);
            this.removeAllToolStripMenuItem.Text = "Remove all";
            this.removeAllToolStripMenuItem.Click += new System.EventHandler(this.RemoveAllToolStripMenuItemClick);
            // 
            // openContainingFolderToolStripMenuItem
            // 
            this.openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            this.openContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(399, 22);
            this.openContainingFolderToolStripMenuItem.Text = "Open containing folder";
            this.openContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.openContainingFolderToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(396, 6);
            // 
            // convertMkvSettingsToolStripMenuItem
            // 
            this.convertMkvSettingsToolStripMenuItem.Name = "convertMkvSettingsToolStripMenuItem";
            this.convertMkvSettingsToolStripMenuItem.Size = new System.Drawing.Size(399, 22);
            this.convertMkvSettingsToolStripMenuItem.Text = "Mkv language in output file name: Three letter language code";
            this.convertMkvSettingsToolStripMenuItem.Click += new System.EventHandler(this.convertMkvThreeLetterLanguageCodesToTwoLettersToolStripMenuItem_Click);
            // 
            // toolStripMenuItemOcrEngine
            // 
            this.toolStripMenuItemOcrEngine.Name = "toolStripMenuItemOcrEngine";
            this.toolStripMenuItemOcrEngine.Size = new System.Drawing.Size(399, 22);
            this.toolStripMenuItemOcrEngine.Text = "OCR engine";
            this.toolStripMenuItemOcrEngine.Click += new System.EventHandler(this.toolStripMenuItemOcrEngine_Click);
            // 
            // alsoScanVideoFilesInSearchFolderslowToolStripMenuItem
            // 
            this.alsoScanVideoFilesInSearchFolderslowToolStripMenuItem.Name = "alsoScanVideoFilesInSearchFolderslowToolStripMenuItem";
            this.alsoScanVideoFilesInSearchFolderslowToolStripMenuItem.Size = new System.Drawing.Size(399, 22);
            this.alsoScanVideoFilesInSearchFolderslowToolStripMenuItem.Text = "Also scan video files in \"Search folder\" (slow)";
            this.alsoScanVideoFilesInSearchFolderslowToolStripMenuItem.Click += new System.EventHandler(this.alsoScanVideoFilesInSearchFolderSlowToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(15, 627);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(820, 10);
            this.progressBar1.TabIndex = 8;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 611);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 9;
            this.labelStatus.Text = "labelStatus";
            // 
            // labelError
            // 
            this.labelError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelError.AutoSize = true;
            this.labelError.Location = new System.Drawing.Point(175, 611);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(51, 13);
            this.labelError.TabIndex = 10;
            this.labelError.Text = "labelError";
            // 
            // nikseComboBoxTranslateModel
            // 
            this.nikseComboBoxTranslateModel.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxTranslateModel.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxTranslateModel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxTranslateModel.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxTranslateModel.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxTranslateModel.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxTranslateModel.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxTranslateModel.DropDownHeight = 400;
            this.nikseComboBoxTranslateModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.nikseComboBoxTranslateModel.DropDownWidth = 221;
            this.nikseComboBoxTranslateModel.FormattingEnabled = true;
            this.nikseComboBoxTranslateModel.Location = new System.Drawing.Point(11, 169);
            this.nikseComboBoxTranslateModel.MaxLength = 32767;
            this.nikseComboBoxTranslateModel.Name = "nikseComboBoxTranslateModel";
            this.nikseComboBoxTranslateModel.SelectedIndex = -1;
            this.nikseComboBoxTranslateModel.SelectedItem = null;
            this.nikseComboBoxTranslateModel.SelectedText = "";
            this.nikseComboBoxTranslateModel.Size = new System.Drawing.Size(221, 21);
            this.nikseComboBoxTranslateModel.TabIndex = 116;
            this.nikseComboBoxTranslateModel.TabStop = false;
            this.nikseComboBoxTranslateModel.UsePopupWindow = false;
            // 
            // nikseLabelModel
            // 
            this.nikseLabelModel.AutoSize = true;
            this.nikseLabelModel.Location = new System.Drawing.Point(11, 153);
            this.nikseLabelModel.Name = "nikseLabelModel";
            this.nikseLabelModel.Size = new System.Drawing.Size(39, 13);
            this.nikseLabelModel.TabIndex = 117;
            this.nikseLabelModel.Text = "Model:";
            // 
            // BatchConvert
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1035, 651);
            this.Controls.Add(this.labelError);
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
            this.Shown += new System.EventHandler(this.BatchConvert_Shown);
            this.ResizeEnd += new System.EventHandler(this.BatchConvert_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BatchConvert_KeyDown);
            this.groupBoxConvertOptions.ResumeLayout(false);
            this.groupBoxAutoTranslate.ResumeLayout(false);
            this.groupBoxAutoTranslate.PerformLayout();
            this.contextMenuStripOptions.ResumeLayout(false);
            this.groupBoxDeleteLines.ResumeLayout(false);
            this.groupBoxDeleteLines.PerformLayout();
            this.groupBoxRemoveStyle.ResumeLayout(false);
            this.groupBoxRemoveStyle.PerformLayout();
            this.groupBoxOffsetTimeCodes.ResumeLayout(false);
            this.groupBoxOffsetTimeCodes.PerformLayout();
            this.groupBoxChangeFrameRate.ResumeLayout(false);
            this.groupBoxChangeFrameRate.PerformLayout();
            this.groupBoxFixRtl.ResumeLayout(false);
            this.groupBoxFixRtl.PerformLayout();
            this.groupBoxSpeed.ResumeLayout(false);
            this.groupBoxSpeed.PerformLayout();
            this.groupBoxRemoveFormatting.ResumeLayout(false);
            this.groupBoxRemoveFormatting.PerformLayout();
            this.groupBoxApplyDurationLimits.ResumeLayout(false);
            this.groupBoxApplyDurationLimits.PerformLayout();
            this.groupBoxAdjustDuration.ResumeLayout(false);
            this.groupBoxAdjustDuration.PerformLayout();
            this.panelAdjustDurationFixed.ResumeLayout(false);
            this.panelAdjustDurationFixed.PerformLayout();
            this.panelAdjustDurationAddPercent.ResumeLayout(false);
            this.panelAdjustDurationAddPercent.PerformLayout();
            this.panelAdjustDurationAddSeconds.ResumeLayout(false);
            this.panelAdjustDurationAddSeconds.PerformLayout();
            this.panelAdjustDurationRecalc.ResumeLayout(false);
            this.panelAdjustDurationRecalc.PerformLayout();
            this.groupBoxBeautifyTimeCodes.ResumeLayout(false);
            this.groupBoxBeautifyTimeCodes.PerformLayout();
            this.groupBoxChangeCasing.ResumeLayout(false);
            this.groupBoxChangeCasing.PerformLayout();
            this.groupBoxMergeShortLines.ResumeLayout(false);
            this.groupBoxMergeShortLines.PerformLayout();
            this.groupBoxAssaChangeRes.ResumeLayout(false);
            this.groupBoxAssaChangeRes.PerformLayout();
            this.groupBoxSortBy.ResumeLayout(false);
            this.groupBoxMergeSameTimeCodes.ResumeLayout(false);
            this.groupBoxMergeSameTimeCodes.PerformLayout();
            this.groupBoxConvertColorsToDialog.ResumeLayout(false);
            this.groupBoxConvertColorsToDialog.PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBoxOutput;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSubtitleFormats;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelEncoding;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxEncoding;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelOutputFormat;
        private System.Windows.Forms.GroupBox groupBoxInput;
        private System.Windows.Forms.Button buttonInputBrowse;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelChooseInputFiles;
        private System.Windows.Forms.ListView listViewInputFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFName;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderFormat;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private System.Windows.Forms.Button buttonStyles;
        private System.Windows.Forms.GroupBox groupBoxOffsetTimeCodes;
        private System.Windows.Forms.GroupBox groupBoxChangeFrameRate;
        private Controls.NikseTimeUpDown timeUpDownAdjust;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelHourMinSecMilliSecond;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFrameRateTo;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelToFrameRate;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFrameRateFrom;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelFromFrameRate;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFiles;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAllToolStripMenuItem;
        private System.Windows.Forms.ProgressBar progressBar1;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelStatus;
        private System.Windows.Forms.RadioButton radioButtonShowLater;
        private System.Windows.Forms.RadioButton radioButtonShowEarlier;
        private System.Windows.Forms.Button buttonSearchFolder;
        private System.Windows.Forms.CheckBox checkBoxScanFolderRecursive;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelFilter;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFilter;
        private System.Windows.Forms.GroupBox groupBoxSpeed;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownPercent;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelPercent;
        private System.Windows.Forms.RadioButton radioButtonToDropFrame;
        private System.Windows.Forms.RadioButton radioButtonSpeedFromDropFrame;
        private System.Windows.Forms.RadioButton radioButtonSpeedCustom;
        private System.Windows.Forms.CheckBox checkBoxUseStyleFromSource;
        private System.Windows.Forms.Button buttonTransportStreamSettings;
        private System.Windows.Forms.RadioButton radioButtonSaveInOutputFolder;
        private System.Windows.Forms.LinkLabel linkLabelOpenOutputFolder;
        private System.Windows.Forms.CheckBox checkBoxOverwrite;
        private System.Windows.Forms.Button buttonChooseFolder;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxOutputFolder;
        private System.Windows.Forms.RadioButton radioButtonSaveInSourceFolder;
        private System.Windows.Forms.ListView listViewConvertOptions;
        private System.Windows.Forms.ColumnHeader ActionCheckBox;
        private System.Windows.Forms.ColumnHeader Action;
        private System.Windows.Forms.Button buttonConvertOptionsSettings;
        private System.Windows.Forms.GroupBox groupBoxFixRtl;
        private System.Windows.Forms.RadioButton radioButtonReverseStartEnd;
        private System.Windows.Forms.RadioButton radioButtonRemoveUnicode;
        private System.Windows.Forms.RadioButton radioButtonAddUnicode;
        private System.Windows.Forms.GroupBox groupBoxMergeShortLines;
        private System.Windows.Forms.CheckBox checkBoxOnlyContinuationLines;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxCharacters;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxMillisecondsBetweenLines;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelMaxMillisecondsBetweenLines;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelMaxCharacters;
        private System.Windows.Forms.Button buttonSwapFrameRate;
        private System.Windows.Forms.GroupBox groupBoxChangeCasing;
        private System.Windows.Forms.RadioButton radioButtonFixOnlyNames;
        private System.Windows.Forms.RadioButton radioButtonLowercase;
        private System.Windows.Forms.RadioButton radioButtonUppercase;
        private System.Windows.Forms.RadioButton radioButtonNormal;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripOptions;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem inverseSelectionToolStripMenuItem;
        private System.Windows.Forms.Button buttonBrowseEncoding;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelNumberOfFiles;
        private System.Windows.Forms.ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem convertMkvSettingsToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxRemoveStyle;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelStyleActor;
        private System.Windows.Forms.GroupBox groupBoxAdjustDuration;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelAdjustDurationVia;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxAdjustDurationVia;
        private System.Windows.Forms.Panel panelAdjustDurationAddPercent;
        private System.Windows.Forms.Panel panelAdjustDurationAddSeconds;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownSeconds;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelAddSeconds;
        private System.Windows.Forms.Panel panelAdjustDurationFixed;
        private Nikse.SubtitleEdit.Controls.NikseLabel label1;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownAdjustViaPercent;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelAdjustViaPercent;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownFixedMilliseconds;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelMillisecondsFixed;
        private System.Windows.Forms.Panel panelAdjustDurationRecalc;
        private System.Windows.Forms.CheckBox checkBoxExtendOnly;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownOptimalCharsSec;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelOptimalCharsSec;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxCharsSec;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelMaxCharsPerSecond;
        private System.Windows.Forms.ToolStripMenuItem addFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.GroupBox groupBoxDeleteLines;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxDeleteContains;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelDeleteLinesContaining;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownDeleteLast;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelDeleteLastLines;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownDeleteFirst;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelDeleteFirstLines;
        private System.Windows.Forms.GroupBox groupBoxAssaChangeRes;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelTargetRes;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownTargetHeight;
        private System.Windows.Forms.Button buttonGetResolutionFromVideo;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelX;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownTargetWidth;
        private System.Windows.Forms.CheckBox checkBoxDrawing;
        private System.Windows.Forms.CheckBox checkBoxPosition;
        private System.Windows.Forms.CheckBox checkBoxFontSize;
        private System.Windows.Forms.CheckBox checkBoxMargins;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOcrEngine;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelError;
        private System.Windows.Forms.GroupBox groupBoxMergeSameTimeCodes;
        private System.Windows.Forms.CheckBox checkBoxMergeSameTimeCodesReBreakLines;
        private System.Windows.Forms.CheckBox checkBoxMergeSameTimeCodesMakeDialog;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMergeSameTimeCodesMaxDifference;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelMergeSameTimeCodesMaxDifference;
        private System.Windows.Forms.GroupBox groupBoxConvertColorsToDialog;
        private System.Windows.Forms.CheckBox checkBoxConvertColorsToDialogReBreakLines;
        private System.Windows.Forms.CheckBox checkBoxConvertColorsToDialogAddNewLines;
        private System.Windows.Forms.CheckBox checkBoxConvertColorsToDialogRemoveColorTags;
        private System.Windows.Forms.GroupBox groupBoxSortBy;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSortBy;
        private System.Windows.Forms.CheckBox checkBoxOnlyAllUpper;
        private System.Windows.Forms.CheckBox checkBoxFixNames;
        private System.Windows.Forms.RadioButton radioButtonProperCase;
        private System.Windows.Forms.CheckBox checkBoxProperCaseOnlyUpper;
        private System.Windows.Forms.ToolStripMenuItem alsoScanVideoFilesInSearchFolderslowToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxEnforceDurationLimits;
        private System.Windows.Forms.GroupBox groupBoxBeautifyTimeCodes;
        private System.Windows.Forms.CheckBox checkBoxBeautifyTimeCodesSnapToShotChanges;
        private System.Windows.Forms.CheckBox checkBoxBeautifyTimeCodesUseExactTimeCodes;
        private System.Windows.Forms.CheckBox checkBoxBeautifyTimeCodesAlignTimeCodes;
        private System.Windows.Forms.Button buttonBeautifyTimeCodesEditProfile;
        private System.Windows.Forms.CheckBox checkBoxAdjustDurationCheckShotChanges;
        private System.Windows.Forms.GroupBox groupBoxApplyDurationLimits;
        private System.Windows.Forms.CheckBox checkBoxApplyDurationLimitsMinDuration;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownApplyDurationLimitsMinDuration;
        private System.Windows.Forms.CheckBox checkBoxApplyDurationLimitsCheckShotChanges;
        private System.Windows.Forms.CheckBox checkBoxApplyDurationLimitsMaxDuration;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownApplyDurationLimitsMaxDuration;
        private System.Windows.Forms.GroupBox groupBoxRemoveFormatting;
        private System.Windows.Forms.CheckBox checkBoxRemoveAlignment;
        private System.Windows.Forms.CheckBox checkBoxRemoveFontName;
        private System.Windows.Forms.CheckBox checkBoxRemoveColor;
        private System.Windows.Forms.CheckBox checkBoxRemoveUnderline;
        private System.Windows.Forms.CheckBox checkBoxRemoveItalic;
        private System.Windows.Forms.CheckBox checkBoxRemoveBold;
        private System.Windows.Forms.CheckBox checkBoxRemoveAllFormatting;
        private Controls.NikseTextBox textBoxFilter;
        private Controls.NikseTextBox textBoxRemoveStyle;
        private System.Windows.Forms.GroupBox groupBoxAutoTranslate;
        private System.Windows.Forms.LinkLabel linkLabelPoweredBy;
        private Controls.NikseComboBox nikseComboBoxEngine;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelTarget;
        private Controls.NikseComboBox comboBoxTarget;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelSource;
        private Controls.NikseComboBox comboBoxSource;
        private Controls.NikseLabel nikseLabelModel;
        private Controls.NikseComboBox nikseComboBoxTranslateModel;
    }
}