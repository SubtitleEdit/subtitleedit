namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class BatchConvertTsSettings
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelBottomMargin = new System.Windows.Forms.Label();
            this.checkBoxOverrideOriginalYPosition = new System.Windows.Forms.CheckBox();
            this.numericUpDownBottomMargin = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownWidth = new System.Windows.Forms.NumericUpDown();
            this.labelWidth = new System.Windows.Forms.Label();
            this.checkBoxOverrideVideoSize = new System.Windows.Forms.CheckBox();
            this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
            this.labelHeight = new System.Windows.Forms.Label();
            this.numericUpDownXMargin = new System.Windows.Forms.NumericUpDown();
            this.checkBoxOverrideOriginalXPosition = new System.Windows.Forms.CheckBox();
            this.labelXMargin = new System.Windows.Forms.Label();
            this.labelXAlignment = new System.Windows.Forms.Label();
            this.comboBoxHAlign = new System.Windows.Forms.ComboBox();
            this.labelXMarginPercent = new System.Windows.Forms.Label();
            this.labelBottomMarginPercent = new System.Windows.Forms.Label();
            this.buttonChooseResolution = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.labelFileNameEnding = new System.Windows.Forms.Label();
            this.textBoxFileNameAppend = new System.Windows.Forms.TextBox();
            this.contextMenuStripFileNameAppend = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.twolettercountrycodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.threelettercontrycodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.twolettercountrycodeuppercaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.threelettercountrycodeuppercaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelFileEndingSample = new System.Windows.Forms.Label();
            this.checkBoxOnlyTeletext = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBottomMargin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownXMargin)).BeginInit();
            this.contextMenuStripFileNameAppend.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(403, 278);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 90;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(484, 278);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 100;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelBottomMargin
            // 
            this.labelBottomMargin.AutoSize = true;
            this.labelBottomMargin.Location = new System.Drawing.Point(13, 137);
            this.labelBottomMargin.Name = "labelBottomMargin";
            this.labelBottomMargin.Size = new System.Drawing.Size(74, 13);
            this.labelBottomMargin.TabIndex = 10;
            this.labelBottomMargin.Text = "Bottom margin";
            // 
            // checkBoxOverrideOriginalYPosition
            // 
            this.checkBoxOverrideOriginalYPosition.AutoSize = true;
            this.checkBoxOverrideOriginalYPosition.Location = new System.Drawing.Point(16, 114);
            this.checkBoxOverrideOriginalYPosition.Name = "checkBoxOverrideOriginalYPosition";
            this.checkBoxOverrideOriginalYPosition.Size = new System.Drawing.Size(151, 17);
            this.checkBoxOverrideOriginalYPosition.TabIndex = 40;
            this.checkBoxOverrideOriginalYPosition.Text = "Override original Y position";
            this.checkBoxOverrideOriginalYPosition.UseVisualStyleBackColor = true;
            this.checkBoxOverrideOriginalYPosition.CheckedChanged += new System.EventHandler(this.CheckBoxOverrideOriginalYPosition_CheckedChanged);
            // 
            // numericUpDownBottomMargin
            // 
            this.numericUpDownBottomMargin.Location = new System.Drawing.Point(95, 135);
            this.numericUpDownBottomMargin.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownBottomMargin.Name = "numericUpDownBottomMargin";
            this.numericUpDownBottomMargin.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownBottomMargin.TabIndex = 50;
            // 
            // numericUpDownWidth
            // 
            this.numericUpDownWidth.Location = new System.Drawing.Point(65, 218);
            this.numericUpDownWidth.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownWidth.Name = "numericUpDownWidth";
            this.numericUpDownWidth.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownWidth.TabIndex = 70;
            this.numericUpDownWidth.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // labelWidth
            // 
            this.labelWidth.AutoSize = true;
            this.labelWidth.Location = new System.Drawing.Point(15, 220);
            this.labelWidth.Name = "labelWidth";
            this.labelWidth.Size = new System.Drawing.Size(35, 13);
            this.labelWidth.TabIndex = 14;
            this.labelWidth.Text = "Width";
            // 
            // checkBoxOverrideVideoSize
            // 
            this.checkBoxOverrideVideoSize.AutoSize = true;
            this.checkBoxOverrideVideoSize.Location = new System.Drawing.Point(18, 192);
            this.checkBoxOverrideVideoSize.Name = "checkBoxOverrideVideoSize";
            this.checkBoxOverrideVideoSize.Size = new System.Drawing.Size(149, 17);
            this.checkBoxOverrideVideoSize.TabIndex = 60;
            this.checkBoxOverrideVideoSize.Text = "Override output video size";
            this.checkBoxOverrideVideoSize.UseVisualStyleBackColor = true;
            this.checkBoxOverrideVideoSize.CheckedChanged += new System.EventHandler(this.CheckBoxOverrideVideoSize_CheckedChanged);
            // 
            // numericUpDownHeight
            // 
            this.numericUpDownHeight.Location = new System.Drawing.Point(65, 244);
            this.numericUpDownHeight.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownHeight.Name = "numericUpDownHeight";
            this.numericUpDownHeight.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownHeight.TabIndex = 80;
            this.numericUpDownHeight.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // labelHeight
            // 
            this.labelHeight.AutoSize = true;
            this.labelHeight.Location = new System.Drawing.Point(15, 246);
            this.labelHeight.Name = "labelHeight";
            this.labelHeight.Size = new System.Drawing.Size(38, 13);
            this.labelHeight.TabIndex = 17;
            this.labelHeight.Text = "Height";
            // 
            // numericUpDownXMargin
            // 
            this.numericUpDownXMargin.Location = new System.Drawing.Point(95, 45);
            this.numericUpDownXMargin.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownXMargin.Name = "numericUpDownXMargin";
            this.numericUpDownXMargin.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownXMargin.TabIndex = 20;
            // 
            // checkBoxOverrideOriginalXPosition
            // 
            this.checkBoxOverrideOriginalXPosition.AutoSize = true;
            this.checkBoxOverrideOriginalXPosition.Location = new System.Drawing.Point(16, 24);
            this.checkBoxOverrideOriginalXPosition.Name = "checkBoxOverrideOriginalXPosition";
            this.checkBoxOverrideOriginalXPosition.Size = new System.Drawing.Size(151, 17);
            this.checkBoxOverrideOriginalXPosition.TabIndex = 10;
            this.checkBoxOverrideOriginalXPosition.Text = "Override original X position";
            this.checkBoxOverrideOriginalXPosition.UseVisualStyleBackColor = true;
            this.checkBoxOverrideOriginalXPosition.CheckedChanged += new System.EventHandler(this.checkBoxOverrideOriginalXPosition_CheckedChanged);
            // 
            // labelXMargin
            // 
            this.labelXMargin.AutoSize = true;
            this.labelXMargin.Location = new System.Drawing.Point(13, 47);
            this.labelXMargin.Name = "labelXMargin";
            this.labelXMargin.Size = new System.Drawing.Size(39, 13);
            this.labelXMargin.TabIndex = 18;
            this.labelXMargin.Text = "Margin";
            // 
            // labelXAlignment
            // 
            this.labelXAlignment.AutoSize = true;
            this.labelXAlignment.Location = new System.Drawing.Point(13, 76);
            this.labelXAlignment.Name = "labelXAlignment";
            this.labelXAlignment.Size = new System.Drawing.Size(53, 13);
            this.labelXAlignment.TabIndex = 21;
            this.labelXAlignment.Text = "Alignment";
            // 
            // comboBoxHAlign
            // 
            this.comboBoxHAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHAlign.FormattingEnabled = true;
            this.comboBoxHAlign.Items.AddRange(new object[] {
            "Left",
            "Center",
            "Right",
            "Center - Left justify"});
            this.comboBoxHAlign.Location = new System.Drawing.Point(95, 73);
            this.comboBoxHAlign.Name = "comboBoxHAlign";
            this.comboBoxHAlign.Size = new System.Drawing.Size(123, 21);
            this.comboBoxHAlign.TabIndex = 30;
            // 
            // labelXMarginPercent
            // 
            this.labelXMarginPercent.AutoSize = true;
            this.labelXMarginPercent.Location = new System.Drawing.Point(161, 48);
            this.labelXMarginPercent.Name = "labelXMarginPercent";
            this.labelXMarginPercent.Size = new System.Drawing.Size(15, 13);
            this.labelXMarginPercent.TabIndex = 101;
            this.labelXMarginPercent.Text = "%";
            // 
            // labelBottomMarginPercent
            // 
            this.labelBottomMarginPercent.AutoSize = true;
            this.labelBottomMarginPercent.Location = new System.Drawing.Point(161, 138);
            this.labelBottomMarginPercent.Name = "labelBottomMarginPercent";
            this.labelBottomMarginPercent.Size = new System.Drawing.Size(15, 13);
            this.labelBottomMarginPercent.TabIndex = 102;
            this.labelBottomMarginPercent.Text = "%";
            // 
            // buttonChooseResolution
            // 
            this.buttonChooseResolution.Location = new System.Drawing.Point(64, 270);
            this.buttonChooseResolution.Name = "buttonChooseResolution";
            this.buttonChooseResolution.Size = new System.Drawing.Size(27, 23);
            this.buttonChooseResolution.TabIndex = 82;
            this.buttonChooseResolution.Text = "...";
            this.buttonChooseResolution.UseVisualStyleBackColor = true;
            this.buttonChooseResolution.Click += new System.EventHandler(this.ButtonChooseResolution_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // labelFileNameEnding
            // 
            this.labelFileNameEnding.AutoSize = true;
            this.labelFileNameEnding.Location = new System.Drawing.Point(267, 24);
            this.labelFileNameEnding.Name = "labelFileNameEnding";
            this.labelFileNameEnding.Size = new System.Drawing.Size(174, 13);
            this.labelFileNameEnding.TabIndex = 103;
            this.labelFileNameEnding.Text = "File name ending (before extension)";
            // 
            // textBoxFileNameAppend
            // 
            this.textBoxFileNameAppend.ContextMenuStrip = this.contextMenuStripFileNameAppend;
            this.textBoxFileNameAppend.Location = new System.Drawing.Point(270, 40);
            this.textBoxFileNameAppend.Name = "textBoxFileNameAppend";
            this.textBoxFileNameAppend.Size = new System.Drawing.Size(289, 20);
            this.textBoxFileNameAppend.TabIndex = 84;
            this.textBoxFileNameAppend.Text = ".{two-letter-country-code}";
            this.textBoxFileNameAppend.TextChanged += new System.EventHandler(this.TextBoxFileNameAppendTextChanged);
            // 
            // contextMenuStripFileNameAppend
            // 
            this.contextMenuStripFileNameAppend.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.twolettercountrycodeToolStripMenuItem,
            this.threelettercontrycodeToolStripMenuItem,
            this.twolettercountrycodeuppercaseToolStripMenuItem,
            this.threelettercountrycodeuppercaseToolStripMenuItem});
            this.contextMenuStripFileNameAppend.Name = "contextMenuStripFileNameAppend";
            this.contextMenuStripFileNameAppend.Size = new System.Drawing.Size(278, 92);
            // 
            // twolettercountrycodeToolStripMenuItem
            // 
            this.twolettercountrycodeToolStripMenuItem.Name = "twolettercountrycodeToolStripMenuItem";
            this.twolettercountrycodeToolStripMenuItem.Size = new System.Drawing.Size(277, 22);
            this.twolettercountrycodeToolStripMenuItem.Text = "{two-letter-country-code}";
            this.twolettercountrycodeToolStripMenuItem.Click += new System.EventHandler(this.TwoLetterCountryCodeToolStripMenuItemClick);
            // 
            // threelettercontrycodeToolStripMenuItem
            // 
            this.threelettercontrycodeToolStripMenuItem.Name = "threelettercontrycodeToolStripMenuItem";
            this.threelettercontrycodeToolStripMenuItem.Size = new System.Drawing.Size(277, 22);
            this.threelettercontrycodeToolStripMenuItem.Text = "{three-letter-contry-code}";
            this.threelettercontrycodeToolStripMenuItem.Click += new System.EventHandler(this.ThreeLetterCountryCodeToolStripMenuItemClick);
            // 
            // twolettercountrycodeuppercaseToolStripMenuItem
            // 
            this.twolettercountrycodeuppercaseToolStripMenuItem.Name = "twolettercountrycodeuppercaseToolStripMenuItem";
            this.twolettercountrycodeuppercaseToolStripMenuItem.Size = new System.Drawing.Size(277, 22);
            this.twolettercountrycodeuppercaseToolStripMenuItem.Text = "{two-letter-country-code-uppercase}";
            this.twolettercountrycodeuppercaseToolStripMenuItem.Click += new System.EventHandler(this.TwoLetterCountryCodeUppercaseToolStripMenuItemClick);
            // 
            // threelettercountrycodeuppercaseToolStripMenuItem
            // 
            this.threelettercountrycodeuppercaseToolStripMenuItem.Name = "threelettercountrycodeuppercaseToolStripMenuItem";
            this.threelettercountrycodeuppercaseToolStripMenuItem.Size = new System.Drawing.Size(277, 22);
            this.threelettercountrycodeuppercaseToolStripMenuItem.Text = "{three-letter-country-code-uppercase}";
            this.threelettercountrycodeuppercaseToolStripMenuItem.Click += new System.EventHandler(this.ThreeLetterCountryCodeUppercaseToolStripMenuItemClick);
            // 
            // labelFileEndingSample
            // 
            this.labelFileEndingSample.AutoSize = true;
            this.labelFileEndingSample.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelFileEndingSample.Location = new System.Drawing.Point(267, 63);
            this.labelFileEndingSample.Name = "labelFileEndingSample";
            this.labelFileEndingSample.Size = new System.Drawing.Size(174, 13);
            this.labelFileEndingSample.TabIndex = 104;
            this.labelFileEndingSample.Text = "File name ending (before extension)";
            // 
            // checkBoxOnlyTeletext
            // 
            this.checkBoxOnlyTeletext.AutoSize = true;
            this.checkBoxOnlyTeletext.Location = new System.Drawing.Point(270, 138);
            this.checkBoxOnlyTeletext.Name = "checkBoxOnlyTeletext";
            this.checkBoxOnlyTeletext.Size = new System.Drawing.Size(84, 17);
            this.checkBoxOnlyTeletext.TabIndex = 105;
            this.checkBoxOnlyTeletext.Text = "Only teletext";
            this.checkBoxOnlyTeletext.UseVisualStyleBackColor = true;
            // 
            // BatchConvertTsSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(571, 311);
            this.Controls.Add(this.checkBoxOnlyTeletext);
            this.Controls.Add(this.labelFileEndingSample);
            this.Controls.Add(this.textBoxFileNameAppend);
            this.Controls.Add(this.labelFileNameEnding);
            this.Controls.Add(this.buttonChooseResolution);
            this.Controls.Add(this.labelBottomMarginPercent);
            this.Controls.Add(this.labelXMarginPercent);
            this.Controls.Add(this.comboBoxHAlign);
            this.Controls.Add(this.labelXAlignment);
            this.Controls.Add(this.numericUpDownXMargin);
            this.Controls.Add(this.checkBoxOverrideOriginalXPosition);
            this.Controls.Add(this.labelXMargin);
            this.Controls.Add(this.numericUpDownHeight);
            this.Controls.Add(this.labelHeight);
            this.Controls.Add(this.checkBoxOverrideVideoSize);
            this.Controls.Add(this.numericUpDownWidth);
            this.Controls.Add(this.labelWidth);
            this.Controls.Add(this.numericUpDownBottomMargin);
            this.Controls.Add(this.checkBoxOverrideOriginalYPosition);
            this.Controls.Add(this.labelBottomMargin);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BatchConvertTsSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BatchConvertTsSettings";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BatchConvertTsSettings_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBottomMargin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownXMargin)).EndInit();
            this.contextMenuStripFileNameAppend.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelBottomMargin;
        private System.Windows.Forms.CheckBox checkBoxOverrideOriginalYPosition;
        private System.Windows.Forms.NumericUpDown numericUpDownBottomMargin;
        private System.Windows.Forms.NumericUpDown numericUpDownWidth;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.CheckBox checkBoxOverrideVideoSize;
        private System.Windows.Forms.NumericUpDown numericUpDownHeight;
        private System.Windows.Forms.Label labelHeight;
        private System.Windows.Forms.NumericUpDown numericUpDownXMargin;
        private System.Windows.Forms.CheckBox checkBoxOverrideOriginalXPosition;
        private System.Windows.Forms.Label labelXMargin;
        private System.Windows.Forms.Label labelXAlignment;
        private System.Windows.Forms.ComboBox comboBoxHAlign;
        private System.Windows.Forms.Label labelXMarginPercent;
        private System.Windows.Forms.Label labelBottomMarginPercent;
        private System.Windows.Forms.Button buttonChooseResolution;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label labelFileNameEnding;
        private System.Windows.Forms.TextBox textBoxFileNameAppend;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFileNameAppend;
        private System.Windows.Forms.ToolStripMenuItem twolettercountrycodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem threelettercontrycodeToolStripMenuItem;
        private System.Windows.Forms.Label labelFileEndingSample;
        private System.Windows.Forms.ToolStripMenuItem twolettercountrycodeuppercaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem threelettercountrycodeuppercaseToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxOnlyTeletext;
    }
}