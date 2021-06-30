namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ExportFcpXmlAdvanced
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.groupBoxImageSettings = new System.Windows.Forms.GroupBox();
            this.buttonCustomResolution = new System.Windows.Forms.Button();
            this.labelResolution = new System.Windows.Forms.Label();
            this.comboBoxResolution = new System.Windows.Forms.ComboBox();
            this.comboBoxFrameRate = new System.Windows.Forms.ComboBox();
            this.labelFrameRate = new System.Windows.Forms.Label();
            this.labelBaseline = new System.Windows.Forms.Label();
            this.comboBoxBaseline = new System.Windows.Forms.ComboBox();
            this.labelSubtitleFontFace = new System.Windows.Forms.Label();
            this.comboBoxFontFace = new System.Windows.Forms.ComboBox();
            this.comboBoxHAlign = new System.Windows.Forms.ComboBox();
            this.labelHorizontalAlign = new System.Windows.Forms.Label();
            this.labelSubtitleFontSize = new System.Windows.Forms.Label();
            this.comboBoxFontName = new System.Windows.Forms.ComboBox();
            this.comboBoxFontSize = new System.Windows.Forms.ComboBox();
            this.labelSubtitleFont = new System.Windows.Forms.Label();
            this.panelColor = new System.Windows.Forms.Panel();
            this.buttonColor = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SubtitleListview = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.groupBoxImageSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(848, 477);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 101;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSave.Location = new System.Drawing.Point(725, 477);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(117, 23);
            this.buttonSave.TabIndex = 100;
            this.buttonSave.Text = "&Save as...";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // groupBoxImageSettings
            // 
            this.groupBoxImageSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImageSettings.Controls.Add(this.comboBoxFontSize);
            this.groupBoxImageSettings.Controls.Add(this.buttonCustomResolution);
            this.groupBoxImageSettings.Controls.Add(this.labelResolution);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxResolution);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxFrameRate);
            this.groupBoxImageSettings.Controls.Add(this.labelFrameRate);
            this.groupBoxImageSettings.Controls.Add(this.labelBaseline);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxBaseline);
            this.groupBoxImageSettings.Controls.Add(this.labelSubtitleFontFace);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxFontFace);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxHAlign);
            this.groupBoxImageSettings.Controls.Add(this.labelHorizontalAlign);
            this.groupBoxImageSettings.Controls.Add(this.labelSubtitleFontSize);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxFontName);
            this.groupBoxImageSettings.Controls.Add(this.labelSubtitleFont);
            this.groupBoxImageSettings.Controls.Add(this.panelColor);
            this.groupBoxImageSettings.Controls.Add(this.buttonColor);
            this.groupBoxImageSettings.Location = new System.Drawing.Point(11, 246);
            this.groupBoxImageSettings.Name = "groupBoxImageSettings";
            this.groupBoxImageSettings.Size = new System.Drawing.Size(912, 225);
            this.groupBoxImageSettings.TabIndex = 0;
            this.groupBoxImageSettings.TabStop = false;
            this.groupBoxImageSettings.Text = "Image settings";
            // 
            // buttonCustomResolution
            // 
            this.buttonCustomResolution.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCustomResolution.Location = new System.Drawing.Point(589, 54);
            this.buttonCustomResolution.Name = "buttonCustomResolution";
            this.buttonCustomResolution.Size = new System.Drawing.Size(24, 21);
            this.buttonCustomResolution.TabIndex = 91;
            this.buttonCustomResolution.Text = "...";
            this.buttonCustomResolution.UseVisualStyleBackColor = true;
            this.buttonCustomResolution.Click += new System.EventHandler(this.buttonCustomResolution_Click);
            // 
            // labelResolution
            // 
            this.labelResolution.AutoSize = true;
            this.labelResolution.Location = new System.Drawing.Point(363, 57);
            this.labelResolution.Name = "labelResolution";
            this.labelResolution.Size = new System.Drawing.Size(51, 13);
            this.labelResolution.TabIndex = 49;
            this.labelResolution.Text = "Video res";
            // 
            // comboBoxResolution
            // 
            this.comboBoxResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxResolution.FormattingEnabled = true;
            this.comboBoxResolution.Items.AddRange(new object[] {
            "4K (4096x2160)",
            "UHD (3840x2160)",
            "2K (2048x1080)",
            "DCI 2K Scope (2048x858)",
            "DCI 2K Flat (1998x1080)",
            "1080p (1920x1080)",
            "1440x1080",
            "720p (1280x720)",
            "960x720",
            "480p (848x480)",
            "PAL (720x576)",
            "NTSC (720x480)",
            "640x352",
            "640x272"});
            this.comboBoxResolution.Location = new System.Drawing.Point(465, 54);
            this.comboBoxResolution.Name = "comboBoxResolution";
            this.comboBoxResolution.Size = new System.Drawing.Size(121, 21);
            this.comboBoxResolution.TabIndex = 90;
            // 
            // comboBoxFrameRate
            // 
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Location = new System.Drawing.Point(465, 24);
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFrameRate.TabIndex = 80;
            // 
            // labelFrameRate
            // 
            this.labelFrameRate.AutoSize = true;
            this.labelFrameRate.Location = new System.Drawing.Point(363, 27);
            this.labelFrameRate.Name = "labelFrameRate";
            this.labelFrameRate.Size = new System.Drawing.Size(57, 13);
            this.labelFrameRate.TabIndex = 47;
            this.labelFrameRate.Text = "Frame rate";
            // 
            // labelBaseline
            // 
            this.labelBaseline.AutoSize = true;
            this.labelBaseline.Location = new System.Drawing.Point(10, 173);
            this.labelBaseline.Name = "labelBaseline";
            this.labelBaseline.Size = new System.Drawing.Size(47, 13);
            this.labelBaseline.TabIndex = 45;
            this.labelBaseline.Text = "Baseline";
            // 
            // comboBoxBaseline
            // 
            this.comboBoxBaseline.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBaseline.FormattingEnabled = true;
            this.comboBoxBaseline.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
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
            "100"});
            this.comboBoxBaseline.Location = new System.Drawing.Point(113, 170);
            this.comboBoxBaseline.Name = "comboBoxBaseline";
            this.comboBoxBaseline.Size = new System.Drawing.Size(121, 21);
            this.comboBoxBaseline.TabIndex = 70;
            // 
            // labelSubtitleFontFace
            // 
            this.labelSubtitleFontFace.AutoSize = true;
            this.labelSubtitleFontFace.Location = new System.Drawing.Point(10, 81);
            this.labelSubtitleFontFace.Name = "labelSubtitleFontFace";
            this.labelSubtitleFontFace.Size = new System.Drawing.Size(52, 13);
            this.labelSubtitleFontFace.TabIndex = 43;
            this.labelSubtitleFontFace.Text = "Font face";
            // 
            // comboBoxFontFace
            // 
            this.comboBoxFontFace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontFace.FormattingEnabled = true;
            this.comboBoxFontFace.Items.AddRange(new object[] {
            "Regular"});
            this.comboBoxFontFace.Location = new System.Drawing.Point(113, 78);
            this.comboBoxFontFace.Name = "comboBoxFontFace";
            this.comboBoxFontFace.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFontFace.TabIndex = 30;
            // 
            // comboBoxHAlign
            // 
            this.comboBoxHAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHAlign.FormattingEnabled = true;
            this.comboBoxHAlign.Items.AddRange(new object[] {
            "left",
            "center",
            "right",
            "justified"});
            this.comboBoxHAlign.Location = new System.Drawing.Point(113, 140);
            this.comboBoxHAlign.Name = "comboBoxHAlign";
            this.comboBoxHAlign.Size = new System.Drawing.Size(121, 21);
            this.comboBoxHAlign.TabIndex = 60;
            // 
            // labelHorizontalAlign
            // 
            this.labelHorizontalAlign.AutoSize = true;
            this.labelHorizontalAlign.Location = new System.Drawing.Point(10, 143);
            this.labelHorizontalAlign.Name = "labelHorizontalAlign";
            this.labelHorizontalAlign.Size = new System.Drawing.Size(53, 13);
            this.labelHorizontalAlign.TabIndex = 7;
            this.labelHorizontalAlign.Text = "Alignment";
            // 
            // labelSubtitleFontSize
            // 
            this.labelSubtitleFontSize.AutoSize = true;
            this.labelSubtitleFontSize.Location = new System.Drawing.Point(10, 54);
            this.labelSubtitleFontSize.Name = "labelSubtitleFontSize";
            this.labelSubtitleFontSize.Size = new System.Drawing.Size(49, 13);
            this.labelSubtitleFontSize.TabIndex = 2;
            this.labelSubtitleFontSize.Text = "Font size";
            // 
            // comboBoxFontName
            // 
            this.comboBoxFontName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontName.DropDownWidth = 190;
            this.comboBoxFontName.FormattingEnabled = true;
            this.comboBoxFontName.Location = new System.Drawing.Point(113, 24);
            this.comboBoxFontName.Name = "comboBoxFontName";
            this.comboBoxFontName.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFontName.TabIndex = 10;
            // 
            // comboBoxFontSize
            // 
            this.comboBoxFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontSize.FormattingEnabled = true;
            this.comboBoxFontSize.Items.AddRange(new object[] {
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
            "150",
            "160",
            "170",
            "180",
            "190",
            "200",
            "225",
            "250",
            "275",
            "300",
            "325",
            "350",
            "375",
            "400",
            "425",
            "450",
            "475",
            "500"});
            this.comboBoxFontSize.Location = new System.Drawing.Point(113, 51);
            this.comboBoxFontSize.Name = "comboBoxFontSize";
            this.comboBoxFontSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFontSize.TabIndex = 20;
            // 
            // labelSubtitleFont
            // 
            this.labelSubtitleFont.AutoSize = true;
            this.labelSubtitleFont.Location = new System.Drawing.Point(10, 27);
            this.labelSubtitleFont.Name = "labelSubtitleFont";
            this.labelSubtitleFont.Size = new System.Drawing.Size(57, 13);
            this.labelSubtitleFont.TabIndex = 0;
            this.labelSubtitleFont.Text = "Font name";
            // 
            // panelColor
            // 
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(240, 106);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(21, 20);
            this.panelColor.TabIndex = 50;
            this.panelColor.Click += new System.EventHandler(this.panelColor_Click);
            // 
            // buttonColor
            // 
            this.buttonColor.Location = new System.Drawing.Point(113, 105);
            this.buttonColor.Name = "buttonColor";
            this.buttonColor.Size = new System.Drawing.Size(121, 23);
            this.buttonColor.TabIndex = 40;
            this.buttonColor.Text = "Font color";
            this.buttonColor.UseVisualStyleBackColor = true;
            this.buttonColor.Click += new System.EventHandler(this.buttonColor_Click);
            // 
            // SubtitleListview
            // 
            this.SubtitleListview.AllowColumnReorder = true;
            this.SubtitleListview.AllowDrop = true;
            this.SubtitleListview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SubtitleListview.FirstVisibleIndex = -1;
            this.SubtitleListview.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubtitleListview.FullRowSelect = true;
            this.SubtitleListview.GridLines = true;
            this.SubtitleListview.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SubtitleListview.HideSelection = false;
            this.SubtitleListview.Location = new System.Drawing.Point(12, 12);
            this.SubtitleListview.MultiSelect = false;
            this.SubtitleListview.Name = "SubtitleListview";
            this.SubtitleListview.OwnerDraw = true;
            this.SubtitleListview.Size = new System.Drawing.Size(911, 228);
            this.SubtitleListview.SubtitleFontBold = false;
            this.SubtitleListview.SubtitleFontName = "Tahoma";
            this.SubtitleListview.SubtitleFontSize = 8;
            this.SubtitleListview.TabIndex = 200;
            this.SubtitleListview.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview.UseSyntaxColoring = true;
            this.SubtitleListview.View = System.Windows.Forms.View.Details;
            // 
            // ExportFcpXmlAdvanced
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(935, 511);
            this.Controls.Add(this.SubtitleListview);
            this.Controls.Add(this.groupBoxImageSettings);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "ExportFcpXmlAdvanced";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ExportFcpXmlAdvanced";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExportFcpXmlAdvanced_FormClosing);
            this.Shown += new System.EventHandler(this.ExportFcpXmlAdvanced_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExportFcpXmlAdvanced_KeyDown);
            this.groupBoxImageSettings.ResumeLayout(false);
            this.groupBoxImageSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.GroupBox groupBoxImageSettings;
        private System.Windows.Forms.ComboBox comboBoxHAlign;
        private System.Windows.Forms.Label labelHorizontalAlign;
        private System.Windows.Forms.Label labelSubtitleFontSize;
        private System.Windows.Forms.ComboBox comboBoxFontName;
        private System.Windows.Forms.ComboBox comboBoxFontSize;
        private System.Windows.Forms.Label labelSubtitleFont;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Button buttonColor;
        private System.Windows.Forms.Label labelSubtitleFontFace;
        private System.Windows.Forms.ComboBox comboBoxFontFace;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label labelBaseline;
        private System.Windows.Forms.ComboBox comboBoxBaseline;
        private Controls.SubtitleListView SubtitleListview;
        private System.Windows.Forms.ComboBox comboBoxFrameRate;
        private System.Windows.Forms.Label labelFrameRate;
        private System.Windows.Forms.Button buttonCustomResolution;
        private System.Windows.Forms.Label labelResolution;
        private System.Windows.Forms.ComboBox comboBoxResolution;
    }
}