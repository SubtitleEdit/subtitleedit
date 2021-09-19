using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms.DCinema
{
    partial class DCinemaPropertiesSmpte
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
            Nikse.SubtitleEdit.Core.Common.TimeCode timeCode1 = new Nikse.SubtitleEdit.Core.Common.TimeCode();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.timeUpDownStartTime = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.labelStartTime = new System.Windows.Forms.Label();
            this.comboBoxTimeCodeRate = new System.Windows.Forms.ComboBox();
            this.labelTimeCodeRate = new System.Windows.Forms.Label();
            this.textBoxEditRate = new System.Windows.Forms.TextBox();
            this.labelEditRate = new System.Windows.Forms.Label();
            this.buttonToday = new System.Windows.Forms.Button();
            this.textBoxIssueDate = new System.Windows.Forms.TextBox();
            this.labelIssueDate = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.buttonGenerateID = new System.Windows.Forms.Button();
            this.numericUpDownReelNumber = new System.Windows.Forms.NumericUpDown();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelFadeUpMs = new System.Windows.Forms.Label();
            this.numericUpDownFadeDown = new System.Windows.Forms.NumericUpDown();
            this.labelFadeDownTime = new System.Windows.Forms.Label();
            this.numericUpDownFadeUp = new System.Windows.Forms.NumericUpDown();
            this.labelFadeUpTime = new System.Windows.Forms.Label();
            this.buttonGenFontUri = new System.Windows.Forms.Button();
            this.numericUpDownTopBottomMargin = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.panelFontEffectColor = new System.Windows.Forms.Panel();
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
            this.checkBoxGenerateIdAuto = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownReelNumber)).BeginInit();
            this.groupBoxFont.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFadeDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFadeUp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTopBottomMargin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxGenerateIdAuto);
            this.groupBox1.Controls.Add(this.timeUpDownStartTime);
            this.groupBox1.Controls.Add(this.labelStartTime);
            this.groupBox1.Controls.Add(this.comboBoxTimeCodeRate);
            this.groupBox1.Controls.Add(this.labelTimeCodeRate);
            this.groupBox1.Controls.Add(this.textBoxEditRate);
            this.groupBox1.Controls.Add(this.labelEditRate);
            this.groupBox1.Controls.Add(this.buttonToday);
            this.groupBox1.Controls.Add(this.textBoxIssueDate);
            this.groupBox1.Controls.Add(this.labelIssueDate);
            this.groupBox1.Controls.Add(this.comboBoxLanguage);
            this.groupBox1.Controls.Add(this.labelLanguage);
            this.groupBox1.Controls.Add(this.buttonGenerateID);
            this.groupBox1.Controls.Add(this.numericUpDownReelNumber);
            this.groupBox1.Controls.Add(this.groupBoxFont);
            this.groupBox1.Controls.Add(this.labelReelNumber);
            this.groupBox1.Controls.Add(this.textBoxMovieTitle);
            this.groupBox1.Controls.Add(this.labelMovieTitle);
            this.groupBox1.Controls.Add(this.textBoxSubtitleID);
            this.groupBox1.Controls.Add(this.labelSubtitleID);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(583, 571);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // timeUpDownStartTime
            // 
            this.timeUpDownStartTime.AutoSize = true;
            this.timeUpDownStartTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownStartTime.Location = new System.Drawing.Point(181, 248);
            this.timeUpDownStartTime.Name = "timeUpDownStartTime";
            this.timeUpDownStartTime.Size = new System.Drawing.Size(111, 27);
            this.timeUpDownStartTime.TabIndex = 8;
            timeCode1.Hours = 0;
            timeCode1.Milliseconds = 0;
            timeCode1.Minutes = 0;
            timeCode1.Seconds = 0;
            timeCode1.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode1.TotalMilliseconds = 0D;
            timeCode1.TotalSeconds = 0D;
            this.timeUpDownStartTime.TimeCode = timeCode1;
            this.timeUpDownStartTime.UseVideoOffset = false;
            // 
            // labelStartTime
            // 
            this.labelStartTime.AutoSize = true;
            this.labelStartTime.Location = new System.Drawing.Point(15, 248);
            this.labelStartTime.Name = "labelStartTime";
            this.labelStartTime.Size = new System.Drawing.Size(51, 13);
            this.labelStartTime.TabIndex = 39;
            this.labelStartTime.Text = "Start time";
            // 
            // comboBoxTimeCodeRate
            // 
            this.comboBoxTimeCodeRate.FormattingEnabled = true;
            this.comboBoxTimeCodeRate.Items.AddRange(new object[] {
            "24",
            "25",
            "30",
            "48"});
            this.comboBoxTimeCodeRate.Location = new System.Drawing.Point(181, 219);
            this.comboBoxTimeCodeRate.Name = "comboBoxTimeCodeRate";
            this.comboBoxTimeCodeRate.Size = new System.Drawing.Size(112, 21);
            this.comboBoxTimeCodeRate.TabIndex = 7;
            // 
            // labelTimeCodeRate
            // 
            this.labelTimeCodeRate.AutoSize = true;
            this.labelTimeCodeRate.Location = new System.Drawing.Point(15, 222);
            this.labelTimeCodeRate.Name = "labelTimeCodeRate";
            this.labelTimeCodeRate.Size = new System.Drawing.Size(78, 13);
            this.labelTimeCodeRate.TabIndex = 37;
            this.labelTimeCodeRate.Text = "Time code rate";
            // 
            // textBoxEditRate
            // 
            this.textBoxEditRate.Location = new System.Drawing.Point(181, 195);
            this.textBoxEditRate.Name = "textBoxEditRate";
            this.textBoxEditRate.Size = new System.Drawing.Size(112, 20);
            this.textBoxEditRate.TabIndex = 6;
            this.textBoxEditRate.Text = "1 24";
            // 
            // labelEditRate
            // 
            this.labelEditRate.AutoSize = true;
            this.labelEditRate.Location = new System.Drawing.Point(15, 197);
            this.labelEditRate.Name = "labelEditRate";
            this.labelEditRate.Size = new System.Drawing.Size(46, 13);
            this.labelEditRate.TabIndex = 35;
            this.labelEditRate.Text = "Edit rate";
            // 
            // buttonToday
            // 
            this.buttonToday.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonToday.Location = new System.Drawing.Point(461, 168);
            this.buttonToday.Name = "buttonToday";
            this.buttonToday.Size = new System.Drawing.Size(100, 23);
            this.buttonToday.TabIndex = 5;
            this.buttonToday.Text = "Now";
            this.buttonToday.UseVisualStyleBackColor = true;
            this.buttonToday.Click += new System.EventHandler(this.buttonToday_Click);
            // 
            // textBoxIssueDate
            // 
            this.textBoxIssueDate.Location = new System.Drawing.Point(181, 169);
            this.textBoxIssueDate.Name = "textBoxIssueDate";
            this.textBoxIssueDate.Size = new System.Drawing.Size(274, 20);
            this.textBoxIssueDate.TabIndex = 4;
            this.textBoxIssueDate.Text = "2005-07-14T21:52:02.000-00:00";
            // 
            // labelIssueDate
            // 
            this.labelIssueDate.AutoSize = true;
            this.labelIssueDate.Location = new System.Drawing.Point(15, 172);
            this.labelIssueDate.Name = "labelIssueDate";
            this.labelIssueDate.Size = new System.Drawing.Size(56, 13);
            this.labelIssueDate.TabIndex = 32;
            this.labelIssueDate.Text = "Issue date";
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(181, 142);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(112, 21);
            this.comboBoxLanguage.TabIndex = 3;
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(15, 145);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(55, 13);
            this.labelLanguage.TabIndex = 30;
            this.labelLanguage.Text = "Language";
            // 
            // buttonGenerateID
            // 
            this.buttonGenerateID.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenerateID.Location = new System.Drawing.Point(461, 29);
            this.buttonGenerateID.Name = "buttonGenerateID";
            this.buttonGenerateID.Size = new System.Drawing.Size(100, 23);
            this.buttonGenerateID.TabIndex = 1;
            this.buttonGenerateID.Text = "Generate ID";
            this.buttonGenerateID.UseVisualStyleBackColor = true;
            this.buttonGenerateID.Click += new System.EventHandler(this.buttonGenerateID_Click);
            // 
            // numericUpDownReelNumber
            // 
            this.numericUpDownReelNumber.Location = new System.Drawing.Point(181, 116);
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
            this.numericUpDownReelNumber.TabIndex = 2;
            this.numericUpDownReelNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFont.Controls.Add(this.label1);
            this.groupBoxFont.Controls.Add(this.labelFadeUpMs);
            this.groupBoxFont.Controls.Add(this.numericUpDownFadeDown);
            this.groupBoxFont.Controls.Add(this.labelFadeDownTime);
            this.groupBoxFont.Controls.Add(this.numericUpDownFadeUp);
            this.groupBoxFont.Controls.Add(this.labelFadeUpTime);
            this.groupBoxFont.Controls.Add(this.buttonGenFontUri);
            this.groupBoxFont.Controls.Add(this.numericUpDownTopBottomMargin);
            this.groupBoxFont.Controls.Add(this.label2);
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
            this.groupBoxFont.Location = new System.Drawing.Point(6, 284);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(571, 281);
            this.groupBoxFont.TabIndex = 9;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "Font";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(294, 232);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 43;
            this.label1.Text = "Frames";
            // 
            // labelFadeUpMs
            // 
            this.labelFadeUpMs.AutoSize = true;
            this.labelFadeUpMs.Location = new System.Drawing.Point(294, 206);
            this.labelFadeUpMs.Name = "labelFadeUpMs";
            this.labelFadeUpMs.Size = new System.Drawing.Size(41, 13);
            this.labelFadeUpMs.TabIndex = 42;
            this.labelFadeUpMs.Text = "Frames";
            // 
            // numericUpDownFadeDown
            // 
            this.numericUpDownFadeDown.Location = new System.Drawing.Point(174, 230);
            this.numericUpDownFadeDown.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownFadeDown.Name = "numericUpDownFadeDown";
            this.numericUpDownFadeDown.Size = new System.Drawing.Size(112, 20);
            this.numericUpDownFadeDown.TabIndex = 39;
            // 
            // labelFadeDownTime
            // 
            this.labelFadeDownTime.AutoSize = true;
            this.labelFadeDownTime.Location = new System.Drawing.Point(10, 232);
            this.labelFadeDownTime.Name = "labelFadeDownTime";
            this.labelFadeDownTime.Size = new System.Drawing.Size(82, 13);
            this.labelFadeDownTime.TabIndex = 41;
            this.labelFadeDownTime.Text = "Fade down time";
            // 
            // numericUpDownFadeUp
            // 
            this.numericUpDownFadeUp.Location = new System.Drawing.Point(174, 204);
            this.numericUpDownFadeUp.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownFadeUp.Name = "numericUpDownFadeUp";
            this.numericUpDownFadeUp.Size = new System.Drawing.Size(112, 20);
            this.numericUpDownFadeUp.TabIndex = 38;
            // 
            // labelFadeUpTime
            // 
            this.labelFadeUpTime.AutoSize = true;
            this.labelFadeUpTime.Location = new System.Drawing.Point(10, 206);
            this.labelFadeUpTime.Name = "labelFadeUpTime";
            this.labelFadeUpTime.Size = new System.Drawing.Size(68, 13);
            this.labelFadeUpTime.TabIndex = 40;
            this.labelFadeUpTime.Text = "Fade up time";
            // 
            // buttonGenFontUri
            // 
            this.buttonGenFontUri.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenFontUri.Location = new System.Drawing.Point(454, 41);
            this.buttonGenFontUri.Name = "buttonGenFontUri";
            this.buttonGenFontUri.Size = new System.Drawing.Size(100, 23);
            this.buttonGenFontUri.TabIndex = 2;
            this.buttonGenFontUri.Text = "Generate";
            this.buttonGenFontUri.UseVisualStyleBackColor = true;
            this.buttonGenFontUri.Click += new System.EventHandler(this.button1_Click);
            // 
            // numericUpDownTopBottomMargin
            // 
            this.numericUpDownTopBottomMargin.Location = new System.Drawing.Point(175, 176);
            this.numericUpDownTopBottomMargin.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownTopBottomMargin.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTopBottomMargin.Name = "numericUpDownTopBottomMargin";
            this.numericUpDownTopBottomMargin.Size = new System.Drawing.Size(112, 20);
            this.numericUpDownTopBottomMargin.TabIndex = 9;
            this.numericUpDownTopBottomMargin.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 178);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "Top/bottom margin";
            // 
            // panelFontEffectColor
            // 
            this.panelFontEffectColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFontEffectColor.Location = new System.Drawing.Point(297, 123);
            this.panelFontEffectColor.Name = "panelFontEffectColor";
            this.panelFontEffectColor.Size = new System.Drawing.Size(21, 20);
            this.panelFontEffectColor.TabIndex = 7;
            this.panelFontEffectColor.Click += new System.EventHandler(this.buttonFontEffectColor_Click);
            // 
            // buttonFontEffectColor
            // 
            this.buttonFontEffectColor.Location = new System.Drawing.Point(175, 123);
            this.buttonFontEffectColor.Name = "buttonFontEffectColor";
            this.buttonFontEffectColor.Size = new System.Drawing.Size(112, 21);
            this.buttonFontEffectColor.TabIndex = 6;
            this.buttonFontEffectColor.Text = "Choose color";
            this.buttonFontEffectColor.UseVisualStyleBackColor = true;
            this.buttonFontEffectColor.Click += new System.EventHandler(this.buttonFontEffectColor_Click);
            // 
            // panelFontColor
            // 
            this.panelFontColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFontColor.Location = new System.Drawing.Point(297, 70);
            this.panelFontColor.Name = "panelFontColor";
            this.panelFontColor.Size = new System.Drawing.Size(21, 20);
            this.panelFontColor.TabIndex = 4;
            this.panelFontColor.Click += new System.EventHandler(this.buttonFontColor_Click);
            // 
            // buttonFontColor
            // 
            this.buttonFontColor.Location = new System.Drawing.Point(175, 70);
            this.buttonFontColor.Name = "buttonFontColor";
            this.buttonFontColor.Size = new System.Drawing.Size(112, 21);
            this.buttonFontColor.TabIndex = 3;
            this.buttonFontColor.Text = "Choose color";
            this.buttonFontColor.UseVisualStyleBackColor = true;
            this.buttonFontColor.Click += new System.EventHandler(this.buttonFontColor_Click);
            // 
            // labelEffectColor
            // 
            this.labelEffectColor.AutoSize = true;
            this.labelEffectColor.Location = new System.Drawing.Point(10, 127);
            this.labelEffectColor.Name = "labelEffectColor";
            this.labelEffectColor.Size = new System.Drawing.Size(61, 13);
            this.labelEffectColor.TabIndex = 27;
            this.labelEffectColor.Text = "Effect color";
            // 
            // numericUpDownFontSize
            // 
            this.numericUpDownFontSize.Location = new System.Drawing.Point(175, 150);
            this.numericUpDownFontSize.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.numericUpDownFontSize.Name = "numericUpDownFontSize";
            this.numericUpDownFontSize.Size = new System.Drawing.Size(112, 20);
            this.numericUpDownFontSize.TabIndex = 8;
            // 
            // labelFontSize
            // 
            this.labelFontSize.AutoSize = true;
            this.labelFontSize.Location = new System.Drawing.Point(10, 152);
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
            this.comboBoxFontEffect.Location = new System.Drawing.Point(175, 97);
            this.comboBoxFontEffect.Name = "comboBoxFontEffect";
            this.comboBoxFontEffect.Size = new System.Drawing.Size(112, 21);
            this.comboBoxFontEffect.TabIndex = 5;
            // 
            // labelEffect
            // 
            this.labelEffect.AutoSize = true;
            this.labelEffect.Location = new System.Drawing.Point(10, 100);
            this.labelEffect.Name = "labelEffect";
            this.labelEffect.Size = new System.Drawing.Size(35, 13);
            this.labelEffect.TabIndex = 22;
            this.labelEffect.Text = "Effect";
            // 
            // labelFontColor
            // 
            this.labelFontColor.AutoSize = true;
            this.labelFontColor.Location = new System.Drawing.Point(10, 74);
            this.labelFontColor.Name = "labelFontColor";
            this.labelFontColor.Size = new System.Drawing.Size(31, 13);
            this.labelFontColor.TabIndex = 18;
            this.labelFontColor.Text = "Color";
            // 
            // textBoxFontID
            // 
            this.textBoxFontID.Location = new System.Drawing.Point(175, 19);
            this.textBoxFontID.Name = "textBoxFontID";
            this.textBoxFontID.Size = new System.Drawing.Size(274, 20);
            this.textBoxFontID.TabIndex = 0;
            this.textBoxFontID.Text = "Freds_Font";
            // 
            // labelFontId
            // 
            this.labelFontId.AutoSize = true;
            this.labelFontId.Location = new System.Drawing.Point(10, 22);
            this.labelFontId.Name = "labelFontId";
            this.labelFontId.Size = new System.Drawing.Size(18, 13);
            this.labelFontId.TabIndex = 16;
            this.labelFontId.Text = "ID";
            // 
            // textBoxFontUri
            // 
            this.textBoxFontUri.Location = new System.Drawing.Point(175, 45);
            this.textBoxFontUri.Name = "textBoxFontUri";
            this.textBoxFontUri.Size = new System.Drawing.Size(274, 20);
            this.textBoxFontUri.TabIndex = 1;
            this.textBoxFontUri.Text = "Fred.ttf";
            // 
            // labelFontUri
            // 
            this.labelFontUri.AutoSize = true;
            this.labelFontUri.Location = new System.Drawing.Point(10, 48);
            this.labelFontUri.Name = "labelFontUri";
            this.labelFontUri.Size = new System.Drawing.Size(26, 13);
            this.labelFontUri.TabIndex = 14;
            this.labelFontUri.Text = "URI";
            // 
            // labelReelNumber
            // 
            this.labelReelNumber.AutoSize = true;
            this.labelReelNumber.Location = new System.Drawing.Point(15, 119);
            this.labelReelNumber.Name = "labelReelNumber";
            this.labelReelNumber.Size = new System.Drawing.Size(67, 13);
            this.labelReelNumber.TabIndex = 4;
            this.labelReelNumber.Text = "Reel number";
            // 
            // textBoxMovieTitle
            // 
            this.textBoxMovieTitle.Location = new System.Drawing.Point(181, 90);
            this.textBoxMovieTitle.Name = "textBoxMovieTitle";
            this.textBoxMovieTitle.Size = new System.Drawing.Size(274, 20);
            this.textBoxMovieTitle.TabIndex = 2;
            // 
            // labelMovieTitle
            // 
            this.labelMovieTitle.AutoSize = true;
            this.labelMovieTitle.Location = new System.Drawing.Point(15, 93);
            this.labelMovieTitle.Name = "labelMovieTitle";
            this.labelMovieTitle.Size = new System.Drawing.Size(55, 13);
            this.labelMovieTitle.TabIndex = 2;
            this.labelMovieTitle.Text = "Movie title";
            // 
            // textBoxSubtitleID
            // 
            this.textBoxSubtitleID.Location = new System.Drawing.Point(181, 30);
            this.textBoxSubtitleID.Name = "textBoxSubtitleID";
            this.textBoxSubtitleID.Size = new System.Drawing.Size(274, 20);
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
            this.buttonCancel.Location = new System.Drawing.Point(521, 588);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(440, 588);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click_1);
            // 
            // checkBoxGenerateIdAuto
            // 
            this.checkBoxGenerateIdAuto.AutoSize = true;
            this.checkBoxGenerateIdAuto.Location = new System.Drawing.Point(181, 56);
            this.checkBoxGenerateIdAuto.Name = "checkBoxGenerateIdAuto";
            this.checkBoxGenerateIdAuto.Size = new System.Drawing.Size(148, 17);
            this.checkBoxGenerateIdAuto.TabIndex = 40;
            this.checkBoxGenerateIdAuto.Text = "Generate new ID on save";
            this.checkBoxGenerateIdAuto.UseVisualStyleBackColor = true;
            // 
            // DCinemaPropertiesSmpte
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(607, 622);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DCinemaPropertiesSmpte";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "D-Cinema properties (SMPTE)";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownReelNumber)).EndInit();
            this.groupBoxFont.ResumeLayout(false);
            this.groupBoxFont.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFadeDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFadeUp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTopBottomMargin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBoxFont;
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
        private System.Windows.Forms.Label labelEffectColor;
        private System.Windows.Forms.NumericUpDown numericUpDownFontSize;
        private System.Windows.Forms.Label labelFontSize;
        private System.Windows.Forms.ComboBox comboBoxFontEffect;
        private System.Windows.Forms.Label labelEffect;
        private System.Windows.Forms.Panel panelFontEffectColor;
        private System.Windows.Forms.Button buttonFontEffectColor;
        private System.Windows.Forms.Panel panelFontColor;
        private System.Windows.Forms.Button buttonFontColor;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.NumericUpDown numericUpDownReelNumber;
        private System.Windows.Forms.Button buttonGenerateID;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.Button buttonToday;
        private System.Windows.Forms.TextBox textBoxIssueDate;
        private System.Windows.Forms.Label labelIssueDate;
        private System.Windows.Forms.TextBox textBoxEditRate;
        private System.Windows.Forms.Label labelEditRate;
        private System.Windows.Forms.NumericUpDown numericUpDownTopBottomMargin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxTimeCodeRate;
        private System.Windows.Forms.Label labelTimeCodeRate;
        private Controls.TimeUpDown timeUpDownStartTime;
        private System.Windows.Forms.Label labelStartTime;
        private System.Windows.Forms.Button buttonGenFontUri;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.NumericUpDown numericUpDownFadeDown;
        private System.Windows.Forms.Label labelFadeDownTime;
        private System.Windows.Forms.NumericUpDown numericUpDownFadeUp;
        private System.Windows.Forms.Label labelFadeUpTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelFadeUpMs;
        private System.Windows.Forms.CheckBox checkBoxGenerateIdAuto;
    }
}