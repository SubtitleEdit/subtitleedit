namespace Nikse.SubtitleEdit.Forms.Ocr
{
    partial class VobSubNOcrTrain
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxBold = new System.Windows.Forms.CheckBox();
            this.listViewFonts = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownSegmentsPerCharacter = new System.Windows.Forms.NumericUpDown();
            this.checkBoxVeryAccurate = new System.Windows.Forms.CheckBox();
            this.labelSubtitleFontSize = new System.Windows.Forms.Label();
            this.comboBoxSubtitleFontSize = new System.Windows.Forms.ComboBox();
            this.labelSubtitleFont = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxInputFile = new System.Windows.Forms.TextBox();
            this.buttonInputChoose = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxMerged = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonTrain = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.checkBoxItalic = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSegmentsPerCharacter)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxItalic);
            this.groupBox1.Controls.Add(this.checkBoxBold);
            this.groupBox1.Controls.Add(this.listViewFonts);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numericUpDownSegmentsPerCharacter);
            this.groupBox1.Controls.Add(this.checkBoxVeryAccurate);
            this.groupBox1.Controls.Add(this.labelSubtitleFontSize);
            this.groupBox1.Controls.Add(this.comboBoxSubtitleFontSize);
            this.groupBox1.Controls.Add(this.labelSubtitleFont);
            this.groupBox1.Location = new System.Drawing.Point(12, 153);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(493, 443);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Training options";
            // 
            // checkBoxBold
            // 
            this.checkBoxBold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxBold.AutoSize = true;
            this.checkBoxBold.Checked = true;
            this.checkBoxBold.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBold.Location = new System.Drawing.Point(9, 420);
            this.checkBoxBold.Name = "checkBoxBold";
            this.checkBoxBold.Size = new System.Drawing.Size(92, 17);
            this.checkBoxBold.TabIndex = 7;
            this.checkBoxBold.Text = "Also train bold";
            this.checkBoxBold.UseVisualStyleBackColor = true;
            // 
            // listViewFonts
            // 
            this.listViewFonts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFonts.CheckBoxes = true;
            this.listViewFonts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listViewFonts.FullRowSelect = true;
            this.listViewFonts.HideSelection = false;
            this.listViewFonts.Location = new System.Drawing.Point(9, 47);
            this.listViewFonts.Name = "listViewFonts";
            this.listViewFonts.Size = new System.Drawing.Size(469, 270);
            this.listViewFonts.TabIndex = 1;
            this.listViewFonts.UseCompatibleStateImageBehavior = false;
            this.listViewFonts.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Font name";
            this.columnHeader1.Width = 445;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 392);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(189, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Number of line segments per character";
            // 
            // numericUpDownSegmentsPerCharacter
            // 
            this.numericUpDownSegmentsPerCharacter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.numericUpDownSegmentsPerCharacter.Location = new System.Drawing.Point(264, 390);
            this.numericUpDownSegmentsPerCharacter.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.numericUpDownSegmentsPerCharacter.Minimum = new decimal(new int[] {
            75,
            0,
            0,
            0});
            this.numericUpDownSegmentsPerCharacter.Name = "numericUpDownSegmentsPerCharacter";
            this.numericUpDownSegmentsPerCharacter.Size = new System.Drawing.Size(66, 20);
            this.numericUpDownSegmentsPerCharacter.TabIndex = 6;
            this.numericUpDownSegmentsPerCharacter.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // checkBoxVeryAccurate
            // 
            this.checkBoxVeryAccurate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxVeryAccurate.AutoSize = true;
            this.checkBoxVeryAccurate.Location = new System.Drawing.Point(9, 367);
            this.checkBoxVeryAccurate.Name = "checkBoxVeryAccurate";
            this.checkBoxVeryAccurate.Size = new System.Drawing.Size(303, 17);
            this.checkBoxVeryAccurate.TabIndex = 4;
            this.checkBoxVeryAccurate.Text = "Very accurate (be less general), require more line segments";
            this.checkBoxVeryAccurate.UseVisualStyleBackColor = true;
            // 
            // labelSubtitleFontSize
            // 
            this.labelSubtitleFontSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSubtitleFontSize.AutoSize = true;
            this.labelSubtitleFontSize.Location = new System.Drawing.Point(5, 334);
            this.labelSubtitleFontSize.Name = "labelSubtitleFontSize";
            this.labelSubtitleFontSize.Size = new System.Drawing.Size(84, 13);
            this.labelSubtitleFontSize.TabIndex = 2;
            this.labelSubtitleFontSize.Text = "Subtitle font size";
            // 
            // comboBoxSubtitleFontSize
            // 
            this.comboBoxSubtitleFontSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
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
            this.comboBoxSubtitleFontSize.Location = new System.Drawing.Point(110, 331);
            this.comboBoxSubtitleFontSize.Name = "comboBoxSubtitleFontSize";
            this.comboBoxSubtitleFontSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSubtitleFontSize.TabIndex = 3;
            // 
            // labelSubtitleFont
            // 
            this.labelSubtitleFont.AutoSize = true;
            this.labelSubtitleFont.Location = new System.Drawing.Point(6, 31);
            this.labelSubtitleFont.Name = "labelSubtitleFont";
            this.labelSubtitleFont.Size = new System.Drawing.Size(63, 13);
            this.labelSubtitleFont.TabIndex = 0;
            this.labelSubtitleFont.Text = "Subtitle font";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(272, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Subtitle to train with  (must have spaces between letters)";
            // 
            // textBoxInputFile
            // 
            this.textBoxInputFile.Location = new System.Drawing.Point(8, 40);
            this.textBoxInputFile.Name = "textBoxInputFile";
            this.textBoxInputFile.Size = new System.Drawing.Size(438, 20);
            this.textBoxInputFile.TabIndex = 1;
            this.textBoxInputFile.Text = "c:\\data\\SE-OCR\\train.srt";
            // 
            // buttonInputChoose
            // 
            this.buttonInputChoose.Location = new System.Drawing.Point(452, 37);
            this.buttonInputChoose.Name = "buttonInputChoose";
            this.buttonInputChoose.Size = new System.Drawing.Size(26, 23);
            this.buttonInputChoose.TabIndex = 2;
            this.buttonInputChoose.Text = "...";
            this.buttonInputChoose.UseVisualStyleBackColor = true;
            this.buttonInputChoose.Click += new System.EventHandler(this.buttonInputChoose_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.textBoxMerged);
            this.groupBox2.Controls.Add(this.buttonInputChoose);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.textBoxInputFile);
            this.groupBox2.Location = new System.Drawing.Point(12, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(493, 134);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Input file";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(201, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Letter combinations that might be merged";
            // 
            // textBoxMerged
            // 
            this.textBoxMerged.Location = new System.Drawing.Point(9, 98);
            this.textBoxMerged.Name = "textBoxMerged";
            this.textBoxMerged.Size = new System.Drawing.Size(469, 20);
            this.textBoxMerged.TabIndex = 4;
            this.textBoxMerged.Text = "ff ft fi fj fy fl rf rt rv rw ry rt ryt tt TV tw yt yw";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonTrain
            // 
            this.buttonTrain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTrain.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTrain.Location = new System.Drawing.Point(296, 602);
            this.buttonTrain.Name = "buttonTrain";
            this.buttonTrain.Size = new System.Drawing.Size(128, 23);
            this.buttonTrain.TabIndex = 2;
            this.buttonTrain.Text = "Start training";
            this.buttonTrain.UseVisualStyleBackColor = true;
            this.buttonTrain.Click += new System.EventHandler(this.buttonTrain_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(430, 602);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(12, 628);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(47, 13);
            this.labelInfo.TabIndex = 4;
            this.labelInfo.Text = "labelInfo";
            // 
            // checkBoxItalic
            // 
            this.checkBoxItalic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxItalic.AutoSize = true;
            this.checkBoxItalic.Location = new System.Drawing.Point(116, 420);
            this.checkBoxItalic.Name = "checkBoxItalic";
            this.checkBoxItalic.Size = new System.Drawing.Size(93, 17);
            this.checkBoxItalic.TabIndex = 8;
            this.checkBoxItalic.Text = "Also train italic";
            this.checkBoxItalic.UseVisualStyleBackColor = true;
            // 
            // VobSubNOcrTrain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(523, 649);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonTrain);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(519, 600);
            this.Name = "VobSubNOcrTrain";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "nOCR Train";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VobSubNOcrTrain_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSegmentsPerCharacter)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxVeryAccurate;
        private System.Windows.Forms.Button buttonInputChoose;
        private System.Windows.Forms.TextBox textBoxInputFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelSubtitleFontSize;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFontSize;
        private System.Windows.Forms.Label labelSubtitleFont;
        private System.Windows.Forms.ListView listViewFonts;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownSegmentsPerCharacter;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonTrain;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.CheckBox checkBoxBold;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxMerged;
        private System.Windows.Forms.CheckBox checkBoxItalic;
    }
}