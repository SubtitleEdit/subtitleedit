namespace Nikse.SubtitleEdit.Forms
{
    partial class Beamer
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
            this.groupBoxImageSettings = new System.Windows.Forms.GroupBox();
            this.comboBoxHAlign = new System.Windows.Forms.ComboBox();
            this.labelHorizontalAlign = new System.Windows.Forms.Label();
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBoxImageSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxImageSettings
            // 
            this.groupBoxImageSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImageSettings.Controls.Add(this.comboBoxHAlign);
            this.groupBoxImageSettings.Controls.Add(this.labelHorizontalAlign);
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
            this.groupBoxImageSettings.Location = new System.Drawing.Point(4, 6);
            this.groupBoxImageSettings.Name = "groupBoxImageSettings";
            this.groupBoxImageSettings.Size = new System.Drawing.Size(854, 114);
            this.groupBoxImageSettings.TabIndex = 23;
            this.groupBoxImageSettings.TabStop = false;
            this.groupBoxImageSettings.Text = "Image settings";
            // 
            // comboBoxHAlign
            // 
            this.comboBoxHAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHAlign.FormattingEnabled = true;
            this.comboBoxHAlign.Items.AddRange(new object[] {
            "Left",
            "Center",
            "Right"});
            this.comboBoxHAlign.Location = new System.Drawing.Point(100, 78);
            this.comboBoxHAlign.Name = "comboBoxHAlign";
            this.comboBoxHAlign.Size = new System.Drawing.Size(121, 21);
            this.comboBoxHAlign.TabIndex = 25;
            this.comboBoxHAlign.SelectedIndexChanged += new System.EventHandler(this.ComboBoxHAlignSelectedIndexChanged);
            // 
            // labelHorizontalAlign
            // 
            this.labelHorizontalAlign.AutoSize = true;
            this.labelHorizontalAlign.Location = new System.Drawing.Point(10, 81);
            this.labelHorizontalAlign.Name = "labelHorizontalAlign";
            this.labelHorizontalAlign.Size = new System.Drawing.Size(30, 13);
            this.labelHorizontalAlign.TabIndex = 24;
            this.labelHorizontalAlign.Text = "Align";
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
            this.comboBoxSubtitleFont.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSubtitleFontSelectedValueChanged);
            // 
            // comboBoxSubtitleFontSize
            // 
            this.comboBoxSubtitleFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFontSize.FormattingEnabled = true;
            this.comboBoxSubtitleFontSize.Items.AddRange(new object[] {
            "10",
            "11",
            "12",
            "13",
            "14",
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
            "100"});
            this.comboBoxSubtitleFontSize.Location = new System.Drawing.Point(100, 51);
            this.comboBoxSubtitleFontSize.Name = "comboBoxSubtitleFontSize";
            this.comboBoxSubtitleFontSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSubtitleFontSize.TabIndex = 18;
            this.comboBoxSubtitleFontSize.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSubtitleFontSizeSelectedIndexChanged);
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
            this.comboBoxBorderWidth.SelectedIndexChanged += new System.EventHandler(this.ComboBoxBorderWidthSelectedIndexChanged);
            // 
            // panelBorderColor
            // 
            this.panelBorderColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBorderColor.Location = new System.Drawing.Point(664, 25);
            this.panelBorderColor.Name = "panelBorderColor";
            this.panelBorderColor.Size = new System.Drawing.Size(21, 20);
            this.panelBorderColor.TabIndex = 14;
            this.panelBorderColor.Click += new System.EventHandler(this.ButtonBorderColorClick);
            // 
            // buttonBorderColor
            // 
            this.buttonBorderColor.Location = new System.Drawing.Point(537, 24);
            this.buttonBorderColor.Name = "buttonBorderColor";
            this.buttonBorderColor.Size = new System.Drawing.Size(121, 21);
            this.buttonBorderColor.TabIndex = 13;
            this.buttonBorderColor.Text = "Border color";
            this.buttonBorderColor.UseVisualStyleBackColor = true;
            this.buttonBorderColor.Click += new System.EventHandler(this.ButtonBorderColorClick);
            // 
            // panelColor
            // 
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(403, 25);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(21, 20);
            this.panelColor.TabIndex = 12;
            this.panelColor.Click += new System.EventHandler(this.ButtonColorClick);
            // 
            // buttonColor
            // 
            this.buttonColor.Location = new System.Drawing.Point(276, 24);
            this.buttonColor.Name = "buttonColor";
            this.buttonColor.Size = new System.Drawing.Size(121, 21);
            this.buttonColor.TabIndex = 11;
            this.buttonColor.Text = "Color";
            this.buttonColor.UseVisualStyleBackColor = true;
            this.buttonColor.Click += new System.EventHandler(this.ButtonColorClick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(4, 126);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(855, 211);
            this.pictureBox1.TabIndex = 22;
            this.pictureBox1.TabStop = false;
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(4, 343);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(121, 21);
            this.buttonStart.TabIndex = 24;
            this.buttonStart.Text = "&Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.ButtonStartClick);
            // 
            // timer1
            // 
            this.timer1.Interval = 25;
            this.timer1.Tick += new System.EventHandler(this.Timer1Tick);
            // 
            // Beamer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(862, 376);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.groupBoxImageSettings);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Beamer";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Beamer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BeamerFormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BeamerKeyDown);
            this.groupBoxImageSettings.ResumeLayout(false);
            this.groupBoxImageSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxImageSettings;
        private System.Windows.Forms.Label labelSubtitleFontSize;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFont;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFontSize;
        private System.Windows.Forms.Label labelSubtitleFont;
        private System.Windows.Forms.Label labelBorderWidth;
        private System.Windows.Forms.ComboBox comboBoxBorderWidth;
        private System.Windows.Forms.Panel panelBorderColor;
        private System.Windows.Forms.Button buttonBorderColor;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Button buttonColor;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ComboBox comboBoxHAlign;
        private System.Windows.Forms.Label labelHorizontalAlign;
    }
}