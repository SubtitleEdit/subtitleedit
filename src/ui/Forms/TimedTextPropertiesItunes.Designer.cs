namespace Nikse.SubtitleEdit.Forms
{
    partial class TimedTextPropertiesItunes
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
            this.comboBoxStyleAttribute = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelStyleAttributeName = new System.Windows.Forms.Label();
            this.groupBoxAlignment = new System.Windows.Forms.GroupBox();
            this.textBoxBottomExtent = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.labelBottomExtent = new System.Windows.Forms.Label();
            this.textBoxBottomOrigin = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.labelBottomOrigin = new System.Windows.Forms.Label();
            this.textBoxTopExtent = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.labelTopExtent = new System.Windows.Forms.Label();
            this.textBoxTopOrigin = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.labelTopOrigin = new System.Windows.Forms.Label();
            this.comboBoxFileExtensions = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelFileExtension = new System.Windows.Forms.Label();
            this.comboBoxTimeCodeFormat = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelTimeCode = new System.Windows.Forms.Label();
            this.comboBoxFrameRateMultiplier = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.textBoxDescription = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxTitle = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxDefaultRegion = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelDefaultRegion = new System.Windows.Forms.Label();
            this.comboBoxDropMode = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxFrameRate = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxTimeBase = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelCollision = new System.Windows.Forms.Label();
            this.comboBoxDefaultStyle = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelDefaultStyle = new System.Windows.Forms.Label();
            this.groupBoxOptions.SuspendLayout();
            this.groupBoxAlignment.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(745, 439);
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
            this.buttonCancel.Location = new System.Drawing.Point(826, 439);
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
            this.groupBoxOptions.Controls.Add(this.comboBoxStyleAttribute);
            this.groupBoxOptions.Controls.Add(this.labelStyleAttributeName);
            this.groupBoxOptions.Controls.Add(this.groupBoxAlignment);
            this.groupBoxOptions.Controls.Add(this.comboBoxFileExtensions);
            this.groupBoxOptions.Controls.Add(this.labelFileExtension);
            this.groupBoxOptions.Controls.Add(this.comboBoxTimeCodeFormat);
            this.groupBoxOptions.Controls.Add(this.labelTimeCode);
            this.groupBoxOptions.Controls.Add(this.comboBoxFrameRateMultiplier);
            this.groupBoxOptions.Controls.Add(this.textBoxDescription);
            this.groupBoxOptions.Controls.Add(this.label7);
            this.groupBoxOptions.Controls.Add(this.textBoxTitle);
            this.groupBoxOptions.Controls.Add(this.label6);
            this.groupBoxOptions.Controls.Add(this.comboBoxDefaultRegion);
            this.groupBoxOptions.Controls.Add(this.labelDefaultRegion);
            this.groupBoxOptions.Controls.Add(this.comboBoxDropMode);
            this.groupBoxOptions.Controls.Add(this.label4);
            this.groupBoxOptions.Controls.Add(this.label3);
            this.groupBoxOptions.Controls.Add(this.comboBoxFrameRate);
            this.groupBoxOptions.Controls.Add(this.label2);
            this.groupBoxOptions.Controls.Add(this.comboBoxTimeBase);
            this.groupBoxOptions.Controls.Add(this.label1);
            this.groupBoxOptions.Controls.Add(this.comboBoxLanguage);
            this.groupBoxOptions.Controls.Add(this.labelCollision);
            this.groupBoxOptions.Controls.Add(this.comboBoxDefaultStyle);
            this.groupBoxOptions.Controls.Add(this.labelDefaultStyle);
            this.groupBoxOptions.Location = new System.Drawing.Point(12, 12);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(889, 421);
            this.groupBoxOptions.TabIndex = 0;
            this.groupBoxOptions.TabStop = false;
            // 
            // comboBoxStyleAttribute
            // 
            this.comboBoxStyleAttribute.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxStyleAttribute.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxStyleAttribute.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxStyleAttribute.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxStyleAttribute.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxStyleAttribute.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxStyleAttribute.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxStyleAttribute.DropDownHeight = 400;
            this.comboBoxStyleAttribute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStyleAttribute.DropDownWidth = 263;
            this.comboBoxStyleAttribute.FormattingEnabled = true;
            this.comboBoxStyleAttribute.Items.AddRange(new object[] {
            "tts:fontStyle",
            "style"});
            this.comboBoxStyleAttribute.Location = new System.Drawing.Point(191, 270);
            this.comboBoxStyleAttribute.MaxLength = 32767;
            this.comboBoxStyleAttribute.Name = "comboBoxStyleAttribute";
            this.comboBoxStyleAttribute.SelectedIndex = -1;
            this.comboBoxStyleAttribute.SelectedItem = null;
            this.comboBoxStyleAttribute.SelectedText = "";
            this.comboBoxStyleAttribute.Size = new System.Drawing.Size(263, 21);
            this.comboBoxStyleAttribute.TabIndex = 8;
            // 
            // labelStyleAttributeName
            // 
            this.labelStyleAttributeName.AutoSize = true;
            this.labelStyleAttributeName.Location = new System.Drawing.Point(6, 273);
            this.labelStyleAttributeName.Name = "labelStyleAttributeName";
            this.labelStyleAttributeName.Size = new System.Drawing.Size(100, 13);
            this.labelStyleAttributeName.TabIndex = 24;
            this.labelStyleAttributeName.Text = "Style attribute name";
            // 
            // groupBoxAlignment
            // 
            this.groupBoxAlignment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAlignment.Controls.Add(this.textBoxBottomExtent);
            this.groupBoxAlignment.Controls.Add(this.labelBottomExtent);
            this.groupBoxAlignment.Controls.Add(this.textBoxBottomOrigin);
            this.groupBoxAlignment.Controls.Add(this.labelBottomOrigin);
            this.groupBoxAlignment.Controls.Add(this.textBoxTopExtent);
            this.groupBoxAlignment.Controls.Add(this.labelTopExtent);
            this.groupBoxAlignment.Controls.Add(this.textBoxTopOrigin);
            this.groupBoxAlignment.Controls.Add(this.labelTopOrigin);
            this.groupBoxAlignment.Location = new System.Drawing.Point(485, 30);
            this.groupBoxAlignment.Name = "groupBoxAlignment";
            this.groupBoxAlignment.Size = new System.Drawing.Size(398, 375);
            this.groupBoxAlignment.TabIndex = 23;
            this.groupBoxAlignment.TabStop = false;
            this.groupBoxAlignment.Text = "Alignment";
            // 
            // textBoxBottomExtent
            // 
            this.textBoxBottomExtent.BackColor = System.Drawing.Color.DarkGray;
            this.textBoxBottomExtent.CurrentLanguage = "";
            this.textBoxBottomExtent.CurrentLineIndex = 0;
            this.textBoxBottomExtent.HideSelection = true;
            this.textBoxBottomExtent.IsDictionaryDownloaded = true;
            this.textBoxBottomExtent.IsSpellCheckerInitialized = false;
            this.textBoxBottomExtent.IsSpellCheckRequested = false;
            this.textBoxBottomExtent.IsWrongWord = false;
            this.textBoxBottomExtent.LanguageChanged = false;
            this.textBoxBottomExtent.Location = new System.Drawing.Point(135, 141);
            this.textBoxBottomExtent.MaxLength = 32767;
            this.textBoxBottomExtent.Multiline = true;
            this.textBoxBottomExtent.Name = "textBoxBottomExtent";
            this.textBoxBottomExtent.Padding = new System.Windows.Forms.Padding(1);
            this.textBoxBottomExtent.ReadOnly = false;
            this.textBoxBottomExtent.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.textBoxBottomExtent.SelectedText = "";
            this.textBoxBottomExtent.SelectionLength = 0;
            this.textBoxBottomExtent.SelectionStart = 0;
            this.textBoxBottomExtent.Size = new System.Drawing.Size(263, 20);
            this.textBoxBottomExtent.TabIndex = 23;
            this.textBoxBottomExtent.TextBoxFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxBottomExtent.UseSystemPasswordChar = false;
            // 
            // labelBottomExtent
            // 
            this.labelBottomExtent.AutoSize = true;
            this.labelBottomExtent.Location = new System.Drawing.Point(14, 144);
            this.labelBottomExtent.Name = "labelBottomExtent";
            this.labelBottomExtent.Size = new System.Drawing.Size(72, 13);
            this.labelBottomExtent.TabIndex = 25;
            this.labelBottomExtent.Text = "Bottom extent";
            // 
            // textBoxBottomOrigin
            // 
            this.textBoxBottomOrigin.BackColor = System.Drawing.Color.DarkGray;
            this.textBoxBottomOrigin.CurrentLanguage = "";
            this.textBoxBottomOrigin.CurrentLineIndex = 0;
            this.textBoxBottomOrigin.HideSelection = true;
            this.textBoxBottomOrigin.IsDictionaryDownloaded = true;
            this.textBoxBottomOrigin.IsSpellCheckerInitialized = false;
            this.textBoxBottomOrigin.IsSpellCheckRequested = false;
            this.textBoxBottomOrigin.IsWrongWord = false;
            this.textBoxBottomOrigin.LanguageChanged = false;
            this.textBoxBottomOrigin.Location = new System.Drawing.Point(135, 115);
            this.textBoxBottomOrigin.MaxLength = 32767;
            this.textBoxBottomOrigin.Multiline = true;
            this.textBoxBottomOrigin.Name = "textBoxBottomOrigin";
            this.textBoxBottomOrigin.Padding = new System.Windows.Forms.Padding(1);
            this.textBoxBottomOrigin.ReadOnly = false;
            this.textBoxBottomOrigin.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.textBoxBottomOrigin.SelectedText = "";
            this.textBoxBottomOrigin.SelectionLength = 0;
            this.textBoxBottomOrigin.SelectionStart = 0;
            this.textBoxBottomOrigin.Size = new System.Drawing.Size(263, 20);
            this.textBoxBottomOrigin.TabIndex = 22;
            this.textBoxBottomOrigin.TextBoxFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxBottomOrigin.UseSystemPasswordChar = false;
            // 
            // labelBottomOrigin
            // 
            this.labelBottomOrigin.AutoSize = true;
            this.labelBottomOrigin.Location = new System.Drawing.Point(14, 118);
            this.labelBottomOrigin.Name = "labelBottomOrigin";
            this.labelBottomOrigin.Size = new System.Drawing.Size(68, 13);
            this.labelBottomOrigin.TabIndex = 24;
            this.labelBottomOrigin.Text = "Bottom origin";
            // 
            // textBoxTopExtent
            // 
            this.textBoxTopExtent.BackColor = System.Drawing.Color.DarkGray;
            this.textBoxTopExtent.CurrentLanguage = "";
            this.textBoxTopExtent.CurrentLineIndex = 0;
            this.textBoxTopExtent.HideSelection = true;
            this.textBoxTopExtent.IsDictionaryDownloaded = true;
            this.textBoxTopExtent.IsSpellCheckerInitialized = false;
            this.textBoxTopExtent.IsSpellCheckRequested = false;
            this.textBoxTopExtent.IsWrongWord = false;
            this.textBoxTopExtent.LanguageChanged = false;
            this.textBoxTopExtent.Location = new System.Drawing.Point(135, 65);
            this.textBoxTopExtent.MaxLength = 32767;
            this.textBoxTopExtent.Multiline = true;
            this.textBoxTopExtent.Name = "textBoxTopExtent";
            this.textBoxTopExtent.Padding = new System.Windows.Forms.Padding(1);
            this.textBoxTopExtent.ReadOnly = false;
            this.textBoxTopExtent.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.textBoxTopExtent.SelectedText = "";
            this.textBoxTopExtent.SelectionLength = 0;
            this.textBoxTopExtent.SelectionStart = 0;
            this.textBoxTopExtent.Size = new System.Drawing.Size(263, 20);
            this.textBoxTopExtent.TabIndex = 21;
            this.textBoxTopExtent.TextBoxFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTopExtent.UseSystemPasswordChar = false;
            // 
            // labelTopExtent
            // 
            this.labelTopExtent.AutoSize = true;
            this.labelTopExtent.Location = new System.Drawing.Point(14, 68);
            this.labelTopExtent.Name = "labelTopExtent";
            this.labelTopExtent.Size = new System.Drawing.Size(58, 13);
            this.labelTopExtent.TabIndex = 21;
            this.labelTopExtent.Text = "Top extent";
            // 
            // textBoxTopOrigin
            // 
            this.textBoxTopOrigin.BackColor = System.Drawing.Color.DarkGray;
            this.textBoxTopOrigin.CurrentLanguage = "";
            this.textBoxTopOrigin.CurrentLineIndex = 0;
            this.textBoxTopOrigin.HideSelection = true;
            this.textBoxTopOrigin.IsDictionaryDownloaded = true;
            this.textBoxTopOrigin.IsSpellCheckerInitialized = false;
            this.textBoxTopOrigin.IsSpellCheckRequested = false;
            this.textBoxTopOrigin.IsWrongWord = false;
            this.textBoxTopOrigin.LanguageChanged = false;
            this.textBoxTopOrigin.Location = new System.Drawing.Point(135, 39);
            this.textBoxTopOrigin.MaxLength = 32767;
            this.textBoxTopOrigin.Multiline = true;
            this.textBoxTopOrigin.Name = "textBoxTopOrigin";
            this.textBoxTopOrigin.Padding = new System.Windows.Forms.Padding(1);
            this.textBoxTopOrigin.ReadOnly = false;
            this.textBoxTopOrigin.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.textBoxTopOrigin.SelectedText = "";
            this.textBoxTopOrigin.SelectionLength = 0;
            this.textBoxTopOrigin.SelectionStart = 0;
            this.textBoxTopOrigin.Size = new System.Drawing.Size(263, 20);
            this.textBoxTopOrigin.TabIndex = 20;
            this.textBoxTopOrigin.TextBoxFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTopOrigin.UseSystemPasswordChar = false;
            // 
            // labelTopOrigin
            // 
            this.labelTopOrigin.AutoSize = true;
            this.labelTopOrigin.Location = new System.Drawing.Point(14, 42);
            this.labelTopOrigin.Name = "labelTopOrigin";
            this.labelTopOrigin.Size = new System.Drawing.Size(54, 13);
            this.labelTopOrigin.TabIndex = 20;
            this.labelTopOrigin.Text = "Top origin";
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
            this.comboBoxFileExtensions.Items.AddRange(new object[] {
            ".xml",
            ".ttml",
            ".dfxp"});
            this.comboBoxFileExtensions.Location = new System.Drawing.Point(191, 378);
            this.comboBoxFileExtensions.MaxLength = 32767;
            this.comboBoxFileExtensions.Name = "comboBoxFileExtensions";
            this.comboBoxFileExtensions.SelectedIndex = -1;
            this.comboBoxFileExtensions.SelectedItem = null;
            this.comboBoxFileExtensions.SelectedText = "";
            this.comboBoxFileExtensions.Size = new System.Drawing.Size(263, 21);
            this.comboBoxFileExtensions.TabIndex = 11;
            // 
            // labelFileExtension
            // 
            this.labelFileExtension.AutoSize = true;
            this.labelFileExtension.Location = new System.Drawing.Point(6, 381);
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
            this.comboBoxTimeCodeFormat.Items.AddRange(new object[] {
            "Frames",
            "Source",
            "Seconds",
            "Milliseconds",
            "Ticks",
            "hh:mm:ss:ff",
            "hh:mm:ss.ms",
            "hh:mm:ss.ms-two-digits",
            "hh:mm:ss,ms"});
            this.comboBoxTimeCodeFormat.Location = new System.Drawing.Point(191, 341);
            this.comboBoxTimeCodeFormat.MaxLength = 32767;
            this.comboBoxTimeCodeFormat.Name = "comboBoxTimeCodeFormat";
            this.comboBoxTimeCodeFormat.SelectedIndex = -1;
            this.comboBoxTimeCodeFormat.SelectedItem = null;
            this.comboBoxTimeCodeFormat.SelectedText = "";
            this.comboBoxTimeCodeFormat.Size = new System.Drawing.Size(263, 21);
            this.comboBoxTimeCodeFormat.TabIndex = 10;
            // 
            // labelTimeCode
            // 
            this.labelTimeCode.AutoSize = true;
            this.labelTimeCode.Location = new System.Drawing.Point(6, 344);
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
            this.comboBoxFrameRateMultiplier.Items.AddRange(new object[] {
            "999 1000",
            "1 1",
            "1000 1001"});
            this.comboBoxFrameRateMultiplier.Location = new System.Drawing.Point(191, 176);
            this.comboBoxFrameRateMultiplier.MaxLength = 32767;
            this.comboBoxFrameRateMultiplier.Name = "comboBoxFrameRateMultiplier";
            this.comboBoxFrameRateMultiplier.SelectedIndex = -1;
            this.comboBoxFrameRateMultiplier.SelectedItem = null;
            this.comboBoxFrameRateMultiplier.SelectedText = "";
            this.comboBoxFrameRateMultiplier.Size = new System.Drawing.Size(263, 21);
            this.comboBoxFrameRateMultiplier.TabIndex = 5;
            this.comboBoxFrameRateMultiplier.TabStop = false;
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.BackColor = System.Drawing.Color.DarkGray;
            this.textBoxDescription.CurrentLanguage = "";
            this.textBoxDescription.CurrentLineIndex = 0;
            this.textBoxDescription.HideSelection = true;
            this.textBoxDescription.IsDictionaryDownloaded = true;
            this.textBoxDescription.IsSpellCheckerInitialized = false;
            this.textBoxDescription.IsSpellCheckRequested = false;
            this.textBoxDescription.IsWrongWord = false;
            this.textBoxDescription.LanguageChanged = false;
            this.textBoxDescription.Location = new System.Drawing.Point(191, 56);
            this.textBoxDescription.MaxLength = 32767;
            this.textBoxDescription.Multiline = true;
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Padding = new System.Windows.Forms.Padding(1);
            this.textBoxDescription.ReadOnly = false;
            this.textBoxDescription.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.textBoxDescription.SelectedText = "";
            this.textBoxDescription.SelectionLength = 0;
            this.textBoxDescription.SelectionStart = 0;
            this.textBoxDescription.Size = new System.Drawing.Size(263, 20);
            this.textBoxDescription.TabIndex = 1;
            this.textBoxDescription.TextBoxFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxDescription.UseSystemPasswordChar = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 58);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Description";
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
            this.textBoxTitle.Location = new System.Drawing.Point(191, 30);
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
            this.textBoxTitle.TabIndex = 0;
            this.textBoxTitle.TextBoxFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTitle.UseSystemPasswordChar = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 32);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(27, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Title";
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
            this.comboBoxDefaultRegion.Location = new System.Drawing.Point(191, 302);
            this.comboBoxDefaultRegion.MaxLength = 32767;
            this.comboBoxDefaultRegion.Name = "comboBoxDefaultRegion";
            this.comboBoxDefaultRegion.SelectedIndex = -1;
            this.comboBoxDefaultRegion.SelectedItem = null;
            this.comboBoxDefaultRegion.SelectedText = "";
            this.comboBoxDefaultRegion.Size = new System.Drawing.Size(263, 21);
            this.comboBoxDefaultRegion.TabIndex = 9;
            // 
            // labelDefaultRegion
            // 
            this.labelDefaultRegion.AutoSize = true;
            this.labelDefaultRegion.Location = new System.Drawing.Point(6, 305);
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
            this.comboBoxDropMode.Items.AddRange(new object[] {
            "[N/A]",
            "dropNTSC",
            "dropPAL",
            "nonDrop"});
            this.comboBoxDropMode.Location = new System.Drawing.Point(191, 203);
            this.comboBoxDropMode.MaxLength = 32767;
            this.comboBoxDropMode.Name = "comboBoxDropMode";
            this.comboBoxDropMode.SelectedIndex = -1;
            this.comboBoxDropMode.SelectedItem = null;
            this.comboBoxDropMode.SelectedText = "";
            this.comboBoxDropMode.Size = new System.Drawing.Size(263, 21);
            this.comboBoxDropMode.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 206);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Drop mode";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 179);
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
            this.comboBoxFrameRate.Location = new System.Drawing.Point(191, 149);
            this.comboBoxFrameRate.MaxLength = 32767;
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.SelectedIndex = -1;
            this.comboBoxFrameRate.SelectedItem = null;
            this.comboBoxFrameRate.SelectedText = "";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(263, 21);
            this.comboBoxFrameRate.TabIndex = 4;
            this.comboBoxFrameRate.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 152);
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
            this.comboBoxTimeBase.Items.AddRange(new object[] {
            "[N/A]",
            "media",
            "smpte",
            "clock"});
            this.comboBoxTimeBase.Location = new System.Drawing.Point(191, 122);
            this.comboBoxTimeBase.MaxLength = 32767;
            this.comboBoxTimeBase.Name = "comboBoxTimeBase";
            this.comboBoxTimeBase.SelectedIndex = -1;
            this.comboBoxTimeBase.SelectedItem = null;
            this.comboBoxTimeBase.SelectedText = "";
            this.comboBoxTimeBase.Size = new System.Drawing.Size(263, 21);
            this.comboBoxTimeBase.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 125);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Time base";
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxLanguage.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxLanguage.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxLanguage.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxLanguage.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxLanguage.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxLanguage.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxLanguage.DropDownHeight = 400;
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxLanguage.DropDownWidth = 263;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(191, 95);
            this.comboBoxLanguage.MaxLength = 32767;
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.SelectedIndex = -1;
            this.comboBoxLanguage.SelectedItem = null;
            this.comboBoxLanguage.SelectedText = "";
            this.comboBoxLanguage.Size = new System.Drawing.Size(263, 21);
            this.comboBoxLanguage.TabIndex = 2;
            this.comboBoxLanguage.TabStop = false;
            // 
            // labelCollision
            // 
            this.labelCollision.AutoSize = true;
            this.labelCollision.Location = new System.Drawing.Point(6, 98);
            this.labelCollision.Name = "labelCollision";
            this.labelCollision.Size = new System.Drawing.Size(55, 13);
            this.labelCollision.TabIndex = 3;
            this.labelCollision.Text = "Language";
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
            this.comboBoxDefaultStyle.Location = new System.Drawing.Point(191, 243);
            this.comboBoxDefaultStyle.MaxLength = 32767;
            this.comboBoxDefaultStyle.Name = "comboBoxDefaultStyle";
            this.comboBoxDefaultStyle.SelectedIndex = -1;
            this.comboBoxDefaultStyle.SelectedItem = null;
            this.comboBoxDefaultStyle.SelectedText = "";
            this.comboBoxDefaultStyle.Size = new System.Drawing.Size(263, 21);
            this.comboBoxDefaultStyle.TabIndex = 7;
            // 
            // labelDefaultStyle
            // 
            this.labelDefaultStyle.AutoSize = true;
            this.labelDefaultStyle.Location = new System.Drawing.Point(6, 246);
            this.labelDefaultStyle.Name = "labelDefaultStyle";
            this.labelDefaultStyle.Size = new System.Drawing.Size(65, 13);
            this.labelDefaultStyle.TabIndex = 1;
            this.labelDefaultStyle.Text = "Default style";
            // 
            // TimedTextPropertiesItunes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 472);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxOptions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TimedTextPropertiesItunes";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Timed Text properties";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TimedTextPropertiesItunes_KeyDown);
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxOptions.PerformLayout();
            this.groupBoxAlignment.ResumeLayout(false);
            this.groupBoxAlignment.PerformLayout();
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
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelCollision;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxDefaultStyle;
        private System.Windows.Forms.Label labelDefaultStyle;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxDefaultRegion;
        private System.Windows.Forms.Label labelDefaultRegion;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxDescription;
        private System.Windows.Forms.Label label7;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxTitle;
        private System.Windows.Forms.Label label6;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFrameRateMultiplier;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxTimeCodeFormat;
        private System.Windows.Forms.Label labelTimeCode;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFileExtensions;
        private System.Windows.Forms.Label labelFileExtension;
        private System.Windows.Forms.GroupBox groupBoxAlignment;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxTopExtent;
        private System.Windows.Forms.Label labelTopExtent;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxTopOrigin;
        private System.Windows.Forms.Label labelTopOrigin;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxBottomExtent;
        private System.Windows.Forms.Label labelBottomExtent;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxBottomOrigin;
        private System.Windows.Forms.Label labelBottomOrigin;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxStyleAttribute;
        private System.Windows.Forms.Label labelStyleAttributeName;
    }
}