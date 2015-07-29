namespace Nikse.SubtitleEdit.Forms
{
    partial class ExportTextST
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
            this.groupBoxPropertiesRoot = new System.Windows.Forms.GroupBox();
            this.groupBoxTextST = new System.Windows.Forms.GroupBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.textBoxRoot = new System.Windows.Forms.TextBox();
            this.groupBoxPropertiesRegionStyle = new System.Windows.Forms.GroupBox();
            this.groupBoxPropertiesPalette = new System.Windows.Forms.GroupBox();
            this.groupBoxPropertiesUserStyle = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownPaletteEntry = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownPaletteY = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownPaletteCr = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownPaletteCb = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownPaletteOpacity = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.panelPaletteColor = new System.Windows.Forms.Panel();
            this.buttonColor = new System.Windows.Forms.Button();
            this.numericUpDownRegionStyleId = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numericUpDownRegionStyleHPos = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.numericUpDownRegionStyleVPos = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.numericUpDownRegionStyleHeight = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.numericUpDownRegionStyleWidth = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.numericUpDownRegionStylePaletteEntryId = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.numericUpDownRegionStyleFontSize = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.subtitleListView1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.groupBoxPropertiesRoot.SuspendLayout();
            this.groupBoxTextST.SuspendLayout();
            this.groupBoxPropertiesRegionStyle.SuspendLayout();
            this.groupBoxPropertiesPalette.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaletteEntry)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaletteY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaletteCr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaletteCb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaletteOpacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStyleId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStyleHPos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStyleVPos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStyleHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStyleWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStylePaletteEntryId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStyleFontSize)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxPropertiesRoot
            // 
            this.groupBoxPropertiesRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPropertiesRoot.Controls.Add(this.textBoxRoot);
            this.groupBoxPropertiesRoot.Location = new System.Drawing.Point(547, 21);
            this.groupBoxPropertiesRoot.Name = "groupBoxPropertiesRoot";
            this.groupBoxPropertiesRoot.Size = new System.Drawing.Size(397, 550);
            this.groupBoxPropertiesRoot.TabIndex = 7;
            this.groupBoxPropertiesRoot.TabStop = false;
            this.groupBoxPropertiesRoot.Text = "Properties";
            // 
            // groupBoxTextST
            // 
            this.groupBoxTextST.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTextST.Controls.Add(this.treeView1);
            this.groupBoxTextST.Location = new System.Drawing.Point(7, 21);
            this.groupBoxTextST.Name = "groupBoxTextST";
            this.groupBoxTextST.Size = new System.Drawing.Size(534, 338);
            this.groupBoxTextST.TabIndex = 6;
            this.groupBoxTextST.TabStop = false;
            this.groupBoxTextST.Text = "TextST structure";
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(3, 16);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(528, 319);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(869, 577);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(788, 577);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonImport.Location = new System.Drawing.Point(805, 3);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(139, 21);
            this.buttonImport.TabIndex = 7;
            this.buttonImport.Text = "Open TextST...";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.ButtonImportClick);
            // 
            // textBoxRoot
            // 
            this.textBoxRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxRoot.Location = new System.Drawing.Point(3, 16);
            this.textBoxRoot.Multiline = true;
            this.textBoxRoot.Name = "textBoxRoot";
            this.textBoxRoot.ReadOnly = true;
            this.textBoxRoot.Size = new System.Drawing.Size(391, 531);
            this.textBoxRoot.TabIndex = 0;
            // 
            // groupBoxPropertiesRegionStyle
            // 
            this.groupBoxPropertiesRegionStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.numericUpDownRegionStyleFontSize);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.label12);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.numericUpDownRegionStylePaletteEntryId);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.label11);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.numericUpDownRegionStyleHeight);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.label9);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.numericUpDownRegionStyleWidth);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.label10);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.numericUpDownRegionStyleVPos);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.label8);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.numericUpDownRegionStyleHPos);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.label7);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.numericUpDownRegionStyleId);
            this.groupBoxPropertiesRegionStyle.Controls.Add(this.label6);
            this.groupBoxPropertiesRegionStyle.Location = new System.Drawing.Point(129, 12);
            this.groupBoxPropertiesRegionStyle.Name = "groupBoxPropertiesRegionStyle";
            this.groupBoxPropertiesRegionStyle.Size = new System.Drawing.Size(202, 438);
            this.groupBoxPropertiesRegionStyle.TabIndex = 8;
            this.groupBoxPropertiesRegionStyle.TabStop = false;
            this.groupBoxPropertiesRegionStyle.Text = "Properties: Region style";
            this.groupBoxPropertiesRegionStyle.Visible = false;
            // 
            // groupBoxPropertiesPalette
            // 
            this.groupBoxPropertiesPalette.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPropertiesPalette.Controls.Add(this.panelPaletteColor);
            this.groupBoxPropertiesPalette.Controls.Add(this.buttonColor);
            this.groupBoxPropertiesPalette.Controls.Add(this.numericUpDownPaletteOpacity);
            this.groupBoxPropertiesPalette.Controls.Add(this.label5);
            this.groupBoxPropertiesPalette.Controls.Add(this.numericUpDownPaletteCb);
            this.groupBoxPropertiesPalette.Controls.Add(this.label4);
            this.groupBoxPropertiesPalette.Controls.Add(this.numericUpDownPaletteCr);
            this.groupBoxPropertiesPalette.Controls.Add(this.label3);
            this.groupBoxPropertiesPalette.Controls.Add(this.numericUpDownPaletteY);
            this.groupBoxPropertiesPalette.Controls.Add(this.label2);
            this.groupBoxPropertiesPalette.Controls.Add(this.numericUpDownPaletteEntry);
            this.groupBoxPropertiesPalette.Controls.Add(this.label1);
            this.groupBoxPropertiesPalette.Location = new System.Drawing.Point(220, 12);
            this.groupBoxPropertiesPalette.Name = "groupBoxPropertiesPalette";
            this.groupBoxPropertiesPalette.Size = new System.Drawing.Size(167, 211);
            this.groupBoxPropertiesPalette.TabIndex = 9;
            this.groupBoxPropertiesPalette.TabStop = false;
            this.groupBoxPropertiesPalette.Text = "Properties: Region palette";
            this.groupBoxPropertiesPalette.Visible = false;
            // 
            // groupBoxPropertiesUserStyle
            // 
            this.groupBoxPropertiesUserStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPropertiesUserStyle.Location = new System.Drawing.Point(337, 12);
            this.groupBoxPropertiesUserStyle.Name = "groupBoxPropertiesUserStyle";
            this.groupBoxPropertiesUserStyle.Size = new System.Drawing.Size(111, 122);
            this.groupBoxPropertiesUserStyle.TabIndex = 10;
            this.groupBoxPropertiesUserStyle.TabStop = false;
            this.groupBoxPropertiesUserStyle.Text = "Properties: User style";
            this.groupBoxPropertiesUserStyle.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "EntryId";
            // 
            // numericUpDownPaletteEntry
            // 
            this.numericUpDownPaletteEntry.Location = new System.Drawing.Point(82, 27);
            this.numericUpDownPaletteEntry.Maximum = new decimal(new int[] {
            254,
            0,
            0,
            0});
            this.numericUpDownPaletteEntry.Name = "numericUpDownPaletteEntry";
            this.numericUpDownPaletteEntry.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownPaletteEntry.TabIndex = 1;
            // 
            // numericUpDownPaletteY
            // 
            this.numericUpDownPaletteY.Location = new System.Drawing.Point(82, 53);
            this.numericUpDownPaletteY.Maximum = new decimal(new int[] {
            235,
            0,
            0,
            0});
            this.numericUpDownPaletteY.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numericUpDownPaletteY.Name = "numericUpDownPaletteY";
            this.numericUpDownPaletteY.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownPaletteY.TabIndex = 3;
            this.numericUpDownPaletteY.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Y";
            // 
            // numericUpDownPaletteCr
            // 
            this.numericUpDownPaletteCr.Location = new System.Drawing.Point(82, 79);
            this.numericUpDownPaletteCr.Maximum = new decimal(new int[] {
            240,
            0,
            0,
            0});
            this.numericUpDownPaletteCr.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numericUpDownPaletteCr.Name = "numericUpDownPaletteCr";
            this.numericUpDownPaletteCr.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownPaletteCr.TabIndex = 5;
            this.numericUpDownPaletteCr.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Cr";
            // 
            // numericUpDownPaletteCb
            // 
            this.numericUpDownPaletteCb.Location = new System.Drawing.Point(82, 105);
            this.numericUpDownPaletteCb.Maximum = new decimal(new int[] {
            240,
            0,
            0,
            0});
            this.numericUpDownPaletteCb.Name = "numericUpDownPaletteCb";
            this.numericUpDownPaletteCb.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownPaletteCb.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Cb";
            // 
            // numericUpDownPaletteOpacity
            // 
            this.numericUpDownPaletteOpacity.Location = new System.Drawing.Point(82, 131);
            this.numericUpDownPaletteOpacity.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownPaletteOpacity.Name = "numericUpDownPaletteOpacity";
            this.numericUpDownPaletteOpacity.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownPaletteOpacity.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 133);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Opacity";
            // 
            // panelPaletteColor
            // 
            this.panelPaletteColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPaletteColor.Location = new System.Drawing.Point(137, 174);
            this.panelPaletteColor.Name = "panelPaletteColor";
            this.panelPaletteColor.Size = new System.Drawing.Size(21, 20);
            this.panelPaletteColor.TabIndex = 14;
            // 
            // buttonColor
            // 
            this.buttonColor.Location = new System.Drawing.Point(10, 173);
            this.buttonColor.Name = "buttonColor";
            this.buttonColor.Size = new System.Drawing.Size(121, 21);
            this.buttonColor.TabIndex = 13;
            this.buttonColor.Text = "Pick color";
            this.buttonColor.UseVisualStyleBackColor = true;
            this.buttonColor.Click += new System.EventHandler(this.buttonColor_Click);
            // 
            // numericUpDownRegionStyleId
            // 
            this.numericUpDownRegionStyleId.Location = new System.Drawing.Point(114, 22);
            this.numericUpDownRegionStyleId.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numericUpDownRegionStyleId.Name = "numericUpDownRegionStyleId";
            this.numericUpDownRegionStyleId.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownRegionStyleId.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 24);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "StyleId";
            // 
            // numericUpDownRegionStyleHPos
            // 
            this.numericUpDownRegionStyleHPos.Location = new System.Drawing.Point(114, 48);
            this.numericUpDownRegionStyleHPos.Maximum = new decimal(new int[] {
            7679,
            0,
            0,
            0});
            this.numericUpDownRegionStyleHPos.Name = "numericUpDownRegionStyleHPos";
            this.numericUpDownRegionStyleHPos.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownRegionStyleHPos.TabIndex = 5;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 50);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(93, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Horizontal position";
            // 
            // numericUpDownRegionStyleVPos
            // 
            this.numericUpDownRegionStyleVPos.Location = new System.Drawing.Point(114, 74);
            this.numericUpDownRegionStyleVPos.Maximum = new decimal(new int[] {
            4319,
            0,
            0,
            0});
            this.numericUpDownRegionStyleVPos.Name = "numericUpDownRegionStyleVPos";
            this.numericUpDownRegionStyleVPos.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownRegionStyleVPos.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 76);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Vertical position";
            // 
            // numericUpDownRegionStyleHeight
            // 
            this.numericUpDownRegionStyleHeight.Location = new System.Drawing.Point(114, 126);
            this.numericUpDownRegionStyleHeight.Maximum = new decimal(new int[] {
            4319,
            0,
            0,
            0});
            this.numericUpDownRegionStyleHeight.Name = "numericUpDownRegionStyleHeight";
            this.numericUpDownRegionStyleHeight.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownRegionStyleHeight.TabIndex = 11;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(10, 128);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(38, 13);
            this.label9.TabIndex = 10;
            this.label9.Text = "Height";
            // 
            // numericUpDownRegionStyleWidth
            // 
            this.numericUpDownRegionStyleWidth.Location = new System.Drawing.Point(114, 100);
            this.numericUpDownRegionStyleWidth.Maximum = new decimal(new int[] {
            7679,
            0,
            0,
            0});
            this.numericUpDownRegionStyleWidth.Name = "numericUpDownRegionStyleWidth";
            this.numericUpDownRegionStyleWidth.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownRegionStyleWidth.TabIndex = 9;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(10, 102);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "Width";
            // 
            // numericUpDownRegionStylePaletteEntryId
            // 
            this.numericUpDownRegionStylePaletteEntryId.Location = new System.Drawing.Point(114, 152);
            this.numericUpDownRegionStylePaletteEntryId.Maximum = new decimal(new int[] {
            254,
            0,
            0,
            0});
            this.numericUpDownRegionStylePaletteEntryId.Name = "numericUpDownRegionStylePaletteEntryId";
            this.numericUpDownRegionStylePaletteEntryId.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownRegionStylePaletteEntryId.TabIndex = 13;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(10, 154);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(90, 13);
            this.label11.TabIndex = 12;
            this.label11.Text = "Bg palette entryId";
            // 
            // numericUpDownRegionStyleFontSize
            // 
            this.numericUpDownRegionStyleFontSize.Location = new System.Drawing.Point(115, 209);
            this.numericUpDownRegionStyleFontSize.Maximum = new decimal(new int[] {
            144,
            0,
            0,
            0});
            this.numericUpDownRegionStyleFontSize.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDownRegionStyleFontSize.Name = "numericUpDownRegionStyleFontSize";
            this.numericUpDownRegionStyleFontSize.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownRegionStyleFontSize.TabIndex = 15;
            this.numericUpDownRegionStyleFontSize.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(11, 211);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(49, 13);
            this.label12.TabIndex = 14;
            this.label12.Text = "Font size";
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.subtitleListView1.DisplayExtraFromExtra = false;
            this.subtitleListView1.FirstVisibleIndex = -1;
            this.subtitleListView1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.HideSelection = false;
            this.subtitleListView1.Location = new System.Drawing.Point(9, 365);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.OwnerDraw = true;
            this.subtitleListView1.Size = new System.Drawing.Size(532, 206);
            this.subtitleListView1.SubtitleFontBold = false;
            this.subtitleListView1.SubtitleFontName = "Tahoma";
            this.subtitleListView1.SubtitleFontSize = 8;
            this.subtitleListView1.TabIndex = 112;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.UseSyntaxColoring = true;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            // 
            // ExportTextST
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(952, 610);
            this.Controls.Add(this.groupBoxPropertiesRegionStyle);
            this.Controls.Add(this.groupBoxPropertiesPalette);
            this.Controls.Add(this.groupBoxPropertiesUserStyle);
            this.Controls.Add(this.subtitleListView1);
            this.Controls.Add(this.buttonImport);
            this.Controls.Add(this.groupBoxPropertiesRoot);
            this.Controls.Add(this.groupBoxTextST);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Name = "ExportTextST";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ExportTextST";
            this.groupBoxPropertiesRoot.ResumeLayout(false);
            this.groupBoxPropertiesRoot.PerformLayout();
            this.groupBoxTextST.ResumeLayout(false);
            this.groupBoxPropertiesRegionStyle.ResumeLayout(false);
            this.groupBoxPropertiesRegionStyle.PerformLayout();
            this.groupBoxPropertiesPalette.ResumeLayout(false);
            this.groupBoxPropertiesPalette.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaletteEntry)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaletteY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaletteCr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaletteCb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPaletteOpacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStyleId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStyleHPos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStyleVPos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStyleHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStyleWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStylePaletteEntryId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRegionStyleFontSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxPropertiesRoot;
        private System.Windows.Forms.GroupBox groupBoxTextST;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private Controls.SubtitleListView subtitleListView1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.TextBox textBoxRoot;
        private System.Windows.Forms.GroupBox groupBoxPropertiesRegionStyle;
        private System.Windows.Forms.GroupBox groupBoxPropertiesPalette;
        private System.Windows.Forms.GroupBox groupBoxPropertiesUserStyle;
        private System.Windows.Forms.NumericUpDown numericUpDownPaletteOpacity;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDownPaletteCb;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownPaletteCr;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownPaletteY;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownPaletteEntry;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelPaletteColor;
        private System.Windows.Forms.Button buttonColor;
        private System.Windows.Forms.NumericUpDown numericUpDownRegionStyleHeight;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numericUpDownRegionStyleWidth;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numericUpDownRegionStyleVPos;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericUpDownRegionStyleHPos;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numericUpDownRegionStyleId;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericUpDownRegionStyleFontSize;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown numericUpDownRegionStylePaletteEntryId;
        private System.Windows.Forms.Label label11;
    }
}