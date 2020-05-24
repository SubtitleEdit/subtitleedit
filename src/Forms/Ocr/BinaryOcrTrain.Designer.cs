namespace Nikse.SubtitleEdit.Forms.Ocr
{
    partial class BinaryOcrTrain
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonInputChoose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxInputFile = new System.Windows.Forms.TextBox();
            this.checkBoxBold = new System.Windows.Forms.CheckBox();
            this.listViewFonts = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelSubtitleFontSize = new System.Windows.Forms.Label();
            this.comboBoxSubtitleFontSize = new System.Windows.Forms.ComboBox();
            this.labelSubtitleFont = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.buttonTrain = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBoxFontSizeEnd = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonInputChoose
            // 
            this.buttonInputChoose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInputChoose.Location = new System.Drawing.Point(696, 38);
            this.buttonInputChoose.Name = "buttonInputChoose";
            this.buttonInputChoose.Size = new System.Drawing.Size(26, 23);
            this.buttonInputChoose.TabIndex = 10;
            this.buttonInputChoose.Text = "...";
            this.buttonInputChoose.UseVisualStyleBackColor = true;
            this.buttonInputChoose.Click += new System.EventHandler(this.buttonInputChoose_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(269, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Subtitle to train with (must have spaces between letters)";
            // 
            // textBoxInputFile
            // 
            this.textBoxInputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxInputFile.Location = new System.Drawing.Point(8, 40);
            this.textBoxInputFile.Name = "textBoxInputFile";
            this.textBoxInputFile.Size = new System.Drawing.Size(682, 20);
            this.textBoxInputFile.TabIndex = 9;
            this.textBoxInputFile.Text = "C:\\Data\\SE-OCR\\Train.srt";
            // 
            // checkBoxBold
            // 
            this.checkBoxBold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxBold.AutoSize = true;
            this.checkBoxBold.Checked = true;
            this.checkBoxBold.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBold.Location = new System.Drawing.Point(9, 524);
            this.checkBoxBold.Name = "checkBoxBold";
            this.checkBoxBold.Size = new System.Drawing.Size(92, 17);
            this.checkBoxBold.TabIndex = 17;
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
            this.listViewFonts.Size = new System.Drawing.Size(711, 429);
            this.listViewFonts.TabIndex = 16;
            this.listViewFonts.UseCompatibleStateImageBehavior = false;
            this.listViewFonts.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Font name";
            this.columnHeader1.Width = 445;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(672, 644);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 24;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelSubtitleFontSize
            // 
            this.labelSubtitleFontSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSubtitleFontSize.AutoSize = true;
            this.labelSubtitleFontSize.Location = new System.Drawing.Point(5, 497);
            this.labelSubtitleFontSize.Name = "labelSubtitleFontSize";
            this.labelSubtitleFontSize.Size = new System.Drawing.Size(84, 13);
            this.labelSubtitleFontSize.TabIndex = 6;
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
            this.comboBoxSubtitleFontSize.Location = new System.Drawing.Point(114, 494);
            this.comboBoxSubtitleFontSize.Name = "comboBoxSubtitleFontSize";
            this.comboBoxSubtitleFontSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSubtitleFontSize.TabIndex = 7;
            // 
            // labelSubtitleFont
            // 
            this.labelSubtitleFont.AutoSize = true;
            this.labelSubtitleFont.Location = new System.Drawing.Point(6, 31);
            this.labelSubtitleFont.Name = "labelSubtitleFont";
            this.labelSubtitleFont.Size = new System.Drawing.Size(63, 13);
            this.labelSubtitleFont.TabIndex = 4;
            this.labelSubtitleFont.Text = "Subtitle font";
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(9, 670);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(47, 13);
            this.labelInfo.TabIndex = 26;
            this.labelInfo.Text = "labelInfo";
            // 
            // buttonTrain
            // 
            this.buttonTrain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTrain.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTrain.Location = new System.Drawing.Point(538, 644);
            this.buttonTrain.Name = "buttonTrain";
            this.buttonTrain.Size = new System.Drawing.Size(128, 23);
            this.buttonTrain.TabIndex = 23;
            this.buttonTrain.Text = "Start train";
            this.buttonTrain.UseVisualStyleBackColor = true;
            this.buttonTrain.Click += new System.EventHandler(this.buttonTrain_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.comboBoxFontSizeEnd);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.checkBoxBold);
            this.groupBox1.Controls.Add(this.listViewFonts);
            this.groupBox1.Controls.Add(this.labelSubtitleFontSize);
            this.groupBox1.Controls.Add(this.comboBoxSubtitleFontSize);
            this.groupBox1.Controls.Add(this.labelSubtitleFont);
            this.groupBox1.Location = new System.Drawing.Point(12, 91);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(735, 547);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Training options";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(556, 487);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(164, 23);
            this.button1.TabIndex = 27;
            this.button1.Text = "Select all";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.SelectAll_Click);
            // 
            // comboBoxFontSizeEnd
            // 
            this.comboBoxFontSizeEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxFontSizeEnd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontSizeEnd.FormattingEnabled = true;
            this.comboBoxFontSizeEnd.Items.AddRange(new object[] {
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
            this.comboBoxFontSizeEnd.Location = new System.Drawing.Point(320, 494);
            this.comboBoxFontSizeEnd.Name = "comboBoxFontSizeEnd";
            this.comboBoxFontSizeEnd.Size = new System.Drawing.Size(85, 21);
            this.comboBoxFontSizeEnd.TabIndex = 21;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(256, 497);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 20;
            this.label4.Text = "to font size";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.buttonInputChoose);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.textBoxInputFile);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(735, 73);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Input file";
            // 
            // BinaryOcrTrain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(759, 692);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonTrain);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.MinimumSize = new System.Drawing.Size(770, 600);
            this.Name = "BinaryOcrTrain";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Binary image compare - train";
            this.Shown += new System.EventHandler(this.BinaryOcrTrain_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonInputChoose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxInputFile;
        private System.Windows.Forms.CheckBox checkBoxBold;
        private System.Windows.Forms.ListView listViewFonts;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelSubtitleFontSize;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFontSize;
        private System.Windows.Forms.Label labelSubtitleFont;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Button buttonTrain;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ComboBox comboBoxFontSizeEnd;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button button1;
    }
}