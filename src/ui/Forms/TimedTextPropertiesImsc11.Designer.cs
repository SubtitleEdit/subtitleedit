namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class TimedTextPropertiesImsc11
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.textBoxTitle = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxFileExtensions = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelFileExtension = new System.Windows.Forms.Label();
            this.comboBoxTimeCodeFormat = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelTimeCode = new System.Windows.Forms.Label();
            this.comboBoxFrameRateMultiplier = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxDefaultRegion = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelDefaultRegion = new System.Windows.Forms.Label();
            this.comboBoxDropMode = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxFrameRate = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxTimeBase = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxDefaultStyle = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelWrapStyle = new System.Windows.Forms.Label();
            this.groupBoxOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(329, 342);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 98;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(410, 342);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 99;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOptions.Controls.Add(this.textBoxTitle);
            this.groupBoxOptions.Controls.Add(this.label6);
            this.groupBoxOptions.Controls.Add(this.comboBoxFileExtensions);
            this.groupBoxOptions.Controls.Add(this.labelFileExtension);
            this.groupBoxOptions.Controls.Add(this.comboBoxTimeCodeFormat);
            this.groupBoxOptions.Controls.Add(this.labelTimeCode);
            this.groupBoxOptions.Controls.Add(this.comboBoxFrameRateMultiplier);
            this.groupBoxOptions.Controls.Add(this.comboBoxDefaultRegion);
            this.groupBoxOptions.Controls.Add(this.labelDefaultRegion);
            this.groupBoxOptions.Controls.Add(this.comboBoxDropMode);
            this.groupBoxOptions.Controls.Add(this.label4);
            this.groupBoxOptions.Controls.Add(this.label3);
            this.groupBoxOptions.Controls.Add(this.comboBoxFrameRate);
            this.groupBoxOptions.Controls.Add(this.label2);
            this.groupBoxOptions.Controls.Add(this.comboBoxTimeBase);
            this.groupBoxOptions.Controls.Add(this.label1);
            this.groupBoxOptions.Controls.Add(this.comboBoxDefaultStyle);
            this.groupBoxOptions.Controls.Add(this.labelWrapStyle);
            this.groupBoxOptions.Location = new System.Drawing.Point(12, 12);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(473, 324);
            this.groupBoxOptions.TabIndex = 0;
            this.groupBoxOptions.TabStop = false;
            // 
            // textBoxTitle
            // 
            this.textBoxTitle.BackColor = System.Drawing.Color.DarkGray;
            this.textBoxTitle.CurrentLanguage = "";
            this.textBoxTitle.CurrentLineIndex = 0;
            this.textBoxTitle.HideSelection = true;
            this.textBoxTitle.IsDictionaryDownloaded = true;
            this.textBoxTitle.IsSpellCheckerInitialized = false;
            this.textBoxTitle.IsSpellCheckRequested = false;
            this.textBoxTitle.IsWrongWord = false;
            this.textBoxTitle.LanguageChanged = false;
            this.textBoxTitle.Location = new System.Drawing.Point(191, 19);
            this.textBoxTitle.MaxLength = 32767;
            this.textBoxTitle.Multiline = true;
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Padding = new System.Windows.Forms.Padding(1);
            this.textBoxTitle.ReadOnly = false;
            this.textBoxTitle.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.textBoxTitle.SelectedText = "";
            this.textBoxTitle.SelectionLength = 0;
            this.textBoxTitle.SelectionStart = 0;
            this.textBoxTitle.Size = new System.Drawing.Size(263, 20);
            this.textBoxTitle.TabIndex = 1;
            this.textBoxTitle.TextBoxFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTitle.UseSystemPasswordChar = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(27, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "Title";
            // 
            // comboBoxFileExtensions
            // 
            this.comboBoxFileExtensions.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFileExtensions.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFileExtensions.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFileExtensions.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFileExtensions.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFileExtensions.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFileExtensions.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFileExtensions.DropDownHeight = 400;
            this.comboBoxFileExtensions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFileExtensions.DropDownWidth = 263;
            this.comboBoxFileExtensions.FormattingEnabled = true;
            this.comboBoxFileExtensions.Items.AddRange(new string[] {
            ".xml",
            ".ttml",
            ".dfxp"});
            this.comboBoxFileExtensions.Location = new System.Drawing.Point(191, 276);
            this.comboBoxFileExtensions.MaxLength = 32767;
            this.comboBoxFileExtensions.Name = "comboBoxFileExtensions";
            this.comboBoxFileExtensions.SelectedIndex = -1;
            this.comboBoxFileExtensions.SelectedItem = null;
            this.comboBoxFileExtensions.SelectedText = "";
            this.comboBoxFileExtensions.Size = new System.Drawing.Size(263, 23);
            this.comboBoxFileExtensions.TabIndex = 21;
            this.comboBoxFileExtensions.UsePopupWindow = false;
            // 
            // labelFileExtension
            // 
            this.labelFileExtension.AutoSize = true;
            this.labelFileExtension.Location = new System.Drawing.Point(6, 279);
            this.labelFileExtension.Name = "labelFileExtension";
            this.labelFileExtension.Size = new System.Drawing.Size(71, 13);
            this.labelFileExtension.TabIndex = 22;
            this.labelFileExtension.Text = "File extension";
            // 
            // comboBoxTimeCodeFormat
            // 
            this.comboBoxTimeCodeFormat.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxTimeCodeFormat.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxTimeCodeFormat.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxTimeCodeFormat.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxTimeCodeFormat.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxTimeCodeFormat.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxTimeCodeFormat.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxTimeCodeFormat.DropDownHeight = 400;
            this.comboBoxTimeCodeFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTimeCodeFormat.DropDownWidth = 263;
            this.comboBoxTimeCodeFormat.FormattingEnabled = true;
            this.comboBoxTimeCodeFormat.Items.AddRange(new string[] {
            "Source",
            "Seconds",
            "Milliseconds",
            "Ticks",
            "hh:mm:ss:ff",
            "hh:mm:ss.ms",
            "hh:mm:ss.ms-two-digits",
            "hh:mm:ss,ms",
            "Frames"});
            this.comboBoxTimeCodeFormat.Location = new System.Drawing.Point(191, 239);
            this.comboBoxTimeCodeFormat.MaxLength = 32767;
            this.comboBoxTimeCodeFormat.Name = "comboBoxTimeCodeFormat";
            this.comboBoxTimeCodeFormat.SelectedIndex = -1;
            this.comboBoxTimeCodeFormat.SelectedItem = null;
            this.comboBoxTimeCodeFormat.SelectedText = "";
            this.comboBoxTimeCodeFormat.Size = new System.Drawing.Size(263, 23);
            this.comboBoxTimeCodeFormat.TabIndex = 9;
            this.comboBoxTimeCodeFormat.UsePopupWindow = false;
            // 
            // labelTimeCode
            // 
            this.labelTimeCode.AutoSize = true;
            this.labelTimeCode.Location = new System.Drawing.Point(6, 242);
            this.labelTimeCode.Name = "labelTimeCode";
            this.labelTimeCode.Size = new System.Drawing.Size(89, 13);
            this.labelTimeCode.TabIndex = 20;
            this.labelTimeCode.Text = "Time code format";
            // 
            // comboBoxFrameRateMultiplier
            // 
            this.comboBoxFrameRateMultiplier.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFrameRateMultiplier.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFrameRateMultiplier.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFrameRateMultiplier.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFrameRateMultiplier.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFrameRateMultiplier.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFrameRateMultiplier.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFrameRateMultiplier.DropDownHeight = 400;
            this.comboBoxFrameRateMultiplier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxFrameRateMultiplier.DropDownWidth = 263;
            this.comboBoxFrameRateMultiplier.FormattingEnabled = true;
            this.comboBoxFrameRateMultiplier.Items.AddRange(new string[] {
            "999 1000",
            "1 1",
            "1000 1001"});
            this.comboBoxFrameRateMultiplier.Location = new System.Drawing.Point(191, 106);
            this.comboBoxFrameRateMultiplier.MaxLength = 32767;
            this.comboBoxFrameRateMultiplier.Name = "comboBoxFrameRateMultiplier";
            this.comboBoxFrameRateMultiplier.SelectedIndex = -1;
            this.comboBoxFrameRateMultiplier.SelectedItem = null;
            this.comboBoxFrameRateMultiplier.SelectedText = "";
            this.comboBoxFrameRateMultiplier.Size = new System.Drawing.Size(263, 23);
            this.comboBoxFrameRateMultiplier.TabIndex = 5;
            this.comboBoxFrameRateMultiplier.TabStop = false;
            this.comboBoxFrameRateMultiplier.UsePopupWindow = false;
            // 
            // comboBoxDefaultRegion
            // 
            this.comboBoxDefaultRegion.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxDefaultRegion.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxDefaultRegion.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxDefaultRegion.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxDefaultRegion.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxDefaultRegion.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxDefaultRegion.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxDefaultRegion.DropDownHeight = 400;
            this.comboBoxDefaultRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDefaultRegion.DropDownWidth = 263;
            this.comboBoxDefaultRegion.FormattingEnabled = true;
            this.comboBoxDefaultRegion.Location = new System.Drawing.Point(191, 200);
            this.comboBoxDefaultRegion.MaxLength = 32767;
            this.comboBoxDefaultRegion.Name = "comboBoxDefaultRegion";
            this.comboBoxDefaultRegion.SelectedIndex = -1;
            this.comboBoxDefaultRegion.SelectedItem = null;
            this.comboBoxDefaultRegion.SelectedText = "";
            this.comboBoxDefaultRegion.Size = new System.Drawing.Size(263, 23);
            this.comboBoxDefaultRegion.TabIndex = 8;
            this.comboBoxDefaultRegion.UsePopupWindow = false;
            // 
            // labelDefaultRegion
            // 
            this.labelDefaultRegion.AutoSize = true;
            this.labelDefaultRegion.Location = new System.Drawing.Point(6, 203);
            this.labelDefaultRegion.Name = "labelDefaultRegion";
            this.labelDefaultRegion.Size = new System.Drawing.Size(73, 13);
            this.labelDefaultRegion.TabIndex = 12;
            this.labelDefaultRegion.Text = "Default region";
            // 
            // comboBoxDropMode
            // 
            this.comboBoxDropMode.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxDropMode.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxDropMode.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxDropMode.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxDropMode.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxDropMode.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxDropMode.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxDropMode.DropDownHeight = 400;
            this.comboBoxDropMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDropMode.DropDownWidth = 263;
            this.comboBoxDropMode.FormattingEnabled = true;
            this.comboBoxDropMode.Items.AddRange(new string[] {
            "[N/A]",
            "dropNTSC",
            "dropPAL",
            "nonDrop"});
            this.comboBoxDropMode.Location = new System.Drawing.Point(191, 133);
            this.comboBoxDropMode.MaxLength = 32767;
            this.comboBoxDropMode.Name = "comboBoxDropMode";
            this.comboBoxDropMode.SelectedIndex = -1;
            this.comboBoxDropMode.SelectedItem = null;
            this.comboBoxDropMode.SelectedText = "";
            this.comboBoxDropMode.Size = new System.Drawing.Size(263, 23);
            this.comboBoxDropMode.TabIndex = 6;
            this.comboBoxDropMode.UsePopupWindow = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 136);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Drop mode";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 109);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Frame rate multiplier";
            // 
            // comboBoxFrameRate
            // 
            this.comboBoxFrameRate.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFrameRate.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFrameRate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFrameRate.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFrameRate.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFrameRate.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFrameRate.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFrameRate.DropDownHeight = 400;
            this.comboBoxFrameRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxFrameRate.DropDownWidth = 263;
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Location = new System.Drawing.Point(191, 79);
            this.comboBoxFrameRate.MaxLength = 32767;
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.SelectedIndex = -1;
            this.comboBoxFrameRate.SelectedItem = null;
            this.comboBoxFrameRate.SelectedText = "";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(263, 23);
            this.comboBoxFrameRate.TabIndex = 4;
            this.comboBoxFrameRate.TabStop = false;
            this.comboBoxFrameRate.UsePopupWindow = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Frame rate";
            // 
            // comboBoxTimeBase
            // 
            this.comboBoxTimeBase.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxTimeBase.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxTimeBase.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxTimeBase.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxTimeBase.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxTimeBase.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxTimeBase.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxTimeBase.DropDownHeight = 400;
            this.comboBoxTimeBase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTimeBase.DropDownWidth = 263;
            this.comboBoxTimeBase.FormattingEnabled = true;
            this.comboBoxTimeBase.Items.AddRange(new string[] {
            "[N/A]",
            "media",
            "smpte",
            "clock"});
            this.comboBoxTimeBase.Location = new System.Drawing.Point(191, 52);
            this.comboBoxTimeBase.MaxLength = 32767;
            this.comboBoxTimeBase.Name = "comboBoxTimeBase";
            this.comboBoxTimeBase.SelectedIndex = -1;
            this.comboBoxTimeBase.SelectedItem = null;
            this.comboBoxTimeBase.SelectedText = "";
            this.comboBoxTimeBase.Size = new System.Drawing.Size(263, 23);
            this.comboBoxTimeBase.TabIndex = 3;
            this.comboBoxTimeBase.UsePopupWindow = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Time base";
            // 
            // comboBoxDefaultStyle
            // 
            this.comboBoxDefaultStyle.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxDefaultStyle.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxDefaultStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxDefaultStyle.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxDefaultStyle.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxDefaultStyle.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxDefaultStyle.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxDefaultStyle.DropDownHeight = 400;
            this.comboBoxDefaultStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDefaultStyle.DropDownWidth = 263;
            this.comboBoxDefaultStyle.FormattingEnabled = true;
            this.comboBoxDefaultStyle.Location = new System.Drawing.Point(191, 173);
            this.comboBoxDefaultStyle.MaxLength = 32767;
            this.comboBoxDefaultStyle.Name = "comboBoxDefaultStyle";
            this.comboBoxDefaultStyle.SelectedIndex = -1;
            this.comboBoxDefaultStyle.SelectedItem = null;
            this.comboBoxDefaultStyle.SelectedText = "";
            this.comboBoxDefaultStyle.Size = new System.Drawing.Size(263, 23);
            this.comboBoxDefaultStyle.TabIndex = 7;
            this.comboBoxDefaultStyle.UsePopupWindow = false;
            // 
            // labelWrapStyle
            // 
            this.labelWrapStyle.AutoSize = true;
            this.labelWrapStyle.Location = new System.Drawing.Point(6, 176);
            this.labelWrapStyle.Name = "labelWrapStyle";
            this.labelWrapStyle.Size = new System.Drawing.Size(65, 13);
            this.labelWrapStyle.TabIndex = 1;
            this.labelWrapStyle.Text = "Default style";
            // 
            // TimedTextPropertiesImsc11
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 375);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxOptions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TimedTextPropertiesImsc11";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Timed Text properties";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TimedTextProperties_KeyDown);
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxDropMode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFrameRate;
        private System.Windows.Forms.Label label2;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxTimeBase;
        private System.Windows.Forms.Label label1;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxDefaultStyle;
        private System.Windows.Forms.Label labelWrapStyle;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxDefaultRegion;
        private System.Windows.Forms.Label labelDefaultRegion;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFrameRateMultiplier;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxTimeCodeFormat;
        private System.Windows.Forms.Label labelTimeCode;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFileExtensions;
        private System.Windows.Forms.Label labelFileExtension;
        private Controls.SETextBox textBoxTitle;
        private System.Windows.Forms.Label label6;
    }
}