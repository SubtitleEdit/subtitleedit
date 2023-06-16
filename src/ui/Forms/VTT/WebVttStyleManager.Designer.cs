namespace Nikse.SubtitleEdit.Forms.VTT
{
    partial class WebVttStyleManager
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
            this.groupBoxStyles = new System.Windows.Forms.GroupBox();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.buttonRemoveAll = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.listViewStyles = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFontName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFontSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderUseCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPrimaryColor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderOutline = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxProperties = new System.Windows.Forms.GroupBox();
            this.textBoxStyleName = new System.Windows.Forms.TextBox();
            this.labelStyleName = new System.Windows.Forms.Label();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.panelBackColor = new System.Windows.Forms.Panel();
            this.buttonBackColor = new System.Windows.Forms.Button();
            this.numericUpDownShadowWidth = new System.Windows.Forms.NumericUpDown();
            this.panelSecondaryColor = new System.Windows.Forms.Panel();
            this.labelShadow = new System.Windows.Forms.Label();
            this.buttonSecondaryColor = new System.Windows.Forms.Button();
            this.panelPrimaryColor = new System.Windows.Forms.Panel();
            this.checkBoxStrikeout = new System.Windows.Forms.CheckBox();
            this.buttonPrimaryColor = new System.Windows.Forms.Button();
            this.buttonPickAttachmentFont = new System.Windows.Forms.Button();
            this.checkBoxFontUnderline = new System.Windows.Forms.CheckBox();
            this.numericUpDownFontSize = new System.Windows.Forms.NumericUpDown();
            this.checkBoxFontItalic = new System.Windows.Forms.CheckBox();
            this.checkBoxFontBold = new System.Windows.Forms.CheckBox();
            this.comboBoxFontName = new System.Windows.Forms.ComboBox();
            this.labelFontSize = new System.Windows.Forms.Label();
            this.labelFontName = new System.Windows.Forms.Label();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.checkBoxColorEnabled = new System.Windows.Forms.CheckBox();
            this.checkBoxBackgroundColorEnabled = new System.Windows.Forms.CheckBox();
            this.groupBoxStyles.SuspendLayout();
            this.groupBoxProperties.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.groupBoxFont.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxStyles
            // 
            this.groupBoxStyles.Controls.Add(this.buttonExport);
            this.groupBoxStyles.Controls.Add(this.buttonImport);
            this.groupBoxStyles.Controls.Add(this.buttonCopy);
            this.groupBoxStyles.Controls.Add(this.buttonRemoveAll);
            this.groupBoxStyles.Controls.Add(this.buttonAdd);
            this.groupBoxStyles.Controls.Add(this.buttonRemove);
            this.groupBoxStyles.Controls.Add(this.listViewStyles);
            this.groupBoxStyles.Location = new System.Drawing.Point(8, 8);
            this.groupBoxStyles.Name = "groupBoxStyles";
            this.groupBoxStyles.Size = new System.Drawing.Size(600, 599);
            this.groupBoxStyles.TabIndex = 1;
            this.groupBoxStyles.TabStop = false;
            this.groupBoxStyles.Text = "File styles";
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonExport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonExport.Location = new System.Drawing.Point(6, 570);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(82, 23);
            this.buttonExport.TabIndex = 5;
            this.buttonExport.Text = "Export...";
            this.buttonExport.UseVisualStyleBackColor = true;
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonImport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonImport.Location = new System.Drawing.Point(6, 541);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(82, 23);
            this.buttonImport.TabIndex = 1;
            this.buttonImport.Text = "Import...";
            this.buttonImport.UseVisualStyleBackColor = true;
            // 
            // buttonCopy
            // 
            this.buttonCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCopy.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCopy.Location = new System.Drawing.Point(94, 570);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(82, 23);
            this.buttonCopy.TabIndex = 6;
            this.buttonCopy.Text = "Copy";
            this.buttonCopy.UseVisualStyleBackColor = true;
            // 
            // buttonRemoveAll
            // 
            this.buttonRemoveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemoveAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemoveAll.Location = new System.Drawing.Point(182, 570);
            this.buttonRemoveAll.Name = "buttonRemoveAll";
            this.buttonRemoveAll.Size = new System.Drawing.Size(102, 23);
            this.buttonRemoveAll.TabIndex = 7;
            this.buttonRemoveAll.Text = "Remove all";
            this.buttonRemoveAll.UseVisualStyleBackColor = true;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAdd.Location = new System.Drawing.Point(94, 541);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(82, 23);
            this.buttonAdd.TabIndex = 2;
            this.buttonAdd.Text = "New";
            this.buttonAdd.UseVisualStyleBackColor = true;
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemove.Location = new System.Drawing.Point(182, 541);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(102, 23);
            this.buttonRemove.TabIndex = 3;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
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
            this.columnHeaderUseCount,
            this.columnHeaderPrimaryColor,
            this.columnHeaderOutline});
            this.listViewStyles.FullRowSelect = true;
            this.listViewStyles.HideSelection = false;
            this.listViewStyles.Location = new System.Drawing.Point(6, 19);
            this.listViewStyles.Name = "listViewStyles";
            this.listViewStyles.Size = new System.Drawing.Size(588, 516);
            this.listViewStyles.TabIndex = 0;
            this.listViewStyles.UseCompatibleStateImageBehavior = false;
            this.listViewStyles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 130;
            // 
            // columnHeaderFontName
            // 
            this.columnHeaderFontName.Text = "Font name";
            this.columnHeaderFontName.Width = 128;
            // 
            // columnHeaderFontSize
            // 
            this.columnHeaderFontSize.Text = "Font size";
            this.columnHeaderFontSize.Width = 80;
            // 
            // columnHeaderUseCount
            // 
            this.columnHeaderUseCount.Text = "Used#";
            // 
            // columnHeaderPrimaryColor
            // 
            this.columnHeaderPrimaryColor.Text = "Primary";
            this.columnHeaderPrimaryColor.Width = 70;
            // 
            // columnHeaderOutline
            // 
            this.columnHeaderOutline.Text = "Outline";
            this.columnHeaderOutline.Width = 55;
            // 
            // groupBoxProperties
            // 
            this.groupBoxProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxProperties.Controls.Add(this.textBoxStyleName);
            this.groupBoxProperties.Controls.Add(this.labelStyleName);
            this.groupBoxProperties.Controls.Add(this.groupBoxPreview);
            this.groupBoxProperties.Controls.Add(this.groupBoxFont);
            this.groupBoxProperties.Location = new System.Drawing.Point(614, 8);
            this.groupBoxProperties.Name = "groupBoxProperties";
            this.groupBoxProperties.Size = new System.Drawing.Size(484, 599);
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
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.pictureBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(7, 335);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(472, 258);
            this.groupBoxPreview.TabIndex = 7;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 16);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(466, 239);
            this.pictureBoxPreview.TabIndex = 0;
            this.pictureBoxPreview.TabStop = false;
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFont.Controls.Add(this.checkBoxBackgroundColorEnabled);
            this.groupBoxFont.Controls.Add(this.checkBoxColorEnabled);
            this.groupBoxFont.Controls.Add(this.panelBackColor);
            this.groupBoxFont.Controls.Add(this.buttonBackColor);
            this.groupBoxFont.Controls.Add(this.numericUpDownShadowWidth);
            this.groupBoxFont.Controls.Add(this.panelSecondaryColor);
            this.groupBoxFont.Controls.Add(this.labelShadow);
            this.groupBoxFont.Controls.Add(this.buttonSecondaryColor);
            this.groupBoxFont.Controls.Add(this.panelPrimaryColor);
            this.groupBoxFont.Controls.Add(this.checkBoxStrikeout);
            this.groupBoxFont.Controls.Add(this.buttonPrimaryColor);
            this.groupBoxFont.Controls.Add(this.buttonPickAttachmentFont);
            this.groupBoxFont.Controls.Add(this.checkBoxFontUnderline);
            this.groupBoxFont.Controls.Add(this.numericUpDownFontSize);
            this.groupBoxFont.Controls.Add(this.checkBoxFontItalic);
            this.groupBoxFont.Controls.Add(this.checkBoxFontBold);
            this.groupBoxFont.Controls.Add(this.comboBoxFontName);
            this.groupBoxFont.Controls.Add(this.labelFontSize);
            this.groupBoxFont.Controls.Add(this.labelFontName);
            this.groupBoxFont.Location = new System.Drawing.Point(7, 51);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(472, 278);
            this.groupBoxFont.TabIndex = 2;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "Font";
            // 
            // panelBackColor
            // 
            this.panelBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBackColor.Location = new System.Drawing.Point(371, 138);
            this.panelBackColor.Name = "panelBackColor";
            this.panelBackColor.Size = new System.Drawing.Size(21, 20);
            this.panelBackColor.TabIndex = 7;
            // 
            // buttonBackColor
            // 
            this.buttonBackColor.Location = new System.Drawing.Point(282, 137);
            this.buttonBackColor.Name = "buttonBackColor";
            this.buttonBackColor.Size = new System.Drawing.Size(84, 23);
            this.buttonBackColor.TabIndex = 6;
            this.buttonBackColor.Text = "Shadow";
            this.buttonBackColor.UseVisualStyleBackColor = true;
            // 
            // numericUpDownShadowWidth
            // 
            this.numericUpDownShadowWidth.DecimalPlaces = 1;
            this.numericUpDownShadowWidth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownShadowWidth.Location = new System.Drawing.Point(286, 166);
            this.numericUpDownShadowWidth.Name = "numericUpDownShadowWidth";
            this.numericUpDownShadowWidth.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownShadowWidth.TabIndex = 2;
            // 
            // panelSecondaryColor
            // 
            this.panelSecondaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSecondaryColor.Location = new System.Drawing.Point(242, 138);
            this.panelSecondaryColor.Name = "panelSecondaryColor";
            this.panelSecondaryColor.Size = new System.Drawing.Size(21, 20);
            this.panelSecondaryColor.TabIndex = 3;
            // 
            // labelShadow
            // 
            this.labelShadow.AutoSize = true;
            this.labelShadow.Location = new System.Drawing.Point(344, 168);
            this.labelShadow.Name = "labelShadow";
            this.labelShadow.Size = new System.Drawing.Size(46, 13);
            this.labelShadow.TabIndex = 2;
            this.labelShadow.Text = "Shadow";
            // 
            // buttonSecondaryColor
            // 
            this.buttonSecondaryColor.Location = new System.Drawing.Point(135, 137);
            this.buttonSecondaryColor.Name = "buttonSecondaryColor";
            this.buttonSecondaryColor.Size = new System.Drawing.Size(101, 23);
            this.buttonSecondaryColor.TabIndex = 2;
            this.buttonSecondaryColor.Text = "Background";
            this.buttonSecondaryColor.UseVisualStyleBackColor = true;
            // 
            // panelPrimaryColor
            // 
            this.panelPrimaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPrimaryColor.Location = new System.Drawing.Point(100, 138);
            this.panelPrimaryColor.Name = "panelPrimaryColor";
            this.panelPrimaryColor.Size = new System.Drawing.Size(21, 20);
            this.panelPrimaryColor.TabIndex = 1;
            // 
            // checkBoxStrikeout
            // 
            this.checkBoxStrikeout.AutoSize = true;
            this.checkBoxStrikeout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxStrikeout.Location = new System.Drawing.Point(246, 62);
            this.checkBoxStrikeout.Name = "checkBoxStrikeout";
            this.checkBoxStrikeout.Size = new System.Drawing.Size(68, 17);
            this.checkBoxStrikeout.TabIndex = 8;
            this.checkBoxStrikeout.Text = "Strikeout";
            this.checkBoxStrikeout.UseVisualStyleBackColor = true;
            // 
            // buttonPrimaryColor
            // 
            this.buttonPrimaryColor.Location = new System.Drawing.Point(14, 137);
            this.buttonPrimaryColor.Name = "buttonPrimaryColor";
            this.buttonPrimaryColor.Size = new System.Drawing.Size(80, 23);
            this.buttonPrimaryColor.TabIndex = 0;
            this.buttonPrimaryColor.Text = "&Color";
            this.buttonPrimaryColor.UseVisualStyleBackColor = true;
            // 
            // buttonPickAttachmentFont
            // 
            this.buttonPickAttachmentFont.Location = new System.Drawing.Point(267, 17);
            this.buttonPickAttachmentFont.Name = "buttonPickAttachmentFont";
            this.buttonPickAttachmentFont.Size = new System.Drawing.Size(24, 23);
            this.buttonPickAttachmentFont.TabIndex = 2;
            this.buttonPickAttachmentFont.Text = "...";
            this.buttonPickAttachmentFont.UseVisualStyleBackColor = true;
            // 
            // checkBoxFontUnderline
            // 
            this.checkBoxFontUnderline.AutoSize = true;
            this.checkBoxFontUnderline.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxFontUnderline.Location = new System.Drawing.Point(169, 62);
            this.checkBoxFontUnderline.Name = "checkBoxFontUnderline";
            this.checkBoxFontUnderline.Size = new System.Drawing.Size(71, 17);
            this.checkBoxFontUnderline.TabIndex = 7;
            this.checkBoxFontUnderline.Text = "Underline";
            this.checkBoxFontUnderline.UseVisualStyleBackColor = true;
            // 
            // numericUpDownFontSize
            // 
            this.numericUpDownFontSize.DecimalPlaces = 1;
            this.numericUpDownFontSize.Location = new System.Drawing.Point(374, 18);
            this.numericUpDownFontSize.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownFontSize.Name = "numericUpDownFontSize";
            this.numericUpDownFontSize.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownFontSize.TabIndex = 4;
            // 
            // checkBoxFontItalic
            // 
            this.checkBoxFontItalic.AutoSize = true;
            this.checkBoxFontItalic.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxFontItalic.Location = new System.Drawing.Point(90, 62);
            this.checkBoxFontItalic.Name = "checkBoxFontItalic";
            this.checkBoxFontItalic.Size = new System.Drawing.Size(48, 17);
            this.checkBoxFontItalic.TabIndex = 6;
            this.checkBoxFontItalic.Text = "Italic";
            this.checkBoxFontItalic.UseVisualStyleBackColor = true;
            // 
            // checkBoxFontBold
            // 
            this.checkBoxFontBold.AutoSize = true;
            this.checkBoxFontBold.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxFontBold.Location = new System.Drawing.Point(13, 62);
            this.checkBoxFontBold.Name = "checkBoxFontBold";
            this.checkBoxFontBold.Size = new System.Drawing.Size(51, 17);
            this.checkBoxFontBold.TabIndex = 5;
            this.checkBoxFontBold.Text = "Bold";
            this.checkBoxFontBold.UseVisualStyleBackColor = true;
            // 
            // comboBoxFontName
            // 
            this.comboBoxFontName.FormattingEnabled = true;
            this.comboBoxFontName.Location = new System.Drawing.Point(73, 17);
            this.comboBoxFontName.Name = "comboBoxFontName";
            this.comboBoxFontName.Size = new System.Drawing.Size(188, 21);
            this.comboBoxFontName.TabIndex = 1;
            // 
            // labelFontSize
            // 
            this.labelFontSize.AutoSize = true;
            this.labelFontSize.Location = new System.Drawing.Point(319, 20);
            this.labelFontSize.Name = "labelFontSize";
            this.labelFontSize.Size = new System.Drawing.Size(49, 13);
            this.labelFontSize.TabIndex = 3;
            this.labelFontSize.Text = "Font size";
            // 
            // labelFontName
            // 
            this.labelFontName.AutoSize = true;
            this.labelFontName.Location = new System.Drawing.Point(10, 20);
            this.labelFontName.Name = "labelFontName";
            this.labelFontName.Size = new System.Drawing.Size(57, 13);
            this.labelFontName.TabIndex = 0;
            this.labelFontName.Text = "Font name";
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonApply.Location = new System.Drawing.Point(831, 619);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(105, 23);
            this.buttonApply.TabIndex = 4;
            this.buttonApply.Text = "&Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(1023, 619);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(942, 619);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // checkBoxColorEnabled
            // 
            this.checkBoxColorEnabled.AutoSize = true;
            this.checkBoxColorEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxColorEnabled.Location = new System.Drawing.Point(16, 114);
            this.checkBoxColorEnabled.Name = "checkBoxColorEnabled";
            this.checkBoxColorEnabled.Size = new System.Drawing.Size(65, 17);
            this.checkBoxColorEnabled.TabIndex = 9;
            this.checkBoxColorEnabled.Text = "Enabled";
            this.checkBoxColorEnabled.UseVisualStyleBackColor = true;
            // 
            // checkBoxBackgroundColorEnabled
            // 
            this.checkBoxBackgroundColorEnabled.AutoSize = true;
            this.checkBoxBackgroundColorEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxBackgroundColorEnabled.Location = new System.Drawing.Point(135, 114);
            this.checkBoxBackgroundColorEnabled.Name = "checkBoxBackgroundColorEnabled";
            this.checkBoxBackgroundColorEnabled.Size = new System.Drawing.Size(65, 17);
            this.checkBoxBackgroundColorEnabled.TabIndex = 10;
            this.checkBoxBackgroundColorEnabled.Text = "Enabled";
            this.checkBoxBackgroundColorEnabled.UseVisualStyleBackColor = true;
            // 
            // WebVttStyleManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1110, 654);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxProperties);
            this.Controls.Add(this.groupBoxStyles);
            this.KeyPreview = true;
            this.Name = "WebVttStyleManager";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WebVttStyleManager";
            this.groupBoxStyles.ResumeLayout(false);
            this.groupBoxProperties.ResumeLayout(false);
            this.groupBoxProperties.PerformLayout();
            this.groupBoxPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.groupBoxFont.ResumeLayout(false);
            this.groupBoxFont.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxStyles;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.Button buttonRemoveAll;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.ListView listViewStyles;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderFontName;
        private System.Windows.Forms.ColumnHeader columnHeaderFontSize;
        private System.Windows.Forms.ColumnHeader columnHeaderUseCount;
        private System.Windows.Forms.ColumnHeader columnHeaderPrimaryColor;
        private System.Windows.Forms.ColumnHeader columnHeaderOutline;
        private System.Windows.Forms.GroupBox groupBoxProperties;
        private System.Windows.Forms.NumericUpDown numericUpDownShadowWidth;
        private System.Windows.Forms.Label labelShadow;
        private System.Windows.Forms.TextBox textBoxStyleName;
        private System.Windows.Forms.Label labelStyleName;
        private System.Windows.Forms.Panel panelBackColor;
        private System.Windows.Forms.Button buttonBackColor;
        private System.Windows.Forms.Panel panelSecondaryColor;
        private System.Windows.Forms.Button buttonSecondaryColor;
        private System.Windows.Forms.Panel panelPrimaryColor;
        private System.Windows.Forms.Button buttonPrimaryColor;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.GroupBox groupBoxFont;
        private System.Windows.Forms.CheckBox checkBoxStrikeout;
        private System.Windows.Forms.Button buttonPickAttachmentFont;
        private System.Windows.Forms.CheckBox checkBoxFontUnderline;
        private System.Windows.Forms.NumericUpDown numericUpDownFontSize;
        private System.Windows.Forms.CheckBox checkBoxFontItalic;
        private System.Windows.Forms.CheckBox checkBoxFontBold;
        private System.Windows.Forms.ComboBox comboBoxFontName;
        private System.Windows.Forms.Label labelFontSize;
        private System.Windows.Forms.Label labelFontName;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.CheckBox checkBoxColorEnabled;
        private System.Windows.Forms.CheckBox checkBoxBackgroundColorEnabled;
    }
}