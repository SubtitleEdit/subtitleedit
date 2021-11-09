namespace Nikse.SubtitleEdit.Forms.Ocr
{
    sealed partial class VobSubNOcrEdit
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
            this.labelImageCompareFiles = new System.Windows.Forms.Label();
            this.labelChooseCharacters = new System.Windows.Forms.Label();
            this.listBoxFileNames = new System.Windows.Forms.ListBox();
            this.comboBoxTexts = new System.Windows.Forms.ComboBox();
            this.groupBoxCurrentCompareImage = new System.Windows.Forms.GroupBox();
            this.labelNOcrCharInfo = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonCold = new System.Windows.Forms.RadioButton();
            this.radioButtonHot = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.listBoxlinesBackground = new System.Windows.Forms.ListBox();
            this.contextMenuStripLinesBackground = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeBackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.labelLines = new System.Windows.Forms.Label();
            this.listBoxLinesForeground = new System.Windows.Forms.ListBox();
            this.contextMenuLinesForeground = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeForegroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonZoomOut = new System.Windows.Forms.Button();
            this.buttonZoomIn = new System.Windows.Forms.Button();
            this.labelCharacters = new System.Windows.Forms.Label();
            this.pictureBoxCharacter = new System.Windows.Forms.PictureBox();
            this.checkBoxItalic = new System.Windows.Forms.CheckBox();
            this.labelTextAssociatedWithImage = new System.Windows.Forms.Label();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.labelInfo = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.contextMenuStripLetters = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.groupBoxCurrentCompareImage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.contextMenuStripLinesBackground.SuspendLayout();
            this.contextMenuLinesForeground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCharacter)).BeginInit();
            this.SuspendLayout();
            // 
            // labelImageCompareFiles
            // 
            this.labelImageCompareFiles.AutoSize = true;
            this.labelImageCompareFiles.Location = new System.Drawing.Point(12, 55);
            this.labelImageCompareFiles.Name = "labelImageCompareFiles";
            this.labelImageCompareFiles.Size = new System.Drawing.Size(76, 13);
            this.labelImageCompareFiles.TabIndex = 6;
            this.labelImageCompareFiles.Text = "Compare items";
            // 
            // labelChooseCharacters
            // 
            this.labelChooseCharacters.AutoSize = true;
            this.labelChooseCharacters.Location = new System.Drawing.Point(12, 9);
            this.labelChooseCharacters.Name = "labelChooseCharacters";
            this.labelChooseCharacters.Size = new System.Drawing.Size(102, 13);
            this.labelChooseCharacters.TabIndex = 4;
            this.labelChooseCharacters.Text = "Choose character(s)";
            // 
            // listBoxFileNames
            // 
            this.listBoxFileNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxFileNames.FormattingEnabled = true;
            this.listBoxFileNames.Location = new System.Drawing.Point(12, 72);
            this.listBoxFileNames.Name = "listBoxFileNames";
            this.listBoxFileNames.Size = new System.Drawing.Size(240, 342);
            this.listBoxFileNames.TabIndex = 7;
            this.listBoxFileNames.SelectedIndexChanged += new System.EventHandler(this.listBoxFileNames_SelectedIndexChanged);
            // 
            // comboBoxTexts
            // 
            this.comboBoxTexts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTexts.FormattingEnabled = true;
            this.comboBoxTexts.Location = new System.Drawing.Point(12, 26);
            this.comboBoxTexts.Name = "comboBoxTexts";
            this.comboBoxTexts.Size = new System.Drawing.Size(240, 21);
            this.comboBoxTexts.TabIndex = 5;
            this.comboBoxTexts.SelectedIndexChanged += new System.EventHandler(this.comboBoxTexts_SelectedIndexChanged);
            // 
            // groupBoxCurrentCompareImage
            // 
            this.groupBoxCurrentCompareImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelNOcrCharInfo);
            this.groupBoxCurrentCompareImage.Controls.Add(this.groupBox2);
            this.groupBoxCurrentCompareImage.Controls.Add(this.label2);
            this.groupBoxCurrentCompareImage.Controls.Add(this.listBoxlinesBackground);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelLines);
            this.groupBoxCurrentCompareImage.Controls.Add(this.listBoxLinesForeground);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonZoomOut);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonZoomIn);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelCharacters);
            this.groupBoxCurrentCompareImage.Controls.Add(this.pictureBoxCharacter);
            this.groupBoxCurrentCompareImage.Controls.Add(this.checkBoxItalic);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelTextAssociatedWithImage);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonDelete);
            this.groupBoxCurrentCompareImage.Controls.Add(this.textBoxText);
            this.groupBoxCurrentCompareImage.Location = new System.Drawing.Point(258, 12);
            this.groupBoxCurrentCompareImage.Name = "groupBoxCurrentCompareImage";
            this.groupBoxCurrentCompareImage.Size = new System.Drawing.Size(442, 434);
            this.groupBoxCurrentCompareImage.TabIndex = 15;
            this.groupBoxCurrentCompareImage.TabStop = false;
            this.groupBoxCurrentCompareImage.Text = "Current compare image";
            // 
            // labelNOcrCharInfo
            // 
            this.labelNOcrCharInfo.AutoSize = true;
            this.labelNOcrCharInfo.Location = new System.Drawing.Point(104, 59);
            this.labelNOcrCharInfo.Name = "labelNOcrCharInfo";
            this.labelNOcrCharInfo.Size = new System.Drawing.Size(94, 13);
            this.labelNOcrCharInfo.TabIndex = 43;
            this.labelNOcrCharInfo.Text = "labelNOcrCharInfo";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButtonCold);
            this.groupBox2.Controls.Add(this.radioButtonHot);
            this.groupBox2.Location = new System.Drawing.Point(6, 89);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(157, 61);
            this.groupBox2.TabIndex = 42;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "New lines are";
            // 
            // radioButtonCold
            // 
            this.radioButtonCold.AutoSize = true;
            this.radioButtonCold.Location = new System.Drawing.Point(6, 40);
            this.radioButtonCold.Name = "radioButtonCold";
            this.radioButtonCold.Size = new System.Drawing.Size(102, 17);
            this.radioButtonCold.TabIndex = 1;
            this.radioButtonCold.Text = "NOT foreground";
            this.radioButtonCold.UseVisualStyleBackColor = true;
            // 
            // radioButtonHot
            // 
            this.radioButtonHot.AutoSize = true;
            this.radioButtonHot.Checked = true;
            this.radioButtonHot.Location = new System.Drawing.Point(6, 17);
            this.radioButtonHot.Name = "radioButtonHot";
            this.radioButtonHot.Size = new System.Drawing.Size(79, 17);
            this.radioButtonHot.TabIndex = 0;
            this.radioButtonHot.TabStop = true;
            this.radioButtonHot.Text = "Foreground";
            this.radioButtonHot.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 285);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 13);
            this.label2.TabIndex = 41;
            this.label2.Text = "Lines - not foreground";
            // 
            // listBoxlinesBackground
            // 
            this.listBoxlinesBackground.ContextMenuStrip = this.contextMenuStripLinesBackground;
            this.listBoxlinesBackground.FormattingEnabled = true;
            this.listBoxlinesBackground.Location = new System.Drawing.Point(12, 301);
            this.listBoxlinesBackground.Name = "listBoxlinesBackground";
            this.listBoxlinesBackground.Size = new System.Drawing.Size(151, 95);
            this.listBoxlinesBackground.TabIndex = 40;
            this.listBoxlinesBackground.SelectedIndexChanged += new System.EventHandler(this.listBoxLinesBackground_SelectedIndexChanged);
            this.listBoxlinesBackground.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxLinesBackground_KeyDown);
            // 
            // contextMenuStripLinesBackground
            // 
            this.contextMenuStripLinesBackground.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeBackToolStripMenuItem,
            this.clearToolStripMenuItem1});
            this.contextMenuStripLinesBackground.Name = "contextMenuStripLines";
            this.contextMenuStripLinesBackground.Size = new System.Drawing.Size(118, 48);
            // 
            // removeBackToolStripMenuItem
            // 
            this.removeBackToolStripMenuItem.Name = "removeBackToolStripMenuItem";
            this.removeBackToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeBackToolStripMenuItem.Text = "Remove";
            this.removeBackToolStripMenuItem.Click += new System.EventHandler(this.removeBackToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem1
            // 
            this.clearToolStripMenuItem1.Name = "clearToolStripMenuItem1";
            this.clearToolStripMenuItem1.Size = new System.Drawing.Size(117, 22);
            this.clearToolStripMenuItem1.Text = "Clear";
            this.clearToolStripMenuItem1.Click += new System.EventHandler(this.clearToolStripMenuItem1_Click);
            // 
            // labelLines
            // 
            this.labelLines.AutoSize = true;
            this.labelLines.Location = new System.Drawing.Point(9, 163);
            this.labelLines.Name = "labelLines";
            this.labelLines.Size = new System.Drawing.Size(92, 13);
            this.labelLines.TabIndex = 39;
            this.labelLines.Text = "Lines - foreground";
            // 
            // listBoxLinesForeground
            // 
            this.listBoxLinesForeground.ContextMenuStrip = this.contextMenuLinesForeground;
            this.listBoxLinesForeground.FormattingEnabled = true;
            this.listBoxLinesForeground.Location = new System.Drawing.Point(12, 179);
            this.listBoxLinesForeground.Name = "listBoxLinesForeground";
            this.listBoxLinesForeground.Size = new System.Drawing.Size(151, 95);
            this.listBoxLinesForeground.TabIndex = 38;
            this.listBoxLinesForeground.SelectedIndexChanged += new System.EventHandler(this.listBoxLinesForeground_SelectedIndexChanged);
            this.listBoxLinesForeground.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxLinesForeground_KeyDown);
            // 
            // contextMenuLinesForeground
            // 
            this.contextMenuLinesForeground.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeForegroundToolStripMenuItem,
            this.clearToolStripMenuItem});
            this.contextMenuLinesForeground.Name = "contextMenuStripLines";
            this.contextMenuLinesForeground.Size = new System.Drawing.Size(118, 48);
            // 
            // removeForegroundToolStripMenuItem
            // 
            this.removeForegroundToolStripMenuItem.Name = "removeForegroundToolStripMenuItem";
            this.removeForegroundToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeForegroundToolStripMenuItem.Text = "Remove";
            this.removeForegroundToolStripMenuItem.Click += new System.EventHandler(this.removeForegroundToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // buttonZoomOut
            // 
            this.buttonZoomOut.Location = new System.Drawing.Point(269, 110);
            this.buttonZoomOut.Name = "buttonZoomOut";
            this.buttonZoomOut.Size = new System.Drawing.Size(25, 23);
            this.buttonZoomOut.TabIndex = 36;
            this.buttonZoomOut.Text = "-";
            this.buttonZoomOut.UseVisualStyleBackColor = true;
            this.buttonZoomOut.Click += new System.EventHandler(this.buttonZoomOut_Click);
            // 
            // buttonZoomIn
            // 
            this.buttonZoomIn.Location = new System.Drawing.Point(300, 110);
            this.buttonZoomIn.Name = "buttonZoomIn";
            this.buttonZoomIn.Size = new System.Drawing.Size(25, 23);
            this.buttonZoomIn.TabIndex = 35;
            this.buttonZoomIn.Text = "+";
            this.buttonZoomIn.UseVisualStyleBackColor = true;
            this.buttonZoomIn.Click += new System.EventHandler(this.buttonZoomIn_Click);
            // 
            // labelCharacters
            // 
            this.labelCharacters.AutoSize = true;
            this.labelCharacters.Location = new System.Drawing.Point(199, 121);
            this.labelCharacters.Name = "labelCharacters";
            this.labelCharacters.Size = new System.Drawing.Size(64, 13);
            this.labelCharacters.TabIndex = 34;
            this.labelCharacters.Text = "Character(s)";
            // 
            // pictureBoxCharacter
            // 
            this.pictureBoxCharacter.BackColor = System.Drawing.Color.DimGray;
            this.pictureBoxCharacter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxCharacter.Location = new System.Drawing.Point(197, 142);
            this.pictureBoxCharacter.Name = "pictureBoxCharacter";
            this.pictureBoxCharacter.Size = new System.Drawing.Size(99, 47);
            this.pictureBoxCharacter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxCharacter.TabIndex = 33;
            this.pictureBoxCharacter.TabStop = false;
            this.pictureBoxCharacter.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxCharacter_Paint);
            this.pictureBoxCharacter.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxCharacter_MouseClick);
            this.pictureBoxCharacter.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxCharacter_MouseMove);
            // 
            // checkBoxItalic
            // 
            this.checkBoxItalic.AutoSize = true;
            this.checkBoxItalic.Location = new System.Drawing.Point(15, 62);
            this.checkBoxItalic.Name = "checkBoxItalic";
            this.checkBoxItalic.Size = new System.Drawing.Size(58, 17);
            this.checkBoxItalic.TabIndex = 2;
            this.checkBoxItalic.Text = "Is &italic";
            this.checkBoxItalic.UseVisualStyleBackColor = true;
            this.checkBoxItalic.CheckedChanged += new System.EventHandler(this.checkBoxItalic_CheckedChanged);
            // 
            // labelTextAssociatedWithImage
            // 
            this.labelTextAssociatedWithImage.AutoSize = true;
            this.labelTextAssociatedWithImage.Location = new System.Drawing.Point(14, 19);
            this.labelTextAssociatedWithImage.Name = "labelTextAssociatedWithImage";
            this.labelTextAssociatedWithImage.Size = new System.Drawing.Size(135, 13);
            this.labelTextAssociatedWithImage.TabIndex = 0;
            this.labelTextAssociatedWithImage.Text = "Text associated with image";
            // 
            // buttonDelete
            // 
            this.buttonDelete.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDelete.Location = new System.Drawing.Point(107, 35);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(140, 23);
            this.buttonDelete.TabIndex = 4;
            this.buttonDelete.Text = "Delete character";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // textBoxText
            // 
            this.textBoxText.ContextMenuStrip = this.contextMenuStripLetters;
            this.textBoxText.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxText.Location = new System.Drawing.Point(14, 35);
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.Size = new System.Drawing.Size(87, 23);
            this.textBoxText.TabIndex = 1;
            this.textBoxText.TextChanged += new System.EventHandler(this.textBoxText_TextChanged);
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(12, 465);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(47, 13);
            this.labelInfo.TabIndex = 16;
            this.labelInfo.Text = "labelInfo";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(543, 456);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 17;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(625, 456);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 18;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonImport.Location = new System.Drawing.Point(146, 423);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(106, 23);
            this.buttonImport.TabIndex = 19;
            this.buttonImport.Text = "&Import";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // contextMenuStripLetters
            // 
            this.contextMenuStripLetters.Name = "contextMenuStripLetters";
            this.contextMenuStripLetters.Size = new System.Drawing.Size(61, 4);
            // 
            // VobSubNOcrEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 487);
            this.Controls.Add(this.buttonImport);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.groupBoxCurrentCompareImage);
            this.Controls.Add(this.labelImageCompareFiles);
            this.Controls.Add(this.labelChooseCharacters);
            this.Controls.Add(this.listBoxFileNames);
            this.Controls.Add(this.comboBoxTexts);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(720, 520);
            this.Name = "VobSubNOcrEdit";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "nOCR DB";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VobSubNOcrEdit_KeyDown);
            this.groupBoxCurrentCompareImage.ResumeLayout(false);
            this.groupBoxCurrentCompareImage.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.contextMenuStripLinesBackground.ResumeLayout(false);
            this.contextMenuLinesForeground.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCharacter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelImageCompareFiles;
        private System.Windows.Forms.Label labelChooseCharacters;
        private System.Windows.Forms.ListBox listBoxFileNames;
        private System.Windows.Forms.ComboBox comboBoxTexts;
        private System.Windows.Forms.GroupBox groupBoxCurrentCompareImage;
        private System.Windows.Forms.Button buttonZoomOut;
        private System.Windows.Forms.Button buttonZoomIn;
        private System.Windows.Forms.Label labelCharacters;
        private System.Windows.Forms.PictureBox pictureBoxCharacter;
        private System.Windows.Forms.CheckBox checkBoxItalic;
        private System.Windows.Forms.Label labelTextAssociatedWithImage;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonCold;
        private System.Windows.Forms.RadioButton radioButtonHot;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listBoxlinesBackground;
        private System.Windows.Forms.Label labelLines;
        private System.Windows.Forms.ListBox listBoxLinesForeground;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLinesBackground;
        private System.Windows.Forms.ToolStripMenuItem removeBackToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuLinesForeground;
        private System.Windows.Forms.ToolStripMenuItem removeForegroundToolStripMenuItem;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label labelNOcrCharInfo;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLetters;
    }
}