namespace Nikse.SubtitleEdit.Forms
{
    partial class ExportPngXml
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBoxImageSettings = new System.Windows.Forms.GroupBox();
            this.checkBoxAntiAlias = new System.Windows.Forms.CheckBox();
            this.labelSubtitleFontSize = new System.Windows.Forms.Label();
            this.comboBoxSubtitleFont = new System.Windows.Forms.ComboBox();
            this.comboBoxSubtitleFontSize = new System.Windows.Forms.ComboBox();
            this.labelSubtitleFont = new System.Windows.Forms.Label();
            this.labelBorderWidth = new System.Windows.Forms.Label();
            this.comboBoxBorderWidth = new System.Windows.Forms.ComboBox();
            this.panelBorderColor = new System.Windows.Forms.Panel();
            this.buttonBorderColor = new System.Windows.Forms.Button();
            this.panelColor = new System.Windows.Forms.Panel();
            this.buttonColor = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.labelImageResolution = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.labelHorizontalAlign = new System.Windows.Forms.Label();
            this.comboBoxHAlign = new System.Windows.Forms.ComboBox();
            this.subtitleListView1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBoxImageSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(12, 355);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(712, 154);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonExport.Location = new System.Drawing.Point(517, 515);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(126, 21);
            this.buttonExport.TabIndex = 19;
            this.buttonExport.Text = "Export all lines...";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(649, 515);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 18;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 515);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(499, 21);
            this.progressBar1.TabIndex = 20;
            this.progressBar1.Visible = false;
            // 
            // groupBoxImageSettings
            // 
            this.groupBoxImageSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImageSettings.Controls.Add(this.comboBoxHAlign);
            this.groupBoxImageSettings.Controls.Add(this.labelHorizontalAlign);
            this.groupBoxImageSettings.Controls.Add(this.checkBoxAntiAlias);
            this.groupBoxImageSettings.Controls.Add(this.labelSubtitleFontSize);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxSubtitleFont);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxSubtitleFontSize);
            this.groupBoxImageSettings.Controls.Add(this.labelSubtitleFont);
            this.groupBoxImageSettings.Controls.Add(this.labelBorderWidth);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxBorderWidth);
            this.groupBoxImageSettings.Controls.Add(this.panelBorderColor);
            this.groupBoxImageSettings.Controls.Add(this.buttonBorderColor);
            this.groupBoxImageSettings.Controls.Add(this.panelColor);
            this.groupBoxImageSettings.Controls.Add(this.buttonColor);
            this.groupBoxImageSettings.Location = new System.Drawing.Point(12, 218);
            this.groupBoxImageSettings.Name = "groupBoxImageSettings";
            this.groupBoxImageSettings.Size = new System.Drawing.Size(712, 117);
            this.groupBoxImageSettings.TabIndex = 21;
            this.groupBoxImageSettings.TabStop = false;
            this.groupBoxImageSettings.Text = "Image settings";
            // 
            // checkBoxAntiAlias
            // 
            this.checkBoxAntiAlias.AutoSize = true;
            this.checkBoxAntiAlias.Checked = true;
            this.checkBoxAntiAlias.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAntiAlias.Location = new System.Drawing.Point(276, 55);
            this.checkBoxAntiAlias.Name = "checkBoxAntiAlias";
            this.checkBoxAntiAlias.Size = new System.Drawing.Size(66, 17);
            this.checkBoxAntiAlias.TabIndex = 21;
            this.checkBoxAntiAlias.Text = "AntiAlias";
            this.checkBoxAntiAlias.UseVisualStyleBackColor = true;
            this.checkBoxAntiAlias.CheckedChanged += new System.EventHandler(this.checkBoxAntiAlias_CheckedChanged);
            // 
            // labelSubtitleFontSize
            // 
            this.labelSubtitleFontSize.AutoSize = true;
            this.labelSubtitleFontSize.Location = new System.Drawing.Point(10, 54);
            this.labelSubtitleFontSize.Name = "labelSubtitleFontSize";
            this.labelSubtitleFontSize.Size = new System.Drawing.Size(84, 13);
            this.labelSubtitleFontSize.TabIndex = 20;
            this.labelSubtitleFontSize.Text = "Subtitle font size";
            // 
            // comboBoxSubtitleFont
            // 
            this.comboBoxSubtitleFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFont.FormattingEnabled = true;
            this.comboBoxSubtitleFont.Location = new System.Drawing.Point(100, 24);
            this.comboBoxSubtitleFont.Name = "comboBoxSubtitleFont";
            this.comboBoxSubtitleFont.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSubtitleFont.TabIndex = 17;
            this.comboBoxSubtitleFont.SelectedValueChanged += new System.EventHandler(this.comboBoxSubtitleFont_SelectedValueChanged);
            // 
            // comboBoxSubtitleFontSize
            // 
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
            "80",
            "85",
            "90",
            "100"});
            this.comboBoxSubtitleFontSize.Location = new System.Drawing.Point(100, 51);
            this.comboBoxSubtitleFontSize.Name = "comboBoxSubtitleFontSize";
            this.comboBoxSubtitleFontSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSubtitleFontSize.TabIndex = 18;
            this.comboBoxSubtitleFontSize.SelectedIndexChanged += new System.EventHandler(this.comboBoxSubtitleFontSize_SelectedIndexChanged);
            // 
            // labelSubtitleFont
            // 
            this.labelSubtitleFont.AutoSize = true;
            this.labelSubtitleFont.Location = new System.Drawing.Point(10, 27);
            this.labelSubtitleFont.Name = "labelSubtitleFont";
            this.labelSubtitleFont.Size = new System.Drawing.Size(63, 13);
            this.labelSubtitleFont.TabIndex = 19;
            this.labelSubtitleFont.Text = "Subtitle font";
            // 
            // labelBorderWidth
            // 
            this.labelBorderWidth.Location = new System.Drawing.Point(426, 56);
            this.labelBorderWidth.Name = "labelBorderWidth";
            this.labelBorderWidth.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelBorderWidth.Size = new System.Drawing.Size(110, 13);
            this.labelBorderWidth.TabIndex = 16;
            this.labelBorderWidth.Text = "Border width";
            this.labelBorderWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxBorderWidth
            // 
            this.comboBoxBorderWidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBorderWidth.FormattingEnabled = true;
            this.comboBoxBorderWidth.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.comboBoxBorderWidth.Location = new System.Drawing.Point(537, 51);
            this.comboBoxBorderWidth.Name = "comboBoxBorderWidth";
            this.comboBoxBorderWidth.Size = new System.Drawing.Size(121, 21);
            this.comboBoxBorderWidth.TabIndex = 15;
            this.comboBoxBorderWidth.SelectedIndexChanged += new System.EventHandler(this.comboBoxBorderWidth_SelectedIndexChanged);
            // 
            // panelBorderColor
            // 
            this.panelBorderColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBorderColor.Location = new System.Drawing.Point(664, 25);
            this.panelBorderColor.Name = "panelBorderColor";
            this.panelBorderColor.Size = new System.Drawing.Size(21, 20);
            this.panelBorderColor.TabIndex = 14;
            this.panelBorderColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelBorderColor_MouseClick);
            // 
            // buttonBorderColor
            // 
            this.buttonBorderColor.Location = new System.Drawing.Point(537, 24);
            this.buttonBorderColor.Name = "buttonBorderColor";
            this.buttonBorderColor.Size = new System.Drawing.Size(121, 21);
            this.buttonBorderColor.TabIndex = 13;
            this.buttonBorderColor.Text = "Border color";
            this.buttonBorderColor.UseVisualStyleBackColor = true;
            this.buttonBorderColor.Click += new System.EventHandler(this.buttonBorderColor_Click);
            // 
            // panelColor
            // 
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(403, 25);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(21, 20);
            this.panelColor.TabIndex = 12;
            this.panelColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelColor_MouseClick);
            // 
            // buttonColor
            // 
            this.buttonColor.Location = new System.Drawing.Point(276, 24);
            this.buttonColor.Name = "buttonColor";
            this.buttonColor.Size = new System.Drawing.Size(121, 21);
            this.buttonColor.TabIndex = 11;
            this.buttonColor.Text = "Color";
            this.buttonColor.UseVisualStyleBackColor = true;
            this.buttonColor.Click += new System.EventHandler(this.buttonColor_Click);
            // 
            // labelImageResolution
            // 
            this.labelImageResolution.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelImageResolution.BackColor = System.Drawing.Color.Transparent;
            this.labelImageResolution.Location = new System.Drawing.Point(651, 338);
            this.labelImageResolution.Name = "labelImageResolution";
            this.labelImageResolution.Size = new System.Drawing.Size(73, 14);
            this.labelImageResolution.TabIndex = 22;
            this.labelImageResolution.Text = "320x240";
            this.labelImageResolution.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelHorizontalAlign
            // 
            this.labelHorizontalAlign.AutoSize = true;
            this.labelHorizontalAlign.Location = new System.Drawing.Point(10, 82);
            this.labelHorizontalAlign.Name = "labelHorizontalAlign";
            this.labelHorizontalAlign.Size = new System.Drawing.Size(30, 13);
            this.labelHorizontalAlign.TabIndex = 22;
            this.labelHorizontalAlign.Text = "Align";
            // 
            // comboBoxHAlign
            // 
            this.comboBoxHAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHAlign.FormattingEnabled = true;
            this.comboBoxHAlign.Items.AddRange(new object[] {
            "Left",
            "Center"});
            this.comboBoxHAlign.Location = new System.Drawing.Point(100, 79);
            this.comboBoxHAlign.Name = "comboBoxHAlign";
            this.comboBoxHAlign.Size = new System.Drawing.Size(121, 21);
            this.comboBoxHAlign.TabIndex = 23;
            this.comboBoxHAlign.SelectedIndexChanged += new System.EventHandler(this.comboBoxHAlign_SelectedIndexChanged);
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.subtitleListView1.DisplayExtraFromExtra = false;
            this.subtitleListView1.FirstVisibleIndex = -1;
            this.subtitleListView1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.Location = new System.Drawing.Point(12, 12);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.Size = new System.Drawing.Size(712, 200);
            this.subtitleListView1.TabIndex = 0;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            this.subtitleListView1.SelectedIndexChanged += new System.EventHandler(this.subtitleListView1_SelectedIndexChanged);
            // 
            // ExportPngXml
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(730, 541);
            this.Controls.Add(this.labelImageResolution);
            this.Controls.Add(this.groupBoxImageSettings);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.subtitleListView1);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(730, 430);
            this.Name = "ExportPngXml";
            this.ShowIcon = false;
            this.Text = "ExportPngXml";
            this.Shown += new System.EventHandler(this.ExportPngXml_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExportPngXml_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBoxImageSettings.ResumeLayout(false);
            this.groupBoxImageSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.SubtitleListView subtitleListView1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.GroupBox groupBoxImageSettings;
        private System.Windows.Forms.Label labelBorderWidth;
        private System.Windows.Forms.ComboBox comboBoxBorderWidth;
        private System.Windows.Forms.Panel panelBorderColor;
        private System.Windows.Forms.Button buttonBorderColor;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Button buttonColor;
        private System.Windows.Forms.Label labelSubtitleFontSize;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFont;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFontSize;
        private System.Windows.Forms.Label labelSubtitleFont;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.CheckBox checkBoxAntiAlias;
        private System.Windows.Forms.Label labelImageResolution;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ComboBox comboBoxHAlign;
        private System.Windows.Forms.Label labelHorizontalAlign;
    }
}