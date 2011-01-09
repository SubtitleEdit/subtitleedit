using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class VobSubOcr
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
            this.pictureBoxSubtitleImage = new System.Windows.Forms.PictureBox();
            this.contextMenuStripListview = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.normalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.italicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveImageAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelSubtitleText = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxOcrMethod = new System.Windows.Forms.GroupBox();
            this.comboBoxOcrMethod = new System.Windows.Forms.ComboBox();
            this.GroupBoxTesseractMethod = new System.Windows.Forms.GroupBox();
            this.checkBoxUseModiInTesseractForUnknownWords = new System.Windows.Forms.CheckBox();
            this.labelTesseractLanguage = new System.Windows.Forms.Label();
            this.comboBoxTesseractLanguages = new System.Windows.Forms.ComboBox();
            this.groupBoxModiMethod = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxModiLanguage = new System.Windows.Forms.ComboBox();
            this.groupBoxImageCompareMethod = new System.Windows.Forms.GroupBox();
            this.checkBoxRightToLeft = new System.Windows.Forms.CheckBox();
            this.numericUpDownPixelsIsSpace = new System.Windows.Forms.NumericUpDown();
            this.buttonEditCharacterDatabase = new System.Windows.Forms.Button();
            this.labelNoOfPixelsIsSpace = new System.Windows.Forms.Label();
            this.comboBoxCharacterDatabase = new System.Windows.Forms.ComboBox();
            this.labelImageDatabase = new System.Windows.Forms.Label();
            this.buttonNewCharacterDatabase = new System.Windows.Forms.Button();
            this.textBoxCurrentText = new System.Windows.Forms.TextBox();
            this.groupBoxOCRControls = new System.Windows.Forms.GroupBox();
            this.labelStartFrom = new System.Windows.Forms.Label();
            this.numericUpDownStartNumber = new System.Windows.Forms.NumericUpDown();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonStartOcr = new System.Windows.Forms.Button();
            this.groupBoxOcrAutoFix = new System.Windows.Forms.GroupBox();
            this.checkBoxGuessUnknownWords = new System.Windows.Forms.CheckBox();
            this.tabControlLogs = new System.Windows.Forms.TabControl();
            this.tabPageAllFixes = new System.Windows.Forms.TabPage();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.tabPageSuggestions = new System.Windows.Forms.TabPage();
            this.listBoxLogSuggestions = new System.Windows.Forms.ListBox();
            this.tabPageUnknownWords = new System.Windows.Forms.TabPage();
            this.listBoxUnknownWords = new System.Windows.Forms.ListBox();
            this.labelFixesMade = new System.Windows.Forms.Label();
            this.checkBoxPromptForUnknownWords = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoBreakLines = new System.Windows.Forms.CheckBox();
            this.labelDictionaryLoaded = new System.Windows.Forms.Label();
            this.checkBoxAutoFixCommonErrors = new System.Windows.Forms.CheckBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBoxImagePalette = new System.Windows.Forms.GroupBox();
            this.checkBoxEmphasis2Transparent = new System.Windows.Forms.CheckBox();
            this.checkBoxEmphasis1Transparent = new System.Windows.Forms.CheckBox();
            this.checkBoxPatternTransparent = new System.Windows.Forms.CheckBox();
            this.pictureBoxEmphasis2 = new System.Windows.Forms.PictureBox();
            this.pictureBoxEmphasis1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxPattern = new System.Windows.Forms.PictureBox();
            this.checkBoxCustomFourColors = new System.Windows.Forms.CheckBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.groupBoxSubtitleImage = new System.Windows.Forms.GroupBox();
            this.checkBoxShowOnlyForced = new System.Windows.Forms.CheckBox();
            this.checkBoxUseTimeCodesFromIdx = new System.Windows.Forms.CheckBox();
            this.subtitleListView1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.saveAllImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).BeginInit();
            this.contextMenuStripListview.SuspendLayout();
            this.groupBoxOcrMethod.SuspendLayout();
            this.GroupBoxTesseractMethod.SuspendLayout();
            this.groupBoxModiMethod.SuspendLayout();
            this.groupBoxImageCompareMethod.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPixelsIsSpace)).BeginInit();
            this.groupBoxOCRControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStartNumber)).BeginInit();
            this.groupBoxOcrAutoFix.SuspendLayout();
            this.tabControlLogs.SuspendLayout();
            this.tabPageAllFixes.SuspendLayout();
            this.tabPageSuggestions.SuspendLayout();
            this.tabPageUnknownWords.SuspendLayout();
            this.groupBoxImagePalette.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEmphasis2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEmphasis1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPattern)).BeginInit();
            this.groupBoxSubtitleImage.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxSubtitleImage
            // 
            this.pictureBoxSubtitleImage.ContextMenuStrip = this.contextMenuStripListview;
            this.pictureBoxSubtitleImage.Location = new System.Drawing.Point(13, 58);
            this.pictureBoxSubtitleImage.Name = "pictureBoxSubtitleImage";
            this.pictureBoxSubtitleImage.Size = new System.Drawing.Size(550, 127);
            this.pictureBoxSubtitleImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxSubtitleImage.TabIndex = 3;
            this.pictureBoxSubtitleImage.TabStop = false;
            // 
            // contextMenuStripListview
            // 
            this.contextMenuStripListview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.normalToolStripMenuItem,
            this.italicToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveImageAsToolStripMenuItem,
            this.saveAllImagesToolStripMenuItem});
            this.contextMenuStripListview.Name = "contextMenuStripListview";
            this.contextMenuStripListview.Size = new System.Drawing.Size(164, 120);
            this.contextMenuStripListview.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripListviewOpening);
            // 
            // normalToolStripMenuItem
            // 
            this.normalToolStripMenuItem.Name = "normalToolStripMenuItem";
            this.normalToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.normalToolStripMenuItem.Text = "Normal";
            this.normalToolStripMenuItem.Click += new System.EventHandler(this.NormalToolStripMenuItemClick);
            // 
            // italicToolStripMenuItem
            // 
            this.italicToolStripMenuItem.Name = "italicToolStripMenuItem";
            this.italicToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.italicToolStripMenuItem.Text = "Italic";
            this.italicToolStripMenuItem.Click += new System.EventHandler(this.ItalicToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(154, 6);
            // 
            // saveImageAsToolStripMenuItem
            // 
            this.saveImageAsToolStripMenuItem.Name = "saveImageAsToolStripMenuItem";
            this.saveImageAsToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.saveImageAsToolStripMenuItem.Text = "Save image as...";
            this.saveImageAsToolStripMenuItem.Click += new System.EventHandler(this.SaveImageAsToolStripMenuItemClick);
            // 
            // labelSubtitleText
            // 
            this.labelSubtitleText.AutoSize = true;
            this.labelSubtitleText.Location = new System.Drawing.Point(12, 207);
            this.labelSubtitleText.Name = "labelSubtitleText";
            this.labelSubtitleText.Size = new System.Drawing.Size(66, 13);
            this.labelSubtitleText.TabIndex = 6;
            this.labelSubtitleText.Text = "Subtitle text";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 552);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(827, 10);
            this.progressBar1.TabIndex = 7;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 535);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(131, 13);
            this.labelStatus.TabIndex = 8;
            this.labelStatus.Text = "Loading VobSub images...";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(845, 541);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(926, 541);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // groupBoxOcrMethod
            // 
            this.groupBoxOcrMethod.Controls.Add(this.comboBoxOcrMethod);
            this.groupBoxOcrMethod.Controls.Add(this.GroupBoxTesseractMethod);
            this.groupBoxOcrMethod.Controls.Add(this.groupBoxModiMethod);
            this.groupBoxOcrMethod.Controls.Add(this.groupBoxImageCompareMethod);
            this.groupBoxOcrMethod.Location = new System.Drawing.Point(13, 12);
            this.groupBoxOcrMethod.Name = "groupBoxOcrMethod";
            this.groupBoxOcrMethod.Size = new System.Drawing.Size(392, 192);
            this.groupBoxOcrMethod.TabIndex = 13;
            this.groupBoxOcrMethod.TabStop = false;
            this.groupBoxOcrMethod.Text = "OCR method";
            // 
            // comboBoxOcrMethod
            // 
            this.comboBoxOcrMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOcrMethod.FormattingEnabled = true;
            this.comboBoxOcrMethod.Items.AddRange(new object[] {
            "OCR via tesseract",
            "OCR via image compare",
            "OCR via Microsoftr MODI"});
            this.comboBoxOcrMethod.Location = new System.Drawing.Point(13, 16);
            this.comboBoxOcrMethod.Name = "comboBoxOcrMethod";
            this.comboBoxOcrMethod.Size = new System.Drawing.Size(366, 21);
            this.comboBoxOcrMethod.TabIndex = 33;
            this.comboBoxOcrMethod.SelectedIndexChanged += new System.EventHandler(this.ComboBoxOcrMethodSelectedIndexChanged);
            // 
            // GroupBoxTesseractMethod
            // 
            this.GroupBoxTesseractMethod.Controls.Add(this.checkBoxUseModiInTesseractForUnknownWords);
            this.GroupBoxTesseractMethod.Controls.Add(this.labelTesseractLanguage);
            this.GroupBoxTesseractMethod.Controls.Add(this.comboBoxTesseractLanguages);
            this.GroupBoxTesseractMethod.Location = new System.Drawing.Point(13, 31);
            this.GroupBoxTesseractMethod.Name = "GroupBoxTesseractMethod";
            this.GroupBoxTesseractMethod.Size = new System.Drawing.Size(366, 131);
            this.GroupBoxTesseractMethod.TabIndex = 36;
            this.GroupBoxTesseractMethod.TabStop = false;
            this.GroupBoxTesseractMethod.Text = "Tesseract";
            // 
            // checkBoxUseModiInTesseractForUnknownWords
            // 
            this.checkBoxUseModiInTesseractForUnknownWords.AutoSize = true;
            this.checkBoxUseModiInTesseractForUnknownWords.Checked = true;
            this.checkBoxUseModiInTesseractForUnknownWords.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxUseModiInTesseractForUnknownWords.Enabled = false;
            this.checkBoxUseModiInTesseractForUnknownWords.Location = new System.Drawing.Point(22, 74);
            this.checkBoxUseModiInTesseractForUnknownWords.Name = "checkBoxUseModiInTesseractForUnknownWords";
            this.checkBoxUseModiInTesseractForUnknownWords.Size = new System.Drawing.Size(165, 17);
            this.checkBoxUseModiInTesseractForUnknownWords.TabIndex = 39;
            this.checkBoxUseModiInTesseractForUnknownWords.Text = "Try MODI for unknown words";
            this.checkBoxUseModiInTesseractForUnknownWords.UseVisualStyleBackColor = true;
            // 
            // labelTesseractLanguage
            // 
            this.labelTesseractLanguage.AutoSize = true;
            this.labelTesseractLanguage.Location = new System.Drawing.Point(18, 34);
            this.labelTesseractLanguage.Name = "labelTesseractLanguage";
            this.labelTesseractLanguage.Size = new System.Drawing.Size(54, 13);
            this.labelTesseractLanguage.TabIndex = 4;
            this.labelTesseractLanguage.Text = "Language";
            // 
            // comboBoxTesseractLanguages
            // 
            this.comboBoxTesseractLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTesseractLanguages.FormattingEnabled = true;
            this.comboBoxTesseractLanguages.Location = new System.Drawing.Point(79, 31);
            this.comboBoxTesseractLanguages.Name = "comboBoxTesseractLanguages";
            this.comboBoxTesseractLanguages.Size = new System.Drawing.Size(195, 21);
            this.comboBoxTesseractLanguages.TabIndex = 34;
            this.comboBoxTesseractLanguages.SelectedIndexChanged += new System.EventHandler(this.ComboBoxTesseractLanguagesSelectedIndexChanged);
            // 
            // groupBoxModiMethod
            // 
            this.groupBoxModiMethod.Controls.Add(this.label1);
            this.groupBoxModiMethod.Controls.Add(this.comboBoxModiLanguage);
            this.groupBoxModiMethod.Location = new System.Drawing.Point(7, 50);
            this.groupBoxModiMethod.Name = "groupBoxModiMethod";
            this.groupBoxModiMethod.Size = new System.Drawing.Size(366, 131);
            this.groupBoxModiMethod.TabIndex = 35;
            this.groupBoxModiMethod.TabStop = false;
            this.groupBoxModiMethod.Text = "MODI";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 33;
            this.label1.Text = "Language";
            // 
            // comboBoxModiLanguage
            // 
            this.comboBoxModiLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxModiLanguage.FormattingEnabled = true;
            this.comboBoxModiLanguage.Location = new System.Drawing.Point(79, 33);
            this.comboBoxModiLanguage.Name = "comboBoxModiLanguage";
            this.comboBoxModiLanguage.Size = new System.Drawing.Size(195, 21);
            this.comboBoxModiLanguage.TabIndex = 9;
            this.comboBoxModiLanguage.SelectedIndexChanged += new System.EventHandler(this.ComboBoxModiLanguageSelectedIndexChanged);
            // 
            // groupBoxImageCompareMethod
            // 
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
            this.groupBoxImageCompareMethod.TabIndex = 35;
            this.groupBoxImageCompareMethod.TabStop = false;
            this.groupBoxImageCompareMethod.Text = "Image compare";
            // 
            // checkBoxRightToLeft
            // 
            this.checkBoxRightToLeft.AutoSize = true;
            this.checkBoxRightToLeft.Location = new System.Drawing.Point(128, 112);
            this.checkBoxRightToLeft.Name = "checkBoxRightToLeft";
            this.checkBoxRightToLeft.Size = new System.Drawing.Size(80, 17);
            this.checkBoxRightToLeft.TabIndex = 40;
            this.checkBoxRightToLeft.Text = "Right to left";
            this.checkBoxRightToLeft.UseVisualStyleBackColor = true;
            // 
            // numericUpDownPixelsIsSpace
            // 
            this.numericUpDownPixelsIsSpace.Location = new System.Drawing.Point(128, 86);
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
            this.numericUpDownPixelsIsSpace.TabIndex = 35;
            this.numericUpDownPixelsIsSpace.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonEditCharacterDatabase
            // 
            this.buttonEditCharacterDatabase.Location = new System.Drawing.Point(204, 57);
            this.buttonEditCharacterDatabase.Name = "buttonEditCharacterDatabase";
            this.buttonEditCharacterDatabase.Size = new System.Drawing.Size(68, 21);
            this.buttonEditCharacterDatabase.TabIndex = 33;
            this.buttonEditCharacterDatabase.Text = "Edit";
            this.buttonEditCharacterDatabase.UseVisualStyleBackColor = true;
            this.buttonEditCharacterDatabase.Click += new System.EventHandler(this.ButtonEditCharacterDatabaseClick);
            // 
            // labelNoOfPixelsIsSpace
            // 
            this.labelNoOfPixelsIsSpace.AutoSize = true;
            this.labelNoOfPixelsIsSpace.Location = new System.Drawing.Point(18, 90);
            this.labelNoOfPixelsIsSpace.Name = "labelNoOfPixelsIsSpace";
            this.labelNoOfPixelsIsSpace.Size = new System.Drawing.Size(104, 13);
            this.labelNoOfPixelsIsSpace.TabIndex = 34;
            this.labelNoOfPixelsIsSpace.Text = "No of pixels is space";
            // 
            // comboBoxCharacterDatabase
            // 
            this.comboBoxCharacterDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCharacterDatabase.FormattingEnabled = true;
            this.comboBoxCharacterDatabase.Location = new System.Drawing.Point(130, 30);
            this.comboBoxCharacterDatabase.Name = "comboBoxCharacterDatabase";
            this.comboBoxCharacterDatabase.Size = new System.Drawing.Size(167, 21);
            this.comboBoxCharacterDatabase.TabIndex = 31;
            this.comboBoxCharacterDatabase.SelectedIndexChanged += new System.EventHandler(this.ComboBoxCharacterDatabaseSelectedIndexChanged);
            // 
            // labelImageDatabase
            // 
            this.labelImageDatabase.AutoSize = true;
            this.labelImageDatabase.Location = new System.Drawing.Point(41, 33);
            this.labelImageDatabase.Name = "labelImageDatabase";
            this.labelImageDatabase.Size = new System.Drawing.Size(85, 13);
            this.labelImageDatabase.TabIndex = 30;
            this.labelImageDatabase.Text = "Image database";
            // 
            // buttonNewCharacterDatabase
            // 
            this.buttonNewCharacterDatabase.Location = new System.Drawing.Point(130, 57);
            this.buttonNewCharacterDatabase.Name = "buttonNewCharacterDatabase";
            this.buttonNewCharacterDatabase.Size = new System.Drawing.Size(68, 21);
            this.buttonNewCharacterDatabase.TabIndex = 32;
            this.buttonNewCharacterDatabase.Text = "New";
            this.buttonNewCharacterDatabase.UseVisualStyleBackColor = true;
            this.buttonNewCharacterDatabase.Click += new System.EventHandler(this.ButtonNewCharacterDatabaseClick);
            // 
            // textBoxCurrentText
            // 
            this.textBoxCurrentText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxCurrentText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxCurrentText.Location = new System.Drawing.Point(12, 419);
            this.textBoxCurrentText.Multiline = true;
            this.textBoxCurrentText.Name = "textBoxCurrentText";
            this.textBoxCurrentText.Size = new System.Drawing.Size(390, 77);
            this.textBoxCurrentText.TabIndex = 5;
            this.textBoxCurrentText.TextChanged += new System.EventHandler(this.TextBoxCurrentTextTextChanged);
            // 
            // groupBoxOCRControls
            // 
            this.groupBoxOCRControls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxOCRControls.Controls.Add(this.labelStartFrom);
            this.groupBoxOCRControls.Controls.Add(this.numericUpDownStartNumber);
            this.groupBoxOCRControls.Controls.Add(this.buttonStop);
            this.groupBoxOCRControls.Controls.Add(this.buttonStartOcr);
            this.groupBoxOCRControls.Location = new System.Drawing.Point(412, 416);
            this.groupBoxOCRControls.Name = "groupBoxOCRControls";
            this.groupBoxOCRControls.Size = new System.Drawing.Size(287, 84);
            this.groupBoxOCRControls.TabIndex = 28;
            this.groupBoxOCRControls.TabStop = false;
            this.groupBoxOCRControls.Text = "OCR Start/stop";
            // 
            // labelStartFrom
            // 
            this.labelStartFrom.AutoSize = true;
            this.labelStartFrom.Location = new System.Drawing.Point(124, 14);
            this.labelStartFrom.Name = "labelStartFrom";
            this.labelStartFrom.Size = new System.Drawing.Size(127, 13);
            this.labelStartFrom.TabIndex = 31;
            this.labelStartFrom.Text = "Start OCR from subtitle#";
            // 
            // numericUpDownStartNumber
            // 
            this.numericUpDownStartNumber.Location = new System.Drawing.Point(127, 31);
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
            this.numericUpDownStartNumber.TabIndex = 30;
            this.numericUpDownStartNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonStop
            // 
            this.buttonStop.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonStop.Location = new System.Drawing.Point(13, 58);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(105, 22);
            this.buttonStop.TabIndex = 29;
            this.buttonStop.Text = "Stop OCR";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.ButtonStopClick);
            // 
            // buttonStartOcr
            // 
            this.buttonStartOcr.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStartOcr.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonStartOcr.Location = new System.Drawing.Point(13, 30);
            this.buttonStartOcr.Name = "buttonStartOcr";
            this.buttonStartOcr.Size = new System.Drawing.Size(105, 22);
            this.buttonStartOcr.TabIndex = 28;
            this.buttonStartOcr.Text = "Start OCR";
            this.buttonStartOcr.UseVisualStyleBackColor = true;
            this.buttonStartOcr.Click += new System.EventHandler(this.ButtonStartOcrClick);
            // 
            // groupBoxOcrAutoFix
            // 
            this.groupBoxOcrAutoFix.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOcrAutoFix.Controls.Add(this.checkBoxGuessUnknownWords);
            this.groupBoxOcrAutoFix.Controls.Add(this.tabControlLogs);
            this.groupBoxOcrAutoFix.Controls.Add(this.labelFixesMade);
            this.groupBoxOcrAutoFix.Controls.Add(this.checkBoxPromptForUnknownWords);
            this.groupBoxOcrAutoFix.Controls.Add(this.checkBoxAutoBreakLines);
            this.groupBoxOcrAutoFix.Controls.Add(this.labelDictionaryLoaded);
            this.groupBoxOcrAutoFix.Controls.Add(this.checkBoxAutoFixCommonErrors);
            this.groupBoxOcrAutoFix.Location = new System.Drawing.Point(705, 221);
            this.groupBoxOcrAutoFix.Name = "groupBoxOcrAutoFix";
            this.groupBoxOcrAutoFix.Size = new System.Drawing.Size(306, 318);
            this.groupBoxOcrAutoFix.TabIndex = 34;
            this.groupBoxOcrAutoFix.TabStop = false;
            this.groupBoxOcrAutoFix.Text = "OCR auto correction / spellchecking";
            // 
            // checkBoxGuessUnknownWords
            // 
            this.checkBoxGuessUnknownWords.AutoSize = true;
            this.checkBoxGuessUnknownWords.Checked = true;
            this.checkBoxGuessUnknownWords.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxGuessUnknownWords.Location = new System.Drawing.Point(11, 83);
            this.checkBoxGuessUnknownWords.Name = "checkBoxGuessUnknownWords";
            this.checkBoxGuessUnknownWords.Size = new System.Drawing.Size(162, 17);
            this.checkBoxGuessUnknownWords.TabIndex = 39;
            this.checkBoxGuessUnknownWords.Text = "Try to guess unknown words";
            this.checkBoxGuessUnknownWords.UseVisualStyleBackColor = true;
            // 
            // tabControlLogs
            // 
            this.tabControlLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlLogs.Controls.Add(this.tabPageAllFixes);
            this.tabControlLogs.Controls.Add(this.tabPageSuggestions);
            this.tabControlLogs.Controls.Add(this.tabPageUnknownWords);
            this.tabControlLogs.Location = new System.Drawing.Point(11, 128);
            this.tabControlLogs.Name = "tabControlLogs";
            this.tabControlLogs.SelectedIndex = 0;
            this.tabControlLogs.Size = new System.Drawing.Size(289, 184);
            this.tabControlLogs.TabIndex = 35;
            // 
            // tabPageAllFixes
            // 
            this.tabPageAllFixes.Controls.Add(this.listBoxLog);
            this.tabPageAllFixes.Location = new System.Drawing.Point(4, 22);
            this.tabPageAllFixes.Name = "tabPageAllFixes";
            this.tabPageAllFixes.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAllFixes.Size = new System.Drawing.Size(281, 158);
            this.tabPageAllFixes.TabIndex = 0;
            this.tabPageAllFixes.Text = "All fixes";
            this.tabPageAllFixes.UseVisualStyleBackColor = true;
            // 
            // listBoxLog
            // 
            this.listBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxLog.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.Location = new System.Drawing.Point(5, 6);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(270, 95);
            this.listBoxLog.TabIndex = 39;
            this.listBoxLog.SelectedIndexChanged += new System.EventHandler(this.ListBoxLogSelectedIndexChanged);
            // 
            // tabPageSuggestions
            // 
            this.tabPageSuggestions.Controls.Add(this.listBoxLogSuggestions);
            this.tabPageSuggestions.Location = new System.Drawing.Point(4, 22);
            this.tabPageSuggestions.Name = "tabPageSuggestions";
            this.tabPageSuggestions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSuggestions.Size = new System.Drawing.Size(281, 158);
            this.tabPageSuggestions.TabIndex = 1;
            this.tabPageSuggestions.Text = "Guesses used";
            this.tabPageSuggestions.UseVisualStyleBackColor = true;
            // 
            // listBoxLogSuggestions
            // 
            this.listBoxLogSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxLogSuggestions.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxLogSuggestions.FormattingEnabled = true;
            this.listBoxLogSuggestions.Location = new System.Drawing.Point(5, 6);
            this.listBoxLogSuggestions.Name = "listBoxLogSuggestions";
            this.listBoxLogSuggestions.Size = new System.Drawing.Size(244, 82);
            this.listBoxLogSuggestions.TabIndex = 40;
            this.listBoxLogSuggestions.SelectedIndexChanged += new System.EventHandler(this.ListBoxLogSelectedIndexChanged);
            // 
            // tabPageUnknownWords
            // 
            this.tabPageUnknownWords.Controls.Add(this.listBoxUnknownWords);
            this.tabPageUnknownWords.Location = new System.Drawing.Point(4, 22);
            this.tabPageUnknownWords.Name = "tabPageUnknownWords";
            this.tabPageUnknownWords.Size = new System.Drawing.Size(281, 158);
            this.tabPageUnknownWords.TabIndex = 2;
            this.tabPageUnknownWords.Text = "Unknown words";
            this.tabPageUnknownWords.UseVisualStyleBackColor = true;
            // 
            // listBoxUnknownWords
            // 
            this.listBoxUnknownWords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxUnknownWords.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxUnknownWords.FormattingEnabled = true;
            this.listBoxUnknownWords.Location = new System.Drawing.Point(5, 6);
            this.listBoxUnknownWords.Name = "listBoxUnknownWords";
            this.listBoxUnknownWords.Size = new System.Drawing.Size(244, 82);
            this.listBoxUnknownWords.TabIndex = 40;
            this.listBoxUnknownWords.SelectedIndexChanged += new System.EventHandler(this.ListBoxLogSelectedIndexChanged);
            // 
            // labelFixesMade
            // 
            this.labelFixesMade.AutoSize = true;
            this.labelFixesMade.Location = new System.Drawing.Point(144, 40);
            this.labelFixesMade.Name = "labelFixesMade";
            this.labelFixesMade.Size = new System.Drawing.Size(98, 13);
            this.labelFixesMade.TabIndex = 35;
            this.labelFixesMade.Text = "NumberOfOcrFixes";
            // 
            // checkBoxPromptForUnknownWords
            // 
            this.checkBoxPromptForUnknownWords.AutoSize = true;
            this.checkBoxPromptForUnknownWords.Checked = true;
            this.checkBoxPromptForUnknownWords.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPromptForUnknownWords.Location = new System.Drawing.Point(11, 61);
            this.checkBoxPromptForUnknownWords.Name = "checkBoxPromptForUnknownWords";
            this.checkBoxPromptForUnknownWords.Size = new System.Drawing.Size(246, 17);
            this.checkBoxPromptForUnknownWords.TabIndex = 38;
            this.checkBoxPromptForUnknownWords.Text = "Prompt for unknown words (requires dictionary)";
            this.checkBoxPromptForUnknownWords.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoBreakLines
            // 
            this.checkBoxAutoBreakLines.AutoSize = true;
            this.checkBoxAutoBreakLines.Checked = true;
            this.checkBoxAutoBreakLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoBreakLines.Location = new System.Drawing.Point(11, 105);
            this.checkBoxAutoBreakLines.Name = "checkBoxAutoBreakLines";
            this.checkBoxAutoBreakLines.Size = new System.Drawing.Size(200, 17);
            this.checkBoxAutoBreakLines.TabIndex = 37;
            this.checkBoxAutoBreakLines.Text = "Auto break subtitle, if line number > 2";
            this.checkBoxAutoBreakLines.UseVisualStyleBackColor = true;
            // 
            // labelDictionaryLoaded
            // 
            this.labelDictionaryLoaded.AutoSize = true;
            this.labelDictionaryLoaded.Location = new System.Drawing.Point(11, 19);
            this.labelDictionaryLoaded.Name = "labelDictionaryLoaded";
            this.labelDictionaryLoaded.Size = new System.Drawing.Size(112, 13);
            this.labelDictionaryLoaded.TabIndex = 36;
            this.labelDictionaryLoaded.Text = "labelDictionaryLoaded";
            // 
            // checkBoxAutoFixCommonErrors
            // 
            this.checkBoxAutoFixCommonErrors.AutoSize = true;
            this.checkBoxAutoFixCommonErrors.Checked = true;
            this.checkBoxAutoFixCommonErrors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoFixCommonErrors.Location = new System.Drawing.Point(11, 39);
            this.checkBoxAutoFixCommonErrors.Name = "checkBoxAutoFixCommonErrors";
            this.checkBoxAutoFixCommonErrors.Size = new System.Drawing.Size(137, 17);
            this.checkBoxAutoFixCommonErrors.TabIndex = 34;
            this.checkBoxAutoFixCommonErrors.Text = "Fix common OCR errors";
            this.checkBoxAutoFixCommonErrors.UseVisualStyleBackColor = true;
            // 
            // groupBoxImagePalette
            // 
            this.groupBoxImagePalette.Controls.Add(this.checkBoxEmphasis2Transparent);
            this.groupBoxImagePalette.Controls.Add(this.checkBoxEmphasis1Transparent);
            this.groupBoxImagePalette.Controls.Add(this.checkBoxPatternTransparent);
            this.groupBoxImagePalette.Controls.Add(this.pictureBoxEmphasis2);
            this.groupBoxImagePalette.Controls.Add(this.pictureBoxEmphasis1);
            this.groupBoxImagePalette.Controls.Add(this.pictureBoxPattern);
            this.groupBoxImagePalette.Controls.Add(this.checkBoxCustomFourColors);
            this.groupBoxImagePalette.Location = new System.Drawing.Point(13, 16);
            this.groupBoxImagePalette.Name = "groupBoxImagePalette";
            this.groupBoxImagePalette.Size = new System.Drawing.Size(550, 37);
            this.groupBoxImagePalette.TabIndex = 35;
            this.groupBoxImagePalette.TabStop = false;
            this.groupBoxImagePalette.Text = "Image palette";
            // 
            // checkBoxEmphasis2Transparent
            // 
            this.checkBoxEmphasis2Transparent.AutoSize = true;
            this.checkBoxEmphasis2Transparent.Location = new System.Drawing.Point(437, 19);
            this.checkBoxEmphasis2Transparent.Name = "checkBoxEmphasis2Transparent";
            this.checkBoxEmphasis2Transparent.Size = new System.Drawing.Size(83, 17);
            this.checkBoxEmphasis2Transparent.TabIndex = 6;
            this.checkBoxEmphasis2Transparent.Text = "Transparent";
            this.checkBoxEmphasis2Transparent.UseVisualStyleBackColor = true;
            this.checkBoxEmphasis2Transparent.CheckedChanged += new System.EventHandler(this.CheckBoxEmphasis2TransparentCheckedChanged);
            // 
            // checkBoxEmphasis1Transparent
            // 
            this.checkBoxEmphasis1Transparent.AutoSize = true;
            this.checkBoxEmphasis1Transparent.Location = new System.Drawing.Point(304, 19);
            this.checkBoxEmphasis1Transparent.Name = "checkBoxEmphasis1Transparent";
            this.checkBoxEmphasis1Transparent.Size = new System.Drawing.Size(83, 17);
            this.checkBoxEmphasis1Transparent.TabIndex = 5;
            this.checkBoxEmphasis1Transparent.Text = "Transparent";
            this.checkBoxEmphasis1Transparent.UseVisualStyleBackColor = true;
            this.checkBoxEmphasis1Transparent.CheckedChanged += new System.EventHandler(this.CheckBoxEmphasis1TransparentCheckedChanged);
            // 
            // checkBoxPatternTransparent
            // 
            this.checkBoxPatternTransparent.AutoSize = true;
            this.checkBoxPatternTransparent.Location = new System.Drawing.Point(167, 19);
            this.checkBoxPatternTransparent.Name = "checkBoxPatternTransparent";
            this.checkBoxPatternTransparent.Size = new System.Drawing.Size(83, 17);
            this.checkBoxPatternTransparent.TabIndex = 4;
            this.checkBoxPatternTransparent.Text = "Transparent";
            this.checkBoxPatternTransparent.UseVisualStyleBackColor = true;
            this.checkBoxPatternTransparent.CheckedChanged += new System.EventHandler(this.CheckBoxPatternTransparentCheckedChanged);
            // 
            // pictureBoxEmphasis2
            // 
            this.pictureBoxEmphasis2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxEmphasis2.Location = new System.Drawing.Point(414, 12);
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
            this.pictureBoxEmphasis1.Location = new System.Drawing.Point(280, 12);
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
            this.pictureBoxPattern.Location = new System.Drawing.Point(143, 12);
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
            this.groupBoxSubtitleImage.Controls.Add(this.groupBoxImagePalette);
            this.groupBoxSubtitleImage.Controls.Add(this.pictureBoxSubtitleImage);
            this.groupBoxSubtitleImage.Location = new System.Drawing.Point(412, 13);
            this.groupBoxSubtitleImage.Name = "groupBoxSubtitleImage";
            this.groupBoxSubtitleImage.Size = new System.Drawing.Size(573, 191);
            this.groupBoxSubtitleImage.TabIndex = 36;
            this.groupBoxSubtitleImage.TabStop = false;
            this.groupBoxSubtitleImage.Text = "Subtitle image";
            // 
            // checkBoxShowOnlyForced
            // 
            this.checkBoxShowOnlyForced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxShowOnlyForced.AutoSize = true;
            this.checkBoxShowOnlyForced.Location = new System.Drawing.Point(414, 513);
            this.checkBoxShowOnlyForced.Name = "checkBoxShowOnlyForced";
            this.checkBoxShowOnlyForced.Size = new System.Drawing.Size(152, 17);
            this.checkBoxShowOnlyForced.TabIndex = 37;
            this.checkBoxShowOnlyForced.Text = "Show only forced subtitles";
            this.checkBoxShowOnlyForced.UseVisualStyleBackColor = true;
            this.checkBoxShowOnlyForced.CheckedChanged += new System.EventHandler(this.checkBoxShowOnlyForced_CheckedChanged);
            // 
            // checkBoxUseTimeCodesFromIdx
            // 
            this.checkBoxUseTimeCodesFromIdx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxUseTimeCodesFromIdx.AutoSize = true;
            this.checkBoxUseTimeCodesFromIdx.Location = new System.Drawing.Point(414, 530);
            this.checkBoxUseTimeCodesFromIdx.Name = "checkBoxUseTimeCodesFromIdx";
            this.checkBoxUseTimeCodesFromIdx.Size = new System.Drawing.Size(186, 17);
            this.checkBoxUseTimeCodesFromIdx.TabIndex = 38;
            this.checkBoxUseTimeCodesFromIdx.Text = "Use lines/time codes from .idx file";
            this.checkBoxUseTimeCodesFromIdx.UseVisualStyleBackColor = true;
            this.checkBoxUseTimeCodesFromIdx.CheckedChanged += new System.EventHandler(this.checkBoxUseTimeCodesFromIdx_CheckedChanged);
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.subtitleListView1.ContextMenuStrip = this.contextMenuStripListview;
            this.subtitleListView1.FirstVisibleIndex = -1;
            this.subtitleListView1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.HideSelection = false;
            this.subtitleListView1.Location = new System.Drawing.Point(13, 221);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.Size = new System.Drawing.Size(686, 194);
            this.subtitleListView1.TabIndex = 4;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            this.subtitleListView1.SelectedIndexChanged += new System.EventHandler(this.SubtitleListView1SelectedIndexChanged);
            // 
            // saveAllImagesToolStripMenuItem
            // 
            this.saveAllImagesToolStripMenuItem.Name = "saveAllImagesToolStripMenuItem";
            this.saveAllImagesToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.saveAllImagesToolStripMenuItem.Text = "Save all images...";
            this.saveAllImagesToolStripMenuItem.Click += new System.EventHandler(this.saveAllImagesToolStripMenuItem_Click);
            // 
            // VobSubOcr
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1023, 568);
            this.Controls.Add(this.checkBoxShowOnlyForced);
            this.Controls.Add(this.checkBoxUseTimeCodesFromIdx);
            this.Controls.Add(this.groupBoxSubtitleImage);
            this.Controls.Add(this.textBoxCurrentText);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelSubtitleText);
            this.Controls.Add(this.groupBoxOcrAutoFix);
            this.Controls.Add(this.groupBoxOCRControls);
            this.Controls.Add(this.subtitleListView1);
            this.Controls.Add(this.groupBoxOcrMethod);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1013, 578);
            this.Name = "VobSubOcr";
            this.ShowIcon = false;
            this.Text = "Import/OCR VobSub (sub/idx) subtitle";
            this.Shown += new System.EventHandler(this.FormVobSubOcr_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VobSubOcr_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).EndInit();
            this.contextMenuStripListview.ResumeLayout(false);
            this.groupBoxOcrMethod.ResumeLayout(false);
            this.GroupBoxTesseractMethod.ResumeLayout(false);
            this.GroupBoxTesseractMethod.PerformLayout();
            this.groupBoxModiMethod.ResumeLayout(false);
            this.groupBoxModiMethod.PerformLayout();
            this.groupBoxImageCompareMethod.ResumeLayout(false);
            this.groupBoxImageCompareMethod.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPixelsIsSpace)).EndInit();
            this.groupBoxOCRControls.ResumeLayout(false);
            this.groupBoxOCRControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStartNumber)).EndInit();
            this.groupBoxOcrAutoFix.ResumeLayout(false);
            this.groupBoxOcrAutoFix.PerformLayout();
            this.tabControlLogs.ResumeLayout(false);
            this.tabPageAllFixes.ResumeLayout(false);
            this.tabPageSuggestions.ResumeLayout(false);
            this.tabPageUnknownWords.ResumeLayout(false);
            this.groupBoxImagePalette.ResumeLayout(false);
            this.groupBoxImagePalette.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEmphasis2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEmphasis1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPattern)).EndInit();
            this.groupBoxSubtitleImage.ResumeLayout(false);
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
        private System.Windows.Forms.TextBox textBoxCurrentText;
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
        private System.Windows.Forms.ToolStripMenuItem saveAllImagesToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}