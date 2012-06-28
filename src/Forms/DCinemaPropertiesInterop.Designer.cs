namespace Nikse.SubtitleEdit.Forms
{
    partial class DCinemaPropertiesInterop
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
            this.panelFontEffectColor = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.numericUpDownReelNumber = new System.Windows.Forms.NumericUpDown();
            this.buttonGenerateID = new System.Windows.Forms.Button();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.buttonFontEffectColor = new System.Windows.Forms.Button();
            this.panelFontColor = new System.Windows.Forms.Panel();
            this.buttonFontColor = new System.Windows.Forms.Button();
            this.labelEffectColor = new System.Windows.Forms.Label();
            this.numericUpDownFontSize = new System.Windows.Forms.NumericUpDown();
            this.labelFontSize = new System.Windows.Forms.Label();
            this.comboBoxFontEffect = new System.Windows.Forms.ComboBox();
            this.labelEffect = new System.Windows.Forms.Label();
            this.labelFontColor = new System.Windows.Forms.Label();
            this.textBoxFontID = new System.Windows.Forms.TextBox();
            this.labelFontId = new System.Windows.Forms.Label();
            this.textBoxFontUri = new System.Windows.Forms.TextBox();
            this.labelFontUri = new System.Windows.Forms.Label();
            this.labelReelNumber = new System.Windows.Forms.Label();
            this.textBoxMovieTitle = new System.Windows.Forms.TextBox();
            this.labelMovieTitle = new System.Windows.Forms.Label();
            this.textBoxSubtitleID = new System.Windows.Forms.TextBox();
            this.labelSubtitleID = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownReelNumber)).BeginInit();
            this.groupBoxFont.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).BeginInit();
            this.SuspendLayout();
            // 
            // panelFontEffectColor
            // 
            this.panelFontEffectColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFontEffectColor.Location = new System.Drawing.Point(224, 128);
            this.panelFontEffectColor.Name = "panelFontEffectColor";
            this.panelFontEffectColor.Size = new System.Drawing.Size(21, 20);
            this.panelFontEffectColor.TabIndex = 6;
            this.panelFontEffectColor.Click += new System.EventHandler(this.buttonFontEffectColor_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.comboBoxLanguage);
            this.groupBox1.Controls.Add(this.labelLanguage);
            this.groupBox1.Controls.Add(this.numericUpDownReelNumber);
            this.groupBox1.Controls.Add(this.buttonGenerateID);
            this.groupBox1.Controls.Add(this.groupBoxFont);
            this.groupBox1.Controls.Add(this.labelReelNumber);
            this.groupBox1.Controls.Add(this.textBoxMovieTitle);
            this.groupBox1.Controls.Add(this.labelMovieTitle);
            this.groupBox1.Controls.Add(this.textBoxSubtitleID);
            this.groupBox1.Controls.Add(this.labelSubtitleID);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(444, 338);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(108, 109);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(233, 21);
            this.comboBoxLanguage.TabIndex = 4;
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(15, 112);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(55, 13);
            this.labelLanguage.TabIndex = 32;
            this.labelLanguage.Text = "Language";
            // 
            // numericUpDownReelNumber
            // 
            this.numericUpDownReelNumber.Location = new System.Drawing.Point(108, 83);
            this.numericUpDownReelNumber.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.numericUpDownReelNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownReelNumber.Name = "numericUpDownReelNumber";
            this.numericUpDownReelNumber.Size = new System.Drawing.Size(112, 20);
            this.numericUpDownReelNumber.TabIndex = 3;
            this.numericUpDownReelNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonGenerateID
            // 
            this.buttonGenerateID.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenerateID.Location = new System.Drawing.Point(338, 29);
            this.buttonGenerateID.Name = "buttonGenerateID";
            this.buttonGenerateID.Size = new System.Drawing.Size(100, 21);
            this.buttonGenerateID.TabIndex = 1;
            this.buttonGenerateID.Text = "Generate ID";
            this.buttonGenerateID.UseVisualStyleBackColor = true;
            this.buttonGenerateID.Click += new System.EventHandler(this.buttonGenerateID_Click);
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFont.Controls.Add(this.panelFontEffectColor);
            this.groupBoxFont.Controls.Add(this.buttonFontEffectColor);
            this.groupBoxFont.Controls.Add(this.panelFontColor);
            this.groupBoxFont.Controls.Add(this.buttonFontColor);
            this.groupBoxFont.Controls.Add(this.labelEffectColor);
            this.groupBoxFont.Controls.Add(this.numericUpDownFontSize);
            this.groupBoxFont.Controls.Add(this.labelFontSize);
            this.groupBoxFont.Controls.Add(this.comboBoxFontEffect);
            this.groupBoxFont.Controls.Add(this.labelEffect);
            this.groupBoxFont.Controls.Add(this.labelFontColor);
            this.groupBoxFont.Controls.Add(this.textBoxFontID);
            this.groupBoxFont.Controls.Add(this.labelFontId);
            this.groupBoxFont.Controls.Add(this.textBoxFontUri);
            this.groupBoxFont.Controls.Add(this.labelFontUri);
            this.groupBoxFont.Location = new System.Drawing.Point(6, 141);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(432, 191);
            this.groupBoxFont.TabIndex = 5;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "Font";
            // 
            // buttonFontEffectColor
            // 
            this.buttonFontEffectColor.Location = new System.Drawing.Point(102, 128);
            this.buttonFontEffectColor.Name = "buttonFontEffectColor";
            this.buttonFontEffectColor.Size = new System.Drawing.Size(112, 21);
            this.buttonFontEffectColor.TabIndex = 5;
            this.buttonFontEffectColor.Text = "Choose color";
            this.buttonFontEffectColor.UseVisualStyleBackColor = true;
            this.buttonFontEffectColor.Click += new System.EventHandler(this.buttonFontEffectColor_Click);
            // 
            // panelFontColor
            // 
            this.panelFontColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFontColor.Location = new System.Drawing.Point(224, 75);
            this.panelFontColor.Name = "panelFontColor";
            this.panelFontColor.Size = new System.Drawing.Size(21, 20);
            this.panelFontColor.TabIndex = 3;
            this.panelFontColor.Click += new System.EventHandler(this.buttonFontColor_Click);
            // 
            // buttonFontColor
            // 
            this.buttonFontColor.Location = new System.Drawing.Point(102, 75);
            this.buttonFontColor.Name = "buttonFontColor";
            this.buttonFontColor.Size = new System.Drawing.Size(112, 21);
            this.buttonFontColor.TabIndex = 2;
            this.buttonFontColor.Text = "Choose color";
            this.buttonFontColor.UseVisualStyleBackColor = true;
            this.buttonFontColor.Click += new System.EventHandler(this.buttonFontColor_Click);
            // 
            // labelEffectColor
            // 
            this.labelEffectColor.AutoSize = true;
            this.labelEffectColor.Location = new System.Drawing.Point(9, 132);
            this.labelEffectColor.Name = "labelEffectColor";
            this.labelEffectColor.Size = new System.Drawing.Size(61, 13);
            this.labelEffectColor.TabIndex = 27;
            this.labelEffectColor.Text = "Effect color";
            // 
            // numericUpDownFontSize
            // 
            this.numericUpDownFontSize.Location = new System.Drawing.Point(102, 155);
            this.numericUpDownFontSize.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.numericUpDownFontSize.Name = "numericUpDownFontSize";
            this.numericUpDownFontSize.Size = new System.Drawing.Size(112, 20);
            this.numericUpDownFontSize.TabIndex = 7;
            // 
            // labelFontSize
            // 
            this.labelFontSize.AutoSize = true;
            this.labelFontSize.Location = new System.Drawing.Point(9, 162);
            this.labelFontSize.Name = "labelFontSize";
            this.labelFontSize.Size = new System.Drawing.Size(27, 13);
            this.labelFontSize.TabIndex = 25;
            this.labelFontSize.Text = "Size";
            // 
            // comboBoxFontEffect
            // 
            this.comboBoxFontEffect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontEffect.FormattingEnabled = true;
            this.comboBoxFontEffect.Items.AddRange(new object[] {
            "None",
            "Border",
            "Shadow"});
            this.comboBoxFontEffect.Location = new System.Drawing.Point(102, 102);
            this.comboBoxFontEffect.Name = "comboBoxFontEffect";
            this.comboBoxFontEffect.Size = new System.Drawing.Size(112, 21);
            this.comboBoxFontEffect.TabIndex = 4;
            // 
            // labelEffect
            // 
            this.labelEffect.AutoSize = true;
            this.labelEffect.Location = new System.Drawing.Point(9, 105);
            this.labelEffect.Name = "labelEffect";
            this.labelEffect.Size = new System.Drawing.Size(35, 13);
            this.labelEffect.TabIndex = 22;
            this.labelEffect.Text = "Effect";
            // 
            // labelFontColor
            // 
            this.labelFontColor.AutoSize = true;
            this.labelFontColor.Location = new System.Drawing.Point(9, 79);
            this.labelFontColor.Name = "labelFontColor";
            this.labelFontColor.Size = new System.Drawing.Size(31, 13);
            this.labelFontColor.TabIndex = 18;
            this.labelFontColor.Text = "Color";
            // 
            // textBoxFontID
            // 
            this.textBoxFontID.Location = new System.Drawing.Point(102, 24);
            this.textBoxFontID.Name = "textBoxFontID";
            this.textBoxFontID.Size = new System.Drawing.Size(224, 20);
            this.textBoxFontID.TabIndex = 0;
            this.textBoxFontID.Text = "Freds_Font";
            // 
            // labelFontId
            // 
            this.labelFontId.AutoSize = true;
            this.labelFontId.Location = new System.Drawing.Point(9, 27);
            this.labelFontId.Name = "labelFontId";
            this.labelFontId.Size = new System.Drawing.Size(18, 13);
            this.labelFontId.TabIndex = 16;
            this.labelFontId.Text = "ID";
            // 
            // textBoxFontUri
            // 
            this.textBoxFontUri.Location = new System.Drawing.Point(102, 50);
            this.textBoxFontUri.Name = "textBoxFontUri";
            this.textBoxFontUri.Size = new System.Drawing.Size(224, 20);
            this.textBoxFontUri.TabIndex = 1;
            this.textBoxFontUri.Text = "Fred.ttf";
            // 
            // labelFontUri
            // 
            this.labelFontUri.AutoSize = true;
            this.labelFontUri.Location = new System.Drawing.Point(9, 53);
            this.labelFontUri.Name = "labelFontUri";
            this.labelFontUri.Size = new System.Drawing.Size(26, 13);
            this.labelFontUri.TabIndex = 14;
            this.labelFontUri.Text = "URI";
            // 
            // labelReelNumber
            // 
            this.labelReelNumber.AutoSize = true;
            this.labelReelNumber.Location = new System.Drawing.Point(15, 85);
            this.labelReelNumber.Name = "labelReelNumber";
            this.labelReelNumber.Size = new System.Drawing.Size(67, 13);
            this.labelReelNumber.TabIndex = 4;
            this.labelReelNumber.Text = "Reel number";
            // 
            // textBoxMovieTitle
            // 
            this.textBoxMovieTitle.Location = new System.Drawing.Point(108, 56);
            this.textBoxMovieTitle.Name = "textBoxMovieTitle";
            this.textBoxMovieTitle.Size = new System.Drawing.Size(224, 20);
            this.textBoxMovieTitle.TabIndex = 2;
            this.textBoxMovieTitle.Text = "Bedtime For Bonzo";
            // 
            // labelMovieTitle
            // 
            this.labelMovieTitle.AutoSize = true;
            this.labelMovieTitle.Location = new System.Drawing.Point(15, 59);
            this.labelMovieTitle.Name = "labelMovieTitle";
            this.labelMovieTitle.Size = new System.Drawing.Size(55, 13);
            this.labelMovieTitle.TabIndex = 2;
            this.labelMovieTitle.Text = "Movie title";
            // 
            // textBoxSubtitleID
            // 
            this.textBoxSubtitleID.Location = new System.Drawing.Point(108, 30);
            this.textBoxSubtitleID.Name = "textBoxSubtitleID";
            this.textBoxSubtitleID.Size = new System.Drawing.Size(224, 20);
            this.textBoxSubtitleID.TabIndex = 0;
            this.textBoxSubtitleID.Text = "40950d85-63eb-4ee2-b1e8-45c126601b94";
            // 
            // labelSubtitleID
            // 
            this.labelSubtitleID.AutoSize = true;
            this.labelSubtitleID.Location = new System.Drawing.Point(15, 33);
            this.labelSubtitleID.Name = "labelSubtitleID";
            this.labelSubtitleID.Size = new System.Drawing.Size(56, 13);
            this.labelSubtitleID.TabIndex = 0;
            this.labelSubtitleID.Text = "Subtitle ID";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(381, 356);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(300, 356);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // DCinemaPropertiesInterop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 389);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DCinemaPropertiesInterop";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "D-Cinema properties (interop)";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownReelNumber)).EndInit();
            this.groupBoxFont.ResumeLayout(false);
            this.groupBoxFont.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelFontEffectColor;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBoxFont;
        private System.Windows.Forms.Button buttonFontEffectColor;
        private System.Windows.Forms.Panel panelFontColor;
        private System.Windows.Forms.Button buttonFontColor;
        private System.Windows.Forms.Label labelEffectColor;
        private System.Windows.Forms.NumericUpDown numericUpDownFontSize;
        private System.Windows.Forms.Label labelFontSize;
        private System.Windows.Forms.ComboBox comboBoxFontEffect;
        private System.Windows.Forms.Label labelEffect;
        private System.Windows.Forms.Label labelFontColor;
        private System.Windows.Forms.TextBox textBoxFontID;
        private System.Windows.Forms.Label labelFontId;
        private System.Windows.Forms.TextBox textBoxFontUri;
        private System.Windows.Forms.Label labelFontUri;
        private System.Windows.Forms.Label labelReelNumber;
        private System.Windows.Forms.TextBox textBoxMovieTitle;
        private System.Windows.Forms.Label labelMovieTitle;
        private System.Windows.Forms.TextBox textBoxSubtitleID;
        private System.Windows.Forms.Label labelSubtitleID;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button buttonGenerateID;
        private System.Windows.Forms.NumericUpDown numericUpDownReelNumber;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelLanguage;
    }
}