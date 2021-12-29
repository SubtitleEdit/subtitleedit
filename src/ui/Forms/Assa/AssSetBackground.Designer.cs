
namespace Nikse.SubtitleEdit.Forms.Assa
{
    sealed partial class AssSetBackground
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
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBoxDrawing = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.labelRotateX = new System.Windows.Forms.Label();
            this.numericUpDownRotateY = new System.Windows.Forms.NumericUpDown();
            this.labelRotateY = new System.Windows.Forms.Label();
            this.numericUpDownRotateX = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownPaddingBottom = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownPaddingTop = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.labelEdgeStyle = new System.Windows.Forms.Label();
            this.comboBoxBoxStyle = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownBoxLayer = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDownPaddingRight = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numericUpDownPaddingLeft = new System.Windows.Forms.NumericUpDown();
            this.panelPrimaryColor = new System.Windows.Forms.Panel();
            this.buttonPrimaryColor = new System.Windows.Forms.Button();
            this.panelShadowColor = new System.Windows.Forms.Panel();
            this.buttonShadowColor = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.numericUpDownShadowWidth = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.numericUpDownOutlineWidth = new System.Windows.Forms.NumericUpDown();
            this.panelOutlineColor = new System.Windows.Forms.Panel();
            this.buttonOutlineColor = new System.Windows.Forms.Button();
            this.groupBoxFillWidth = new System.Windows.Forms.GroupBox();
            this.checkBoxFillWidth = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.numericUpDownFillWidthMarginRight = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.numericUpDownFillWidthMarginLeft = new System.Windows.Forms.NumericUpDown();
            this.labelRadius = new System.Windows.Forms.Label();
            this.numericUpDownRadius = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.groupBoxPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.groupBoxDrawing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaddingBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaddingTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBoxLayer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaddingRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaddingLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOutlineWidth)).BeginInit();
            this.groupBoxFillWidth.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFillWidthMarginRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFillWidthMarginLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.pictureBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(12, 232);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(949, 462);
            this.groupBoxPreview.TabIndex = 2;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 16);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(943, 443);
            this.pictureBoxPreview.TabIndex = 0;
            this.pictureBoxPreview.TabStop = false;
            this.pictureBoxPreview.Click += new System.EventHandler(this.pictureBoxPreview_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(805, 699);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(886, 699);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 250;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // groupBoxDrawing
            // 
            this.groupBoxDrawing.Controls.Add(this.label4);
            this.groupBoxDrawing.Controls.Add(this.numericUpDown4);
            this.groupBoxDrawing.Controls.Add(this.textBox1);
            this.groupBoxDrawing.Controls.Add(this.labelRotateX);
            this.groupBoxDrawing.Controls.Add(this.numericUpDownRotateY);
            this.groupBoxDrawing.Controls.Add(this.labelRotateY);
            this.groupBoxDrawing.Controls.Add(this.numericUpDownRotateX);
            this.groupBoxDrawing.Location = new System.Drawing.Point(727, 12);
            this.groupBoxDrawing.Name = "groupBoxDrawing";
            this.groupBoxDrawing.Size = new System.Drawing.Size(231, 214);
            this.groupBoxDrawing.TabIndex = 29;
            this.groupBoxDrawing.TabStop = false;
            this.groupBoxDrawing.Text = "Drawing";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 32;
            this.label4.Text = "Layer";
            // 
            // numericUpDown4
            // 
            this.numericUpDown4.Location = new System.Drawing.Point(82, 70);
            this.numericUpDown4.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDown4.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numericUpDown4.Name = "numericUpDown4";
            this.numericUpDown4.Size = new System.Drawing.Size(52, 20);
            this.numericUpDown4.TabIndex = 33;
            this.numericUpDown4.Value = new decimal(new int[] {
            8,
            0,
            0,
            -2147483648});
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 96);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(219, 112);
            this.textBox1.TabIndex = 21;
            // 
            // labelRotateX
            // 
            this.labelRotateX.AutoSize = true;
            this.labelRotateX.Location = new System.Drawing.Point(6, 17);
            this.labelRotateX.Name = "labelRotateX";
            this.labelRotateX.Size = new System.Drawing.Size(56, 13);
            this.labelRotateX.TabIndex = 17;
            this.labelRotateX.Text = "Margin left";
            // 
            // numericUpDownRotateY
            // 
            this.numericUpDownRotateY.Location = new System.Drawing.Point(82, 40);
            this.numericUpDownRotateY.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDownRotateY.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numericUpDownRotateY.Name = "numericUpDownRotateY";
            this.numericUpDownRotateY.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownRotateY.TabIndex = 20;
            this.numericUpDownRotateY.ValueChanged += new System.EventHandler(this.numericUpDownRotateX_ValueChanged);
            // 
            // labelRotateY
            // 
            this.labelRotateY.AutoSize = true;
            this.labelRotateY.Location = new System.Drawing.Point(6, 42);
            this.labelRotateY.Name = "labelRotateY";
            this.labelRotateY.Size = new System.Drawing.Size(62, 13);
            this.labelRotateY.TabIndex = 19;
            this.labelRotateY.Text = "Margin right";
            // 
            // numericUpDownRotateX
            // 
            this.numericUpDownRotateX.Location = new System.Drawing.Point(82, 15);
            this.numericUpDownRotateX.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDownRotateX.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numericUpDownRotateX.Name = "numericUpDownRotateX";
            this.numericUpDownRotateX.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownRotateX.TabIndex = 18;
            this.numericUpDownRotateX.ValueChanged += new System.EventHandler(this.numericUpDownRotateX_ValueChanged);
            // 
            // numericUpDownPaddingBottom
            // 
            this.numericUpDownPaddingBottom.Location = new System.Drawing.Point(113, 96);
            this.numericUpDownPaddingBottom.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownPaddingBottom.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDownPaddingBottom.Name = "numericUpDownPaddingBottom";
            this.numericUpDownPaddingBottom.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownPaddingBottom.TabIndex = 24;
            this.numericUpDownPaddingBottom.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.numericUpDownPaddingBottom.ValueChanged += new System.EventHandler(this.PreviewValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 98);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "Padding bottom";
            // 
            // numericUpDownPaddingTop
            // 
            this.numericUpDownPaddingTop.Location = new System.Drawing.Point(113, 71);
            this.numericUpDownPaddingTop.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownPaddingTop.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDownPaddingTop.Name = "numericUpDownPaddingTop";
            this.numericUpDownPaddingTop.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownPaddingTop.TabIndex = 22;
            this.numericUpDownPaddingTop.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.numericUpDownPaddingTop.ValueChanged += new System.EventHandler(this.PreviewValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Padding top";
            // 
            // labelEdgeStyle
            // 
            this.labelEdgeStyle.AutoSize = true;
            this.labelEdgeStyle.Location = new System.Drawing.Point(13, 136);
            this.labelEdgeStyle.Name = "labelEdgeStyle";
            this.labelEdgeStyle.Size = new System.Drawing.Size(30, 13);
            this.labelEdgeStyle.TabIndex = 25;
            this.labelEdgeStyle.Text = "Style";
            // 
            // comboBoxBoxStyle
            // 
            this.comboBoxBoxStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBoxStyle.FormattingEnabled = true;
            this.comboBoxBoxStyle.Items.AddRange(new object[] {
            "Square corners",
            "Rounded corners",
            "Spikes"});
            this.comboBoxBoxStyle.Location = new System.Drawing.Point(49, 133);
            this.comboBoxBoxStyle.Name = "comboBoxBoxStyle";
            this.comboBoxBoxStyle.Size = new System.Drawing.Size(116, 21);
            this.comboBoxBoxStyle.TabIndex = 26;
            this.comboBoxBoxStyle.SelectedIndexChanged += new System.EventHandler(this.comboBoxBoxStyle_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(355, 142);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 30;
            this.label3.Text = "Layer";
            // 
            // numericUpDownBoxLayer
            // 
            this.numericUpDownBoxLayer.Location = new System.Drawing.Point(394, 142);
            this.numericUpDownBoxLayer.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDownBoxLayer.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numericUpDownBoxLayer.Name = "numericUpDownBoxLayer";
            this.numericUpDownBoxLayer.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownBoxLayer.TabIndex = 31;
            this.numericUpDownBoxLayer.Value = new decimal(new int[] {
            9,
            0,
            0,
            -2147483648});
            this.numericUpDownBoxLayer.ValueChanged += new System.EventHandler(this.PreviewValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 13);
            this.label5.TabIndex = 34;
            this.label5.Text = "Padding left";
            // 
            // numericUpDownPaddingRight
            // 
            this.numericUpDownPaddingRight.Location = new System.Drawing.Point(113, 42);
            this.numericUpDownPaddingRight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownPaddingRight.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDownPaddingRight.Name = "numericUpDownPaddingRight";
            this.numericUpDownPaddingRight.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownPaddingRight.TabIndex = 37;
            this.numericUpDownPaddingRight.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownPaddingRight.ValueChanged += new System.EventHandler(this.PreviewValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 44);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(69, 13);
            this.label6.TabIndex = 36;
            this.label6.Text = "Padding right";
            // 
            // numericUpDownPaddingLeft
            // 
            this.numericUpDownPaddingLeft.Location = new System.Drawing.Point(113, 17);
            this.numericUpDownPaddingLeft.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownPaddingLeft.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDownPaddingLeft.Name = "numericUpDownPaddingLeft";
            this.numericUpDownPaddingLeft.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownPaddingLeft.TabIndex = 35;
            this.numericUpDownPaddingLeft.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownPaddingLeft.ValueChanged += new System.EventHandler(this.PreviewValueChanged);
            // 
            // panelPrimaryColor
            // 
            this.panelPrimaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPrimaryColor.Location = new System.Drawing.Point(291, 20);
            this.panelPrimaryColor.Name = "panelPrimaryColor";
            this.panelPrimaryColor.Size = new System.Drawing.Size(21, 20);
            this.panelPrimaryColor.TabIndex = 39;
            this.panelPrimaryColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelPrimaryColor_MouseClick);
            // 
            // buttonPrimaryColor
            // 
            this.buttonPrimaryColor.Location = new System.Drawing.Point(200, 19);
            this.buttonPrimaryColor.Name = "buttonPrimaryColor";
            this.buttonPrimaryColor.Size = new System.Drawing.Size(85, 23);
            this.buttonPrimaryColor.TabIndex = 38;
            this.buttonPrimaryColor.Text = "Box color";
            this.buttonPrimaryColor.UseVisualStyleBackColor = true;
            this.buttonPrimaryColor.Click += new System.EventHandler(this.buttonPrimaryColor_Click);
            // 
            // panelShadowColor
            // 
            this.panelShadowColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelShadowColor.Location = new System.Drawing.Point(291, 140);
            this.panelShadowColor.Name = "panelShadowColor";
            this.panelShadowColor.Size = new System.Drawing.Size(21, 20);
            this.panelShadowColor.TabIndex = 41;
            this.panelShadowColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelShadowColor_MouseClick);
            // 
            // buttonShadowColor
            // 
            this.buttonShadowColor.Location = new System.Drawing.Point(200, 139);
            this.buttonShadowColor.Name = "buttonShadowColor";
            this.buttonShadowColor.Size = new System.Drawing.Size(85, 23);
            this.buttonShadowColor.TabIndex = 40;
            this.buttonShadowColor.Text = "Shadow color";
            this.buttonShadowColor.UseVisualStyleBackColor = true;
            this.buttonShadowColor.Click += new System.EventHandler(this.buttonShadowColor_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(197, 169);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 13);
            this.label7.TabIndex = 42;
            this.label7.Text = "Shadow distance";
            // 
            // numericUpDownShadowWidth
            // 
            this.numericUpDownShadowWidth.Location = new System.Drawing.Point(200, 185);
            this.numericUpDownShadowWidth.Name = "numericUpDownShadowWidth";
            this.numericUpDownShadowWidth.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownShadowWidth.TabIndex = 43;
            this.numericUpDownShadowWidth.ValueChanged += new System.EventHandler(this.PreviewValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(197, 88);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(68, 13);
            this.label8.TabIndex = 46;
            this.label8.Text = "Outline width";
            // 
            // numericUpDownOutlineWidth
            // 
            this.numericUpDownOutlineWidth.Location = new System.Drawing.Point(200, 104);
            this.numericUpDownOutlineWidth.Name = "numericUpDownOutlineWidth";
            this.numericUpDownOutlineWidth.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownOutlineWidth.TabIndex = 47;
            this.numericUpDownOutlineWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownOutlineWidth.ValueChanged += new System.EventHandler(this.PreviewValueChanged);
            // 
            // panelOutlineColor
            // 
            this.panelOutlineColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelOutlineColor.Location = new System.Drawing.Point(291, 59);
            this.panelOutlineColor.Name = "panelOutlineColor";
            this.panelOutlineColor.Size = new System.Drawing.Size(21, 20);
            this.panelOutlineColor.TabIndex = 45;
            this.panelOutlineColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelOutlineColor_MouseClick);
            // 
            // buttonOutlineColor
            // 
            this.buttonOutlineColor.Location = new System.Drawing.Point(200, 58);
            this.buttonOutlineColor.Name = "buttonOutlineColor";
            this.buttonOutlineColor.Size = new System.Drawing.Size(85, 23);
            this.buttonOutlineColor.TabIndex = 44;
            this.buttonOutlineColor.Text = "Outline color";
            this.buttonOutlineColor.UseVisualStyleBackColor = true;
            this.buttonOutlineColor.Click += new System.EventHandler(this.buttonOutlineColor_Click);
            // 
            // groupBoxFillWidth
            // 
            this.groupBoxFillWidth.Controls.Add(this.checkBoxFillWidth);
            this.groupBoxFillWidth.Controls.Add(this.label10);
            this.groupBoxFillWidth.Controls.Add(this.numericUpDownFillWidthMarginRight);
            this.groupBoxFillWidth.Controls.Add(this.label11);
            this.groupBoxFillWidth.Controls.Add(this.numericUpDownFillWidthMarginLeft);
            this.groupBoxFillWidth.Location = new System.Drawing.Point(347, 20);
            this.groupBoxFillWidth.Name = "groupBoxFillWidth";
            this.groupBoxFillWidth.Size = new System.Drawing.Size(178, 104);
            this.groupBoxFillWidth.TabIndex = 48;
            this.groupBoxFillWidth.TabStop = false;
            this.groupBoxFillWidth.Text = "Fill width";
            // 
            // checkBoxFillWidth
            // 
            this.checkBoxFillWidth.AutoSize = true;
            this.checkBoxFillWidth.Location = new System.Drawing.Point(7, 22);
            this.checkBoxFillWidth.Name = "checkBoxFillWidth";
            this.checkBoxFillWidth.Size = new System.Drawing.Size(66, 17);
            this.checkBoxFillWidth.TabIndex = 34;
            this.checkBoxFillWidth.Text = "Fill width";
            this.checkBoxFillWidth.UseVisualStyleBackColor = true;
            this.checkBoxFillWidth.CheckedChanged += new System.EventHandler(this.checkBoxFillWidth_CheckedChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(17, 50);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(56, 13);
            this.label10.TabIndex = 17;
            this.label10.Text = "Margin left";
            // 
            // numericUpDownFillWidthMarginRight
            // 
            this.numericUpDownFillWidthMarginRight.Location = new System.Drawing.Point(93, 73);
            this.numericUpDownFillWidthMarginRight.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDownFillWidthMarginRight.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numericUpDownFillWidthMarginRight.Name = "numericUpDownFillWidthMarginRight";
            this.numericUpDownFillWidthMarginRight.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownFillWidthMarginRight.TabIndex = 20;
            this.numericUpDownFillWidthMarginRight.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownFillWidthMarginRight.ValueChanged += new System.EventHandler(this.PreviewValueChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(17, 75);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(62, 13);
            this.label11.TabIndex = 19;
            this.label11.Text = "Margin right";
            // 
            // numericUpDownFillWidthMarginLeft
            // 
            this.numericUpDownFillWidthMarginLeft.Location = new System.Drawing.Point(93, 48);
            this.numericUpDownFillWidthMarginLeft.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDownFillWidthMarginLeft.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numericUpDownFillWidthMarginLeft.Name = "numericUpDownFillWidthMarginLeft";
            this.numericUpDownFillWidthMarginLeft.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownFillWidthMarginLeft.TabIndex = 18;
            this.numericUpDownFillWidthMarginLeft.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownFillWidthMarginLeft.ValueChanged += new System.EventHandler(this.PreviewValueChanged);
            // 
            // labelRadius
            // 
            this.labelRadius.AutoSize = true;
            this.labelRadius.Location = new System.Drawing.Point(13, 162);
            this.labelRadius.Name = "labelRadius";
            this.labelRadius.Size = new System.Drawing.Size(40, 13);
            this.labelRadius.TabIndex = 49;
            this.labelRadius.Text = "Radius";
            // 
            // numericUpDownRadius
            // 
            this.numericUpDownRadius.Location = new System.Drawing.Point(113, 160);
            this.numericUpDownRadius.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDownRadius.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownRadius.Name = "numericUpDownRadius";
            this.numericUpDownRadius.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownRadius.TabIndex = 50;
            this.numericUpDownRadius.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownRadius.ValueChanged += new System.EventHandler(this.PreviewValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 188);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(40, 13);
            this.label9.TabIndex = 51;
            this.label9.Text = "Radius";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(113, 186);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(52, 20);
            this.numericUpDown1.TabIndex = 52;
            this.numericUpDown1.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // AssSetBackground
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(973, 734);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.labelRadius);
            this.Controls.Add(this.numericUpDownRadius);
            this.Controls.Add(this.groupBoxFillWidth);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.numericUpDownOutlineWidth);
            this.Controls.Add(this.panelOutlineColor);
            this.Controls.Add(this.buttonOutlineColor);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.numericUpDownShadowWidth);
            this.Controls.Add(this.panelShadowColor);
            this.Controls.Add(this.buttonShadowColor);
            this.Controls.Add(this.panelPrimaryColor);
            this.Controls.Add(this.buttonPrimaryColor);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numericUpDownPaddingRight);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numericUpDownBoxLayer);
            this.Controls.Add(this.numericUpDownPaddingLeft);
            this.Controls.Add(this.groupBoxDrawing);
            this.Controls.Add(this.comboBoxBoxStyle);
            this.Controls.Add(this.labelEdgeStyle);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDownPaddingTop);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericUpDownPaddingBottom);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxPreview);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(790, 545);
            this.Name = "AssSetBackground";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generate background box";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ApplyCustomStyles_FormClosing);
            this.Shown += new System.EventHandler(this.SetPosition_Shown);
            this.ResizeEnd += new System.EventHandler(this.SetPosition_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ApplyCustomStyles_KeyDown);
            this.groupBoxPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.groupBoxDrawing.ResumeLayout(false);
            this.groupBoxDrawing.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaddingBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaddingTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBoxLayer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaddingRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaddingLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOutlineWidth)).EndInit();
            this.groupBoxFillWidth.ResumeLayout(false);
            this.groupBoxFillWidth.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFillWidthMarginRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFillWidthMarginLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRadius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox groupBoxDrawing;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDown4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label labelRotateX;
        private System.Windows.Forms.NumericUpDown numericUpDownRotateY;
        private System.Windows.Forms.Label labelRotateY;
        private System.Windows.Forms.NumericUpDown numericUpDownRotateX;
        private System.Windows.Forms.NumericUpDown numericUpDownPaddingBottom;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownPaddingTop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelEdgeStyle;
        private System.Windows.Forms.ComboBox comboBoxBoxStyle;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownBoxLayer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDownPaddingRight;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericUpDownPaddingLeft;
        private System.Windows.Forms.Panel panelPrimaryColor;
        private System.Windows.Forms.Button buttonPrimaryColor;
        private System.Windows.Forms.Panel panelShadowColor;
        private System.Windows.Forms.Button buttonShadowColor;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numericUpDownShadowWidth;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericUpDownOutlineWidth;
        private System.Windows.Forms.Panel panelOutlineColor;
        private System.Windows.Forms.Button buttonOutlineColor;
        private System.Windows.Forms.GroupBox groupBoxFillWidth;
        private System.Windows.Forms.CheckBox checkBoxFillWidth;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numericUpDownFillWidthMarginRight;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown numericUpDownFillWidthMarginLeft;
        private System.Windows.Forms.Label labelRadius;
        private System.Windows.Forms.NumericUpDown numericUpDownRadius;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
    }
}