﻿using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    sealed partial class VobSubOcr
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;       

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStripListview = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.normalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.italicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.importTextWithMatchingTimeCodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importNewTimeCodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveImageAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAllImagesWithHtmlIndexViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExport = new System.Windows.Forms.ToolStripMenuItem();
            this.bDNXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bluraySupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vobSubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dOSTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorImageCompare = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemInspectNOcrMatches = new System.Windows.Forms.ToolStripMenuItem();
            this.inspectImageCompareMatchesForCurrentImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EditLastAdditionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nOcrTrainingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemSetUnItalicFactor = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelSubtitleText = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxOcrMethod = new System.Windows.Forms.GroupBox();
            this.comboBoxOcrMethod = new System.Windows.Forms.ComboBox();
            this.groupBoxImageCompareMethod = new System.Windows.Forms.GroupBox();
            this.labelMaxErrorPercent = new System.Windows.Forms.Label();
            this.numericUpDownMaxErrorPct = new System.Windows.Forms.NumericUpDown();
            this.checkBoxRightToLeft = new System.Windows.Forms.CheckBox();
            this.numericUpDownPixelsIsSpace = new System.Windows.Forms.NumericUpDown();
            this.buttonEditCharacterDatabase = new System.Windows.Forms.Button();
            this.labelNoOfPixelsIsSpace = new System.Windows.Forms.Label();
            this.comboBoxCharacterDatabase = new System.Windows.Forms.ComboBox();
            this.labelImageDatabase = new System.Windows.Forms.Label();
            this.buttonNewCharacterDatabase = new System.Windows.Forms.Button();
            this.GroupBoxTesseractMethod = new System.Windows.Forms.GroupBox();
            this.buttonGetTesseractDictionaries = new System.Windows.Forms.Button();
            this.checkBoxTesseractMusicOn = new System.Windows.Forms.CheckBox();
            this.checkBoxTesseractItalicsOn = new System.Windows.Forms.CheckBox();
            this.checkBoxUseModiInTesseractForUnknownWords = new System.Windows.Forms.CheckBox();
            this.labelTesseractLanguage = new System.Windows.Forms.Label();
            this.comboBoxTesseractLanguages = new System.Windows.Forms.ComboBox();
            this.groupBoxModiMethod = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxModiLanguage = new System.Windows.Forms.ComboBox();
            this.groupBoxNOCR = new System.Windows.Forms.GroupBox();
            this.buttonLineOcrEditLanguage = new System.Windows.Forms.Button();
            this.buttonLineOcrNewLanguage = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxNOcrLanguage = new System.Windows.Forms.ComboBox();
            this.checkBoxNOcrItalic = new System.Windows.Forms.CheckBox();
            this.checkBoxNOcrCorrect = new System.Windows.Forms.CheckBox();
            this.checkBoxRightToLeftNOCR = new System.Windows.Forms.CheckBox();
            this.numericUpDownNumberOfPixelsIsSpaceNOCR = new System.Windows.Forms.NumericUpDown();
            this.labelNumberOfPixelsIsSpaceNOCR = new System.Windows.Forms.Label();
            this.groupBoxOCRControls = new System.Windows.Forms.GroupBox();
            this.labelStartFrom = new System.Windows.Forms.Label();
            this.numericUpDownStartNumber = new System.Windows.Forms.NumericUpDown();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonStartOcr = new System.Windows.Forms.Button();
            this.groupBoxOcrAutoFix = new System.Windows.Forms.GroupBox();
            this.buttonSpellCheckDownload = new System.Windows.Forms.Button();
            this.labelFixesMade = new System.Windows.Forms.Label();
            this.comboBoxDictionaries = new System.Windows.Forms.ComboBox();
            this.checkBoxGuessUnknownWords = new System.Windows.Forms.CheckBox();
            this.tabControlLogs = new System.Windows.Forms.TabControl();
            this.contextMenuStripAllFixes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemClearFixes = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPageUnknownWords = new System.Windows.Forms.TabPage();
            this.buttonGoogleIt = new System.Windows.Forms.Button();
            this.buttonAddToOcrReplaceList = new System.Windows.Forms.Button();
            this.buttonUknownToUserDic = new System.Windows.Forms.Button();
            this.buttonUknownToNames = new System.Windows.Forms.Button();
            this.listBoxUnknownWords = new System.Windows.Forms.ListBox();
            this.contextMenuStripUnknownWords = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPageAllFixes = new System.Windows.Forms.TabPage();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.tabPageSuggestions = new System.Windows.Forms.TabPage();
            this.listBoxLogSuggestions = new System.Windows.Forms.ListBox();
            this.contextMenuStripGuessesUsed = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemClearGuesses = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxPromptForUnknownWords = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoBreakLines = new System.Windows.Forms.CheckBox();
            this.labelDictionaryLoaded = new System.Windows.Forms.Label();
            this.checkBoxAutoFixCommonErrors = new System.Windows.Forms.CheckBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBoxImagePalette = new System.Windows.Forms.GroupBox();
            this.checkBoxBackgroundTransparent = new System.Windows.Forms.CheckBox();
            this.pictureBoxBackground = new System.Windows.Forms.PictureBox();
            this.checkBoxEmphasis2Transparent = new System.Windows.Forms.CheckBox();
            this.checkBoxEmphasis1Transparent = new System.Windows.Forms.CheckBox();
            this.checkBoxPatternTransparent = new System.Windows.Forms.CheckBox();
            this.pictureBoxEmphasis2 = new System.Windows.Forms.PictureBox();
            this.pictureBoxEmphasis1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxPattern = new System.Windows.Forms.PictureBox();
            this.checkBoxCustomFourColors = new System.Windows.Forms.CheckBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.groupBoxSubtitleImage = new System.Windows.Forms.GroupBox();
            this.labelMinAlpha = new System.Windows.Forms.Label();
            this.numericUpDownAutoTransparentAlphaMax = new System.Windows.Forms.NumericUpDown();
            this.groupBoxTransportStream = new System.Windows.Forms.GroupBox();
            this.checkBoxTransportStreamGetColorAndSplit = new System.Windows.Forms.CheckBox();
            this.checkBoxTransportStreamGrayscale = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoTransparentBackground = new System.Windows.Forms.CheckBox();
            this.pictureBoxSubtitleImage = new System.Windows.Forms.PictureBox();
            this.contextMenuStripImage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemImageSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxShowOnlyForced = new System.Windows.Forms.CheckBox();
            this.checkBoxUseTimeCodesFromIdx = new System.Windows.Forms.CheckBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.splitContainerBottom = new System.Windows.Forms.SplitContainer();
            this.textBoxCurrentText = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.subtitleListView1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.timerHideStatus = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStripListview.SuspendLayout();
            this.groupBoxOcrMethod.SuspendLayout();
            this.groupBoxImageCompareMethod.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxErrorPct)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPixelsIsSpace)).BeginInit();
            this.GroupBoxTesseractMethod.SuspendLayout();
            this.groupBoxModiMethod.SuspendLayout();
            this.groupBoxNOCR.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumberOfPixelsIsSpaceNOCR)).BeginInit();
            this.groupBoxOCRControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStartNumber)).BeginInit();
            this.groupBoxOcrAutoFix.SuspendLayout();
            this.tabControlLogs.SuspendLayout();
            this.contextMenuStripAllFixes.SuspendLayout();
            this.tabPageUnknownWords.SuspendLayout();
            this.contextMenuStripUnknownWords.SuspendLayout();
            this.tabPageAllFixes.SuspendLayout();
            this.tabPageSuggestions.SuspendLayout();
            this.contextMenuStripGuessesUsed.SuspendLayout();
            this.groupBoxImagePalette.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBackground)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEmphasis2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEmphasis1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPattern)).BeginInit();
            this.groupBoxSubtitleImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAutoTransparentAlphaMax)).BeginInit();
            this.groupBoxTransportStream.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).BeginInit();
            this.contextMenuStripImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBottom)).BeginInit();
            this.splitContainerBottom.Panel1.SuspendLayout();
            this.splitContainerBottom.Panel2.SuspendLayout();
            this.splitContainerBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStripListview
            // 
            this.contextMenuStripListview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.normalToolStripMenuItem,
            this.italicToolStripMenuItem,
            this.toolStripSeparator1,
            this.importTextWithMatchingTimeCodesToolStripMenuItem,
            this.importNewTimeCodesToolStripMenuItem,
            this.toolStripSeparator2,
            this.saveImageAsToolStripMenuItem,
            this.saveAllImagesWithHtmlIndexViewToolStripMenuItem,
            this.toolStripMenuItemExport,
            this.toolStripSeparatorImageCompare,
            this.toolStripMenuItemInspectNOcrMatches,
            this.inspectImageCompareMatchesForCurrentImageToolStripMenuItem,
            this.EditLastAdditionsToolStripMenuItem,
            this.nOcrTrainingToolStripMenuItem,
            this.toolStripSeparator4,
            this.toolStripMenuItemSetUnItalicFactor,
            this.toolStripSeparator3,
            this.deleteToolStripMenuItem});
            this.contextMenuStripListview.Name = "contextMenuStripListview";
            this.contextMenuStripListview.Size = new System.Drawing.Size(306, 320);
            this.contextMenuStripListview.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripListviewOpening);
            // 
            // normalToolStripMenuItem
            // 
            this.normalToolStripMenuItem.Name = "normalToolStripMenuItem";
            this.normalToolStripMenuItem.Size = new System.Drawing.Size(305, 22);
            this.normalToolStripMenuItem.Text = "Normal";
            this.normalToolStripMenuItem.Click += new System.EventHandler(this.NormalToolStripMenuItemClick);
            // 
            // italicToolStripMenuItem
            // 
            this.italicToolStripMenuItem.Name = "italicToolStripMenuItem";
            this.italicToolStripMenuItem.Size = new System.Drawing.Size(305, 22);
            this.italicToolStripMenuItem.Text = "Italic";
            this.italicToolStripMenuItem.Click += new System.EventHandler(this.ItalicToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(302, 6);
            // 
            // importTextWithMatchingTimeCodesToolStripMenuItem
            // 
            this.importTextWithMatchingTimeCodesToolStripMenuItem.Name = "importTextWithMatchingTimeCodesToolStripMenuItem";
            this.importTextWithMatchingTimeCodesToolStripMenuItem.Size = new System.Drawing.Size(305, 22);
            this.importTextWithMatchingTimeCodesToolStripMenuItem.Text = "Import text with matching time codes...";
            this.importTextWithMatchingTimeCodesToolStripMenuItem.Click += new System.EventHandler(this.importTextWithMatchingTimeCodesToolStripMenuItem_Click);
            // 
            // importNewTimeCodesToolStripMenuItem
            // 
            this.importNewTimeCodesToolStripMenuItem.Name = "importNewTimeCodesToolStripMenuItem";
            this.importNewTimeCodesToolStripMenuItem.Size = new System.Drawing.Size(305, 22);
            this.importNewTimeCodesToolStripMenuItem.Text = "Import new time codes...";
            this.importNewTimeCodesToolStripMenuItem.Click += new System.EventHandler(this.importNewTimeCodesToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(302, 6);
            // 
            // saveImageAsToolStripMenuItem
            // 
            this.saveImageAsToolStripMenuItem.Name = "saveImageAsToolStripMenuItem";
            this.saveImageAsToolStripMenuItem.Size = new System.Drawing.Size(305, 22);
            this.saveImageAsToolStripMenuItem.Text = "Save image as...";
            this.saveImageAsToolStripMenuItem.Click += new System.EventHandler(this.SaveImageAsToolStripMenuItemClick);
            // 
            // saveAllImagesWithHtmlIndexViewToolStripMenuItem
            // 
            this.saveAllImagesWithHtmlIndexViewToolStripMenuItem.Name = "saveAllImagesWithHtmlIndexViewToolStripMenuItem";
            this.saveAllImagesWithHtmlIndexViewToolStripMenuItem.Size = new System.Drawing.Size(305, 22);
            this.saveAllImagesWithHtmlIndexViewToolStripMenuItem.Text = "Save all images with HTML index view...";
            this.saveAllImagesWithHtmlIndexViewToolStripMenuItem.Click += new System.EventHandler(this.saveAllImagesWithHtmlIndexViewToolStripMenuItem_Click);
            // 
            // toolStripMenuItemExport
            // 
            this.toolStripMenuItemExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bDNXMLToolStripMenuItem,
            this.bluraySupToolStripMenuItem,
            this.vobSubToolStripMenuItem,
            this.dOSTToolStripMenuItem});
            this.toolStripMenuItemExport.Name = "toolStripMenuItemExport";
            this.toolStripMenuItemExport.Size = new System.Drawing.Size(305, 22);
            this.toolStripMenuItemExport.Text = "Export all images as...";
            // 
            // bDNXMLToolStripMenuItem
            // 
            this.bDNXMLToolStripMenuItem.Name = "bDNXMLToolStripMenuItem";
            this.bDNXMLToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.bDNXMLToolStripMenuItem.Text = "BDN XML...";
            this.bDNXMLToolStripMenuItem.Click += new System.EventHandler(this.bDNXMLToolStripMenuItem_Click);
            // 
            // bluraySupToolStripMenuItem
            // 
            this.bluraySupToolStripMenuItem.Name = "bluraySupToolStripMenuItem";
            this.bluraySupToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.bluraySupToolStripMenuItem.Text = "Blu-ray sup...";
            this.bluraySupToolStripMenuItem.Click += new System.EventHandler(this.bluraySupToolStripMenuItem_Click);
            // 
            // vobSubToolStripMenuItem
            // 
            this.vobSubToolStripMenuItem.Name = "vobSubToolStripMenuItem";
            this.vobSubToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.vobSubToolStripMenuItem.Text = "VobSub...";
            this.vobSubToolStripMenuItem.Click += new System.EventHandler(this.vobSubToolStripMenuItem_Click);
            // 
            // dOSTToolStripMenuItem
            // 
            this.dOSTToolStripMenuItem.Name = "dOSTToolStripMenuItem";
            this.dOSTToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.dOSTToolStripMenuItem.Text = "DOST...";
            this.dOSTToolStripMenuItem.Click += new System.EventHandler(this.dOSTToolStripMenuItem_Click);
            // 
            // toolStripSeparatorImageCompare
            // 
            this.toolStripSeparatorImageCompare.Name = "toolStripSeparatorImageCompare";
            this.toolStripSeparatorImageCompare.Size = new System.Drawing.Size(302, 6);
            // 
            // toolStripMenuItemInspectNOcrMatches
            // 
            this.toolStripMenuItemInspectNOcrMatches.Name = "toolStripMenuItemInspectNOcrMatches";
            this.toolStripMenuItemInspectNOcrMatches.Size = new System.Drawing.Size(305, 22);
            this.toolStripMenuItemInspectNOcrMatches.Text = "Inspect nocr matches for current image...";
            this.toolStripMenuItemInspectNOcrMatches.Click += new System.EventHandler(this.toolStripMenuItemInspectNOcrMatches_Click);
            // 
            // inspectImageCompareMatchesForCurrentImageToolStripMenuItem
            // 
            this.inspectImageCompareMatchesForCurrentImageToolStripMenuItem.Name = "inspectImageCompareMatchesForCurrentImageToolStripMenuItem";
            this.inspectImageCompareMatchesForCurrentImageToolStripMenuItem.Size = new System.Drawing.Size(305, 22);
            this.inspectImageCompareMatchesForCurrentImageToolStripMenuItem.Text = "Inspect compare matches for current image";
            this.inspectImageCompareMatchesForCurrentImageToolStripMenuItem.Click += new System.EventHandler(this.InspectImageCompareMatchesForCurrentImageToolStripMenuItem_Click);
            // 
            // EditLastAdditionsToolStripMenuItem
            // 
            this.EditLastAdditionsToolStripMenuItem.Name = "EditLastAdditionsToolStripMenuItem";
            this.EditLastAdditionsToolStripMenuItem.Size = new System.Drawing.Size(305, 22);
            this.EditLastAdditionsToolStripMenuItem.Text = "Edit last OCR image additions...";
            this.EditLastAdditionsToolStripMenuItem.Click += new System.EventHandler(this.inspectLastAdditionsToolStripMenuItem_Click);
            // 
            // nOcrTrainingToolStripMenuItem
            // 
            this.nOcrTrainingToolStripMenuItem.Name = "nOcrTrainingToolStripMenuItem";
            this.nOcrTrainingToolStripMenuItem.Size = new System.Drawing.Size(305, 22);
            this.nOcrTrainingToolStripMenuItem.Text = "N-Ocr training..";
            this.nOcrTrainingToolStripMenuItem.Click += new System.EventHandler(this.nOcrTrainingToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(302, 6);
            // 
            // toolStripMenuItemSetUnItalicFactor
            // 
            this.toolStripMenuItemSetUnItalicFactor.Name = "toolStripMenuItemSetUnItalicFactor";
            this.toolStripMenuItemSetUnItalicFactor.Size = new System.Drawing.Size(305, 22);
            this.toolStripMenuItemSetUnItalicFactor.Text = "Set un-italic factor...";
            this.toolStripMenuItemSetUnItalicFactor.Click += new System.EventHandler(this.toolStripMenuItemSetUnItalicFactor_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(302, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(305, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItemClick);
            // 
            // labelSubtitleText
            // 
            this.labelSubtitleText.AutoSize = true;
            this.labelSubtitleText.Location = new System.Drawing.Point(7, 5);
            this.labelSubtitleText.Name = "labelSubtitleText";
            this.labelSubtitleText.Size = new System.Drawing.Size(66, 13);
            this.labelSubtitleText.TabIndex = 6;
            this.labelSubtitleText.Text = "Subtitle text";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 564);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(893, 10);
            this.progressBar1.TabIndex = 7;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 543);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(131, 13);
            this.labelStatus.TabIndex = 8;
            this.labelStatus.Text = "Loading VobSub images...";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(911, 548);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(992, 548);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // groupBoxOcrMethod
            // 
            this.groupBoxOcrMethod.Controls.Add(this.comboBoxOcrMethod);
            this.groupBoxOcrMethod.Controls.Add(this.groupBoxImageCompareMethod);
            this.groupBoxOcrMethod.Controls.Add(this.GroupBoxTesseractMethod);
            this.groupBoxOcrMethod.Controls.Add(this.groupBoxModiMethod);
            this.groupBoxOcrMethod.Controls.Add(this.groupBoxNOCR);
            this.groupBoxOcrMethod.Location = new System.Drawing.Point(13, 5);
            this.groupBoxOcrMethod.Name = "groupBoxOcrMethod";
            this.groupBoxOcrMethod.Size = new System.Drawing.Size(392, 192);
            this.groupBoxOcrMethod.TabIndex = 0;
            this.groupBoxOcrMethod.TabStop = false;
            this.groupBoxOcrMethod.Text = "OCR method";
            // 
            // comboBoxOcrMethod
            // 
            this.comboBoxOcrMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOcrMethod.FormattingEnabled = true;
            this.comboBoxOcrMethod.Items.AddRange(new object[] {
            "OCR via Tesseract",
            "OCR via image compare",
            "OCR via Microsoft MODI",
            "OCR via nOCR"});
            this.comboBoxOcrMethod.Location = new System.Drawing.Point(13, 20);
            this.comboBoxOcrMethod.Name = "comboBoxOcrMethod";
            this.comboBoxOcrMethod.Size = new System.Drawing.Size(366, 21);
            this.comboBoxOcrMethod.TabIndex = 0;
            this.comboBoxOcrMethod.SelectedIndexChanged += new System.EventHandler(this.ComboBoxOcrMethodSelectedIndexChanged);
            // 
            // groupBoxImageCompareMethod
            // 
            this.groupBoxImageCompareMethod.Controls.Add(this.labelMaxErrorPercent);
            this.groupBoxImageCompareMethod.Controls.Add(this.numericUpDownMaxErrorPct);
            this.groupBoxImageCompareMethod.Controls.Add(this.checkBoxRightToLeft);
            this.groupBoxImageCompareMethod.Controls.Add(this.numericUpDownPixelsIsSpace);
            this.groupBoxImageCompareMethod.Controls.Add(this.buttonEditCharacterDatabase);
            this.groupBoxImageCompareMethod.Controls.Add(this.labelNoOfPixelsIsSpace);
            this.groupBoxImageCompareMethod.Controls.Add(this.comboBoxCharacterDatabase);
            this.groupBoxImageCompareMethod.Controls.Add(this.labelImageDatabase);
            this.groupBoxImageCompareMethod.Controls.Add(this.buttonNewCharacterDatabase);
            this.groupBoxImageCompareMethod.Location = new System.Drawing.Point(13, 38);
            this.groupBoxImageCompareMethod.Name = "groupBoxImageCompareMethod";
            this.groupBoxImageCompareMethod.Size = new System.Drawing.Size(366, 131);
            this.groupBoxImageCompareMethod.TabIndex = 2;
            this.groupBoxImageCompareMethod.TabStop = false;
            this.groupBoxImageCompareMethod.Text = "Image compare";
            // 
            // labelMaxErrorPercent
            // 
            this.labelMaxErrorPercent.AutoSize = true;
            this.labelMaxErrorPercent.Location = new System.Drawing.Point(7, 83);
            this.labelMaxErrorPercent.Name = "labelMaxErrorPercent";
            this.labelMaxErrorPercent.Size = new System.Drawing.Size(55, 13);
            this.labelMaxErrorPercent.TabIndex = 45;
            this.labelMaxErrorPercent.Text = "Max Err%";
            // 
            // numericUpDownMaxErrorPct
            // 
            this.numericUpDownMaxErrorPct.DecimalPlaces = 1;
            this.numericUpDownMaxErrorPct.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownMaxErrorPct.Location = new System.Drawing.Point(173, 81);
            this.numericUpDownMaxErrorPct.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownMaxErrorPct.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numericUpDownMaxErrorPct.Name = "numericUpDownMaxErrorPct";
            this.numericUpDownMaxErrorPct.Size = new System.Drawing.Size(50, 21);
            this.numericUpDownMaxErrorPct.TabIndex = 44;
            this.numericUpDownMaxErrorPct.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numericUpDownMaxErrorPct.ValueChanged += new System.EventHandler(this.numericUpDownMaxErrorPct_ValueChanged);
            // 
            // checkBoxRightToLeft
            // 
            this.checkBoxRightToLeft.AutoSize = true;
            this.checkBoxRightToLeft.Location = new System.Drawing.Point(9, 107);
            this.checkBoxRightToLeft.Name = "checkBoxRightToLeft";
            this.checkBoxRightToLeft.Size = new System.Drawing.Size(83, 17);
            this.checkBoxRightToLeft.TabIndex = 6;
            this.checkBoxRightToLeft.Text = "Right to left";
            this.checkBoxRightToLeft.UseVisualStyleBackColor = true;
            // 
            // numericUpDownPixelsIsSpace
            // 
            this.numericUpDownPixelsIsSpace.Location = new System.Drawing.Point(173, 54);
            this.numericUpDownPixelsIsSpace.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownPixelsIsSpace.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownPixelsIsSpace.Name = "numericUpDownPixelsIsSpace";
            this.numericUpDownPixelsIsSpace.Size = new System.Drawing.Size(50, 21);
            this.numericUpDownPixelsIsSpace.TabIndex = 5;
            this.numericUpDownPixelsIsSpace.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownPixelsIsSpace.ValueChanged += new System.EventHandler(this.numericUpDownPixelsIsSpace_ValueChanged);
            // 
            // buttonEditCharacterDatabase
            // 
            this.buttonEditCharacterDatabase.Location = new System.Drawing.Point(278, 46);
            this.buttonEditCharacterDatabase.Name = "buttonEditCharacterDatabase";
            this.buttonEditCharacterDatabase.Size = new System.Drawing.Size(68, 21);
            this.buttonEditCharacterDatabase.TabIndex = 3;
            this.buttonEditCharacterDatabase.Text = "Edit";
            this.buttonEditCharacterDatabase.UseVisualStyleBackColor = true;
            this.buttonEditCharacterDatabase.Click += new System.EventHandler(this.ButtonEditCharacterDatabaseClick);
            // 
            // labelNoOfPixelsIsSpace
            // 
            this.labelNoOfPixelsIsSpace.AutoSize = true;
            this.labelNoOfPixelsIsSpace.Location = new System.Drawing.Point(6, 56);
            this.labelNoOfPixelsIsSpace.Name = "labelNoOfPixelsIsSpace";
            this.labelNoOfPixelsIsSpace.Size = new System.Drawing.Size(104, 13);
            this.labelNoOfPixelsIsSpace.TabIndex = 4;
            this.labelNoOfPixelsIsSpace.Text = "No of pixels is space";
            // 
            // comboBoxCharacterDatabase
            // 
            this.comboBoxCharacterDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCharacterDatabase.FormattingEnabled = true;
            this.comboBoxCharacterDatabase.Location = new System.Drawing.Point(130, 19);
            this.comboBoxCharacterDatabase.Name = "comboBoxCharacterDatabase";
            this.comboBoxCharacterDatabase.Size = new System.Drawing.Size(142, 21);
            this.comboBoxCharacterDatabase.TabIndex = 1;
            this.comboBoxCharacterDatabase.SelectedIndexChanged += new System.EventHandler(this.ComboBoxCharacterDatabaseSelectedIndexChanged);
            // 
            // labelImageDatabase
            // 
            this.labelImageDatabase.AutoSize = true;
            this.labelImageDatabase.Location = new System.Drawing.Point(7, 22);
            this.labelImageDatabase.Name = "labelImageDatabase";
            this.labelImageDatabase.Size = new System.Drawing.Size(85, 13);
            this.labelImageDatabase.TabIndex = 0;
            this.labelImageDatabase.Text = "Image database";
            // 
            // buttonNewCharacterDatabase
            // 
            this.buttonNewCharacterDatabase.Location = new System.Drawing.Point(278, 19);
            this.buttonNewCharacterDatabase.Name = "buttonNewCharacterDatabase";
            this.buttonNewCharacterDatabase.Size = new System.Drawing.Size(68, 21);
            this.buttonNewCharacterDatabase.TabIndex = 2;
            this.buttonNewCharacterDatabase.Text = "New";
            this.buttonNewCharacterDatabase.UseVisualStyleBackColor = true;
            this.buttonNewCharacterDatabase.Click += new System.EventHandler(this.ButtonNewCharacterDatabaseClick);
            // 
            // GroupBoxTesseractMethod
            // 
            this.GroupBoxTesseractMethod.Controls.Add(this.buttonGetTesseractDictionaries);
            this.GroupBoxTesseractMethod.Controls.Add(this.checkBoxTesseractMusicOn);
            this.GroupBoxTesseractMethod.Controls.Add(this.checkBoxTesseractItalicsOn);
            this.GroupBoxTesseractMethod.Controls.Add(this.checkBoxUseModiInTesseractForUnknownWords);
            this.GroupBoxTesseractMethod.Controls.Add(this.labelTesseractLanguage);
            this.GroupBoxTesseractMethod.Controls.Add(this.comboBoxTesseractLanguages);
            this.GroupBoxTesseractMethod.Location = new System.Drawing.Point(13, 31);
            this.GroupBoxTesseractMethod.Name = "GroupBoxTesseractMethod";
            this.GroupBoxTesseractMethod.Size = new System.Drawing.Size(366, 131);
            this.GroupBoxTesseractMethod.TabIndex = 1;
            this.GroupBoxTesseractMethod.TabStop = false;
            this.GroupBoxTesseractMethod.Text = "Tesseract";
            // 
            // buttonGetTesseractDictionaries
            // 
            this.buttonGetTesseractDictionaries.Location = new System.Drawing.Point(300, 30);
            this.buttonGetTesseractDictionaries.Name = "buttonGetTesseractDictionaries";
            this.buttonGetTesseractDictionaries.Size = new System.Drawing.Size(29, 23);
            this.buttonGetTesseractDictionaries.TabIndex = 2;
            this.buttonGetTesseractDictionaries.Text = "...";
            this.buttonGetTesseractDictionaries.UseVisualStyleBackColor = true;
            this.buttonGetTesseractDictionaries.Click += new System.EventHandler(this.buttonGetTesseractDictionaries_Click);
            // 
            // checkBoxTesseractMusicOn
            // 
            this.checkBoxTesseractMusicOn.AutoSize = true;
            this.checkBoxTesseractMusicOn.Checked = true;
            this.checkBoxTesseractMusicOn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTesseractMusicOn.Location = new System.Drawing.Point(99, 101);
            this.checkBoxTesseractMusicOn.Name = "checkBoxTesseractMusicOn";
            this.checkBoxTesseractMusicOn.Size = new System.Drawing.Size(93, 17);
            this.checkBoxTesseractMusicOn.TabIndex = 4;
            this.checkBoxTesseractMusicOn.Text = "Music symbols";
            this.checkBoxTesseractMusicOn.UseVisualStyleBackColor = true;
            // 
            // checkBoxTesseractItalicsOn
            // 
            this.checkBoxTesseractItalicsOn.AutoSize = true;
            this.checkBoxTesseractItalicsOn.Checked = true;
            this.checkBoxTesseractItalicsOn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTesseractItalicsOn.Location = new System.Drawing.Point(22, 101);
            this.checkBoxTesseractItalicsOn.Name = "checkBoxTesseractItalicsOn";
            this.checkBoxTesseractItalicsOn.Size = new System.Drawing.Size(54, 17);
            this.checkBoxTesseractItalicsOn.TabIndex = 3;
            this.checkBoxTesseractItalicsOn.Text = "Italics";
            this.checkBoxTesseractItalicsOn.UseVisualStyleBackColor = true;
            // 
            // checkBoxUseModiInTesseractForUnknownWords
            // 
            this.checkBoxUseModiInTesseractForUnknownWords.AutoSize = true;
            this.checkBoxUseModiInTesseractForUnknownWords.Checked = true;
            this.checkBoxUseModiInTesseractForUnknownWords.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxUseModiInTesseractForUnknownWords.Enabled = false;
            this.checkBoxUseModiInTesseractForUnknownWords.Location = new System.Drawing.Point(22, 74);
            this.checkBoxUseModiInTesseractForUnknownWords.Name = "checkBoxUseModiInTesseractForUnknownWords";
            this.checkBoxUseModiInTesseractForUnknownWords.Size = new System.Drawing.Size(167, 17);
            this.checkBoxUseModiInTesseractForUnknownWords.TabIndex = 2;
            this.checkBoxUseModiInTesseractForUnknownWords.Text = "Try MODI for unknown words";
            this.checkBoxUseModiInTesseractForUnknownWords.UseVisualStyleBackColor = true;
            // 
            // labelTesseractLanguage
            // 
            this.labelTesseractLanguage.AutoSize = true;
            this.labelTesseractLanguage.Location = new System.Drawing.Point(18, 34);
            this.labelTesseractLanguage.Name = "labelTesseractLanguage";
            this.labelTesseractLanguage.Size = new System.Drawing.Size(54, 13);
            this.labelTesseractLanguage.TabIndex = 0;
            this.labelTesseractLanguage.Text = "Language";
            // 
            // comboBoxTesseractLanguages
            // 
            this.comboBoxTesseractLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTesseractLanguages.FormattingEnabled = true;
            this.comboBoxTesseractLanguages.Location = new System.Drawing.Point(99, 31);
            this.comboBoxTesseractLanguages.Name = "comboBoxTesseractLanguages";
            this.comboBoxTesseractLanguages.Size = new System.Drawing.Size(195, 21);
            this.comboBoxTesseractLanguages.TabIndex = 1;
            this.comboBoxTesseractLanguages.SelectedIndexChanged += new System.EventHandler(this.ComboBoxTesseractLanguagesSelectedIndexChanged);
            // 
            // groupBoxModiMethod
            // 
            this.groupBoxModiMethod.Controls.Add(this.label1);
            this.groupBoxModiMethod.Controls.Add(this.comboBoxModiLanguage);
            this.groupBoxModiMethod.Location = new System.Drawing.Point(7, 50);
            this.groupBoxModiMethod.Name = "groupBoxModiMethod";
            this.groupBoxModiMethod.Size = new System.Drawing.Size(366, 131);
            this.groupBoxModiMethod.TabIndex = 3;
            this.groupBoxModiMethod.TabStop = false;
            this.groupBoxModiMethod.Text = "MODI";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 33;
            this.label1.Text = "Language";
            // 
            // comboBoxModiLanguage
            // 
            this.comboBoxModiLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxModiLanguage.FormattingEnabled = true;
            this.comboBoxModiLanguage.Location = new System.Drawing.Point(108, 55);
            this.comboBoxModiLanguage.Name = "comboBoxModiLanguage";
            this.comboBoxModiLanguage.Size = new System.Drawing.Size(195, 21);
            this.comboBoxModiLanguage.TabIndex = 0;
            this.comboBoxModiLanguage.SelectedIndexChanged += new System.EventHandler(this.ComboBoxModiLanguageSelectedIndexChanged);
            // 
            // groupBoxNOCR
            // 
            this.groupBoxNOCR.Controls.Add(this.buttonLineOcrEditLanguage);
            this.groupBoxNOCR.Controls.Add(this.buttonLineOcrNewLanguage);
            this.groupBoxNOCR.Controls.Add(this.label2);
            this.groupBoxNOCR.Controls.Add(this.comboBoxNOcrLanguage);
            this.groupBoxNOCR.Controls.Add(this.checkBoxNOcrItalic);
            this.groupBoxNOCR.Controls.Add(this.checkBoxNOcrCorrect);
            this.groupBoxNOCR.Controls.Add(this.checkBoxRightToLeftNOCR);
            this.groupBoxNOCR.Controls.Add(this.numericUpDownNumberOfPixelsIsSpaceNOCR);
            this.groupBoxNOCR.Controls.Add(this.labelNumberOfPixelsIsSpaceNOCR);
            this.groupBoxNOCR.Location = new System.Drawing.Point(7, 38);
            this.groupBoxNOCR.Name = "groupBoxNOCR";
            this.groupBoxNOCR.Size = new System.Drawing.Size(366, 131);
            this.groupBoxNOCR.TabIndex = 7;
            this.groupBoxNOCR.TabStop = false;
            this.groupBoxNOCR.Text = "nOCR";
            // 
            // buttonLineOcrEditLanguage
            // 
            this.buttonLineOcrEditLanguage.Location = new System.Drawing.Point(210, 97);
            this.buttonLineOcrEditLanguage.Name = "buttonLineOcrEditLanguage";
            this.buttonLineOcrEditLanguage.Size = new System.Drawing.Size(68, 21);
            this.buttonLineOcrEditLanguage.TabIndex = 41;
            this.buttonLineOcrEditLanguage.Text = "Edit";
            this.buttonLineOcrEditLanguage.UseVisualStyleBackColor = true;
            this.buttonLineOcrEditLanguage.Click += new System.EventHandler(this.buttonLineOcrEditLanguage_Click);
            // 
            // buttonLineOcrNewLanguage
            // 
            this.buttonLineOcrNewLanguage.Location = new System.Drawing.Point(283, 97);
            this.buttonLineOcrNewLanguage.Name = "buttonLineOcrNewLanguage";
            this.buttonLineOcrNewLanguage.Size = new System.Drawing.Size(68, 21);
            this.buttonLineOcrNewLanguage.TabIndex = 40;
            this.buttonLineOcrNewLanguage.Text = "New";
            this.buttonLineOcrNewLanguage.UseVisualStyleBackColor = true;
            this.buttonLineOcrNewLanguage.Click += new System.EventHandler(this.buttonLineOcrNewLanguage_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 35;
            this.label2.Text = "Language";
            // 
            // comboBoxNOcrLanguage
            // 
            this.comboBoxNOcrLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxNOcrLanguage.FormattingEnabled = true;
            this.comboBoxNOcrLanguage.Location = new System.Drawing.Point(74, 97);
            this.comboBoxNOcrLanguage.Name = "comboBoxNOcrLanguage";
            this.comboBoxNOcrLanguage.Size = new System.Drawing.Size(130, 21);
            this.comboBoxNOcrLanguage.TabIndex = 34;
            this.comboBoxNOcrLanguage.SelectedIndexChanged += new System.EventHandler(this.comboBoxNOcrLanguage_SelectedIndexChanged);
            // 
            // checkBoxNOcrItalic
            // 
            this.checkBoxNOcrItalic.AutoSize = true;
            this.checkBoxNOcrItalic.Location = new System.Drawing.Point(15, 42);
            this.checkBoxNOcrItalic.Name = "checkBoxNOcrItalic";
            this.checkBoxNOcrItalic.Size = new System.Drawing.Size(92, 17);
            this.checkBoxNOcrItalic.TabIndex = 8;
            this.checkBoxNOcrItalic.Text = "Contains italic";
            this.checkBoxNOcrItalic.UseVisualStyleBackColor = true;
            // 
            // checkBoxNOcrCorrect
            // 
            this.checkBoxNOcrCorrect.AutoSize = true;
            this.checkBoxNOcrCorrect.Location = new System.Drawing.Point(235, 17);
            this.checkBoxNOcrCorrect.Name = "checkBoxNOcrCorrect";
            this.checkBoxNOcrCorrect.Size = new System.Drawing.Size(116, 17);
            this.checkBoxNOcrCorrect.TabIndex = 7;
            this.checkBoxNOcrCorrect.Text = "Draw missing texts";
            this.checkBoxNOcrCorrect.UseVisualStyleBackColor = true;
            // 
            // checkBoxRightToLeftNOCR
            // 
            this.checkBoxRightToLeftNOCR.AutoSize = true;
            this.checkBoxRightToLeftNOCR.Location = new System.Drawing.Point(235, 40);
            this.checkBoxRightToLeftNOCR.Name = "checkBoxRightToLeftNOCR";
            this.checkBoxRightToLeftNOCR.Size = new System.Drawing.Size(83, 17);
            this.checkBoxRightToLeftNOCR.TabIndex = 6;
            this.checkBoxRightToLeftNOCR.Text = "Right to left";
            this.checkBoxRightToLeftNOCR.UseVisualStyleBackColor = true;
            // 
            // numericUpDownNumberOfPixelsIsSpaceNOCR
            // 
            this.numericUpDownNumberOfPixelsIsSpaceNOCR.Location = new System.Drawing.Point(122, 17);
            this.numericUpDownNumberOfPixelsIsSpaceNOCR.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownNumberOfPixelsIsSpaceNOCR.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownNumberOfPixelsIsSpaceNOCR.Name = "numericUpDownNumberOfPixelsIsSpaceNOCR";
            this.numericUpDownNumberOfPixelsIsSpaceNOCR.Size = new System.Drawing.Size(50, 21);
            this.numericUpDownNumberOfPixelsIsSpaceNOCR.TabIndex = 5;
            this.numericUpDownNumberOfPixelsIsSpaceNOCR.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            // 
            // labelNumberOfPixelsIsSpaceNOCR
            // 
            this.labelNumberOfPixelsIsSpaceNOCR.AutoSize = true;
            this.labelNumberOfPixelsIsSpaceNOCR.Location = new System.Drawing.Point(12, 20);
            this.labelNumberOfPixelsIsSpaceNOCR.Name = "labelNumberOfPixelsIsSpaceNOCR";
            this.labelNumberOfPixelsIsSpaceNOCR.Size = new System.Drawing.Size(104, 13);
            this.labelNumberOfPixelsIsSpaceNOCR.TabIndex = 4;
            this.labelNumberOfPixelsIsSpaceNOCR.Text = "No of pixels is space";
            // 
            // groupBoxOCRControls
            // 
            this.groupBoxOCRControls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOCRControls.Controls.Add(this.labelStartFrom);
            this.groupBoxOCRControls.Controls.Add(this.numericUpDownStartNumber);
            this.groupBoxOCRControls.Controls.Add(this.buttonStop);
            this.groupBoxOCRControls.Controls.Add(this.buttonStartOcr);
            this.groupBoxOCRControls.Location = new System.Drawing.Point(368, 207);
            this.groupBoxOCRControls.Name = "groupBoxOCRControls";
            this.groupBoxOCRControls.Size = new System.Drawing.Size(287, 84);
            this.groupBoxOCRControls.TabIndex = 2;
            this.groupBoxOCRControls.TabStop = false;
            this.groupBoxOCRControls.Text = "OCR Start/stop";
            // 
            // labelStartFrom
            // 
            this.labelStartFrom.AutoSize = true;
            this.labelStartFrom.Location = new System.Drawing.Point(120, 26);
            this.labelStartFrom.Name = "labelStartFrom";
            this.labelStartFrom.Size = new System.Drawing.Size(127, 13);
            this.labelStartFrom.TabIndex = 1;
            this.labelStartFrom.Text = "Start OCR from subtitle#";
            // 
            // numericUpDownStartNumber
            // 
            this.numericUpDownStartNumber.Location = new System.Drawing.Point(123, 47);
            this.numericUpDownStartNumber.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numericUpDownStartNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownStartNumber.Name = "numericUpDownStartNumber";
            this.numericUpDownStartNumber.Size = new System.Drawing.Size(64, 21);
            this.numericUpDownStartNumber.TabIndex = 3;
            this.numericUpDownStartNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonStop
            // 
            this.buttonStop.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonStop.Location = new System.Drawing.Point(11, 52);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(105, 22);
            this.buttonStop.TabIndex = 2;
            this.buttonStop.Text = "Stop OCR";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.ButtonStopClick);
            // 
            // buttonStartOcr
            // 
            this.buttonStartOcr.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStartOcr.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonStartOcr.Location = new System.Drawing.Point(11, 24);
            this.buttonStartOcr.Name = "buttonStartOcr";
            this.buttonStartOcr.Size = new System.Drawing.Size(105, 22);
            this.buttonStartOcr.TabIndex = 0;
            this.buttonStartOcr.Text = "Start OCR";
            this.buttonStartOcr.UseVisualStyleBackColor = true;
            this.buttonStartOcr.Click += new System.EventHandler(this.ButtonStartOcrClick);
            // 
            // groupBoxOcrAutoFix
            // 
            this.groupBoxOcrAutoFix.Controls.Add(this.buttonSpellCheckDownload);
            this.groupBoxOcrAutoFix.Controls.Add(this.labelFixesMade);
            this.groupBoxOcrAutoFix.Controls.Add(this.comboBoxDictionaries);
            this.groupBoxOcrAutoFix.Controls.Add(this.checkBoxGuessUnknownWords);
            this.groupBoxOcrAutoFix.Controls.Add(this.tabControlLogs);
            this.groupBoxOcrAutoFix.Controls.Add(this.checkBoxPromptForUnknownWords);
            this.groupBoxOcrAutoFix.Controls.Add(this.checkBoxAutoBreakLines);
            this.groupBoxOcrAutoFix.Controls.Add(this.labelDictionaryLoaded);
            this.groupBoxOcrAutoFix.Controls.Add(this.checkBoxAutoFixCommonErrors);
            this.groupBoxOcrAutoFix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxOcrAutoFix.Location = new System.Drawing.Point(0, 0);
            this.groupBoxOcrAutoFix.Name = "groupBoxOcrAutoFix";
            this.groupBoxOcrAutoFix.Size = new System.Drawing.Size(400, 333);
            this.groupBoxOcrAutoFix.TabIndex = 0;
            this.groupBoxOcrAutoFix.TabStop = false;
            this.groupBoxOcrAutoFix.Text = "OCR auto correction / spell checking";
            // 
            // buttonSpellCheckDownload
            // 
            this.buttonSpellCheckDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSpellCheckDownload.Location = new System.Drawing.Point(367, 19);
            this.buttonSpellCheckDownload.Name = "buttonSpellCheckDownload";
            this.buttonSpellCheckDownload.Size = new System.Drawing.Size(28, 21);
            this.buttonSpellCheckDownload.TabIndex = 42;
            this.buttonSpellCheckDownload.Text = "...";
            this.buttonSpellCheckDownload.UseVisualStyleBackColor = true;
            this.buttonSpellCheckDownload.Click += new System.EventHandler(this.buttonSpellCheckDownload_Click);
            // 
            // labelFixesMade
            // 
            this.labelFixesMade.AutoSize = true;
            this.labelFixesMade.Location = new System.Drawing.Point(151, 48);
            this.labelFixesMade.Name = "labelFixesMade";
            this.labelFixesMade.Size = new System.Drawing.Size(98, 13);
            this.labelFixesMade.TabIndex = 3;
            this.labelFixesMade.Text = "NumberOfOcrFixes";
            // 
            // comboBoxDictionaries
            // 
            this.comboBoxDictionaries.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDictionaries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDictionaries.FormattingEnabled = true;
            this.comboBoxDictionaries.Location = new System.Drawing.Point(127, 20);
            this.comboBoxDictionaries.Name = "comboBoxDictionaries";
            this.comboBoxDictionaries.Size = new System.Drawing.Size(229, 21);
            this.comboBoxDictionaries.TabIndex = 1;
            this.comboBoxDictionaries.SelectedIndexChanged += new System.EventHandler(this.comboBoxDictionaries_SelectedIndexChanged);
            // 
            // checkBoxGuessUnknownWords
            // 
            this.checkBoxGuessUnknownWords.AutoSize = true;
            this.checkBoxGuessUnknownWords.Checked = true;
            this.checkBoxGuessUnknownWords.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxGuessUnknownWords.Location = new System.Drawing.Point(11, 91);
            this.checkBoxGuessUnknownWords.Name = "checkBoxGuessUnknownWords";
            this.checkBoxGuessUnknownWords.Size = new System.Drawing.Size(164, 17);
            this.checkBoxGuessUnknownWords.TabIndex = 5;
            this.checkBoxGuessUnknownWords.Text = "Try to guess unknown words";
            this.checkBoxGuessUnknownWords.UseVisualStyleBackColor = true;
            // 
            // tabControlLogs
            // 
            this.tabControlLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlLogs.ContextMenuStrip = this.contextMenuStripAllFixes;
            this.tabControlLogs.Controls.Add(this.tabPageUnknownWords);
            this.tabControlLogs.Controls.Add(this.tabPageAllFixes);
            this.tabControlLogs.Controls.Add(this.tabPageSuggestions);
            this.tabControlLogs.Location = new System.Drawing.Point(11, 146);
            this.tabControlLogs.Name = "tabControlLogs";
            this.tabControlLogs.SelectedIndex = 0;
            this.tabControlLogs.Size = new System.Drawing.Size(383, 181);
            this.tabControlLogs.TabIndex = 7;
            // 
            // contextMenuStripAllFixes
            // 
            this.contextMenuStripAllFixes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemClearFixes});
            this.contextMenuStripAllFixes.Name = "contextMenuStripUnknownWords";
            this.contextMenuStripAllFixes.Size = new System.Drawing.Size(102, 26);
            // 
            // toolStripMenuItemClearFixes
            // 
            this.toolStripMenuItemClearFixes.Name = "toolStripMenuItemClearFixes";
            this.toolStripMenuItemClearFixes.Size = new System.Drawing.Size(101, 22);
            this.toolStripMenuItemClearFixes.Text = "Clear";
            this.toolStripMenuItemClearFixes.Click += new System.EventHandler(this.toolStripMenuItemClearFixes_Click);
            // 
            // tabPageUnknownWords
            // 
            this.tabPageUnknownWords.Controls.Add(this.buttonGoogleIt);
            this.tabPageUnknownWords.Controls.Add(this.buttonAddToOcrReplaceList);
            this.tabPageUnknownWords.Controls.Add(this.buttonUknownToUserDic);
            this.tabPageUnknownWords.Controls.Add(this.buttonUknownToNames);
            this.tabPageUnknownWords.Controls.Add(this.listBoxUnknownWords);
            this.tabPageUnknownWords.Location = new System.Drawing.Point(4, 22);
            this.tabPageUnknownWords.Name = "tabPageUnknownWords";
            this.tabPageUnknownWords.Size = new System.Drawing.Size(375, 155);
            this.tabPageUnknownWords.TabIndex = 2;
            this.tabPageUnknownWords.Text = "Unknown words";
            this.tabPageUnknownWords.UseVisualStyleBackColor = true;
            // 
            // buttonGoogleIt
            // 
            this.buttonGoogleIt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGoogleIt.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGoogleIt.Location = new System.Drawing.Point(152, 88);
            this.buttonGoogleIt.Name = "buttonGoogleIt";
            this.buttonGoogleIt.Size = new System.Drawing.Size(217, 21);
            this.buttonGoogleIt.TabIndex = 3;
            this.buttonGoogleIt.Text = "Google it";
            this.buttonGoogleIt.UseVisualStyleBackColor = true;
            this.buttonGoogleIt.Click += new System.EventHandler(this.buttonGoogleIt_Click);
            // 
            // buttonAddToOcrReplaceList
            // 
            this.buttonAddToOcrReplaceList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddToOcrReplaceList.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAddToOcrReplaceList.Location = new System.Drawing.Point(152, 61);
            this.buttonAddToOcrReplaceList.Name = "buttonAddToOcrReplaceList";
            this.buttonAddToOcrReplaceList.Size = new System.Drawing.Size(217, 21);
            this.buttonAddToOcrReplaceList.TabIndex = 2;
            this.buttonAddToOcrReplaceList.Text = "Add OCR replace pair";
            this.buttonAddToOcrReplaceList.UseVisualStyleBackColor = true;
            this.buttonAddToOcrReplaceList.Click += new System.EventHandler(this.buttonAddToOcrReplaceList_Click);
            // 
            // buttonUknownToUserDic
            // 
            this.buttonUknownToUserDic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUknownToUserDic.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonUknownToUserDic.Location = new System.Drawing.Point(152, 34);
            this.buttonUknownToUserDic.Name = "buttonUknownToUserDic";
            this.buttonUknownToUserDic.Size = new System.Drawing.Size(217, 21);
            this.buttonUknownToUserDic.TabIndex = 1;
            this.buttonUknownToUserDic.Text = "Add to user dictionary";
            this.buttonUknownToUserDic.UseVisualStyleBackColor = true;
            this.buttonUknownToUserDic.Click += new System.EventHandler(this.buttonUknownToUserDic_Click);
            // 
            // buttonUknownToNames
            // 
            this.buttonUknownToNames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUknownToNames.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonUknownToNames.Location = new System.Drawing.Point(152, 7);
            this.buttonUknownToNames.Name = "buttonUknownToNames";
            this.buttonUknownToNames.Size = new System.Drawing.Size(217, 21);
            this.buttonUknownToNames.TabIndex = 0;
            this.buttonUknownToNames.Text = "Add to names etc list";
            this.buttonUknownToNames.UseVisualStyleBackColor = true;
            this.buttonUknownToNames.Click += new System.EventHandler(this.buttonUknownToNames_Click);
            // 
            // listBoxUnknownWords
            // 
            this.listBoxUnknownWords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxUnknownWords.ContextMenuStrip = this.contextMenuStripUnknownWords;
            this.listBoxUnknownWords.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxUnknownWords.FormattingEnabled = true;
            this.listBoxUnknownWords.HorizontalScrollbar = true;
            this.listBoxUnknownWords.Location = new System.Drawing.Point(3, 3);
            this.listBoxUnknownWords.Name = "listBoxUnknownWords";
            this.listBoxUnknownWords.Size = new System.Drawing.Size(143, 147);
            this.listBoxUnknownWords.TabIndex = 40;
            this.listBoxUnknownWords.SelectedIndexChanged += new System.EventHandler(this.ListBoxLogSelectedIndexChanged);
            this.listBoxUnknownWords.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxCopyToClipboard_KeyDown);
            // 
            // contextMenuStripUnknownWords
            // 
            this.contextMenuStripUnknownWords.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem});
            this.contextMenuStripUnknownWords.Name = "contextMenuStripUnknownWords";
            this.contextMenuStripUnknownWords.Size = new System.Drawing.Size(102, 26);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // tabPageAllFixes
            // 
            this.tabPageAllFixes.Controls.Add(this.listBoxLog);
            this.tabPageAllFixes.Location = new System.Drawing.Point(4, 22);
            this.tabPageAllFixes.Name = "tabPageAllFixes";
            this.tabPageAllFixes.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAllFixes.Size = new System.Drawing.Size(375, 155);
            this.tabPageAllFixes.TabIndex = 0;
            this.tabPageAllFixes.Text = "All fixes";
            this.tabPageAllFixes.UseVisualStyleBackColor = true;
            // 
            // listBoxLog
            // 
            this.listBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLog.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.HorizontalScrollbar = true;
            this.listBoxLog.Location = new System.Drawing.Point(3, 3);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(369, 149);
            this.listBoxLog.TabIndex = 0;
            this.listBoxLog.SelectedIndexChanged += new System.EventHandler(this.ListBoxLogSelectedIndexChanged);
            this.listBoxLog.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxCopyToClipboard_KeyDown);
            // 
            // tabPageSuggestions
            // 
            this.tabPageSuggestions.Controls.Add(this.listBoxLogSuggestions);
            this.tabPageSuggestions.Location = new System.Drawing.Point(4, 22);
            this.tabPageSuggestions.Name = "tabPageSuggestions";
            this.tabPageSuggestions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSuggestions.Size = new System.Drawing.Size(375, 155);
            this.tabPageSuggestions.TabIndex = 1;
            this.tabPageSuggestions.Text = "Guesses used";
            this.tabPageSuggestions.UseVisualStyleBackColor = true;
            // 
            // listBoxLogSuggestions
            // 
            this.listBoxLogSuggestions.ContextMenuStrip = this.contextMenuStripGuessesUsed;
            this.listBoxLogSuggestions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLogSuggestions.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxLogSuggestions.FormattingEnabled = true;
            this.listBoxLogSuggestions.HorizontalScrollbar = true;
            this.listBoxLogSuggestions.Location = new System.Drawing.Point(3, 3);
            this.listBoxLogSuggestions.Name = "listBoxLogSuggestions";
            this.listBoxLogSuggestions.Size = new System.Drawing.Size(369, 149);
            this.listBoxLogSuggestions.TabIndex = 40;
            this.listBoxLogSuggestions.SelectedIndexChanged += new System.EventHandler(this.ListBoxLogSelectedIndexChanged);
            this.listBoxLogSuggestions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxCopyToClipboard_KeyDown);
            // 
            // contextMenuStripGuessesUsed
            // 
            this.contextMenuStripGuessesUsed.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemClearGuesses});
            this.contextMenuStripGuessesUsed.Name = "contextMenuStripUnknownWords";
            this.contextMenuStripGuessesUsed.Size = new System.Drawing.Size(102, 26);
            // 
            // toolStripMenuItemClearGuesses
            // 
            this.toolStripMenuItemClearGuesses.Name = "toolStripMenuItemClearGuesses";
            this.toolStripMenuItemClearGuesses.Size = new System.Drawing.Size(101, 22);
            this.toolStripMenuItemClearGuesses.Text = "Clear";
            this.toolStripMenuItemClearGuesses.Click += new System.EventHandler(this.toolStripMenuItemClearGuesses_Click);
            // 
            // checkBoxPromptForUnknownWords
            // 
            this.checkBoxPromptForUnknownWords.AutoSize = true;
            this.checkBoxPromptForUnknownWords.Checked = true;
            this.checkBoxPromptForUnknownWords.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPromptForUnknownWords.Location = new System.Drawing.Point(11, 69);
            this.checkBoxPromptForUnknownWords.Name = "checkBoxPromptForUnknownWords";
            this.checkBoxPromptForUnknownWords.Size = new System.Drawing.Size(255, 17);
            this.checkBoxPromptForUnknownWords.TabIndex = 4;
            this.checkBoxPromptForUnknownWords.Text = "Prompt for unknown words (requires dictionary)";
            this.checkBoxPromptForUnknownWords.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoBreakLines
            // 
            this.checkBoxAutoBreakLines.AutoSize = true;
            this.checkBoxAutoBreakLines.Checked = true;
            this.checkBoxAutoBreakLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoBreakLines.Location = new System.Drawing.Point(11, 113);
            this.checkBoxAutoBreakLines.Name = "checkBoxAutoBreakLines";
            this.checkBoxAutoBreakLines.Size = new System.Drawing.Size(208, 17);
            this.checkBoxAutoBreakLines.TabIndex = 6;
            this.checkBoxAutoBreakLines.Text = "Auto break subtitle, if line number > 2";
            this.checkBoxAutoBreakLines.UseVisualStyleBackColor = true;
            // 
            // labelDictionaryLoaded
            // 
            this.labelDictionaryLoaded.AutoSize = true;
            this.labelDictionaryLoaded.Location = new System.Drawing.Point(11, 24);
            this.labelDictionaryLoaded.Name = "labelDictionaryLoaded";
            this.labelDictionaryLoaded.Size = new System.Drawing.Size(112, 13);
            this.labelDictionaryLoaded.TabIndex = 0;
            this.labelDictionaryLoaded.Text = "labelDictionaryLoaded";
            // 
            // checkBoxAutoFixCommonErrors
            // 
            this.checkBoxAutoFixCommonErrors.AutoSize = true;
            this.checkBoxAutoFixCommonErrors.Checked = true;
            this.checkBoxAutoFixCommonErrors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoFixCommonErrors.Location = new System.Drawing.Point(11, 47);
            this.checkBoxAutoFixCommonErrors.Name = "checkBoxAutoFixCommonErrors";
            this.checkBoxAutoFixCommonErrors.Size = new System.Drawing.Size(139, 17);
            this.checkBoxAutoFixCommonErrors.TabIndex = 2;
            this.checkBoxAutoFixCommonErrors.Text = "Fix common OCR errors";
            this.checkBoxAutoFixCommonErrors.UseVisualStyleBackColor = true;
            // 
            // groupBoxImagePalette
            // 
            this.groupBoxImagePalette.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImagePalette.Controls.Add(this.checkBoxBackgroundTransparent);
            this.groupBoxImagePalette.Controls.Add(this.pictureBoxBackground);
            this.groupBoxImagePalette.Controls.Add(this.checkBoxEmphasis2Transparent);
            this.groupBoxImagePalette.Controls.Add(this.checkBoxEmphasis1Transparent);
            this.groupBoxImagePalette.Controls.Add(this.checkBoxPatternTransparent);
            this.groupBoxImagePalette.Controls.Add(this.pictureBoxEmphasis2);
            this.groupBoxImagePalette.Controls.Add(this.pictureBoxEmphasis1);
            this.groupBoxImagePalette.Controls.Add(this.pictureBoxPattern);
            this.groupBoxImagePalette.Controls.Add(this.checkBoxCustomFourColors);
            this.groupBoxImagePalette.Location = new System.Drawing.Point(13, 16);
            this.groupBoxImagePalette.Name = "groupBoxImagePalette";
            this.groupBoxImagePalette.Size = new System.Drawing.Size(636, 38);
            this.groupBoxImagePalette.TabIndex = 35;
            this.groupBoxImagePalette.TabStop = false;
            this.groupBoxImagePalette.Text = "Image palette";
            // 
            // checkBoxBackgroundTransparent
            // 
            this.checkBoxBackgroundTransparent.AutoSize = true;
            this.checkBoxBackgroundTransparent.Checked = true;
            this.checkBoxBackgroundTransparent.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBackgroundTransparent.Location = new System.Drawing.Point(144, 19);
            this.checkBoxBackgroundTransparent.Name = "checkBoxBackgroundTransparent";
            this.checkBoxBackgroundTransparent.Size = new System.Drawing.Size(85, 17);
            this.checkBoxBackgroundTransparent.TabIndex = 8;
            this.checkBoxBackgroundTransparent.Text = "Transparent";
            this.checkBoxBackgroundTransparent.UseVisualStyleBackColor = true;
            this.checkBoxBackgroundTransparent.CheckedChanged += new System.EventHandler(this.CheckBoxPatternTransparentCheckedChanged);
            // 
            // pictureBoxBackground
            // 
            this.pictureBoxBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxBackground.Location = new System.Drawing.Point(120, 12);
            this.pictureBoxBackground.Name = "pictureBoxBackground";
            this.pictureBoxBackground.Size = new System.Drawing.Size(21, 21);
            this.pictureBoxBackground.TabIndex = 7;
            this.pictureBoxBackground.TabStop = false;
            this.pictureBoxBackground.Click += new System.EventHandler(this.PictureBoxColorChooserClick);
            // 
            // checkBoxEmphasis2Transparent
            // 
            this.checkBoxEmphasis2Transparent.AutoSize = true;
            this.checkBoxEmphasis2Transparent.Location = new System.Drawing.Point(507, 19);
            this.checkBoxEmphasis2Transparent.Name = "checkBoxEmphasis2Transparent";
            this.checkBoxEmphasis2Transparent.Size = new System.Drawing.Size(85, 17);
            this.checkBoxEmphasis2Transparent.TabIndex = 6;
            this.checkBoxEmphasis2Transparent.Text = "Transparent";
            this.checkBoxEmphasis2Transparent.UseVisualStyleBackColor = true;
            this.checkBoxEmphasis2Transparent.CheckedChanged += new System.EventHandler(this.CheckBoxEmphasis2TransparentCheckedChanged);
            // 
            // checkBoxEmphasis1Transparent
            // 
            this.checkBoxEmphasis1Transparent.AutoSize = true;
            this.checkBoxEmphasis1Transparent.Location = new System.Drawing.Point(387, 19);
            this.checkBoxEmphasis1Transparent.Name = "checkBoxEmphasis1Transparent";
            this.checkBoxEmphasis1Transparent.Size = new System.Drawing.Size(85, 17);
            this.checkBoxEmphasis1Transparent.TabIndex = 5;
            this.checkBoxEmphasis1Transparent.Text = "Transparent";
            this.checkBoxEmphasis1Transparent.UseVisualStyleBackColor = true;
            this.checkBoxEmphasis1Transparent.CheckedChanged += new System.EventHandler(this.CheckBoxEmphasis1TransparentCheckedChanged);
            // 
            // checkBoxPatternTransparent
            // 
            this.checkBoxPatternTransparent.AutoSize = true;
            this.checkBoxPatternTransparent.Location = new System.Drawing.Point(266, 19);
            this.checkBoxPatternTransparent.Name = "checkBoxPatternTransparent";
            this.checkBoxPatternTransparent.Size = new System.Drawing.Size(85, 17);
            this.checkBoxPatternTransparent.TabIndex = 4;
            this.checkBoxPatternTransparent.Text = "Transparent";
            this.checkBoxPatternTransparent.UseVisualStyleBackColor = true;
            this.checkBoxPatternTransparent.CheckedChanged += new System.EventHandler(this.CheckBoxPatternTransparentCheckedChanged);
            // 
            // pictureBoxEmphasis2
            // 
            this.pictureBoxEmphasis2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxEmphasis2.Location = new System.Drawing.Point(484, 12);
            this.pictureBoxEmphasis2.Name = "pictureBoxEmphasis2";
            this.pictureBoxEmphasis2.Size = new System.Drawing.Size(21, 21);
            this.pictureBoxEmphasis2.TabIndex = 3;
            this.pictureBoxEmphasis2.TabStop = false;
            this.pictureBoxEmphasis2.Click += new System.EventHandler(this.PictureBoxColorChooserClick);
            this.pictureBoxEmphasis2.DoubleClick += new System.EventHandler(this.PictureBoxColorChooserClick);
            // 
            // pictureBoxEmphasis1
            // 
            this.pictureBoxEmphasis1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxEmphasis1.Location = new System.Drawing.Point(363, 12);
            this.pictureBoxEmphasis1.Name = "pictureBoxEmphasis1";
            this.pictureBoxEmphasis1.Size = new System.Drawing.Size(21, 21);
            this.pictureBoxEmphasis1.TabIndex = 2;
            this.pictureBoxEmphasis1.TabStop = false;
            this.pictureBoxEmphasis1.Click += new System.EventHandler(this.PictureBoxColorChooserClick);
            this.pictureBoxEmphasis1.DoubleClick += new System.EventHandler(this.PictureBoxColorChooserClick);
            // 
            // pictureBoxPattern
            // 
            this.pictureBoxPattern.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxPattern.Location = new System.Drawing.Point(242, 12);
            this.pictureBoxPattern.Name = "pictureBoxPattern";
            this.pictureBoxPattern.Size = new System.Drawing.Size(21, 21);
            this.pictureBoxPattern.TabIndex = 1;
            this.pictureBoxPattern.TabStop = false;
            this.pictureBoxPattern.Click += new System.EventHandler(this.PictureBoxColorChooserClick);
            this.pictureBoxPattern.DoubleClick += new System.EventHandler(this.PictureBoxColorChooserClick);
            // 
            // checkBoxCustomFourColors
            // 
            this.checkBoxCustomFourColors.AutoSize = true;
            this.checkBoxCustomFourColors.Location = new System.Drawing.Point(7, 16);
            this.checkBoxCustomFourColors.Name = "checkBoxCustomFourColors";
            this.checkBoxCustomFourColors.Size = new System.Drawing.Size(116, 17);
            this.checkBoxCustomFourColors.TabIndex = 0;
            this.checkBoxCustomFourColors.Text = "Use custom colors:";
            this.checkBoxCustomFourColors.UseVisualStyleBackColor = true;
            this.checkBoxCustomFourColors.CheckedChanged += new System.EventHandler(this.CheckBoxCustomFourColorsCheckedChanged);
            // 
            // groupBoxSubtitleImage
            // 
            this.groupBoxSubtitleImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSubtitleImage.Controls.Add(this.labelMinAlpha);
            this.groupBoxSubtitleImage.Controls.Add(this.numericUpDownAutoTransparentAlphaMax);
            this.groupBoxSubtitleImage.Controls.Add(this.groupBoxTransportStream);
            this.groupBoxSubtitleImage.Controls.Add(this.checkBoxAutoTransparentBackground);
            this.groupBoxSubtitleImage.Controls.Add(this.groupBoxImagePalette);
            this.groupBoxSubtitleImage.Controls.Add(this.pictureBoxSubtitleImage);
            this.groupBoxSubtitleImage.Location = new System.Drawing.Point(412, 6);
            this.groupBoxSubtitleImage.Name = "groupBoxSubtitleImage";
            this.groupBoxSubtitleImage.Size = new System.Drawing.Size(657, 191);
            this.groupBoxSubtitleImage.TabIndex = 36;
            this.groupBoxSubtitleImage.TabStop = false;
            this.groupBoxSubtitleImage.Text = "Subtitle image";
            // 
            // labelMinAlpha
            // 
            this.labelMinAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMinAlpha.AutoSize = true;
            this.labelMinAlpha.Location = new System.Drawing.Point(176, 171);
            this.labelMinAlpha.Name = "labelMinAlpha";
            this.labelMinAlpha.Size = new System.Drawing.Size(252, 13);
            this.labelMinAlpha.TabIndex = 40;
            this.labelMinAlpha.Text = "Min. alpha value (0=transparent, 255=fully visible)";
            this.labelMinAlpha.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.labelMinAlpha.Visible = false;
            // 
            // numericUpDownAutoTransparentAlphaMax
            // 
            this.numericUpDownAutoTransparentAlphaMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownAutoTransparentAlphaMax.Location = new System.Drawing.Point(432, 169);
            this.numericUpDownAutoTransparentAlphaMax.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownAutoTransparentAlphaMax.Name = "numericUpDownAutoTransparentAlphaMax";
            this.numericUpDownAutoTransparentAlphaMax.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownAutoTransparentAlphaMax.TabIndex = 37;
            this.numericUpDownAutoTransparentAlphaMax.Value = new decimal(new int[] {
            140,
            0,
            0,
            0});
            this.numericUpDownAutoTransparentAlphaMax.Visible = false;
            this.numericUpDownAutoTransparentAlphaMax.ValueChanged += new System.EventHandler(this.numericUpDownAutoTransparentAlphaMax_ValueChanged);
            // 
            // groupBoxTransportStream
            // 
            this.groupBoxTransportStream.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTransportStream.Controls.Add(this.checkBoxTransportStreamGetColorAndSplit);
            this.groupBoxTransportStream.Controls.Add(this.checkBoxTransportStreamGrayscale);
            this.groupBoxTransportStream.Location = new System.Drawing.Point(94, 6);
            this.groupBoxTransportStream.Name = "groupBoxTransportStream";
            this.groupBoxTransportStream.Size = new System.Drawing.Size(636, 38);
            this.groupBoxTransportStream.TabIndex = 36;
            this.groupBoxTransportStream.TabStop = false;
            this.groupBoxTransportStream.Text = "Transport stream";
            this.groupBoxTransportStream.Visible = false;
            // 
            // checkBoxTransportStreamGetColorAndSplit
            // 
            this.checkBoxTransportStreamGetColorAndSplit.AutoSize = true;
            this.checkBoxTransportStreamGetColorAndSplit.Location = new System.Drawing.Point(135, 15);
            this.checkBoxTransportStreamGetColorAndSplit.Name = "checkBoxTransportStreamGetColorAndSplit";
            this.checkBoxTransportStreamGetColorAndSplit.Size = new System.Drawing.Size(253, 17);
            this.checkBoxTransportStreamGetColorAndSplit.TabIndex = 1;
            this.checkBoxTransportStreamGetColorAndSplit.Text = "Try get color (will include some splitting of lines)";
            this.checkBoxTransportStreamGetColorAndSplit.UseVisualStyleBackColor = true;
            this.checkBoxTransportStreamGetColorAndSplit.CheckedChanged += new System.EventHandler(this.checkBoxTransportStreamGetColorAndSplit_CheckedChanged);
            // 
            // checkBoxTransportStreamGrayscale
            // 
            this.checkBoxTransportStreamGrayscale.AutoSize = true;
            this.checkBoxTransportStreamGrayscale.Location = new System.Drawing.Point(7, 16);
            this.checkBoxTransportStreamGrayscale.Name = "checkBoxTransportStreamGrayscale";
            this.checkBoxTransportStreamGrayscale.Size = new System.Drawing.Size(73, 17);
            this.checkBoxTransportStreamGrayscale.TabIndex = 0;
            this.checkBoxTransportStreamGrayscale.Text = "Grayscale";
            this.checkBoxTransportStreamGrayscale.UseVisualStyleBackColor = true;
            this.checkBoxTransportStreamGrayscale.CheckedChanged += new System.EventHandler(this.checkBoxTransportStreamGrayscale_CheckedChanged);
            // 
            // checkBoxAutoTransparentBackground
            // 
            this.checkBoxAutoTransparentBackground.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAutoTransparentBackground.AutoSize = true;
            this.checkBoxAutoTransparentBackground.Location = new System.Drawing.Point(481, 170);
            this.checkBoxAutoTransparentBackground.Name = "checkBoxAutoTransparentBackground";
            this.checkBoxAutoTransparentBackground.Size = new System.Drawing.Size(168, 17);
            this.checkBoxAutoTransparentBackground.TabIndex = 36;
            this.checkBoxAutoTransparentBackground.Text = "Auto transparent background";
            this.checkBoxAutoTransparentBackground.UseVisualStyleBackColor = true;
            this.checkBoxAutoTransparentBackground.CheckedChanged += new System.EventHandler(this.checkBoxAutoTransparentBackground_CheckedChanged);
            // 
            // pictureBoxSubtitleImage
            // 
            this.pictureBoxSubtitleImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxSubtitleImage.BackColor = System.Drawing.Color.DimGray;
            this.pictureBoxSubtitleImage.ContextMenuStrip = this.contextMenuStripImage;
            this.pictureBoxSubtitleImage.Location = new System.Drawing.Point(13, 60);
            this.pictureBoxSubtitleImage.Name = "pictureBoxSubtitleImage";
            this.pictureBoxSubtitleImage.Size = new System.Drawing.Size(636, 127);
            this.pictureBoxSubtitleImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxSubtitleImage.TabIndex = 3;
            this.pictureBoxSubtitleImage.TabStop = false;
            // 
            // contextMenuStripImage
            // 
            this.contextMenuStripImage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemImageSaveAs});
            this.contextMenuStripImage.Name = "contextMenuStripUnknownWords";
            this.contextMenuStripImage.Size = new System.Drawing.Size(158, 26);
            // 
            // toolStripMenuItemImageSaveAs
            // 
            this.toolStripMenuItemImageSaveAs.Name = "toolStripMenuItemImageSaveAs";
            this.toolStripMenuItemImageSaveAs.Size = new System.Drawing.Size(157, 22);
            this.toolStripMenuItemImageSaveAs.Text = "Save image as...";
            this.toolStripMenuItemImageSaveAs.Click += new System.EventHandler(this.toolStripMenuItemImageSaveAs_Click);
            // 
            // checkBoxShowOnlyForced
            // 
            this.checkBoxShowOnlyForced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxShowOnlyForced.AutoSize = true;
            this.checkBoxShowOnlyForced.Location = new System.Drawing.Point(369, 313);
            this.checkBoxShowOnlyForced.Name = "checkBoxShowOnlyForced";
            this.checkBoxShowOnlyForced.Size = new System.Drawing.Size(152, 17);
            this.checkBoxShowOnlyForced.TabIndex = 4;
            this.checkBoxShowOnlyForced.Text = "Show only forced subtitles";
            this.checkBoxShowOnlyForced.UseVisualStyleBackColor = true;
            this.checkBoxShowOnlyForced.CheckedChanged += new System.EventHandler(this.checkBoxShowOnlyForced_CheckedChanged);
            // 
            // checkBoxUseTimeCodesFromIdx
            // 
            this.checkBoxUseTimeCodesFromIdx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxUseTimeCodesFromIdx.AutoSize = true;
            this.checkBoxUseTimeCodesFromIdx.Location = new System.Drawing.Point(369, 295);
            this.checkBoxUseTimeCodesFromIdx.Name = "checkBoxUseTimeCodesFromIdx";
            this.checkBoxUseTimeCodesFromIdx.Size = new System.Drawing.Size(186, 17);
            this.checkBoxUseTimeCodesFromIdx.TabIndex = 3;
            this.checkBoxUseTimeCodesFromIdx.Text = "Use lines/time codes from .idx file";
            this.checkBoxUseTimeCodesFromIdx.UseVisualStyleBackColor = true;
            this.checkBoxUseTimeCodesFromIdx.CheckedChanged += new System.EventHandler(this.checkBoxUseTimeCodesFromIdx_CheckedChanged);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // splitContainerBottom
            // 
            this.splitContainerBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerBottom.Location = new System.Drawing.Point(15, 199);
            this.splitContainerBottom.Name = "splitContainerBottom";
            // 
            // splitContainerBottom.Panel1
            // 
            this.splitContainerBottom.Panel1.Controls.Add(this.checkBoxShowOnlyForced);
            this.splitContainerBottom.Panel1.Controls.Add(this.textBoxCurrentText);
            this.splitContainerBottom.Panel1.Controls.Add(this.groupBoxOCRControls);
            this.splitContainerBottom.Panel1.Controls.Add(this.checkBoxUseTimeCodesFromIdx);
            this.splitContainerBottom.Panel1.Controls.Add(this.subtitleListView1);
            this.splitContainerBottom.Panel1.Controls.Add(this.labelSubtitleText);
            this.splitContainerBottom.Panel1MinSize = 100;
            // 
            // splitContainerBottom.Panel2
            // 
            this.splitContainerBottom.Panel2.Controls.Add(this.groupBoxOcrAutoFix);
            this.splitContainerBottom.Panel2MinSize = 100;
            this.splitContainerBottom.Size = new System.Drawing.Size(1062, 333);
            this.splitContainerBottom.SplitterDistance = 658;
            this.splitContainerBottom.TabIndex = 39;
            // 
            // textBoxCurrentText
            // 
            this.textBoxCurrentText.AllowDrop = true;
            this.textBoxCurrentText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCurrentText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxCurrentText.Location = new System.Drawing.Point(8, 214);
            this.textBoxCurrentText.Multiline = true;
            this.textBoxCurrentText.Name = "textBoxCurrentText";
            this.textBoxCurrentText.Size = new System.Drawing.Size(354, 77);
            this.textBoxCurrentText.TabIndex = 1;
            this.textBoxCurrentText.TextChanged += new System.EventHandler(this.TextBoxCurrentTextTextChanged);
            this.textBoxCurrentText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxCurrentText_KeyDown);
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.subtitleListView1.ContextMenuStrip = this.contextMenuStripListview;
            this.subtitleListView1.FirstVisibleIndex = -1;
            this.subtitleListView1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.HideSelection = false;
            this.subtitleListView1.Location = new System.Drawing.Point(8, 21);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.OwnerDraw = true;
            this.subtitleListView1.Size = new System.Drawing.Size(631, 183);
            this.subtitleListView1.SubtitleFontBold = false;
            this.subtitleListView1.SubtitleFontName = "Tahoma";
            this.subtitleListView1.SubtitleFontSize = 8;
            this.subtitleListView1.TabIndex = 0;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.UseSyntaxColoring = true;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            this.subtitleListView1.SelectedIndexChanged += new System.EventHandler(this.SubtitleListView1SelectedIndexChanged);
            this.subtitleListView1.DoubleClick += new System.EventHandler(this.subtitleListView1_DoubleClick);
            this.subtitleListView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.subtitleListView1_KeyDown);
            // 
            // timerHideStatus
            // 
            this.timerHideStatus.Interval = 2000;
            this.timerHideStatus.Tick += new System.EventHandler(this.timerHideStatus_Tick);
            // 
            // VobSubOcr
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1089, 579);
            this.Controls.Add(this.splitContainerBottom);
            this.Controls.Add(this.groupBoxSubtitleImage);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.groupBoxOcrMethod);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1013, 578);
            this.Name = "VobSubOcr";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import/OCR VobSub (sub/idx) subtitle";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VobSubOcr_FormClosing);
            this.Shown += new System.EventHandler(this.FormVobSubOcr_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VobSubOcr_KeyDown);
            this.Resize += new System.EventHandler(this.VobSubOcr_Resize);
            this.contextMenuStripListview.ResumeLayout(false);
            this.groupBoxOcrMethod.ResumeLayout(false);
            this.groupBoxImageCompareMethod.ResumeLayout(false);
            this.groupBoxImageCompareMethod.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxErrorPct)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPixelsIsSpace)).EndInit();
            this.GroupBoxTesseractMethod.ResumeLayout(false);
            this.GroupBoxTesseractMethod.PerformLayout();
            this.groupBoxModiMethod.ResumeLayout(false);
            this.groupBoxModiMethod.PerformLayout();
            this.groupBoxNOCR.ResumeLayout(false);
            this.groupBoxNOCR.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumberOfPixelsIsSpaceNOCR)).EndInit();
            this.groupBoxOCRControls.ResumeLayout(false);
            this.groupBoxOCRControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStartNumber)).EndInit();
            this.groupBoxOcrAutoFix.ResumeLayout(false);
            this.groupBoxOcrAutoFix.PerformLayout();
            this.tabControlLogs.ResumeLayout(false);
            this.contextMenuStripAllFixes.ResumeLayout(false);
            this.tabPageUnknownWords.ResumeLayout(false);
            this.contextMenuStripUnknownWords.ResumeLayout(false);
            this.tabPageAllFixes.ResumeLayout(false);
            this.tabPageSuggestions.ResumeLayout(false);
            this.contextMenuStripGuessesUsed.ResumeLayout(false);
            this.groupBoxImagePalette.ResumeLayout(false);
            this.groupBoxImagePalette.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBackground)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEmphasis2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEmphasis1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPattern)).EndInit();
            this.groupBoxSubtitleImage.ResumeLayout(false);
            this.groupBoxSubtitleImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAutoTransparentAlphaMax)).EndInit();
            this.groupBoxTransportStream.ResumeLayout(false);
            this.groupBoxTransportStream.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).EndInit();
            this.contextMenuStripImage.ResumeLayout(false);
            this.splitContainerBottom.Panel1.ResumeLayout(false);
            this.splitContainerBottom.Panel1.PerformLayout();
            this.splitContainerBottom.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBottom)).EndInit();
            this.splitContainerBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxSubtitleImage;
        private System.Windows.Forms.Label labelSubtitleText;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private SubtitleListView subtitleListView1;
        private System.Windows.Forms.GroupBox groupBoxOcrMethod;
        private System.Windows.Forms.ComboBox comboBoxModiLanguage;
        private SETextBox textBoxCurrentText;
        private System.Windows.Forms.GroupBox groupBoxOCRControls;
        private System.Windows.Forms.Label labelStartFrom;
        private System.Windows.Forms.NumericUpDown numericUpDownStartNumber;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonStartOcr;
        private System.Windows.Forms.GroupBox groupBoxOcrAutoFix;
        private System.Windows.Forms.Label labelFixesMade;
        private System.Windows.Forms.CheckBox checkBoxAutoFixCommonErrors;
        private System.Windows.Forms.CheckBox checkBoxAutoBreakLines;
        private System.Windows.Forms.Label labelDictionaryLoaded;
        private System.Windows.Forms.CheckBox checkBoxPromptForUnknownWords;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.ComboBox comboBoxOcrMethod;
        private System.Windows.Forms.GroupBox groupBoxImageCompareMethod;
        private System.Windows.Forms.NumericUpDown numericUpDownPixelsIsSpace;
        private System.Windows.Forms.Button buttonEditCharacterDatabase;
        private System.Windows.Forms.Label labelNoOfPixelsIsSpace;
        private System.Windows.Forms.ComboBox comboBoxCharacterDatabase;
        private System.Windows.Forms.Label labelImageDatabase;
        private System.Windows.Forms.Button buttonNewCharacterDatabase;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBoxModiMethod;
        private System.Windows.Forms.GroupBox GroupBoxTesseractMethod;
        private System.Windows.Forms.CheckBox checkBoxUseModiInTesseractForUnknownWords;
        private System.Windows.Forms.Label labelTesseractLanguage;
        private System.Windows.Forms.ComboBox comboBoxTesseractLanguages;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListview;
        private System.Windows.Forms.ToolStripMenuItem normalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem italicToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem saveImageAsToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.TabControl tabControlLogs;
        private System.Windows.Forms.TabPage tabPageAllFixes;
        private System.Windows.Forms.TabPage tabPageSuggestions;
        private System.Windows.Forms.ListBox listBoxLogSuggestions;
        private System.Windows.Forms.CheckBox checkBoxGuessUnknownWords;
        private System.Windows.Forms.TabPage tabPageUnknownWords;
        private System.Windows.Forms.ListBox listBoxUnknownWords;
        private System.Windows.Forms.GroupBox groupBoxImagePalette;
        private System.Windows.Forms.PictureBox pictureBoxEmphasis2;
        private System.Windows.Forms.PictureBox pictureBoxEmphasis1;
        private System.Windows.Forms.PictureBox pictureBoxPattern;
        private System.Windows.Forms.CheckBox checkBoxCustomFourColors;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.GroupBox groupBoxSubtitleImage;
        private System.Windows.Forms.CheckBox checkBoxEmphasis2Transparent;
        private System.Windows.Forms.CheckBox checkBoxEmphasis1Transparent;
        private System.Windows.Forms.CheckBox checkBoxPatternTransparent;
        private System.Windows.Forms.CheckBox checkBoxRightToLeft;
        private System.Windows.Forms.CheckBox checkBoxShowOnlyForced;
        private System.Windows.Forms.CheckBox checkBoxUseTimeCodesFromIdx;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ComboBox comboBoxDictionaries;
        private System.Windows.Forms.CheckBox checkBoxBackgroundTransparent;
        private System.Windows.Forms.PictureBox pictureBoxBackground;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem importTextWithMatchingTimeCodesToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem saveAllImagesWithHtmlIndexViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorImageCompare;
        private System.Windows.Forms.ToolStripMenuItem inspectImageCompareMatchesForCurrentImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EditLastAdditionsToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxAutoTransparentBackground;
        private System.Windows.Forms.SplitContainer splitContainerBottom;
        private System.Windows.Forms.CheckBox checkBoxTesseractItalicsOn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Button buttonUknownToUserDic;
        private System.Windows.Forms.Button buttonUknownToNames;
        private System.Windows.Forms.Button buttonAddToOcrReplaceList;
        private System.Windows.Forms.Button buttonGoogleIt;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSetUnItalicFactor;
        private System.Windows.Forms.CheckBox checkBoxTesseractMusicOn;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExport;
        private System.Windows.Forms.ToolStripMenuItem vobSubToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bluraySupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bDNXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripUnknownWords;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAllFixes;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemClearFixes;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripGuessesUsed;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemClearGuesses;
        private System.Windows.Forms.GroupBox groupBoxNOCR;
        private System.Windows.Forms.CheckBox checkBoxRightToLeftNOCR;
        private System.Windows.Forms.NumericUpDown numericUpDownNumberOfPixelsIsSpaceNOCR;
        private System.Windows.Forms.Label labelNumberOfPixelsIsSpaceNOCR;
        private System.Windows.Forms.CheckBox checkBoxNOcrCorrect;
        private System.Windows.Forms.CheckBox checkBoxNOcrItalic;
        private System.Windows.Forms.Button buttonGetTesseractDictionaries;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInspectNOcrMatches;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxNOcrLanguage;
        private System.Windows.Forms.Button buttonLineOcrEditLanguage;
        private System.Windows.Forms.Button buttonLineOcrNewLanguage;
        private System.Windows.Forms.Button buttonSpellCheckDownload;
        private System.Windows.Forms.Timer timerHideStatus;
        private System.Windows.Forms.ToolStripMenuItem dOSTToolStripMenuItem;
        private System.Windows.Forms.Label labelMaxErrorPercent;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxErrorPct;
        private System.Windows.Forms.GroupBox groupBoxTransportStream;
        private System.Windows.Forms.CheckBox checkBoxTransportStreamGrayscale;
        private System.Windows.Forms.CheckBox checkBoxTransportStreamGetColorAndSplit;
        private System.Windows.Forms.NumericUpDown numericUpDownAutoTransparentAlphaMax;
        private System.Windows.Forms.Label labelMinAlpha;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripImage;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImageSaveAs;
        private System.Windows.Forms.ToolStripMenuItem nOcrTrainingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importNewTimeCodesToolStripMenuItem;
    }
}