
namespace Nikse.SubtitleEdit.Forms.BinaryEdit
{
    partial class BinEditNewText
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
            this.groupBoxImageSettings = new System.Windows.Forms.GroupBox();
            this.comboBoxHAlign = new System.Windows.Forms.ComboBox();
            this.labelHorizontalAlign = new System.Windows.Forms.Label();
            this.labelText = new System.Windows.Forms.Label();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.labelLineHeight = new System.Windows.Forms.Label();
            this.numericUpDownLineSpacing = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownShadowTransparency = new System.Windows.Forms.NumericUpDown();
            this.labelShadowTransparency = new System.Windows.Forms.Label();
            this.labelShadowWidth = new System.Windows.Forms.Label();
            this.comboBoxShadowWidth = new System.Windows.Forms.ComboBox();
            this.panelShadowColor = new System.Windows.Forms.Panel();
            this.buttonShadowColor = new System.Windows.Forms.Button();
            this.checkBoxBold = new System.Windows.Forms.CheckBox();
            this.checkBoxSimpleRender = new System.Windows.Forms.CheckBox();
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxImageSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLineSpacing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowTransparency)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxImageSettings
            // 
            this.groupBoxImageSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImageSettings.Controls.Add(this.comboBoxHAlign);
            this.groupBoxImageSettings.Controls.Add(this.labelHorizontalAlign);
            this.groupBoxImageSettings.Controls.Add(this.labelText);
            this.groupBoxImageSettings.Controls.Add(this.textBoxText);
            this.groupBoxImageSettings.Controls.Add(this.labelLineHeight);
            this.groupBoxImageSettings.Controls.Add(this.numericUpDownLineSpacing);
            this.groupBoxImageSettings.Controls.Add(this.numericUpDownShadowTransparency);
            this.groupBoxImageSettings.Controls.Add(this.labelShadowTransparency);
            this.groupBoxImageSettings.Controls.Add(this.labelShadowWidth);
            this.groupBoxImageSettings.Controls.Add(this.comboBoxShadowWidth);
            this.groupBoxImageSettings.Controls.Add(this.panelShadowColor);
            this.groupBoxImageSettings.Controls.Add(this.buttonShadowColor);
            this.groupBoxImageSettings.Controls.Add(this.checkBoxBold);
            this.groupBoxImageSettings.Controls.Add(this.checkBoxSimpleRender);
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
            this.groupBoxImageSettings.Location = new System.Drawing.Point(12, 12);
            this.groupBoxImageSettings.Name = "groupBoxImageSettings";
            this.groupBoxImageSettings.Size = new System.Drawing.Size(964, 200);
            this.groupBoxImageSettings.TabIndex = 4;
            this.groupBoxImageSettings.TabStop = false;
            this.groupBoxImageSettings.Text = "Image settings";
            // 
            // comboBoxHAlign
            // 
            this.comboBoxHAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHAlign.DropDownWidth = 200;
            this.comboBoxHAlign.FormattingEnabled = true;
            this.comboBoxHAlign.Items.AddRange(new object[] {
            "Left",
            "Center",
            "Right",
            "Center - Left justify"});
            this.comboBoxHAlign.Location = new System.Drawing.Point(112, 78);
            this.comboBoxHAlign.Name = "comboBoxHAlign";
            this.comboBoxHAlign.Size = new System.Drawing.Size(123, 21);
            this.comboBoxHAlign.TabIndex = 63;
            this.comboBoxHAlign.SelectedIndexChanged += new System.EventHandler(this.textBoxText_TextChanged);
            // 
            // labelHorizontalAlign
            // 
            this.labelHorizontalAlign.AutoSize = true;
            this.labelHorizontalAlign.Location = new System.Drawing.Point(6, 81);
            this.labelHorizontalAlign.Name = "labelHorizontalAlign";
            this.labelHorizontalAlign.Size = new System.Drawing.Size(30, 13);
            this.labelHorizontalAlign.TabIndex = 62;
            this.labelHorizontalAlign.Text = "Align";
            // 
            // labelText
            // 
            this.labelText.AutoSize = true;
            this.labelText.Location = new System.Drawing.Point(6, 117);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(28, 13);
            this.labelText.TabIndex = 61;
            this.labelText.Text = "Text";
            // 
            // textBoxText
            // 
            this.textBoxText.Location = new System.Drawing.Point(9, 133);
            this.textBoxText.Multiline = true;
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.Size = new System.Drawing.Size(445, 61);
            this.textBoxText.TabIndex = 60;
            this.textBoxText.TextChanged += new System.EventHandler(this.textBoxText_TextChanged);
            this.textBoxText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxText_KeyDown);
            // 
            // labelLineHeight
            // 
            this.labelLineHeight.Location = new System.Drawing.Point(686, 112);
            this.labelLineHeight.Name = "labelLineHeight";
            this.labelLineHeight.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelLineHeight.Size = new System.Drawing.Size(110, 16);
            this.labelLineHeight.TabIndex = 54;
            this.labelLineHeight.Text = "Line height";
            this.labelLineHeight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericUpDownLineSpacing
            // 
            this.numericUpDownLineSpacing.Location = new System.Drawing.Point(802, 110);
            this.numericUpDownLineSpacing.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDownLineSpacing.Name = "numericUpDownLineSpacing";
            this.numericUpDownLineSpacing.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownLineSpacing.TabIndex = 44;
            this.numericUpDownLineSpacing.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownLineSpacing.ValueChanged += new System.EventHandler(this.numericUpDownLineSpacing_ValueChanged);
            // 
            // numericUpDownShadowTransparency
            // 
            this.numericUpDownShadowTransparency.Location = new System.Drawing.Point(802, 81);
            this.numericUpDownShadowTransparency.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownShadowTransparency.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownShadowTransparency.Name = "numericUpDownShadowTransparency";
            this.numericUpDownShadowTransparency.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownShadowTransparency.TabIndex = 43;
            this.numericUpDownShadowTransparency.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownShadowTransparency.ValueChanged += new System.EventHandler(this.textBoxText_TextChanged);
            // 
            // labelShadowTransparency
            // 
            this.labelShadowTransparency.Location = new System.Drawing.Point(686, 83);
            this.labelShadowTransparency.Name = "labelShadowTransparency";
            this.labelShadowTransparency.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelShadowTransparency.Size = new System.Drawing.Size(110, 13);
            this.labelShadowTransparency.TabIndex = 39;
            this.labelShadowTransparency.Text = "Transparency";
            this.labelShadowTransparency.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelShadowWidth
            // 
            this.labelShadowWidth.Location = new System.Drawing.Point(686, 55);
            this.labelShadowWidth.Name = "labelShadowWidth";
            this.labelShadowWidth.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelShadowWidth.Size = new System.Drawing.Size(110, 13);
            this.labelShadowWidth.TabIndex = 35;
            this.labelShadowWidth.Text = "Shadow width";
            this.labelShadowWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxShadowWidth
            // 
            this.comboBoxShadowWidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShadowWidth.FormattingEnabled = true;
            this.comboBoxShadowWidth.Items.AddRange(new object[] {
            "0",
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
            "15"});
            this.comboBoxShadowWidth.Location = new System.Drawing.Point(802, 52);
            this.comboBoxShadowWidth.Name = "comboBoxShadowWidth";
            this.comboBoxShadowWidth.Size = new System.Drawing.Size(121, 21);
            this.comboBoxShadowWidth.TabIndex = 42;
            this.comboBoxShadowWidth.SelectedValueChanged += new System.EventHandler(this.textBoxText_TextChanged);
            // 
            // panelShadowColor
            // 
            this.panelShadowColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelShadowColor.Location = new System.Drawing.Point(929, 26);
            this.panelShadowColor.Name = "panelShadowColor";
            this.panelShadowColor.Size = new System.Drawing.Size(21, 20);
            this.panelShadowColor.TabIndex = 41;
            // 
            // buttonShadowColor
            // 
            this.buttonShadowColor.Location = new System.Drawing.Point(802, 25);
            this.buttonShadowColor.Name = "buttonShadowColor";
            this.buttonShadowColor.Size = new System.Drawing.Size(121, 23);
            this.buttonShadowColor.TabIndex = 40;
            this.buttonShadowColor.Text = "Shadow color";
            this.buttonShadowColor.UseVisualStyleBackColor = true;
            this.buttonShadowColor.Click += new System.EventHandler(this.buttonShadowColor_Click);
            // 
            // checkBoxBold
            // 
            this.checkBoxBold.AutoSize = true;
            this.checkBoxBold.Location = new System.Drawing.Point(287, 50);
            this.checkBoxBold.Name = "checkBoxBold";
            this.checkBoxBold.Size = new System.Drawing.Size(47, 17);
            this.checkBoxBold.TabIndex = 19;
            this.checkBoxBold.Text = "Bold";
            this.checkBoxBold.UseVisualStyleBackColor = true;
            this.checkBoxBold.CheckedChanged += new System.EventHandler(this.textBoxText_TextChanged);
            // 
            // checkBoxSimpleRender
            // 
            this.checkBoxSimpleRender.AutoSize = true;
            this.checkBoxSimpleRender.Location = new System.Drawing.Point(287, 71);
            this.checkBoxSimpleRender.Name = "checkBoxSimpleRender";
            this.checkBoxSimpleRender.Size = new System.Drawing.Size(104, 17);
            this.checkBoxSimpleRender.TabIndex = 20;
            this.checkBoxSimpleRender.Text = "Simple rendering";
            this.checkBoxSimpleRender.UseVisualStyleBackColor = true;
            this.checkBoxSimpleRender.CheckedChanged += new System.EventHandler(this.textBoxText_TextChanged);
            // 
            // labelSubtitleFontSize
            // 
            this.labelSubtitleFontSize.AutoSize = true;
            this.labelSubtitleFontSize.Location = new System.Drawing.Point(6, 54);
            this.labelSubtitleFontSize.Name = "labelSubtitleFontSize";
            this.labelSubtitleFontSize.Size = new System.Drawing.Size(84, 13);
            this.labelSubtitleFontSize.TabIndex = 2;
            this.labelSubtitleFontSize.Text = "Subtitle font size";
            // 
            // comboBoxSubtitleFont
            // 
            this.comboBoxSubtitleFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFont.DropDownWidth = 210;
            this.comboBoxSubtitleFont.FormattingEnabled = true;
            this.comboBoxSubtitleFont.Location = new System.Drawing.Point(112, 24);
            this.comboBoxSubtitleFont.Name = "comboBoxSubtitleFont";
            this.comboBoxSubtitleFont.Size = new System.Drawing.Size(123, 21);
            this.comboBoxSubtitleFont.TabIndex = 1;
            this.comboBoxSubtitleFont.SelectedIndexChanged += new System.EventHandler(this.comboBoxSubtitleFontSize_SelectedIndexChanged);
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
            this.comboBoxSubtitleFontSize.Location = new System.Drawing.Point(112, 51);
            this.comboBoxSubtitleFontSize.Name = "comboBoxSubtitleFontSize";
            this.comboBoxSubtitleFontSize.Size = new System.Drawing.Size(123, 21);
            this.comboBoxSubtitleFontSize.TabIndex = 3;
            this.comboBoxSubtitleFontSize.SelectedIndexChanged += new System.EventHandler(this.comboBoxSubtitleFontSize_SelectedIndexChanged);
            // 
            // labelSubtitleFont
            // 
            this.labelSubtitleFont.AutoSize = true;
            this.labelSubtitleFont.Location = new System.Drawing.Point(6, 29);
            this.labelSubtitleFont.Name = "labelSubtitleFont";
            this.labelSubtitleFont.Size = new System.Drawing.Size(63, 13);
            this.labelSubtitleFont.TabIndex = 0;
            this.labelSubtitleFont.Text = "Subtitle font";
            // 
            // labelBorderWidth
            // 
            this.labelBorderWidth.Location = new System.Drawing.Point(427, 54);
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
            "15"});
            this.comboBoxBorderWidth.Location = new System.Drawing.Point(543, 51);
            this.comboBoxBorderWidth.Name = "comboBoxBorderWidth";
            this.comboBoxBorderWidth.Size = new System.Drawing.Size(128, 21);
            this.comboBoxBorderWidth.TabIndex = 32;
            this.comboBoxBorderWidth.SelectedValueChanged += new System.EventHandler(this.textBoxText_TextChanged);
            // 
            // panelBorderColor
            // 
            this.panelBorderColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBorderColor.Location = new System.Drawing.Point(677, 25);
            this.panelBorderColor.Name = "panelBorderColor";
            this.panelBorderColor.Size = new System.Drawing.Size(21, 20);
            this.panelBorderColor.TabIndex = 31;
            this.panelBorderColor.Click += new System.EventHandler(this.buttonBorderColor_Click);
            // 
            // buttonBorderColor
            // 
            this.buttonBorderColor.Location = new System.Drawing.Point(543, 24);
            this.buttonBorderColor.Name = "buttonBorderColor";
            this.buttonBorderColor.Size = new System.Drawing.Size(128, 23);
            this.buttonBorderColor.TabIndex = 30;
            this.buttonBorderColor.Text = "Border color";
            this.buttonBorderColor.UseVisualStyleBackColor = true;
            this.buttonBorderColor.Click += new System.EventHandler(this.buttonBorderColor_Click);
            // 
            // panelColor
            // 
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(414, 25);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(21, 20);
            this.panelColor.TabIndex = 12;
            this.panelColor.Click += new System.EventHandler(this.buttonColor_Click);
            // 
            // buttonColor
            // 
            this.buttonColor.Location = new System.Drawing.Point(287, 24);
            this.buttonColor.Name = "buttonColor";
            this.buttonColor.Size = new System.Drawing.Size(121, 23);
            this.buttonColor.TabIndex = 18;
            this.buttonColor.Text = "Color";
            this.buttonColor.UseVisualStyleBackColor = true;
            this.buttonColor.Click += new System.EventHandler(this.buttonColor_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(12, 218);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(964, 133);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(817, 357);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(901, 357);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // BinEditNewText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(988, 392);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBoxImageSettings);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1000, 430);
            this.Name = "BinEditNewText";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BinEditNewText";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BinEditNewText_FormClosing);
            this.Shown += new System.EventHandler(this.BinEditNewText_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BinEditNewText_KeyDown);
            this.groupBoxImageSettings.ResumeLayout(false);
            this.groupBoxImageSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLineSpacing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowTransparency)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxImageSettings;
        private System.Windows.Forms.Label labelLineHeight;
        private System.Windows.Forms.NumericUpDown numericUpDownLineSpacing;
        private System.Windows.Forms.NumericUpDown numericUpDownShadowTransparency;
        private System.Windows.Forms.Label labelShadowTransparency;
        private System.Windows.Forms.Label labelShadowWidth;
        private System.Windows.Forms.ComboBox comboBoxShadowWidth;
        private System.Windows.Forms.Panel panelShadowColor;
        private System.Windows.Forms.Button buttonShadowColor;
        private System.Windows.Forms.CheckBox checkBoxBold;
        private System.Windows.Forms.CheckBox checkBoxSimpleRender;
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
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ComboBox comboBoxHAlign;
        private System.Windows.Forms.Label labelHorizontalAlign;
    }
}