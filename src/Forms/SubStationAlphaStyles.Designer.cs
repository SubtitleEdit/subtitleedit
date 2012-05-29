namespace Nikse.SubtitleEdit.Forms
{
    partial class SubStationAlphaStyles
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
            this.listViewStyles = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFontName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFontSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPrimaryColor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderOutline = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonNextFinish = new System.Windows.Forms.Button();
            this.groupBoxStyles = new System.Windows.Forms.GroupBox();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.buttonRemoveAll = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.groupBoxProperties = new System.Windows.Forms.GroupBox();
            this.textBoxStyleName = new System.Windows.Forms.TextBox();
            this.labelStyleName = new System.Windows.Forms.Label();
            this.groupBoxMargins = new System.Windows.Forms.GroupBox();
            this.numericUpDownMarginVertical = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMarginRight = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMarginLeft = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.groupBoxColors = new System.Windows.Forms.GroupBox();
            this.panelBackColor = new System.Windows.Forms.Panel();
            this.numericUpDownOutline = new System.Windows.Forms.NumericUpDown();
            this.buttonBackColor = new System.Windows.Forms.Button();
            this.panelOutlineColor = new System.Windows.Forms.Panel();
            this.buttonOutlineColor = new System.Windows.Forms.Button();
            this.panelSecondaryColor = new System.Windows.Forms.Panel();
            this.buttonSecondaryColor = new System.Windows.Forms.Button();
            this.panelPrimaryColor = new System.Windows.Forms.Panel();
            this.buttonPrimaryColor = new System.Windows.Forms.Button();
            this.groupBoxAlignment = new System.Windows.Forms.GroupBox();
            this.radioButtonBottomRight = new System.Windows.Forms.RadioButton();
            this.radioButtonBottomCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonMiddleRight = new System.Windows.Forms.RadioButton();
            this.radioButtonBottomLeft = new System.Windows.Forms.RadioButton();
            this.radioButtonMiddleLeft = new System.Windows.Forms.RadioButton();
            this.radioButtonTopRight = new System.Windows.Forms.RadioButton();
            this.radioButtonTopCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonMiddleCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonTopLeft = new System.Windows.Forms.RadioButton();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.checkBoxFontUnderline = new System.Windows.Forms.CheckBox();
            this.numericUpDownFontSize = new System.Windows.Forms.NumericUpDown();
            this.checkBoxFontItalic = new System.Windows.Forms.CheckBox();
            this.checkBoxFontBold = new System.Windows.Forms.CheckBox();
            this.comboBoxFontName = new System.Windows.Forms.ComboBox();
            this.labelFontSize = new System.Windows.Forms.Label();
            this.labelFontName = new System.Windows.Forms.Label();
            this.colorDialogSSAStyle = new System.Windows.Forms.ColorDialog();
            this.groupBoxOutlineShadow = new System.Windows.Forms.GroupBox();
            this.numericUpDownShadowWidth = new System.Windows.Forms.NumericUpDown();
            this.labelShadow = new System.Windows.Forms.Label();
            this.labelOutline = new System.Windows.Forms.Label();
            this.groupBoxStyles.SuspendLayout();
            this.groupBoxProperties.SuspendLayout();
            this.groupBoxMargins.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginVertical)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginLeft)).BeginInit();
            this.groupBoxPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.groupBoxColors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOutline)).BeginInit();
            this.groupBoxAlignment.SuspendLayout();
            this.groupBoxFont.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).BeginInit();
            this.groupBoxOutlineShadow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // listViewStyles
            // 
            this.listViewStyles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewStyles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderFontName,
            this.columnHeaderFontSize,
            this.columnHeaderPrimaryColor,
            this.columnHeaderOutline});
            this.listViewStyles.FullRowSelect = true;
            this.listViewStyles.HideSelection = false;
            this.listViewStyles.Location = new System.Drawing.Point(6, 19);
            this.listViewStyles.MinimumSize = new System.Drawing.Size(340, 447);
            this.listViewStyles.MultiSelect = false;
            this.listViewStyles.Name = "listViewStyles";
            this.listViewStyles.Size = new System.Drawing.Size(445, 509);
            this.listViewStyles.TabIndex = 1;
            this.listViewStyles.UseCompatibleStateImageBehavior = false;
            this.listViewStyles.View = System.Windows.Forms.View.Details;
            this.listViewStyles.SelectedIndexChanged += new System.EventHandler(this.listViewStyles_SelectedIndexChanged);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 130;
            // 
            // columnHeaderFontName
            // 
            this.columnHeaderFontName.Text = "Font name";
            this.columnHeaderFontName.Width = 130;
            // 
            // columnHeaderFontSize
            // 
            this.columnHeaderFontSize.Text = "Font size";
            this.columnHeaderFontSize.Width = 80;
            // 
            // columnHeaderPrimaryColor
            // 
            this.columnHeaderPrimaryColor.Text = "Primary";
            this.columnHeaderPrimaryColor.Width = 50;
            // 
            // columnHeaderOutline
            // 
            this.columnHeaderOutline.Text = "Outline";
            this.columnHeaderOutline.Width = 55;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(797, 579);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonNextFinish
            // 
            this.buttonNextFinish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNextFinish.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonNextFinish.Location = new System.Drawing.Point(716, 579);
            this.buttonNextFinish.Name = "buttonNextFinish";
            this.buttonNextFinish.Size = new System.Drawing.Size(75, 21);
            this.buttonNextFinish.TabIndex = 3;
            this.buttonNextFinish.Text = "&OK";
            this.buttonNextFinish.UseVisualStyleBackColor = true;
            this.buttonNextFinish.Click += new System.EventHandler(this.buttonNextFinish_Click);
            // 
            // groupBoxStyles
            // 
            this.groupBoxStyles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxStyles.Controls.Add(this.buttonCopy);
            this.groupBoxStyles.Controls.Add(this.buttonRemoveAll);
            this.groupBoxStyles.Controls.Add(this.buttonAdd);
            this.groupBoxStyles.Controls.Add(this.buttonRemove);
            this.groupBoxStyles.Controls.Add(this.listViewStyles);
            this.groupBoxStyles.Location = new System.Drawing.Point(12, 12);
            this.groupBoxStyles.Name = "groupBoxStyles";
            this.groupBoxStyles.Size = new System.Drawing.Size(457, 561);
            this.groupBoxStyles.TabIndex = 0;
            this.groupBoxStyles.TabStop = false;
            this.groupBoxStyles.Text = "Styles";
            // 
            // buttonCopy
            // 
            this.buttonCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCopy.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCopy.Location = new System.Drawing.Point(133, 534);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(75, 21);
            this.buttonCopy.TabIndex = 2;
            this.buttonCopy.Text = "Copy";
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // buttonRemoveAll
            // 
            this.buttonRemoveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemoveAll.Location = new System.Drawing.Point(376, 534);
            this.buttonRemoveAll.Name = "buttonRemoveAll";
            this.buttonRemoveAll.Size = new System.Drawing.Size(75, 21);
            this.buttonRemoveAll.TabIndex = 0;
            this.buttonRemoveAll.Text = "Remove all";
            this.buttonRemoveAll.UseVisualStyleBackColor = true;
            this.buttonRemoveAll.Click += new System.EventHandler(this.buttonRemoveAll_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAdd.Location = new System.Drawing.Point(214, 534);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 21);
            this.buttonAdd.TabIndex = 3;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemove.Location = new System.Drawing.Point(295, 534);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(75, 21);
            this.buttonRemove.TabIndex = 4;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // groupBoxProperties
            // 
            this.groupBoxProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxProperties.Controls.Add(this.groupBoxOutlineShadow);
            this.groupBoxProperties.Controls.Add(this.textBoxStyleName);
            this.groupBoxProperties.Controls.Add(this.labelStyleName);
            this.groupBoxProperties.Controls.Add(this.groupBoxMargins);
            this.groupBoxProperties.Controls.Add(this.groupBoxPreview);
            this.groupBoxProperties.Controls.Add(this.groupBoxColors);
            this.groupBoxProperties.Controls.Add(this.groupBoxAlignment);
            this.groupBoxProperties.Controls.Add(this.groupBoxFont);
            this.groupBoxProperties.Location = new System.Drawing.Point(475, 12);
            this.groupBoxProperties.Name = "groupBoxProperties";
            this.groupBoxProperties.Size = new System.Drawing.Size(397, 561);
            this.groupBoxProperties.TabIndex = 2;
            this.groupBoxProperties.TabStop = false;
            this.groupBoxProperties.Text = "Properties";
            // 
            // textBoxStyleName
            // 
            this.textBoxStyleName.Location = new System.Drawing.Point(49, 22);
            this.textBoxStyleName.Name = "textBoxStyleName";
            this.textBoxStyleName.Size = new System.Drawing.Size(336, 20);
            this.textBoxStyleName.TabIndex = 1;
            this.textBoxStyleName.TextChanged += new System.EventHandler(this.textBoxStyleName_TextChanged);
            // 
            // labelStyleName
            // 
            this.labelStyleName.AutoSize = true;
            this.labelStyleName.Location = new System.Drawing.Point(7, 26);
            this.labelStyleName.Name = "labelStyleName";
            this.labelStyleName.Size = new System.Drawing.Size(35, 13);
            this.labelStyleName.TabIndex = 0;
            this.labelStyleName.Text = "Name";
            // 
            // groupBoxMargins
            // 
            this.groupBoxMargins.Controls.Add(this.numericUpDownMarginVertical);
            this.groupBoxMargins.Controls.Add(this.numericUpDownMarginRight);
            this.groupBoxMargins.Controls.Add(this.numericUpDownMarginLeft);
            this.groupBoxMargins.Controls.Add(this.label3);
            this.groupBoxMargins.Controls.Add(this.label2);
            this.groupBoxMargins.Controls.Add(this.label1);
            this.groupBoxMargins.Location = new System.Drawing.Point(7, 344);
            this.groupBoxMargins.Name = "groupBoxMargins";
            this.groupBoxMargins.Size = new System.Drawing.Size(207, 65);
            this.groupBoxMargins.TabIndex = 5;
            this.groupBoxMargins.TabStop = false;
            this.groupBoxMargins.Text = "Margins";
            // 
            // numericUpDownMarginVertical
            // 
            this.numericUpDownMarginVertical.Location = new System.Drawing.Point(148, 33);
            this.numericUpDownMarginVertical.Name = "numericUpDownMarginVertical";
            this.numericUpDownMarginVertical.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMarginVertical.TabIndex = 5;
            this.numericUpDownMarginVertical.ValueChanged += new System.EventHandler(this.numericUpDownMarginVertical_ValueChanged);
            // 
            // numericUpDownMarginRight
            // 
            this.numericUpDownMarginRight.Location = new System.Drawing.Point(81, 33);
            this.numericUpDownMarginRight.Name = "numericUpDownMarginRight";
            this.numericUpDownMarginRight.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMarginRight.TabIndex = 3;
            this.numericUpDownMarginRight.ValueChanged += new System.EventHandler(this.numericUpDownMarginRight_ValueChanged);
            // 
            // numericUpDownMarginLeft
            // 
            this.numericUpDownMarginLeft.Location = new System.Drawing.Point(16, 33);
            this.numericUpDownMarginLeft.Name = "numericUpDownMarginLeft";
            this.numericUpDownMarginLeft.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMarginLeft.TabIndex = 1;
            this.numericUpDownMarginLeft.ValueChanged += new System.EventHandler(this.numericUpDownMarginLeft_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(150, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Vertical";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(78, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Right";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Left";
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxPreview.Controls.Add(this.pictureBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(7, 413);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(384, 142);
            this.groupBoxPreview.TabIndex = 0;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 16);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(378, 123);
            this.pictureBoxPreview.TabIndex = 0;
            this.pictureBoxPreview.TabStop = false;
            // 
            // groupBoxColors
            // 
            this.groupBoxColors.Controls.Add(this.panelBackColor);
            this.groupBoxColors.Controls.Add(this.buttonBackColor);
            this.groupBoxColors.Controls.Add(this.panelOutlineColor);
            this.groupBoxColors.Controls.Add(this.buttonOutlineColor);
            this.groupBoxColors.Controls.Add(this.panelSecondaryColor);
            this.groupBoxColors.Controls.Add(this.buttonSecondaryColor);
            this.groupBoxColors.Controls.Add(this.panelPrimaryColor);
            this.groupBoxColors.Controls.Add(this.buttonPrimaryColor);
            this.groupBoxColors.Location = new System.Drawing.Point(6, 255);
            this.groupBoxColors.Name = "groupBoxColors";
            this.groupBoxColors.Size = new System.Drawing.Size(385, 83);
            this.groupBoxColors.TabIndex = 4;
            this.groupBoxColors.TabStop = false;
            this.groupBoxColors.Text = "Colors";
            // 
            // panelBackColor
            // 
            this.panelBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBackColor.Location = new System.Drawing.Point(334, 50);
            this.panelBackColor.Name = "panelBackColor";
            this.panelBackColor.Size = new System.Drawing.Size(21, 20);
            this.panelBackColor.TabIndex = 7;
            this.panelBackColor.Click += new System.EventHandler(this.buttonShadowColor_Click);
            // 
            // numericUpDownOutline
            // 
            this.numericUpDownOutline.Location = new System.Drawing.Point(16, 33);
            this.numericUpDownOutline.Name = "numericUpDownOutline";
            this.numericUpDownOutline.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownOutline.TabIndex = 1;
            this.numericUpDownOutline.ValueChanged += new System.EventHandler(this.numericUpDownOutline_ValueChanged);
            // 
            // buttonBackColor
            // 
            this.buttonBackColor.Location = new System.Drawing.Point(212, 49);
            this.buttonBackColor.Name = "buttonBackColor";
            this.buttonBackColor.Size = new System.Drawing.Size(112, 21);
            this.buttonBackColor.TabIndex = 6;
            this.buttonBackColor.Text = "Back";
            this.buttonBackColor.UseVisualStyleBackColor = true;
            this.buttonBackColor.Click += new System.EventHandler(this.buttonShadowColor_Click);
            // 
            // panelOutlineColor
            // 
            this.panelOutlineColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelOutlineColor.Location = new System.Drawing.Point(334, 23);
            this.panelOutlineColor.Name = "panelOutlineColor";
            this.panelOutlineColor.Size = new System.Drawing.Size(21, 20);
            this.panelOutlineColor.TabIndex = 5;
            this.panelOutlineColor.Click += new System.EventHandler(this.buttonOutlineColor_Click);
            // 
            // buttonOutlineColor
            // 
            this.buttonOutlineColor.Location = new System.Drawing.Point(212, 22);
            this.buttonOutlineColor.Name = "buttonOutlineColor";
            this.buttonOutlineColor.Size = new System.Drawing.Size(112, 21);
            this.buttonOutlineColor.TabIndex = 4;
            this.buttonOutlineColor.Text = "Outline";
            this.buttonOutlineColor.UseVisualStyleBackColor = true;
            this.buttonOutlineColor.Click += new System.EventHandler(this.buttonOutlineColor_Click);
            // 
            // panelSecondaryColor
            // 
            this.panelSecondaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSecondaryColor.Location = new System.Drawing.Point(129, 50);
            this.panelSecondaryColor.Name = "panelSecondaryColor";
            this.panelSecondaryColor.Size = new System.Drawing.Size(21, 20);
            this.panelSecondaryColor.TabIndex = 3;
            this.panelSecondaryColor.Click += new System.EventHandler(this.buttonSecondaryColor_Click);
            // 
            // buttonSecondaryColor
            // 
            this.buttonSecondaryColor.Location = new System.Drawing.Point(7, 49);
            this.buttonSecondaryColor.Name = "buttonSecondaryColor";
            this.buttonSecondaryColor.Size = new System.Drawing.Size(112, 21);
            this.buttonSecondaryColor.TabIndex = 2;
            this.buttonSecondaryColor.Text = "Secondary";
            this.buttonSecondaryColor.UseVisualStyleBackColor = true;
            this.buttonSecondaryColor.Click += new System.EventHandler(this.buttonSecondaryColor_Click);
            // 
            // panelPrimaryColor
            // 
            this.panelPrimaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPrimaryColor.Location = new System.Drawing.Point(129, 22);
            this.panelPrimaryColor.Name = "panelPrimaryColor";
            this.panelPrimaryColor.Size = new System.Drawing.Size(21, 20);
            this.panelPrimaryColor.TabIndex = 1;
            this.panelPrimaryColor.Click += new System.EventHandler(this.buttonPrimaryColor_Click);
            // 
            // buttonPrimaryColor
            // 
            this.buttonPrimaryColor.Location = new System.Drawing.Point(7, 22);
            this.buttonPrimaryColor.Name = "buttonPrimaryColor";
            this.buttonPrimaryColor.Size = new System.Drawing.Size(112, 21);
            this.buttonPrimaryColor.TabIndex = 0;
            this.buttonPrimaryColor.Text = "Primary";
            this.buttonPrimaryColor.UseVisualStyleBackColor = true;
            this.buttonPrimaryColor.Click += new System.EventHandler(this.buttonPrimaryColor_Click);
            // 
            // groupBoxAlignment
            // 
            this.groupBoxAlignment.Controls.Add(this.radioButtonBottomRight);
            this.groupBoxAlignment.Controls.Add(this.radioButtonBottomCenter);
            this.groupBoxAlignment.Controls.Add(this.radioButtonMiddleRight);
            this.groupBoxAlignment.Controls.Add(this.radioButtonBottomLeft);
            this.groupBoxAlignment.Controls.Add(this.radioButtonMiddleLeft);
            this.groupBoxAlignment.Controls.Add(this.radioButtonTopRight);
            this.groupBoxAlignment.Controls.Add(this.radioButtonTopCenter);
            this.groupBoxAlignment.Controls.Add(this.radioButtonMiddleCenter);
            this.groupBoxAlignment.Controls.Add(this.radioButtonTopLeft);
            this.groupBoxAlignment.Location = new System.Drawing.Point(7, 156);
            this.groupBoxAlignment.Name = "groupBoxAlignment";
            this.groupBoxAlignment.Size = new System.Drawing.Size(384, 93);
            this.groupBoxAlignment.TabIndex = 3;
            this.groupBoxAlignment.TabStop = false;
            this.groupBoxAlignment.Text = "Alignment";
            // 
            // radioButtonBottomRight
            // 
            this.radioButtonBottomRight.AutoSize = true;
            this.radioButtonBottomRight.Location = new System.Drawing.Point(279, 65);
            this.radioButtonBottomRight.Name = "radioButtonBottomRight";
            this.radioButtonBottomRight.Size = new System.Drawing.Size(83, 17);
            this.radioButtonBottomRight.TabIndex = 8;
            this.radioButtonBottomRight.Text = "Bottom/right";
            this.radioButtonBottomRight.UseVisualStyleBackColor = true;
            this.radioButtonBottomRight.CheckedChanged += new System.EventHandler(this.radioButtonBottomRight_CheckedChanged);
            // 
            // radioButtonBottomCenter
            // 
            this.radioButtonBottomCenter.AutoSize = true;
            this.radioButtonBottomCenter.Checked = true;
            this.radioButtonBottomCenter.Location = new System.Drawing.Point(138, 65);
            this.radioButtonBottomCenter.Name = "radioButtonBottomCenter";
            this.radioButtonBottomCenter.Size = new System.Drawing.Size(93, 17);
            this.radioButtonBottomCenter.TabIndex = 7;
            this.radioButtonBottomCenter.TabStop = true;
            this.radioButtonBottomCenter.Text = "Bottom/center";
            this.radioButtonBottomCenter.UseVisualStyleBackColor = true;
            this.radioButtonBottomCenter.CheckedChanged += new System.EventHandler(this.radioButtonBottomCenter_CheckedChanged);
            // 
            // radioButtonMiddleRight
            // 
            this.radioButtonMiddleRight.AutoSize = true;
            this.radioButtonMiddleRight.Location = new System.Drawing.Point(279, 42);
            this.radioButtonMiddleRight.Name = "radioButtonMiddleRight";
            this.radioButtonMiddleRight.Size = new System.Drawing.Size(81, 17);
            this.radioButtonMiddleRight.TabIndex = 5;
            this.radioButtonMiddleRight.Text = "Middle/right";
            this.radioButtonMiddleRight.UseVisualStyleBackColor = true;
            this.radioButtonMiddleRight.CheckedChanged += new System.EventHandler(this.radioButtonMiddleRight_CheckedChanged);
            // 
            // radioButtonBottomLeft
            // 
            this.radioButtonBottomLeft.AutoSize = true;
            this.radioButtonBottomLeft.Location = new System.Drawing.Point(13, 65);
            this.radioButtonBottomLeft.Name = "radioButtonBottomLeft";
            this.radioButtonBottomLeft.Size = new System.Drawing.Size(77, 17);
            this.radioButtonBottomLeft.TabIndex = 6;
            this.radioButtonBottomLeft.Text = "Bottom/left";
            this.radioButtonBottomLeft.UseVisualStyleBackColor = true;
            this.radioButtonBottomLeft.CheckedChanged += new System.EventHandler(this.radioButtonBottomLeft_CheckedChanged);
            // 
            // radioButtonMiddleLeft
            // 
            this.radioButtonMiddleLeft.AutoSize = true;
            this.radioButtonMiddleLeft.Location = new System.Drawing.Point(13, 42);
            this.radioButtonMiddleLeft.Name = "radioButtonMiddleLeft";
            this.radioButtonMiddleLeft.Size = new System.Drawing.Size(75, 17);
            this.radioButtonMiddleLeft.TabIndex = 3;
            this.radioButtonMiddleLeft.Text = "Middle/left";
            this.radioButtonMiddleLeft.UseVisualStyleBackColor = true;
            this.radioButtonMiddleLeft.CheckedChanged += new System.EventHandler(this.radioButtonMiddleLeft_CheckedChanged);
            // 
            // radioButtonTopRight
            // 
            this.radioButtonTopRight.AutoSize = true;
            this.radioButtonTopRight.Location = new System.Drawing.Point(279, 19);
            this.radioButtonTopRight.Name = "radioButtonTopRight";
            this.radioButtonTopRight.Size = new System.Drawing.Size(69, 17);
            this.radioButtonTopRight.TabIndex = 2;
            this.radioButtonTopRight.Text = "Top/right";
            this.radioButtonTopRight.UseVisualStyleBackColor = true;
            this.radioButtonTopRight.CheckedChanged += new System.EventHandler(this.radioButtonTopRight_CheckedChanged);
            // 
            // radioButtonTopCenter
            // 
            this.radioButtonTopCenter.AutoSize = true;
            this.radioButtonTopCenter.Location = new System.Drawing.Point(138, 19);
            this.radioButtonTopCenter.Name = "radioButtonTopCenter";
            this.radioButtonTopCenter.Size = new System.Drawing.Size(79, 17);
            this.radioButtonTopCenter.TabIndex = 1;
            this.radioButtonTopCenter.Text = "Top/center";
            this.radioButtonTopCenter.UseVisualStyleBackColor = true;
            this.radioButtonTopCenter.CheckedChanged += new System.EventHandler(this.radioButtonTopCenter_CheckedChanged);
            // 
            // radioButtonMiddleCenter
            // 
            this.radioButtonMiddleCenter.AutoSize = true;
            this.radioButtonMiddleCenter.Location = new System.Drawing.Point(138, 42);
            this.radioButtonMiddleCenter.Name = "radioButtonMiddleCenter";
            this.radioButtonMiddleCenter.Size = new System.Drawing.Size(91, 17);
            this.radioButtonMiddleCenter.TabIndex = 4;
            this.radioButtonMiddleCenter.Text = "Middle/center";
            this.radioButtonMiddleCenter.UseVisualStyleBackColor = true;
            this.radioButtonMiddleCenter.CheckedChanged += new System.EventHandler(this.radioButtonMiddleCenter_CheckedChanged);
            // 
            // radioButtonTopLeft
            // 
            this.radioButtonTopLeft.AutoSize = true;
            this.radioButtonTopLeft.Location = new System.Drawing.Point(13, 19);
            this.radioButtonTopLeft.Name = "radioButtonTopLeft";
            this.radioButtonTopLeft.Size = new System.Drawing.Size(63, 17);
            this.radioButtonTopLeft.TabIndex = 0;
            this.radioButtonTopLeft.Text = "Top/left";
            this.radioButtonTopLeft.UseVisualStyleBackColor = true;
            this.radioButtonTopLeft.CheckedChanged += new System.EventHandler(this.radioButtonTopLeft_CheckedChanged);
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Controls.Add(this.checkBoxFontUnderline);
            this.groupBoxFont.Controls.Add(this.numericUpDownFontSize);
            this.groupBoxFont.Controls.Add(this.checkBoxFontItalic);
            this.groupBoxFont.Controls.Add(this.checkBoxFontBold);
            this.groupBoxFont.Controls.Add(this.comboBoxFontName);
            this.groupBoxFont.Controls.Add(this.labelFontSize);
            this.groupBoxFont.Controls.Add(this.labelFontName);
            this.groupBoxFont.Location = new System.Drawing.Point(7, 51);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(384, 99);
            this.groupBoxFont.TabIndex = 2;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "Font";
            // 
            // checkBoxFontUnderline
            // 
            this.checkBoxFontUnderline.AutoSize = true;
            this.checkBoxFontUnderline.Location = new System.Drawing.Point(169, 71);
            this.checkBoxFontUnderline.Name = "checkBoxFontUnderline";
            this.checkBoxFontUnderline.Size = new System.Drawing.Size(71, 17);
            this.checkBoxFontUnderline.TabIndex = 6;
            this.checkBoxFontUnderline.Text = "Underline";
            this.checkBoxFontUnderline.UseVisualStyleBackColor = true;
            this.checkBoxFontUnderline.CheckedChanged += new System.EventHandler(this.checkBoxUnderline_CheckedChanged);
            // 
            // numericUpDownFontSize
            // 
            this.numericUpDownFontSize.Location = new System.Drawing.Point(190, 48);
            this.numericUpDownFontSize.Name = "numericUpDownFontSize";
            this.numericUpDownFontSize.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownFontSize.TabIndex = 3;
            this.numericUpDownFontSize.ValueChanged += new System.EventHandler(this.numericUpDownFontSize_ValueChanged);
            // 
            // checkBoxFontItalic
            // 
            this.checkBoxFontItalic.AutoSize = true;
            this.checkBoxFontItalic.Location = new System.Drawing.Point(90, 71);
            this.checkBoxFontItalic.Name = "checkBoxFontItalic";
            this.checkBoxFontItalic.Size = new System.Drawing.Size(48, 17);
            this.checkBoxFontItalic.TabIndex = 5;
            this.checkBoxFontItalic.Text = "Italic";
            this.checkBoxFontItalic.UseVisualStyleBackColor = true;
            this.checkBoxFontItalic.CheckedChanged += new System.EventHandler(this.checkBoxFontItalic_CheckedChanged);
            // 
            // checkBoxFontBold
            // 
            this.checkBoxFontBold.AutoSize = true;
            this.checkBoxFontBold.Location = new System.Drawing.Point(13, 71);
            this.checkBoxFontBold.Name = "checkBoxFontBold";
            this.checkBoxFontBold.Size = new System.Drawing.Size(47, 17);
            this.checkBoxFontBold.TabIndex = 4;
            this.checkBoxFontBold.Text = "Bold";
            this.checkBoxFontBold.UseVisualStyleBackColor = true;
            this.checkBoxFontBold.CheckedChanged += new System.EventHandler(this.checkBoxFontBold_CheckedChanged);
            // 
            // comboBoxFontName
            // 
            this.comboBoxFontName.FormattingEnabled = true;
            this.comboBoxFontName.Location = new System.Drawing.Point(190, 19);
            this.comboBoxFontName.Name = "comboBoxFontName";
            this.comboBoxFontName.Size = new System.Drawing.Size(188, 21);
            this.comboBoxFontName.TabIndex = 1;
            this.comboBoxFontName.SelectedValueChanged += new System.EventHandler(this.comboBoxFontName_SelectedValueChanged);
            this.comboBoxFontName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.comboBoxFontName_KeyUp);
            // 
            // labelFontSize
            // 
            this.labelFontSize.AutoSize = true;
            this.labelFontSize.Location = new System.Drawing.Point(10, 50);
            this.labelFontSize.Name = "labelFontSize";
            this.labelFontSize.Size = new System.Drawing.Size(84, 13);
            this.labelFontSize.TabIndex = 2;
            this.labelFontSize.Text = "Subtitle font size";
            // 
            // labelFontName
            // 
            this.labelFontName.AutoSize = true;
            this.labelFontName.Location = new System.Drawing.Point(10, 25);
            this.labelFontName.Name = "labelFontName";
            this.labelFontName.Size = new System.Drawing.Size(63, 13);
            this.labelFontName.TabIndex = 0;
            this.labelFontName.Text = "Subtitle font";
            // 
            // groupBoxOutlineShadow
            // 
            this.groupBoxOutlineShadow.Controls.Add(this.numericUpDownShadowWidth);
            this.groupBoxOutlineShadow.Controls.Add(this.numericUpDownOutline);
            this.groupBoxOutlineShadow.Controls.Add(this.labelShadow);
            this.groupBoxOutlineShadow.Controls.Add(this.labelOutline);
            this.groupBoxOutlineShadow.Location = new System.Drawing.Point(220, 344);
            this.groupBoxOutlineShadow.Name = "groupBoxOutlineShadow";
            this.groupBoxOutlineShadow.Size = new System.Drawing.Size(171, 65);
            this.groupBoxOutlineShadow.TabIndex = 6;
            this.groupBoxOutlineShadow.TabStop = false;
            this.groupBoxOutlineShadow.Text = "Outline/shadow width";
            // 
            // numericUpDownShadowWidth
            // 
            this.numericUpDownShadowWidth.Location = new System.Drawing.Point(81, 33);
            this.numericUpDownShadowWidth.Name = "numericUpDownShadowWidth";
            this.numericUpDownShadowWidth.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownShadowWidth.TabIndex = 3;
            this.numericUpDownShadowWidth.ValueChanged += new System.EventHandler(this.numericUpDownShadowWidth_ValueChanged);
            // 
            // labelShadow
            // 
            this.labelShadow.AutoSize = true;
            this.labelShadow.Location = new System.Drawing.Point(78, 16);
            this.labelShadow.Name = "labelShadow";
            this.labelShadow.Size = new System.Drawing.Size(46, 13);
            this.labelShadow.TabIndex = 2;
            this.labelShadow.Text = "Shadow";
            // 
            // labelOutline
            // 
            this.labelOutline.AutoSize = true;
            this.labelOutline.Location = new System.Drawing.Point(13, 16);
            this.labelOutline.Name = "labelOutline";
            this.labelOutline.Size = new System.Drawing.Size(40, 13);
            this.labelOutline.TabIndex = 0;
            this.labelOutline.Text = "Outline";
            // 
            // SubStationAlphaStyles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 612);
            this.Controls.Add(this.groupBoxProperties);
            this.Controls.Add(this.groupBoxStyles);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonNextFinish);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(700, 590);
            this.Name = "SubStationAlphaStyles";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Advanced SubStation Alpha styles";
            this.ResizeEnd += new System.EventHandler(this.SubStationAlphaStyles_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SubStationAlphaStyles_KeyDown);
            this.groupBoxStyles.ResumeLayout(false);
            this.groupBoxProperties.ResumeLayout(false);
            this.groupBoxProperties.PerformLayout();
            this.groupBoxMargins.ResumeLayout(false);
            this.groupBoxMargins.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginVertical)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginLeft)).EndInit();
            this.groupBoxPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.groupBoxColors.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOutline)).EndInit();
            this.groupBoxAlignment.ResumeLayout(false);
            this.groupBoxAlignment.PerformLayout();
            this.groupBoxFont.ResumeLayout(false);
            this.groupBoxFont.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).EndInit();
            this.groupBoxOutlineShadow.ResumeLayout(false);
            this.groupBoxOutlineShadow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowWidth)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewStyles;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderFontName;
        private System.Windows.Forms.ColumnHeader columnHeaderFontSize;
        private System.Windows.Forms.ColumnHeader columnHeaderPrimaryColor;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonNextFinish;
        private System.Windows.Forms.GroupBox groupBoxStyles;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.GroupBox groupBoxProperties;
        private System.Windows.Forms.GroupBox groupBoxColors;
        private System.Windows.Forms.GroupBox groupBoxAlignment;
        private System.Windows.Forms.GroupBox groupBoxFont;
        private System.Windows.Forms.ComboBox comboBoxFontName;
        private System.Windows.Forms.Label labelFontSize;
        private System.Windows.Forms.Label labelFontName;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.RadioButton radioButtonBottomRight;
        private System.Windows.Forms.RadioButton radioButtonBottomCenter;
        private System.Windows.Forms.RadioButton radioButtonMiddleRight;
        private System.Windows.Forms.RadioButton radioButtonBottomLeft;
        private System.Windows.Forms.RadioButton radioButtonMiddleLeft;
        private System.Windows.Forms.RadioButton radioButtonTopRight;
        private System.Windows.Forms.RadioButton radioButtonTopCenter;
        private System.Windows.Forms.RadioButton radioButtonMiddleCenter;
        private System.Windows.Forms.RadioButton radioButtonTopLeft;
        private System.Windows.Forms.GroupBox groupBoxMargins;
        private System.Windows.Forms.NumericUpDown numericUpDownMarginVertical;
        private System.Windows.Forms.NumericUpDown numericUpDownMarginRight;
        private System.Windows.Forms.NumericUpDown numericUpDownMarginLeft;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelBackColor;
        private System.Windows.Forms.Button buttonBackColor;
        private System.Windows.Forms.Panel panelOutlineColor;
        private System.Windows.Forms.Button buttonOutlineColor;
        private System.Windows.Forms.Panel panelSecondaryColor;
        private System.Windows.Forms.Button buttonSecondaryColor;
        private System.Windows.Forms.Panel panelPrimaryColor;
        private System.Windows.Forms.Button buttonPrimaryColor;
        private System.Windows.Forms.NumericUpDown numericUpDownFontSize;
        private System.Windows.Forms.CheckBox checkBoxFontItalic;
        private System.Windows.Forms.CheckBox checkBoxFontBold;
        private System.Windows.Forms.TextBox textBoxStyleName;
        private System.Windows.Forms.Label labelStyleName;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Button buttonRemoveAll;
        private System.Windows.Forms.CheckBox checkBoxFontUnderline;
        private System.Windows.Forms.ColumnHeader columnHeaderOutline;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.ColorDialog colorDialogSSAStyle;
        private System.Windows.Forms.NumericUpDown numericUpDownOutline;
        private System.Windows.Forms.GroupBox groupBoxOutlineShadow;
        private System.Windows.Forms.NumericUpDown numericUpDownShadowWidth;
        private System.Windows.Forms.Label labelShadow;
        private System.Windows.Forms.Label labelOutline;
    }
}