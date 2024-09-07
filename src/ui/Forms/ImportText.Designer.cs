﻿namespace Nikse.SubtitleEdit.Forms
{
    partial class ImportText
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
            this.buttonOpenText = new System.Windows.Forms.Button();
            this.groupBoxImportText = new System.Windows.Forms.GroupBox();
            this.buttonBrowseEncoding = new System.Windows.Forms.Button();
            this.labelEncoding = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxEncoding = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxMultipleFiles = new System.Windows.Forms.CheckBox();
            this.textBoxText = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.listViewInputFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxImportOptions = new System.Windows.Forms.GroupBox();
            this.checkBoxTakeTimeFromFileNames = new System.Windows.Forms.CheckBox();
            this.groupBoxAutoSplitSettings = new System.Windows.Forms.GroupBox();
            this.checkBoxAutoSplitAtEnd = new System.Windows.Forms.CheckBox();
            this.labelSubMaxLen = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.numericUpDownSubtitleLineMaximumLength = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.textBoxAsEnd = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.checkBoxAutoSplitRemoveLinesNoLetters = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoSplitAtBlankLines = new System.Windows.Forms.CheckBox();
            this.numericUpDownAutoSplitMaxLines = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelAutoSplitNumberOfLines = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.checkBoxAutoBreak = new System.Windows.Forms.CheckBox();
            this.checkBoxGenerateTimeCodes = new System.Windows.Forms.CheckBox();
            this.groupBoxTimeCodes = new System.Windows.Forms.GroupBox();
            this.labelGapBetweenSubtitles = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.numericUpDownGapBetweenLines = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.groupBoxDuration = new System.Windows.Forms.GroupBox();
            this.numericUpDownDurationFixed = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.radioButtonDurationFixed = new System.Windows.Forms.RadioButton();
            this.radioButtonDurationAuto = new System.Windows.Forms.RadioButton();
            this.checkBoxMergeShortLines = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveLinesWithoutLetters = new System.Windows.Forms.CheckBox();
            this.groupBoxSplitting = new System.Windows.Forms.GroupBox();
            this.comboBoxLineMode = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxLineBreak = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelLineBreak = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.radioButtonSplitAtBlankLines = new System.Windows.Forms.RadioButton();
            this.radioButtonAutoSplit = new System.Windows.Forms.RadioButton();
            this.radioButtonLineMode = new System.Windows.Forms.RadioButton();
            this.checkBoxRemoveEmptyLines = new System.Windows.Forms.CheckBox();
            this.checkBoxUseTimeCodeFromCurrentFile = new System.Windows.Forms.CheckBox();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.groupBoxImportResult = new System.Windows.Forms.GroupBox();
            this.SubtitleListview1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.contextMenuStripPreview = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.startNumberingFromToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelStatus = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.groupBoxImportText.SuspendLayout();
            this.contextMenuStripListView.SuspendLayout();
            this.groupBoxImportOptions.SuspendLayout();
            this.groupBoxAutoSplitSettings.SuspendLayout();
            this.groupBoxTimeCodes.SuspendLayout();
            this.groupBoxDuration.SuspendLayout();
            this.groupBoxSplitting.SuspendLayout();
            this.groupBoxImportResult.SuspendLayout();
            this.contextMenuStripPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOpenText
            // 
            this.buttonOpenText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenText.Location = new System.Drawing.Point(475, 19);
            this.buttonOpenText.Name = "buttonOpenText";
            this.buttonOpenText.Size = new System.Drawing.Size(143, 23);
            this.buttonOpenText.TabIndex = 4;
            this.buttonOpenText.Text = "Open file...";
            this.buttonOpenText.UseVisualStyleBackColor = true;
            this.buttonOpenText.Click += new System.EventHandler(this.ButtonOpenTextClick);
            // 
            // groupBoxImportText
            // 
            this.groupBoxImportText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportText.Controls.Add(this.buttonBrowseEncoding);
            this.groupBoxImportText.Controls.Add(this.labelEncoding);
            this.groupBoxImportText.Controls.Add(this.comboBoxEncoding);
            this.groupBoxImportText.Controls.Add(this.checkBoxMultipleFiles);
            this.groupBoxImportText.Controls.Add(this.buttonOpenText);
            this.groupBoxImportText.Controls.Add(this.textBoxText);
            this.groupBoxImportText.Controls.Add(this.listViewInputFiles);
            this.groupBoxImportText.Location = new System.Drawing.Point(12, 12);
            this.groupBoxImportText.Name = "groupBoxImportText";
            this.groupBoxImportText.Size = new System.Drawing.Size(624, 410);
            this.groupBoxImportText.TabIndex = 0;
            this.groupBoxImportText.TabStop = false;
            this.groupBoxImportText.Text = "Import text";
            // 
            // buttonBrowseEncoding
            // 
            this.buttonBrowseEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseEncoding.Location = new System.Drawing.Point(592, 52);
            this.buttonBrowseEncoding.Name = "buttonBrowseEncoding";
            this.buttonBrowseEncoding.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowseEncoding.TabIndex = 6;
            this.buttonBrowseEncoding.Text = "...";
            this.buttonBrowseEncoding.UseVisualStyleBackColor = true;
            this.buttonBrowseEncoding.Click += new System.EventHandler(this.buttonBrowseEncoding_Click);
            // 
            // labelEncoding
            // 
            this.labelEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelEncoding.AutoSize = true;
            this.labelEncoding.Location = new System.Drawing.Point(302, 56);
            this.labelEncoding.Name = "labelEncoding";
            this.labelEncoding.Size = new System.Drawing.Size(50, 13);
            this.labelEncoding.TabIndex = 13;
            this.labelEncoding.Text = "Encoding";
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
            this.comboBoxEncoding.Location = new System.Drawing.Point(372, 53);
            this.comboBoxEncoding.MaxLength = 32767;
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.SelectedIndex = -1;
            this.comboBoxEncoding.SelectedItem = null;
            this.comboBoxEncoding.SelectedText = "";
            this.comboBoxEncoding.Size = new System.Drawing.Size(214, 21);
            this.comboBoxEncoding.TabIndex = 5;
            this.comboBoxEncoding.UsePopupWindow = false;
            // 
            // checkBoxMultipleFiles
            // 
            this.checkBoxMultipleFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxMultipleFiles.AutoSize = true;
            this.checkBoxMultipleFiles.Location = new System.Drawing.Point(271, 22);
            this.checkBoxMultipleFiles.Name = "checkBoxMultipleFiles";
            this.checkBoxMultipleFiles.Size = new System.Drawing.Size(198, 17);
            this.checkBoxMultipleFiles.TabIndex = 2;
            this.checkBoxMultipleFiles.Text = "Multiple files - one file is one subtitle";
            this.checkBoxMultipleFiles.UseVisualStyleBackColor = true;
            this.checkBoxMultipleFiles.CheckedChanged += new System.EventHandler(this.checkBoxMultipleFiles_CheckedChanged);
            // 
            // textBoxText
            // 
            this.textBoxText.AllowDrop = true;
            this.textBoxText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxText.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxText.Location = new System.Drawing.Point(6, 91);
            this.textBoxText.MaxLength = 0;
            this.textBoxText.Multiline = true;
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxText.Size = new System.Drawing.Size(612, 313);
            this.textBoxText.TabIndex = 7;
            this.textBoxText.TextChanged += new System.EventHandler(this.TextBoxTextTextChanged);
            this.textBoxText.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBoxTextDragDrop);
            this.textBoxText.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBoxTextDragEnter);
            // 
            // listViewInputFiles
            // 
            this.listViewInputFiles.AllowDrop = true;
            this.listViewInputFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewInputFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFName,
            this.columnHeaderSize});
            this.listViewInputFiles.ContextMenuStrip = this.contextMenuStripListView;
            this.listViewInputFiles.FullRowSelect = true;
            this.listViewInputFiles.HideSelection = false;
            this.listViewInputFiles.Location = new System.Drawing.Point(6, 91);
            this.listViewInputFiles.Name = "listViewInputFiles";
            this.listViewInputFiles.Size = new System.Drawing.Size(612, 313);
            this.listViewInputFiles.TabIndex = 6;
            this.listViewInputFiles.UseCompatibleStateImageBehavior = false;
            this.listViewInputFiles.View = System.Windows.Forms.View.Details;
            this.listViewInputFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewInputFiles_DragDrop);
            this.listViewInputFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewInputFiles_DragEnter);
            // 
            // columnHeaderFName
            // 
            this.columnHeaderFName.Text = "File name";
            this.columnHeaderFName.Width = 420;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.Width = 81;
            // 
            // contextMenuStripListView
            // 
            this.contextMenuStripListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem});
            this.contextMenuStripListView.Name = "contextMenuStripListView";
            this.contextMenuStripListView.Size = new System.Drawing.Size(102, 26);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // groupBoxImportOptions
            // 
            this.groupBoxImportOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportOptions.Controls.Add(this.checkBoxTakeTimeFromFileNames);
            this.groupBoxImportOptions.Controls.Add(this.groupBoxAutoSplitSettings);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxAutoBreak);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxGenerateTimeCodes);
            this.groupBoxImportOptions.Controls.Add(this.groupBoxTimeCodes);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxMergeShortLines);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxRemoveLinesWithoutLetters);
            this.groupBoxImportOptions.Controls.Add(this.groupBoxSplitting);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxRemoveEmptyLines);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxUseTimeCodeFromCurrentFile);
            this.groupBoxImportOptions.Location = new System.Drawing.Point(642, 12);
            this.groupBoxImportOptions.Name = "groupBoxImportOptions";
            this.groupBoxImportOptions.Size = new System.Drawing.Size(402, 410);
            this.groupBoxImportOptions.TabIndex = 1;
            this.groupBoxImportOptions.TabStop = false;
            this.groupBoxImportOptions.Text = "Import options";
            // 
            // checkBoxTakeTimeFromFileNames
            // 
            this.checkBoxTakeTimeFromFileNames.AutoSize = true;
            this.checkBoxTakeTimeFromFileNames.Location = new System.Drawing.Point(157, 250);
            this.checkBoxTakeTimeFromFileNames.Name = "checkBoxTakeTimeFromFileNames";
            this.checkBoxTakeTimeFromFileNames.Size = new System.Drawing.Size(143, 17);
            this.checkBoxTakeTimeFromFileNames.TabIndex = 5;
            this.checkBoxTakeTimeFromFileNames.Text = "Take time from file name";
            this.checkBoxTakeTimeFromFileNames.UseVisualStyleBackColor = true;
            // 
            // groupBoxAutoSplitSettings
            // 
            this.groupBoxAutoSplitSettings.Controls.Add(this.checkBoxAutoSplitAtEnd);
            this.groupBoxAutoSplitSettings.Controls.Add(this.labelSubMaxLen);
            this.groupBoxAutoSplitSettings.Controls.Add(this.numericUpDownSubtitleLineMaximumLength);
            this.groupBoxAutoSplitSettings.Controls.Add(this.textBoxAsEnd);
            this.groupBoxAutoSplitSettings.Controls.Add(this.checkBoxAutoSplitRemoveLinesNoLetters);
            this.groupBoxAutoSplitSettings.Controls.Add(this.checkBoxAutoSplitAtBlankLines);
            this.groupBoxAutoSplitSettings.Controls.Add(this.numericUpDownAutoSplitMaxLines);
            this.groupBoxAutoSplitSettings.Controls.Add(this.labelAutoSplitNumberOfLines);
            this.groupBoxAutoSplitSettings.Location = new System.Drawing.Point(6, 91);
            this.groupBoxAutoSplitSettings.Name = "groupBoxAutoSplitSettings";
            this.groupBoxAutoSplitSettings.Size = new System.Drawing.Size(390, 153);
            this.groupBoxAutoSplitSettings.TabIndex = 1;
            this.groupBoxAutoSplitSettings.TabStop = false;
            this.groupBoxAutoSplitSettings.Text = "Auto split text settings";
            // 
            // checkBoxAutoSplitAtEnd
            // 
            this.checkBoxAutoSplitAtEnd.AutoSize = true;
            this.checkBoxAutoSplitAtEnd.Checked = true;
            this.checkBoxAutoSplitAtEnd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoSplitAtEnd.Location = new System.Drawing.Point(6, 119);
            this.checkBoxAutoSplitAtEnd.Name = "checkBoxAutoSplitAtEnd";
            this.checkBoxAutoSplitAtEnd.Size = new System.Drawing.Size(109, 17);
            this.checkBoxAutoSplitAtEnd.TabIndex = 50;
            this.checkBoxAutoSplitAtEnd.Text = "Split at end chars";
            this.checkBoxAutoSplitAtEnd.UseVisualStyleBackColor = true;
            this.checkBoxAutoSplitAtEnd.CheckedChanged += new System.EventHandler(this.checkBoxAutoSplitAtEnd_CheckedChanged);
            // 
            // labelSubMaxLen
            // 
            this.labelSubMaxLen.AutoSize = true;
            this.labelSubMaxLen.Location = new System.Drawing.Point(6, 47);
            this.labelSubMaxLen.Name = "labelSubMaxLen";
            this.labelSubMaxLen.Size = new System.Drawing.Size(103, 13);
            this.labelSubMaxLen.TabIndex = 17;
            this.labelSubMaxLen.Text = "Subtitle max. length";
            // 
            // numericUpDownSubtitleLineMaximumLength
            // 
            this.numericUpDownSubtitleLineMaximumLength.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownSubtitleLineMaximumLength.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownSubtitleLineMaximumLength.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownSubtitleLineMaximumLength.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownSubtitleLineMaximumLength.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownSubtitleLineMaximumLength.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownSubtitleLineMaximumLength.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownSubtitleLineMaximumLength.DecimalPlaces = 0;
            this.numericUpDownSubtitleLineMaximumLength.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownSubtitleLineMaximumLength.Location = new System.Drawing.Point(112, 44);
            this.numericUpDownSubtitleLineMaximumLength.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericUpDownSubtitleLineMaximumLength.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownSubtitleLineMaximumLength.Name = "numericUpDownSubtitleLineMaximumLength";
            this.numericUpDownSubtitleLineMaximumLength.Size = new System.Drawing.Size(49, 23);
            this.numericUpDownSubtitleLineMaximumLength.TabIndex = 20;
            this.numericUpDownSubtitleLineMaximumLength.TabStop = false;
            this.numericUpDownSubtitleLineMaximumLength.ThousandsSeparator = false;
            this.numericUpDownSubtitleLineMaximumLength.Value = new decimal(new int[] {
            43,
            0,
            0,
            0});
            this.numericUpDownSubtitleLineMaximumLength.ValueChanged += new System.EventHandler(this.numericUpDownSubtitleLineMaximumLength_ValueChanged);
            // 
            // textBoxAsEnd
            // 
            this.textBoxAsEnd.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxAsEnd.Location = new System.Drawing.Point(118, 117);
            this.textBoxAsEnd.MaxLength = 5;
            this.textBoxAsEnd.Name = "textBoxAsEnd";
            this.textBoxAsEnd.Size = new System.Drawing.Size(50, 21);
            this.textBoxAsEnd.TabIndex = 51;
            this.textBoxAsEnd.TextChanged += new System.EventHandler(this.textBoxAsEnd1_TextChanged);
            this.textBoxAsEnd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxAsEnd1_KeyPress);
            // 
            // checkBoxAutoSplitRemoveLinesNoLetters
            // 
            this.checkBoxAutoSplitRemoveLinesNoLetters.AutoSize = true;
            this.checkBoxAutoSplitRemoveLinesNoLetters.Checked = true;
            this.checkBoxAutoSplitRemoveLinesNoLetters.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoSplitRemoveLinesNoLetters.Location = new System.Drawing.Point(6, 96);
            this.checkBoxAutoSplitRemoveLinesNoLetters.Name = "checkBoxAutoSplitRemoveLinesNoLetters";
            this.checkBoxAutoSplitRemoveLinesNoLetters.Size = new System.Drawing.Size(162, 17);
            this.checkBoxAutoSplitRemoveLinesNoLetters.TabIndex = 40;
            this.checkBoxAutoSplitRemoveLinesNoLetters.Text = "Remove lines without letters";
            this.checkBoxAutoSplitRemoveLinesNoLetters.UseVisualStyleBackColor = true;
            this.checkBoxAutoSplitRemoveLinesNoLetters.CheckedChanged += new System.EventHandler(this.checkBoxAutoSplitRemoveLinesNoLetters_CheckedChanged);
            // 
            // checkBoxAutoSplitAtBlankLines
            // 
            this.checkBoxAutoSplitAtBlankLines.AutoSize = true;
            this.checkBoxAutoSplitAtBlankLines.Checked = true;
            this.checkBoxAutoSplitAtBlankLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoSplitAtBlankLines.Location = new System.Drawing.Point(6, 73);
            this.checkBoxAutoSplitAtBlankLines.Name = "checkBoxAutoSplitAtBlankLines";
            this.checkBoxAutoSplitAtBlankLines.Size = new System.Drawing.Size(111, 17);
            this.checkBoxAutoSplitAtBlankLines.TabIndex = 30;
            this.checkBoxAutoSplitAtBlankLines.Text = "Split at blank lines";
            this.checkBoxAutoSplitAtBlankLines.UseVisualStyleBackColor = true;
            this.checkBoxAutoSplitAtBlankLines.CheckedChanged += new System.EventHandler(this.checkBoxAutoSplitAtBlankLines_CheckedChanged);
            // 
            // numericUpDownAutoSplitMaxLines
            // 
            this.numericUpDownAutoSplitMaxLines.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownAutoSplitMaxLines.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownAutoSplitMaxLines.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownAutoSplitMaxLines.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownAutoSplitMaxLines.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownAutoSplitMaxLines.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownAutoSplitMaxLines.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownAutoSplitMaxLines.DecimalPlaces = 0;
            this.numericUpDownAutoSplitMaxLines.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownAutoSplitMaxLines.Location = new System.Drawing.Point(115, 17);
            this.numericUpDownAutoSplitMaxLines.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownAutoSplitMaxLines.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownAutoSplitMaxLines.Name = "numericUpDownAutoSplitMaxLines";
            this.numericUpDownAutoSplitMaxLines.Size = new System.Drawing.Size(46, 23);
            this.numericUpDownAutoSplitMaxLines.TabIndex = 10;
            this.numericUpDownAutoSplitMaxLines.TabStop = false;
            this.numericUpDownAutoSplitMaxLines.ThousandsSeparator = false;
            this.numericUpDownAutoSplitMaxLines.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownAutoSplitMaxLines.ValueChanged += new System.EventHandler(this.numericUpDownAutoSplitMaxLines_ValueChanged);
            // 
            // labelAutoSplitNumberOfLines
            // 
            this.labelAutoSplitNumberOfLines.AutoSize = true;
            this.labelAutoSplitNumberOfLines.Location = new System.Drawing.Point(6, 19);
            this.labelAutoSplitNumberOfLines.Name = "labelAutoSplitNumberOfLines";
            this.labelAutoSplitNumberOfLines.Size = new System.Drawing.Size(103, 13);
            this.labelAutoSplitNumberOfLines.TabIndex = 6;
            this.labelAutoSplitNumberOfLines.Text = "Max number of lines";
            // 
            // checkBoxAutoBreak
            // 
            this.checkBoxAutoBreak.AutoSize = true;
            this.checkBoxAutoBreak.Location = new System.Drawing.Point(19, 163);
            this.checkBoxAutoBreak.Name = "checkBoxAutoBreak";
            this.checkBoxAutoBreak.Size = new System.Drawing.Size(104, 17);
            this.checkBoxAutoBreak.TabIndex = 7;
            this.checkBoxAutoBreak.Text = "Auto-break lines";
            this.checkBoxAutoBreak.UseVisualStyleBackColor = true;
            this.checkBoxAutoBreak.CheckedChanged += new System.EventHandler(this.checkBoxAutoBreak_CheckedChanged);
            // 
            // checkBoxGenerateTimeCodes
            // 
            this.checkBoxGenerateTimeCodes.AutoSize = true;
            this.checkBoxGenerateTimeCodes.Location = new System.Drawing.Point(16, 250);
            this.checkBoxGenerateTimeCodes.Name = "checkBoxGenerateTimeCodes";
            this.checkBoxGenerateTimeCodes.Size = new System.Drawing.Size(125, 17);
            this.checkBoxGenerateTimeCodes.TabIndex = 4;
            this.checkBoxGenerateTimeCodes.Text = "Generate time codes";
            this.checkBoxGenerateTimeCodes.UseVisualStyleBackColor = true;
            this.checkBoxGenerateTimeCodes.CheckedChanged += new System.EventHandler(this.checkBoxGenerateTimeCodes_CheckedChanged);
            // 
            // groupBoxTimeCodes
            // 
            this.groupBoxTimeCodes.Controls.Add(this.labelGapBetweenSubtitles);
            this.groupBoxTimeCodes.Controls.Add(this.numericUpDownGapBetweenLines);
            this.groupBoxTimeCodes.Controls.Add(this.groupBoxDuration);
            this.groupBoxTimeCodes.Enabled = false;
            this.groupBoxTimeCodes.Location = new System.Drawing.Point(6, 273);
            this.groupBoxTimeCodes.Name = "groupBoxTimeCodes";
            this.groupBoxTimeCodes.Size = new System.Drawing.Size(390, 126);
            this.groupBoxTimeCodes.TabIndex = 6;
            this.groupBoxTimeCodes.TabStop = false;
            this.groupBoxTimeCodes.Text = "Time codes";
            // 
            // labelGapBetweenSubtitles
            // 
            this.labelGapBetweenSubtitles.AutoSize = true;
            this.labelGapBetweenSubtitles.Location = new System.Drawing.Point(6, 23);
            this.labelGapBetweenSubtitles.Name = "labelGapBetweenSubtitles";
            this.labelGapBetweenSubtitles.Size = new System.Drawing.Size(180, 13);
            this.labelGapBetweenSubtitles.TabIndex = 0;
            this.labelGapBetweenSubtitles.Text = "Gap between subtitles (milliseconds)";
            // 
            // numericUpDownGapBetweenLines
            // 
            this.numericUpDownGapBetweenLines.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownGapBetweenLines.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownGapBetweenLines.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownGapBetweenLines.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownGapBetweenLines.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownGapBetweenLines.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownGapBetweenLines.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownGapBetweenLines.DecimalPlaces = 0;
            this.numericUpDownGapBetweenLines.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownGapBetweenLines.Location = new System.Drawing.Point(192, 20);
            this.numericUpDownGapBetweenLines.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownGapBetweenLines.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownGapBetweenLines.Name = "numericUpDownGapBetweenLines";
            this.numericUpDownGapBetweenLines.Size = new System.Drawing.Size(64, 23);
            this.numericUpDownGapBetweenLines.TabIndex = 1;
            this.numericUpDownGapBetweenLines.TabStop = false;
            this.numericUpDownGapBetweenLines.ThousandsSeparator = false;
            this.numericUpDownGapBetweenLines.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownGapBetweenLines.ValueChanged += new System.EventHandler(this.NumericUpDownGapBetweenLinesValueChanged);
            // 
            // groupBoxDuration
            // 
            this.groupBoxDuration.Controls.Add(this.numericUpDownDurationFixed);
            this.groupBoxDuration.Controls.Add(this.radioButtonDurationFixed);
            this.groupBoxDuration.Controls.Add(this.radioButtonDurationAuto);
            this.groupBoxDuration.Location = new System.Drawing.Point(9, 47);
            this.groupBoxDuration.Name = "groupBoxDuration";
            this.groupBoxDuration.Size = new System.Drawing.Size(247, 70);
            this.groupBoxDuration.TabIndex = 2;
            this.groupBoxDuration.TabStop = false;
            this.groupBoxDuration.Text = "Duration";
            // 
            // numericUpDownDurationFixed
            // 
            this.numericUpDownDurationFixed.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownDurationFixed.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownDurationFixed.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownDurationFixed.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownDurationFixed.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownDurationFixed.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownDurationFixed.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownDurationFixed.DecimalPlaces = 0;
            this.numericUpDownDurationFixed.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownDurationFixed.Location = new System.Drawing.Point(111, 42);
            this.numericUpDownDurationFixed.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownDurationFixed.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownDurationFixed.Name = "numericUpDownDurationFixed";
            this.numericUpDownDurationFixed.Size = new System.Drawing.Size(64, 23);
            this.numericUpDownDurationFixed.TabIndex = 2;
            this.numericUpDownDurationFixed.TabStop = false;
            this.numericUpDownDurationFixed.ThousandsSeparator = false;
            this.numericUpDownDurationFixed.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDownDurationFixed.ValueChanged += new System.EventHandler(this.NumericUpDownDurationFixedValueChanged);
            // 
            // radioButtonDurationFixed
            // 
            this.radioButtonDurationFixed.AutoSize = true;
            this.radioButtonDurationFixed.Location = new System.Drawing.Point(16, 42);
            this.radioButtonDurationFixed.Name = "radioButtonDurationFixed";
            this.radioButtonDurationFixed.Size = new System.Drawing.Size(51, 17);
            this.radioButtonDurationFixed.TabIndex = 1;
            this.radioButtonDurationFixed.Text = "Fixed";
            this.radioButtonDurationFixed.UseVisualStyleBackColor = true;
            this.radioButtonDurationFixed.CheckedChanged += new System.EventHandler(this.RadioButtonDurationFixedCheckedChanged);
            // 
            // radioButtonDurationAuto
            // 
            this.radioButtonDurationAuto.AutoSize = true;
            this.radioButtonDurationAuto.Checked = true;
            this.radioButtonDurationAuto.Location = new System.Drawing.Point(16, 19);
            this.radioButtonDurationAuto.Name = "radioButtonDurationAuto";
            this.radioButtonDurationAuto.Size = new System.Drawing.Size(48, 17);
            this.radioButtonDurationAuto.TabIndex = 0;
            this.radioButtonDurationAuto.TabStop = true;
            this.radioButtonDurationAuto.Text = "Auto";
            this.radioButtonDurationAuto.UseVisualStyleBackColor = true;
            this.radioButtonDurationAuto.CheckedChanged += new System.EventHandler(this.RadioButtonDurationAutoCheckedChanged);
            // 
            // checkBoxMergeShortLines
            // 
            this.checkBoxMergeShortLines.AutoSize = true;
            this.checkBoxMergeShortLines.Checked = true;
            this.checkBoxMergeShortLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMergeShortLines.Location = new System.Drawing.Point(19, 94);
            this.checkBoxMergeShortLines.Name = "checkBoxMergeShortLines";
            this.checkBoxMergeShortLines.Size = new System.Drawing.Size(193, 17);
            this.checkBoxMergeShortLines.TabIndex = 1;
            this.checkBoxMergeShortLines.Text = "Merge short lines with continuation";
            this.checkBoxMergeShortLines.UseVisualStyleBackColor = true;
            this.checkBoxMergeShortLines.CheckedChanged += new System.EventHandler(this.CheckBoxMergeShortLinesCheckedChanged);
            // 
            // checkBoxRemoveLinesWithoutLetters
            // 
            this.checkBoxRemoveLinesWithoutLetters.AutoSize = true;
            this.checkBoxRemoveLinesWithoutLetters.Checked = true;
            this.checkBoxRemoveLinesWithoutLetters.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveLinesWithoutLetters.Location = new System.Drawing.Point(19, 140);
            this.checkBoxRemoveLinesWithoutLetters.Name = "checkBoxRemoveLinesWithoutLetters";
            this.checkBoxRemoveLinesWithoutLetters.Size = new System.Drawing.Size(162, 17);
            this.checkBoxRemoveLinesWithoutLetters.TabIndex = 3;
            this.checkBoxRemoveLinesWithoutLetters.Text = "Remove lines without letters";
            this.checkBoxRemoveLinesWithoutLetters.UseVisualStyleBackColor = true;
            this.checkBoxRemoveLinesWithoutLetters.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveLinesWithoutLettersOrNumbersCheckedChanged);
            // 
            // groupBoxSplitting
            // 
            this.groupBoxSplitting.Controls.Add(this.comboBoxLineMode);
            this.groupBoxSplitting.Controls.Add(this.comboBoxLineBreak);
            this.groupBoxSplitting.Controls.Add(this.labelLineBreak);
            this.groupBoxSplitting.Controls.Add(this.radioButtonSplitAtBlankLines);
            this.groupBoxSplitting.Controls.Add(this.radioButtonAutoSplit);
            this.groupBoxSplitting.Controls.Add(this.radioButtonLineMode);
            this.groupBoxSplitting.Location = new System.Drawing.Point(6, 17);
            this.groupBoxSplitting.Name = "groupBoxSplitting";
            this.groupBoxSplitting.Size = new System.Drawing.Size(390, 68);
            this.groupBoxSplitting.TabIndex = 0;
            this.groupBoxSplitting.TabStop = false;
            this.groupBoxSplitting.Text = "Splitting";
            // 
            // comboBoxLineMode
            // 
            this.comboBoxLineMode.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxLineMode.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxLineMode.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxLineMode.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxLineMode.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxLineMode.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxLineMode.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxLineMode.DropDownHeight = 400;
            this.comboBoxLineMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLineMode.DropDownWidth = 144;
            this.comboBoxLineMode.FormattingEnabled = true;
            this.comboBoxLineMode.Items.AddRange(new object[] {
            "One line is a subtitle",
            "Two lines is a subtitle"});
            this.comboBoxLineMode.Location = new System.Drawing.Point(31, 41);
            this.comboBoxLineMode.MaxLength = 32767;
            this.comboBoxLineMode.Name = "comboBoxLineMode";
            this.comboBoxLineMode.SelectedIndex = -1;
            this.comboBoxLineMode.SelectedItem = null;
            this.comboBoxLineMode.SelectedText = "";
            this.comboBoxLineMode.Size = new System.Drawing.Size(144, 21);
            this.comboBoxLineMode.TabIndex = 2;
            this.comboBoxLineMode.UsePopupWindow = false;
            this.comboBoxLineMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxLineMode_SelectedIndexChanged);
            // 
            // comboBoxLineBreak
            // 
            this.comboBoxLineBreak.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxLineBreak.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxLineBreak.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxLineBreak.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxLineBreak.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxLineBreak.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxLineBreak.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxLineBreak.DropDownHeight = 400;
            this.comboBoxLineBreak.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxLineBreak.DropDownWidth = 107;
            this.comboBoxLineBreak.FormattingEnabled = true;
            this.comboBoxLineBreak.Items.AddRange(new object[] {
            "|",
            "\\N;\\n",
            "|;\\N;\\n;//;<br>;<br />;<br/>"});
            this.comboBoxLineBreak.Location = new System.Drawing.Point(246, 41);
            this.comboBoxLineBreak.MaxLength = 32767;
            this.comboBoxLineBreak.Name = "comboBoxLineBreak";
            this.comboBoxLineBreak.SelectedIndex = -1;
            this.comboBoxLineBreak.SelectedItem = null;
            this.comboBoxLineBreak.SelectedText = "";
            this.comboBoxLineBreak.Size = new System.Drawing.Size(107, 21);
            this.comboBoxLineBreak.TabIndex = 5;
            this.comboBoxLineBreak.TabStop = false;
            this.comboBoxLineBreak.UsePopupWindow = false;
            this.comboBoxLineBreak.TextChanged += new System.EventHandler(this.comboBoxLineBreak_TextChanged);
            // 
            // labelLineBreak
            // 
            this.labelLineBreak.AutoSize = true;
            this.labelLineBreak.Location = new System.Drawing.Point(184, 44);
            this.labelLineBreak.Name = "labelLineBreak";
            this.labelLineBreak.Size = new System.Drawing.Size(56, 13);
            this.labelLineBreak.TabIndex = 4;
            this.labelLineBreak.Text = "Line break";
            // 
            // radioButtonSplitAtBlankLines
            // 
            this.radioButtonSplitAtBlankLines.AutoSize = true;
            this.radioButtonSplitAtBlankLines.Location = new System.Drawing.Point(183, 19);
            this.radioButtonSplitAtBlankLines.Name = "radioButtonSplitAtBlankLines";
            this.radioButtonSplitAtBlankLines.Size = new System.Drawing.Size(110, 17);
            this.radioButtonSplitAtBlankLines.TabIndex = 3;
            this.radioButtonSplitAtBlankLines.Text = "Split at blank lines";
            this.radioButtonSplitAtBlankLines.UseVisualStyleBackColor = true;
            this.radioButtonSplitAtBlankLines.CheckedChanged += new System.EventHandler(this.radioButtonSplitAtBlankLines_CheckedChanged);
            // 
            // radioButtonAutoSplit
            // 
            this.radioButtonAutoSplit.AutoSize = true;
            this.radioButtonAutoSplit.Checked = true;
            this.radioButtonAutoSplit.Location = new System.Drawing.Point(14, 19);
            this.radioButtonAutoSplit.Name = "radioButtonAutoSplit";
            this.radioButtonAutoSplit.Size = new System.Drawing.Size(93, 17);
            this.radioButtonAutoSplit.TabIndex = 0;
            this.radioButtonAutoSplit.TabStop = true;
            this.radioButtonAutoSplit.Text = "Auto split text";
            this.radioButtonAutoSplit.UseVisualStyleBackColor = true;
            this.radioButtonAutoSplit.CheckedChanged += new System.EventHandler(this.RadioButtonAutoSplitCheckedChanged);
            // 
            // radioButtonLineMode
            // 
            this.radioButtonLineMode.AutoSize = true;
            this.radioButtonLineMode.Location = new System.Drawing.Point(14, 43);
            this.radioButtonLineMode.Name = "radioButtonLineMode";
            this.radioButtonLineMode.Size = new System.Drawing.Size(133, 17);
            this.radioButtonLineMode.TabIndex = 1;
            this.radioButtonLineMode.Text = "One line is one subtitle";
            this.radioButtonLineMode.UseVisualStyleBackColor = true;
            this.radioButtonLineMode.CheckedChanged += new System.EventHandler(this.RadioButtonLineModeCheckedChanged);
            // 
            // checkBoxRemoveEmptyLines
            // 
            this.checkBoxRemoveEmptyLines.AutoSize = true;
            this.checkBoxRemoveEmptyLines.Checked = true;
            this.checkBoxRemoveEmptyLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveEmptyLines.Location = new System.Drawing.Point(19, 117);
            this.checkBoxRemoveEmptyLines.Name = "checkBoxRemoveEmptyLines";
            this.checkBoxRemoveEmptyLines.Size = new System.Drawing.Size(122, 17);
            this.checkBoxRemoveEmptyLines.TabIndex = 2;
            this.checkBoxRemoveEmptyLines.Text = "Remove empty lines";
            this.checkBoxRemoveEmptyLines.UseVisualStyleBackColor = true;
            this.checkBoxRemoveEmptyLines.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveEmptyLinesCheckedChanged);
            // 
            // checkBoxUseTimeCodeFromCurrentFile
            // 
            this.checkBoxUseTimeCodeFromCurrentFile.AutoSize = true;
            this.checkBoxUseTimeCodeFromCurrentFile.Enabled = false;
            this.checkBoxUseTimeCodeFromCurrentFile.Location = new System.Drawing.Point(157, 250);
            this.checkBoxUseTimeCodeFromCurrentFile.Name = "checkBoxUseTimeCodeFromCurrentFile";
            this.checkBoxUseTimeCodeFromCurrentFile.Size = new System.Drawing.Size(152, 17);
            this.checkBoxUseTimeCodeFromCurrentFile.TabIndex = 8;
            this.checkBoxUseTimeCodeFromCurrentFile.Text = "Take time from current file";
            this.checkBoxUseTimeCodeFromCurrentFile.UseVisualStyleBackColor = true;
            this.checkBoxUseTimeCodeFromCurrentFile.CheckedChanged += new System.EventHandler(this.checkBoxUseTimeCodeFromCurrentFile_CheckedChanged);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefresh.Location = new System.Drawing.Point(774, 735);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(102, 23);
            this.buttonRefresh.TabIndex = 103;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.ButtonRefreshClick);
            // 
            // groupBoxImportResult
            // 
            this.groupBoxImportResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportResult.Controls.Add(this.SubtitleListview1);
            this.groupBoxImportResult.Location = new System.Drawing.Point(12, 428);
            this.groupBoxImportResult.Name = "groupBoxImportResult";
            this.groupBoxImportResult.Size = new System.Drawing.Size(1032, 299);
            this.groupBoxImportResult.TabIndex = 2;
            this.groupBoxImportResult.TabStop = false;
            this.groupBoxImportResult.Text = "Preview";
            // 
            // SubtitleListview1
            // 
            this.SubtitleListview1.AllowColumnReorder = true;
            this.SubtitleListview1.AllowDrop = true;
            this.SubtitleListview1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SubtitleListview1.ContextMenuStrip = this.contextMenuStripPreview;
            this.SubtitleListview1.FirstVisibleIndex = -1;
            this.SubtitleListview1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubtitleListview1.FullRowSelect = true;
            this.SubtitleListview1.GridLines = true;
            this.SubtitleListview1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SubtitleListview1.HideSelection = false;
            this.SubtitleListview1.Location = new System.Drawing.Point(6, 20);
            this.SubtitleListview1.MultiSelect = false;
            this.SubtitleListview1.Name = "SubtitleListview1";
            this.SubtitleListview1.OwnerDraw = true;
            this.SubtitleListview1.Size = new System.Drawing.Size(1020, 257);
            this.SubtitleListview1.SubtitleFontBold = false;
            this.SubtitleListview1.SubtitleFontName = "Tahoma";
            this.SubtitleListview1.SubtitleFontSize = 8;
            this.SubtitleListview1.TabIndex = 0;
            this.SubtitleListview1.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview1.UseSyntaxColoring = true;
            this.SubtitleListview1.View = System.Windows.Forms.View.Details;
            // 
            // contextMenuStripPreview
            // 
            this.contextMenuStripPreview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startNumberingFromToolStripMenuItem});
            this.contextMenuStripPreview.Name = "contextMenuStripPreview";
            this.contextMenuStripPreview.Size = new System.Drawing.Size(199, 26);
            // 
            // startNumberingFromToolStripMenuItem
            // 
            this.startNumberingFromToolStripMenuItem.Name = "startNumberingFromToolStripMenuItem";
            this.startNumberingFromToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.startNumberingFromToolStripMenuItem.Text = "Start numbering from...";
            this.startNumberingFromToolStripMenuItem.Click += new System.EventHandler(this.startNumberingFromToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(963, 735);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 105;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(882, 735);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 104;
            this.buttonOK.Text = "&Next >";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 734);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(60, 13);
            this.labelStatus.TabIndex = 106;
            this.labelStatus.Text = "labelStatus";
            // 
            // ImportText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1056, 772);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxImportResult);
            this.Controls.Add(this.groupBoxImportOptions);
            this.Controls.Add(this.groupBoxImportText);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(810, 648);
            this.Name = "ImportText";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import text";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportText_FormClosing);
            this.Shown += new System.EventHandler(this.ImportText_Shown);
            this.ResizeEnd += new System.EventHandler(this.ImportText_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImportTextKeyDown);
            this.groupBoxImportText.ResumeLayout(false);
            this.groupBoxImportText.PerformLayout();
            this.contextMenuStripListView.ResumeLayout(false);
            this.groupBoxImportOptions.ResumeLayout(false);
            this.groupBoxImportOptions.PerformLayout();
            this.groupBoxAutoSplitSettings.ResumeLayout(false);
            this.groupBoxAutoSplitSettings.PerformLayout();
            this.groupBoxTimeCodes.ResumeLayout(false);
            this.groupBoxTimeCodes.PerformLayout();
            this.groupBoxDuration.ResumeLayout(false);
            this.groupBoxDuration.PerformLayout();
            this.groupBoxSplitting.ResumeLayout(false);
            this.groupBoxSplitting.PerformLayout();
            this.groupBoxImportResult.ResumeLayout(false);
            this.contextMenuStripPreview.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOpenText;
        private Controls.SubtitleListView SubtitleListview1;
        private System.Windows.Forms.GroupBox groupBoxImportText;
        private Controls.NikseTextBox textBoxText;
        private System.Windows.Forms.GroupBox groupBoxImportOptions;
        private System.Windows.Forms.GroupBox groupBoxImportResult;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxSplitting;
        private System.Windows.Forms.RadioButton radioButtonAutoSplit;
        private System.Windows.Forms.RadioButton radioButtonLineMode;
        private System.Windows.Forms.CheckBox checkBoxRemoveEmptyLines;
        private System.Windows.Forms.CheckBox checkBoxRemoveLinesWithoutLetters;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownGapBetweenLines;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelGapBetweenSubtitles;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.GroupBox groupBoxDuration;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownDurationFixed;
        private System.Windows.Forms.RadioButton radioButtonDurationFixed;
        private System.Windows.Forms.RadioButton radioButtonDurationAuto;
        private System.Windows.Forms.CheckBox checkBoxMergeShortLines;
        private System.Windows.Forms.RadioButton radioButtonSplitAtBlankLines;
        private System.Windows.Forms.CheckBox checkBoxGenerateTimeCodes;
        private System.Windows.Forms.GroupBox groupBoxTimeCodes;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelLineBreak;
        private System.Windows.Forms.CheckBox checkBoxMultipleFiles;
        private System.Windows.Forms.ListView listViewInputFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFName;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.CheckBox checkBoxAutoBreak;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLineBreak;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListView;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPreview;
        private System.Windows.Forms.ToolStripMenuItem startNumberingFromToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxAutoSplitSettings;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxAsEnd;
        private System.Windows.Forms.CheckBox checkBoxAutoSplitRemoveLinesNoLetters;
        private System.Windows.Forms.CheckBox checkBoxAutoSplitAtBlankLines;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownAutoSplitMaxLines;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelAutoSplitNumberOfLines;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelSubMaxLen;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownSubtitleLineMaximumLength;
        private System.Windows.Forms.CheckBox checkBoxAutoSplitAtEnd;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLineMode;
        private System.Windows.Forms.CheckBox checkBoxTakeTimeFromFileNames;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelStatus;
        private System.Windows.Forms.CheckBox checkBoxUseTimeCodeFromCurrentFile;
        private System.Windows.Forms.Button buttonBrowseEncoding;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelEncoding;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxEncoding;
    }
}