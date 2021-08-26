
namespace Nikse.SubtitleEdit.Forms.Assa
{
    sealed partial class AssaProgressBar
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
            Nikse.SubtitleEdit.Core.Common.TimeCode timeCode2 = new Nikse.SubtitleEdit.Core.Common.TimeCode();
            this.groupBoxStyle = new System.Windows.Forms.GroupBox();
            this.comboBoxProgressBarEdge = new System.Windows.Forms.ComboBox();
            this.labelEdgeStyle = new System.Windows.Forms.Label();
            this.radioButtonPosTop = new System.Windows.Forms.RadioButton();
            this.radioButtonPosBottom = new System.Windows.Forms.RadioButton();
            this.labelHeight = new System.Windows.Forms.Label();
            this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
            this.panelSecondaryColor = new System.Windows.Forms.Panel();
            this.buttonSecondaryColor = new System.Windows.Forms.Button();
            this.panelPrimaryColor = new System.Windows.Forms.Panel();
            this.buttonForeColor = new System.Windows.Forms.Button();
            this.labelPosition = new System.Windows.Forms.Label();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonPickAttachmentFont = new System.Windows.Forms.Button();
            this.comboBoxFontName = new System.Windows.Forms.ComboBox();
            this.labelFontName = new System.Windows.Forms.Label();
            this.numericUpDownSplitterWidth = new System.Windows.Forms.NumericUpDown();
            this.labelSplitterWidth = new System.Windows.Forms.Label();
            this.numericUpDownSplitterHeight = new System.Windows.Forms.NumericUpDown();
            this.labelSplitterHeight = new System.Windows.Forms.Label();
            this.labelFontSize = new System.Windows.Forms.Label();
            this.numericUpDownFontSize = new System.Windows.Forms.NumericUpDown();
            this.groupBoxChapters = new System.Windows.Forms.GroupBox();
            this.buttonTakePosFromVideo = new System.Windows.Forms.Button();
            this.labelXAdjust = new System.Windows.Forms.Label();
            this.numericUpDownXAdjust = new System.Windows.Forms.NumericUpDown();
            this.comboBoxTextHorizontalAlignment = new System.Windows.Forms.ComboBox();
            this.labelTextHorizontalAlignment = new System.Windows.Forms.Label();
            this.labelYAdjust = new System.Windows.Forms.Label();
            this.numericUpDownYAdjust = new System.Windows.Forms.NumericUpDown();
            this.textBoxChapterText = new System.Windows.Forms.TextBox();
            this.labelText = new System.Windows.Forms.Label();
            this.timeUpDownStartTime = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.labelStartTime = new System.Windows.Forms.Label();
            this.panelTextColor = new System.Windows.Forms.Panel();
            this.buttonTextColor = new System.Windows.Forms.Button();
            this.buttonRemoveAll = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.listViewChapters = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStart = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxSource = new System.Windows.Forms.TextBox();
            this.groupBoxStyle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSplitterWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSplitterHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).BeginInit();
            this.groupBoxChapters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownXAdjust)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownYAdjust)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxStyle
            // 
            this.groupBoxStyle.Controls.Add(this.comboBoxProgressBarEdge);
            this.groupBoxStyle.Controls.Add(this.labelEdgeStyle);
            this.groupBoxStyle.Controls.Add(this.radioButtonPosTop);
            this.groupBoxStyle.Controls.Add(this.radioButtonPosBottom);
            this.groupBoxStyle.Controls.Add(this.labelHeight);
            this.groupBoxStyle.Controls.Add(this.numericUpDownHeight);
            this.groupBoxStyle.Controls.Add(this.panelSecondaryColor);
            this.groupBoxStyle.Controls.Add(this.buttonSecondaryColor);
            this.groupBoxStyle.Controls.Add(this.panelPrimaryColor);
            this.groupBoxStyle.Controls.Add(this.buttonForeColor);
            this.groupBoxStyle.Controls.Add(this.labelPosition);
            this.groupBoxStyle.Location = new System.Drawing.Point(12, 12);
            this.groupBoxStyle.Name = "groupBoxStyle";
            this.groupBoxStyle.Size = new System.Drawing.Size(383, 167);
            this.groupBoxStyle.TabIndex = 0;
            this.groupBoxStyle.TabStop = false;
            this.groupBoxStyle.Text = "Progress bar";
            // 
            // comboBoxProgressBarEdge
            // 
            this.comboBoxProgressBarEdge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxProgressBarEdge.FormattingEnabled = true;
            this.comboBoxProgressBarEdge.Items.AddRange(new object[] {
            "Square corners",
            "Rounded corners"});
            this.comboBoxProgressBarEdge.Location = new System.Drawing.Point(91, 132);
            this.comboBoxProgressBarEdge.Name = "comboBoxProgressBarEdge";
            this.comboBoxProgressBarEdge.Size = new System.Drawing.Size(188, 21);
            this.comboBoxProgressBarEdge.TabIndex = 10;
            // 
            // labelEdgeStyle
            // 
            this.labelEdgeStyle.AutoSize = true;
            this.labelEdgeStyle.Location = new System.Drawing.Point(5, 135);
            this.labelEdgeStyle.Name = "labelEdgeStyle";
            this.labelEdgeStyle.Size = new System.Drawing.Size(30, 13);
            this.labelEdgeStyle.TabIndex = 9;
            this.labelEdgeStyle.Text = "Style";
            // 
            // radioButtonPosTop
            // 
            this.radioButtonPosTop.AutoSize = true;
            this.radioButtonPosTop.Location = new System.Drawing.Point(156, 25);
            this.radioButtonPosTop.Name = "radioButtonPosTop";
            this.radioButtonPosTop.Size = new System.Drawing.Size(44, 17);
            this.radioButtonPosTop.TabIndex = 2;
            this.radioButtonPosTop.Text = "Top";
            this.radioButtonPosTop.UseVisualStyleBackColor = true;
            // 
            // radioButtonPosBottom
            // 
            this.radioButtonPosBottom.AutoSize = true;
            this.radioButtonPosBottom.Checked = true;
            this.radioButtonPosBottom.Location = new System.Drawing.Point(92, 25);
            this.radioButtonPosBottom.Name = "radioButtonPosBottom";
            this.radioButtonPosBottom.Size = new System.Drawing.Size(58, 17);
            this.radioButtonPosBottom.TabIndex = 1;
            this.radioButtonPosBottom.TabStop = true;
            this.radioButtonPosBottom.Text = "Bottom";
            this.radioButtonPosBottom.UseVisualStyleBackColor = true;
            // 
            // labelHeight
            // 
            this.labelHeight.AutoSize = true;
            this.labelHeight.Location = new System.Drawing.Point(10, 55);
            this.labelHeight.Name = "labelHeight";
            this.labelHeight.Size = new System.Drawing.Size(38, 13);
            this.labelHeight.TabIndex = 3;
            this.labelHeight.Text = "Height";
            // 
            // numericUpDownHeight
            // 
            this.numericUpDownHeight.Location = new System.Drawing.Point(92, 48);
            this.numericUpDownHeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownHeight.Name = "numericUpDownHeight";
            this.numericUpDownHeight.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownHeight.TabIndex = 4;
            this.numericUpDownHeight.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.numericUpDownHeight.ValueChanged += new System.EventHandler(this.numericUpDownHeight_ValueChanged);
            // 
            // panelSecondaryColor
            // 
            this.panelSecondaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSecondaryColor.Location = new System.Drawing.Point(206, 104);
            this.panelSecondaryColor.Name = "panelSecondaryColor";
            this.panelSecondaryColor.Size = new System.Drawing.Size(21, 20);
            this.panelSecondaryColor.TabIndex = 8;
            this.panelSecondaryColor.Click += new System.EventHandler(this.panelSecondaryColor_Click);
            // 
            // buttonSecondaryColor
            // 
            this.buttonSecondaryColor.Location = new System.Drawing.Point(92, 103);
            this.buttonSecondaryColor.Name = "buttonSecondaryColor";
            this.buttonSecondaryColor.Size = new System.Drawing.Size(109, 23);
            this.buttonSecondaryColor.TabIndex = 7;
            this.buttonSecondaryColor.Text = "Background color";
            this.buttonSecondaryColor.UseVisualStyleBackColor = true;
            this.buttonSecondaryColor.Click += new System.EventHandler(this.buttonSecondaryColor_Click);
            // 
            // panelPrimaryColor
            // 
            this.panelPrimaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPrimaryColor.Location = new System.Drawing.Point(207, 75);
            this.panelPrimaryColor.Name = "panelPrimaryColor";
            this.panelPrimaryColor.Size = new System.Drawing.Size(21, 20);
            this.panelPrimaryColor.TabIndex = 6;
            this.panelPrimaryColor.Click += new System.EventHandler(this.panelPrimaryColor_Click);
            // 
            // buttonForeColor
            // 
            this.buttonForeColor.Location = new System.Drawing.Point(92, 74);
            this.buttonForeColor.Name = "buttonForeColor";
            this.buttonForeColor.Size = new System.Drawing.Size(108, 23);
            this.buttonForeColor.TabIndex = 5;
            this.buttonForeColor.Text = "Foreground color";
            this.buttonForeColor.UseVisualStyleBackColor = true;
            this.buttonForeColor.Click += new System.EventHandler(this.buttonForeColor_Click);
            // 
            // labelPosition
            // 
            this.labelPosition.AutoSize = true;
            this.labelPosition.Location = new System.Drawing.Point(10, 30);
            this.labelPosition.Name = "labelPosition";
            this.labelPosition.Size = new System.Drawing.Size(44, 13);
            this.labelPosition.TabIndex = 0;
            this.labelPosition.Text = "Position";
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.Location = new System.Drawing.Point(1059, 619);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(103, 23);
            this.buttonReset.TabIndex = 2;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonPickAttachmentFont
            // 
            this.buttonPickAttachmentFont.Location = new System.Drawing.Point(286, 50);
            this.buttonPickAttachmentFont.Name = "buttonPickAttachmentFont";
            this.buttonPickAttachmentFont.Size = new System.Drawing.Size(24, 23);
            this.buttonPickAttachmentFont.TabIndex = 6;
            this.buttonPickAttachmentFont.Text = "...";
            this.buttonPickAttachmentFont.UseVisualStyleBackColor = true;
            this.buttonPickAttachmentFont.Click += new System.EventHandler(this.buttonPickAttachmentFont_Click);
            // 
            // comboBoxFontName
            // 
            this.comboBoxFontName.FormattingEnabled = true;
            this.comboBoxFontName.Location = new System.Drawing.Point(92, 50);
            this.comboBoxFontName.Name = "comboBoxFontName";
            this.comboBoxFontName.Size = new System.Drawing.Size(188, 21);
            this.comboBoxFontName.TabIndex = 5;
            // 
            // labelFontName
            // 
            this.labelFontName.AutoSize = true;
            this.labelFontName.Location = new System.Drawing.Point(10, 53);
            this.labelFontName.Name = "labelFontName";
            this.labelFontName.Size = new System.Drawing.Size(57, 13);
            this.labelFontName.TabIndex = 4;
            this.labelFontName.Text = "Font name";
            // 
            // numericUpDownSplitterWidth
            // 
            this.numericUpDownSplitterWidth.Location = new System.Drawing.Point(92, 24);
            this.numericUpDownSplitterWidth.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownSplitterWidth.Name = "numericUpDownSplitterWidth";
            this.numericUpDownSplitterWidth.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownSplitterWidth.TabIndex = 1;
            this.numericUpDownSplitterWidth.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // labelSplitterWidth
            // 
            this.labelSplitterWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSplitterWidth.AutoSize = true;
            this.labelSplitterWidth.Location = new System.Drawing.Point(10, 26);
            this.labelSplitterWidth.Name = "labelSplitterWidth";
            this.labelSplitterWidth.Size = new System.Drawing.Size(67, 13);
            this.labelSplitterWidth.TabIndex = 0;
            this.labelSplitterWidth.Text = "Splitter width";
            // 
            // numericUpDownSplitterHeight
            // 
            this.numericUpDownSplitterHeight.Location = new System.Drawing.Point(274, 24);
            this.numericUpDownSplitterHeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownSplitterHeight.Name = "numericUpDownSplitterHeight";
            this.numericUpDownSplitterHeight.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownSplitterHeight.TabIndex = 3;
            this.numericUpDownSplitterHeight.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.numericUpDownSplitterHeight.ValueChanged += new System.EventHandler(this.numericUpDownSplitterHeight_ValueChanged);
            // 
            // labelSplitterHeight
            // 
            this.labelSplitterHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSplitterHeight.AutoSize = true;
            this.labelSplitterHeight.Location = new System.Drawing.Point(192, 26);
            this.labelSplitterHeight.Name = "labelSplitterHeight";
            this.labelSplitterHeight.Size = new System.Drawing.Size(71, 13);
            this.labelSplitterHeight.TabIndex = 2;
            this.labelSplitterHeight.Text = "Splitter height";
            // 
            // labelFontSize
            // 
            this.labelFontSize.AutoSize = true;
            this.labelFontSize.Location = new System.Drawing.Point(10, 84);
            this.labelFontSize.Name = "labelFontSize";
            this.labelFontSize.Size = new System.Drawing.Size(49, 13);
            this.labelFontSize.TabIndex = 7;
            this.labelFontSize.Text = "Font size";
            // 
            // numericUpDownFontSize
            // 
            this.numericUpDownFontSize.Location = new System.Drawing.Point(92, 77);
            this.numericUpDownFontSize.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDownFontSize.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownFontSize.Name = "numericUpDownFontSize";
            this.numericUpDownFontSize.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownFontSize.TabIndex = 8;
            this.numericUpDownFontSize.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // groupBoxChapters
            // 
            this.groupBoxChapters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxChapters.Controls.Add(this.buttonTakePosFromVideo);
            this.groupBoxChapters.Controls.Add(this.labelXAdjust);
            this.groupBoxChapters.Controls.Add(this.numericUpDownXAdjust);
            this.groupBoxChapters.Controls.Add(this.comboBoxTextHorizontalAlignment);
            this.groupBoxChapters.Controls.Add(this.labelTextHorizontalAlignment);
            this.groupBoxChapters.Controls.Add(this.labelYAdjust);
            this.groupBoxChapters.Controls.Add(this.numericUpDownYAdjust);
            this.groupBoxChapters.Controls.Add(this.textBoxChapterText);
            this.groupBoxChapters.Controls.Add(this.labelText);
            this.groupBoxChapters.Controls.Add(this.timeUpDownStartTime);
            this.groupBoxChapters.Controls.Add(this.labelStartTime);
            this.groupBoxChapters.Controls.Add(this.panelTextColor);
            this.groupBoxChapters.Controls.Add(this.buttonTextColor);
            this.groupBoxChapters.Controls.Add(this.buttonPickAttachmentFont);
            this.groupBoxChapters.Controls.Add(this.comboBoxFontName);
            this.groupBoxChapters.Controls.Add(this.buttonRemoveAll);
            this.groupBoxChapters.Controls.Add(this.labelFontName);
            this.groupBoxChapters.Controls.Add(this.buttonAdd);
            this.groupBoxChapters.Controls.Add(this.buttonRemove);
            this.groupBoxChapters.Controls.Add(this.listViewChapters);
            this.groupBoxChapters.Controls.Add(this.labelSplitterWidth);
            this.groupBoxChapters.Controls.Add(this.numericUpDownFontSize);
            this.groupBoxChapters.Controls.Add(this.numericUpDownSplitterWidth);
            this.groupBoxChapters.Controls.Add(this.labelFontSize);
            this.groupBoxChapters.Controls.Add(this.labelSplitterHeight);
            this.groupBoxChapters.Controls.Add(this.numericUpDownSplitterHeight);
            this.groupBoxChapters.Location = new System.Drawing.Point(12, 185);
            this.groupBoxChapters.Name = "groupBoxChapters";
            this.groupBoxChapters.Size = new System.Drawing.Size(383, 418);
            this.groupBoxChapters.TabIndex = 1;
            this.groupBoxChapters.TabStop = false;
            this.groupBoxChapters.Text = "Chapters";
            // 
            // buttonTakePosFromVideo
            // 
            this.buttonTakePosFromVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonTakePosFromVideo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonTakePosFromVideo.Location = new System.Drawing.Point(181, 291);
            this.buttonTakePosFromVideo.Name = "buttonTakePosFromVideo";
            this.buttonTakePosFromVideo.Size = new System.Drawing.Size(145, 23);
            this.buttonTakePosFromVideo.TabIndex = 20;
            this.buttonTakePosFromVideo.Text = "Take pos from video";
            this.buttonTakePosFromVideo.UseVisualStyleBackColor = true;
            this.buttonTakePosFromVideo.Click += new System.EventHandler(this.buttonTakePosFromVideo_Click);
            // 
            // labelXAdjust
            // 
            this.labelXAdjust.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelXAdjust.AutoSize = true;
            this.labelXAdjust.Location = new System.Drawing.Point(9, 134);
            this.labelXAdjust.Name = "labelXAdjust";
            this.labelXAdjust.Size = new System.Drawing.Size(45, 13);
            this.labelXAdjust.TabIndex = 11;
            this.labelXAdjust.Text = "X adjust";
            // 
            // numericUpDownXAdjust
            // 
            this.numericUpDownXAdjust.Location = new System.Drawing.Point(92, 134);
            this.numericUpDownXAdjust.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericUpDownXAdjust.Name = "numericUpDownXAdjust";
            this.numericUpDownXAdjust.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownXAdjust.TabIndex = 12;
            // 
            // comboBoxTextHorizontalAlignment
            // 
            this.comboBoxTextHorizontalAlignment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTextHorizontalAlignment.FormattingEnabled = true;
            this.comboBoxTextHorizontalAlignment.Items.AddRange(new object[] {
            "Left",
            "Center",
            "Right"});
            this.comboBoxTextHorizontalAlignment.Location = new System.Drawing.Point(92, 160);
            this.comboBoxTextHorizontalAlignment.Name = "comboBoxTextHorizontalAlignment";
            this.comboBoxTextHorizontalAlignment.Size = new System.Drawing.Size(188, 21);
            this.comboBoxTextHorizontalAlignment.TabIndex = 16;
            // 
            // labelTextHorizontalAlignment
            // 
            this.labelTextHorizontalAlignment.AutoSize = true;
            this.labelTextHorizontalAlignment.Location = new System.Drawing.Point(6, 163);
            this.labelTextHorizontalAlignment.Name = "labelTextHorizontalAlignment";
            this.labelTextHorizontalAlignment.Size = new System.Drawing.Size(76, 13);
            this.labelTextHorizontalAlignment.TabIndex = 15;
            this.labelTextHorizontalAlignment.Text = "Text alignment";
            // 
            // labelYAdjust
            // 
            this.labelYAdjust.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelYAdjust.AutoSize = true;
            this.labelYAdjust.Location = new System.Drawing.Point(176, 136);
            this.labelYAdjust.Name = "labelYAdjust";
            this.labelYAdjust.Size = new System.Drawing.Size(45, 13);
            this.labelYAdjust.TabIndex = 13;
            this.labelYAdjust.Text = "Y adjust";
            // 
            // numericUpDownYAdjust
            // 
            this.numericUpDownYAdjust.Location = new System.Drawing.Point(227, 134);
            this.numericUpDownYAdjust.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericUpDownYAdjust.Name = "numericUpDownYAdjust";
            this.numericUpDownYAdjust.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownYAdjust.TabIndex = 14;
            // 
            // textBoxChapterText
            // 
            this.textBoxChapterText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxChapterText.Location = new System.Drawing.Point(63, 324);
            this.textBoxChapterText.Multiline = true;
            this.textBoxChapterText.Name = "textBoxChapterText";
            this.textBoxChapterText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxChapterText.Size = new System.Drawing.Size(314, 46);
            this.textBoxChapterText.TabIndex = 21;
            // 
            // labelText
            // 
            this.labelText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelText.AutoSize = true;
            this.labelText.Location = new System.Drawing.Point(6, 327);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(28, 13);
            this.labelText.TabIndex = 20;
            this.labelText.Text = "Text";
            // 
            // timeUpDownStartTime
            // 
            this.timeUpDownStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.timeUpDownStartTime.AutoSize = true;
            this.timeUpDownStartTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownStartTime.Location = new System.Drawing.Point(63, 290);
            this.timeUpDownStartTime.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownStartTime.Name = "timeUpDownStartTime";
            this.timeUpDownStartTime.Size = new System.Drawing.Size(111, 27);
            this.timeUpDownStartTime.TabIndex = 19;
            timeCode2.Hours = 0;
            timeCode2.Milliseconds = 0;
            timeCode2.Minutes = 0;
            timeCode2.Seconds = 0;
            timeCode2.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode2.TotalMilliseconds = 0D;
            timeCode2.TotalSeconds = 0D;
            this.timeUpDownStartTime.TimeCode = timeCode2;
            this.timeUpDownStartTime.UseVideoOffset = false;
            // 
            // labelStartTime
            // 
            this.labelStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStartTime.AutoSize = true;
            this.labelStartTime.Location = new System.Drawing.Point(6, 297);
            this.labelStartTime.Name = "labelStartTime";
            this.labelStartTime.Size = new System.Drawing.Size(51, 13);
            this.labelStartTime.TabIndex = 18;
            this.labelStartTime.Text = "Start time";
            // 
            // panelTextColor
            // 
            this.panelTextColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTextColor.Location = new System.Drawing.Point(207, 104);
            this.panelTextColor.Name = "panelTextColor";
            this.panelTextColor.Size = new System.Drawing.Size(21, 20);
            this.panelTextColor.TabIndex = 10;
            this.panelTextColor.Click += new System.EventHandler(this.panelTextColor_Click);
            // 
            // buttonTextColor
            // 
            this.buttonTextColor.Location = new System.Drawing.Point(91, 103);
            this.buttonTextColor.Name = "buttonTextColor";
            this.buttonTextColor.Size = new System.Drawing.Size(109, 23);
            this.buttonTextColor.TabIndex = 9;
            this.buttonTextColor.Text = "Text color";
            this.buttonTextColor.UseVisualStyleBackColor = true;
            this.buttonTextColor.Click += new System.EventHandler(this.buttonTextColor_Click);
            // 
            // buttonRemoveAll
            // 
            this.buttonRemoveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemoveAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemoveAll.Location = new System.Drawing.Point(190, 386);
            this.buttonRemoveAll.Name = "buttonRemoveAll";
            this.buttonRemoveAll.Size = new System.Drawing.Size(92, 23);
            this.buttonRemoveAll.TabIndex = 24;
            this.buttonRemoveAll.Text = "Remove all";
            this.buttonRemoveAll.UseVisualStyleBackColor = true;
            this.buttonRemoveAll.Click += new System.EventHandler(this.buttonRemoveAll_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAdd.Location = new System.Drawing.Point(4, 386);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(82, 23);
            this.buttonAdd.TabIndex = 22;
            this.buttonAdd.Text = "New";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemove.Location = new System.Drawing.Point(92, 386);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(92, 23);
            this.buttonRemove.TabIndex = 23;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // listViewChapters
            // 
            this.listViewChapters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewChapters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderStart});
            this.listViewChapters.FullRowSelect = true;
            this.listViewChapters.HideSelection = false;
            this.listViewChapters.Location = new System.Drawing.Point(6, 187);
            this.listViewChapters.MultiSelect = false;
            this.listViewChapters.Name = "listViewChapters";
            this.listViewChapters.Size = new System.Drawing.Size(371, 96);
            this.listViewChapters.TabIndex = 17;
            this.listViewChapters.UseCompatibleStateImageBehavior = false;
            this.listViewChapters.View = System.Windows.Forms.View.Details;
            this.listViewChapters.SelectedIndexChanged += new System.EventHandler(this.listViewChapters_SelectedIndexChanged);
            this.listViewChapters.DoubleClick += new System.EventHandler(this.listViewChapters_DoubleClick);
            this.listViewChapters.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewChapters_KeyDown);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Text";
            this.columnHeaderName.Width = 210;
            // 
            // columnHeaderStart
            // 
            this.columnHeaderStart.Text = "Start";
            this.columnHeaderStart.Width = 128;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(1168, 619);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(1249, 619);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // textBoxSource
            // 
            this.textBoxSource.Location = new System.Drawing.Point(0, 3);
            this.textBoxSource.Multiline = true;
            this.textBoxSource.Name = "textBoxSource";
            this.textBoxSource.Size = new System.Drawing.Size(923, 595);
            this.textBoxSource.TabIndex = 9;
            this.textBoxSource.Visible = false;
            // 
            // AssaProgressBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1336, 654);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.groupBoxChapters);
            this.Controls.Add(this.groupBoxStyle);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1200, 680);
            this.Name = "AssaProgressBar";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generate progress bar";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AssaProgressBar_FormClosing);
            this.Shown += new System.EventHandler(this.AssaProgressBar_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AssaProgressBar_KeyDown);
            this.groupBoxStyle.ResumeLayout(false);
            this.groupBoxStyle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSplitterWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSplitterHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).EndInit();
            this.groupBoxChapters.ResumeLayout(false);
            this.groupBoxChapters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownXAdjust)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownYAdjust)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxStyle;
        private System.Windows.Forms.GroupBox groupBoxChapters;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelFontSize;
        private System.Windows.Forms.NumericUpDown numericUpDownFontSize;
        private System.Windows.Forms.Label labelPosition;
        private System.Windows.Forms.Panel panelSecondaryColor;
        private System.Windows.Forms.Button buttonSecondaryColor;
        private System.Windows.Forms.Panel panelPrimaryColor;
        private System.Windows.Forms.Button buttonForeColor;
        private System.Windows.Forms.Label labelSplitterHeight;
        private System.Windows.Forms.ListView listViewChapters;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderStart;
        private System.Windows.Forms.Button buttonRemoveAll;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Label labelHeight;
        private System.Windows.Forms.NumericUpDown numericUpDownHeight;
        private System.Windows.Forms.NumericUpDown numericUpDownSplitterWidth;
        private System.Windows.Forms.Label labelSplitterWidth;
        private System.Windows.Forms.NumericUpDown numericUpDownSplitterHeight;
        private System.Windows.Forms.RadioButton radioButtonPosTop;
        private System.Windows.Forms.RadioButton radioButtonPosBottom;
        private System.Windows.Forms.Button buttonPickAttachmentFont;
        private System.Windows.Forms.ComboBox comboBoxFontName;
        private System.Windows.Forms.Label labelFontName;
        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.Panel panelTextColor;
        private System.Windows.Forms.Button buttonTextColor;
        private Controls.TimeUpDown timeUpDownStartTime;
        private System.Windows.Forms.Label labelStartTime;
        private System.Windows.Forms.TextBox textBoxChapterText;
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.Label labelYAdjust;
        private System.Windows.Forms.NumericUpDown numericUpDownYAdjust;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.ComboBox comboBoxTextHorizontalAlignment;
        private System.Windows.Forms.Label labelTextHorizontalAlignment;
        private System.Windows.Forms.Label labelXAdjust;
        private System.Windows.Forms.NumericUpDown numericUpDownXAdjust;
        private System.Windows.Forms.ComboBox comboBoxProgressBarEdge;
        private System.Windows.Forms.Label labelEdgeStyle;
        private System.Windows.Forms.Button buttonTakePosFromVideo;
    }
}