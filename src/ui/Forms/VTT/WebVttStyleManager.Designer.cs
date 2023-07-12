namespace Nikse.SubtitleEdit.Forms.VTT
{
    sealed partial class WebVttStyleManager
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
            this.columnHeaderItalic = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderForeColor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBackgroundColor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderUseCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripStyles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveBottomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.newToolStripMenuItemNew = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemImport = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExport = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxProperties = new System.Windows.Forms.GroupBox();
            this.groupBoxAfter = new System.Windows.Forms.GroupBox();
            this.labelAfter = new System.Windows.Forms.Label();
            this.groupBoxBefore = new System.Windows.Forms.GroupBox();
            this.labelBefore = new System.Windows.Forms.Label();
            this.textBoxStyleName = new System.Windows.Forms.TextBox();
            this.labelStyleName = new System.Windows.Forms.Label();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.checkBoxShadowEnabled = new System.Windows.Forms.CheckBox();
            this.checkBoxBackgroundColorEnabled = new System.Windows.Forms.CheckBox();
            this.checkBoxColorEnabled = new System.Windows.Forms.CheckBox();
            this.panelShadowColor = new System.Windows.Forms.Panel();
            this.buttonShadowColor = new System.Windows.Forms.Button();
            this.numericUpDownShadowWidth = new System.Windows.Forms.NumericUpDown();
            this.panelBackgroundColor = new System.Windows.Forms.Panel();
            this.labelShadow = new System.Windows.Forms.Label();
            this.buttonBackgroundColor = new System.Windows.Forms.Button();
            this.panelPrimaryColor = new System.Windows.Forms.Panel();
            this.checkBoxStrikeout = new System.Windows.Forms.CheckBox();
            this.buttonPrimaryColor = new System.Windows.Forms.Button();
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
            this.labelDuplicateStyleNames = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.groupBoxStyles.SuspendLayout();
            this.contextMenuStripStyles.SuspendLayout();
            this.groupBoxProperties.SuspendLayout();
            this.groupBoxAfter.SuspendLayout();
            this.groupBoxBefore.SuspendLayout();
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
            this.groupBoxStyles.Text = "Styles";
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
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
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
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
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
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
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
            this.buttonRemoveAll.Click += new System.EventHandler(this.buttonRemoveAll_Click);
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
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
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
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
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
            this.columnHeaderItalic,
            this.columnHeaderForeColor,
            this.columnHeaderBackgroundColor,
            this.columnHeaderUseCount});
            this.listViewStyles.ContextMenuStrip = this.contextMenuStripStyles;
            this.listViewStyles.FullRowSelect = true;
            this.listViewStyles.HideSelection = false;
            this.listViewStyles.Location = new System.Drawing.Point(6, 19);
            this.listViewStyles.Name = "listViewStyles";
            this.listViewStyles.Size = new System.Drawing.Size(588, 516);
            this.listViewStyles.TabIndex = 0;
            this.listViewStyles.UseCompatibleStateImageBehavior = false;
            this.listViewStyles.View = System.Windows.Forms.View.Details;
            this.listViewStyles.SelectedIndexChanged += new System.EventHandler(this.listViewStyles_SelectedIndexChanged);
            this.listViewStyles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewStyles_KeyDown);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 130;
            // 
            // columnHeaderFontName
            // 
            this.columnHeaderFontName.Text = "Font name";
            this.columnHeaderFontName.Width = 122;
            // 
            // columnHeaderFontSize
            // 
            this.columnHeaderFontSize.Text = "Font size";
            this.columnHeaderFontSize.Width = 80;
            // 
            // columnHeaderItalic
            // 
            this.columnHeaderItalic.Text = "Italic";
            // 
            // columnHeaderForeColor
            // 
            this.columnHeaderForeColor.Text = "BG";
            this.columnHeaderForeColor.Width = 70;
            // 
            // columnHeaderBackgroundColor
            // 
            this.columnHeaderBackgroundColor.Text = "Text";
            this.columnHeaderBackgroundColor.Width = 55;
            // 
            // columnHeaderUseCount
            // 
            this.columnHeaderUseCount.Text = "Used#";
            // 
            // contextMenuStripStyles
            // 
            this.contextMenuStripStyles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.toolStripMenuItemRemoveAll,
            this.toolStripSeparator4,
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem,
            this.moveTopToolStripMenuItem,
            this.moveBottomToolStripMenuItem,
            this.toolStripSeparator3,
            this.newToolStripMenuItemNew,
            this.copyToolStripMenuItemCopy,
            this.toolStripSeparator1,
            this.toolStripMenuItemImport,
            this.toolStripMenuItemExport});
            this.contextMenuStripStyles.Name = "contextMenuStrip1";
            this.contextMenuStripStyles.Size = new System.Drawing.Size(216, 242);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.deleteToolStripMenuItem.Text = "Remove";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripMenuItemRemoveAll
            // 
            this.toolStripMenuItemRemoveAll.Name = "toolStripMenuItemRemoveAll";
            this.toolStripMenuItemRemoveAll.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemRemoveAll.Text = "Remove all";
            this.toolStripMenuItemRemoveAll.Click += new System.EventHandler(this.toolStripMenuItemRemoveAll_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(212, 6);
            // 
            // moveUpToolStripMenuItem
            // 
            this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            this.moveUpToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.moveUpToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveUpToolStripMenuItem.Text = "Move up";
            this.moveUpToolStripMenuItem.Click += new System.EventHandler(this.moveUpToolStripMenuItem_Click);
            // 
            // moveDownToolStripMenuItem
            // 
            this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            this.moveDownToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.moveDownToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveDownToolStripMenuItem.Text = "Move down";
            this.moveDownToolStripMenuItem.Click += new System.EventHandler(this.moveDownToolStripMenuItem_Click);
            // 
            // moveTopToolStripMenuItem
            // 
            this.moveTopToolStripMenuItem.Name = "moveTopToolStripMenuItem";
            this.moveTopToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Home)));
            this.moveTopToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveTopToolStripMenuItem.Text = "Move to top";
            this.moveTopToolStripMenuItem.Click += new System.EventHandler(this.moveTopToolStripMenuItem_Click);
            // 
            // moveBottomToolStripMenuItem
            // 
            this.moveBottomToolStripMenuItem.Name = "moveBottomToolStripMenuItem";
            this.moveBottomToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.End)));
            this.moveBottomToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveBottomToolStripMenuItem.Text = "Move to bottom";
            this.moveBottomToolStripMenuItem.Click += new System.EventHandler(this.moveBottomToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(212, 6);
            // 
            // newToolStripMenuItemNew
            // 
            this.newToolStripMenuItemNew.Name = "newToolStripMenuItemNew";
            this.newToolStripMenuItemNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItemNew.Size = new System.Drawing.Size(215, 22);
            this.newToolStripMenuItemNew.Text = "New";
            this.newToolStripMenuItemNew.Click += new System.EventHandler(this.newToolStripMenuItemNew_Click);
            // 
            // copyToolStripMenuItemCopy
            // 
            this.copyToolStripMenuItemCopy.Name = "copyToolStripMenuItemCopy";
            this.copyToolStripMenuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItemCopy.Size = new System.Drawing.Size(215, 22);
            this.copyToolStripMenuItemCopy.Text = "Copy";
            this.copyToolStripMenuItemCopy.Click += new System.EventHandler(this.copyToolStripMenuItemCopy_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(212, 6);
            // 
            // toolStripMenuItemImport
            // 
            this.toolStripMenuItemImport.Name = "toolStripMenuItemImport";
            this.toolStripMenuItemImport.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.toolStripMenuItemImport.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemImport.Text = "Import...";
            this.toolStripMenuItemImport.Click += new System.EventHandler(this.toolStripMenuItemImport_Click);
            // 
            // toolStripMenuItemExport
            // 
            this.toolStripMenuItemExport.Name = "toolStripMenuItemExport";
            this.toolStripMenuItemExport.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.toolStripMenuItemExport.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemExport.Text = "Export...";
            this.toolStripMenuItemExport.Click += new System.EventHandler(this.toolStripMenuItemExport_Click);
            // 
            // groupBoxProperties
            // 
            this.groupBoxProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxProperties.Controls.Add(this.groupBoxAfter);
            this.groupBoxProperties.Controls.Add(this.groupBoxBefore);
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
            // groupBoxAfter
            // 
            this.groupBoxAfter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAfter.Controls.Add(this.labelAfter);
            this.groupBoxAfter.Location = new System.Drawing.Point(249, 239);
            this.groupBoxAfter.Name = "groupBoxAfter";
            this.groupBoxAfter.Size = new System.Drawing.Size(229, 100);
            this.groupBoxAfter.TabIndex = 12;
            this.groupBoxAfter.TabStop = false;
            this.groupBoxAfter.Text = "After";
            // 
            // labelAfter
            // 
            this.labelAfter.AutoSize = true;
            this.labelAfter.Location = new System.Drawing.Point(6, 16);
            this.labelAfter.Name = "labelAfter";
            this.labelAfter.Size = new System.Drawing.Size(51, 13);
            this.labelAfter.TabIndex = 1;
            this.labelAfter.Text = "labelAfter";
            // 
            // groupBoxBefore
            // 
            this.groupBoxBefore.Controls.Add(this.labelBefore);
            this.groupBoxBefore.Location = new System.Drawing.Point(10, 239);
            this.groupBoxBefore.Name = "groupBoxBefore";
            this.groupBoxBefore.Size = new System.Drawing.Size(233, 100);
            this.groupBoxBefore.TabIndex = 11;
            this.groupBoxBefore.TabStop = false;
            this.groupBoxBefore.Text = "Before";
            // 
            // labelBefore
            // 
            this.labelBefore.AutoSize = true;
            this.labelBefore.Location = new System.Drawing.Point(7, 20);
            this.labelBefore.Name = "labelBefore";
            this.labelBefore.Size = new System.Drawing.Size(60, 13);
            this.labelBefore.TabIndex = 0;
            this.labelBefore.Text = "labelBefore";
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
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.pictureBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(7, 345);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(472, 248);
            this.groupBoxPreview.TabIndex = 7;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 16);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(466, 229);
            this.pictureBoxPreview.TabIndex = 0;
            this.pictureBoxPreview.TabStop = false;
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFont.Controls.Add(this.checkBoxShadowEnabled);
            this.groupBoxFont.Controls.Add(this.checkBoxBackgroundColorEnabled);
            this.groupBoxFont.Controls.Add(this.checkBoxColorEnabled);
            this.groupBoxFont.Controls.Add(this.panelShadowColor);
            this.groupBoxFont.Controls.Add(this.buttonShadowColor);
            this.groupBoxFont.Controls.Add(this.numericUpDownShadowWidth);
            this.groupBoxFont.Controls.Add(this.panelBackgroundColor);
            this.groupBoxFont.Controls.Add(this.labelShadow);
            this.groupBoxFont.Controls.Add(this.buttonBackgroundColor);
            this.groupBoxFont.Controls.Add(this.panelPrimaryColor);
            this.groupBoxFont.Controls.Add(this.checkBoxStrikeout);
            this.groupBoxFont.Controls.Add(this.buttonPrimaryColor);
            this.groupBoxFont.Controls.Add(this.checkBoxFontUnderline);
            this.groupBoxFont.Controls.Add(this.numericUpDownFontSize);
            this.groupBoxFont.Controls.Add(this.checkBoxFontItalic);
            this.groupBoxFont.Controls.Add(this.checkBoxFontBold);
            this.groupBoxFont.Controls.Add(this.comboBoxFontName);
            this.groupBoxFont.Controls.Add(this.labelFontSize);
            this.groupBoxFont.Controls.Add(this.labelFontName);
            this.groupBoxFont.Location = new System.Drawing.Point(7, 51);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(472, 182);
            this.groupBoxFont.TabIndex = 2;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "Font";
            // 
            // checkBoxShadowEnabled
            // 
            this.checkBoxShadowEnabled.AutoSize = true;
            this.checkBoxShadowEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxShadowEnabled.Location = new System.Drawing.Point(282, 100);
            this.checkBoxShadowEnabled.Name = "checkBoxShadowEnabled";
            this.checkBoxShadowEnabled.Size = new System.Drawing.Size(65, 17);
            this.checkBoxShadowEnabled.TabIndex = 11;
            this.checkBoxShadowEnabled.Text = "Enabled";
            this.checkBoxShadowEnabled.UseVisualStyleBackColor = true;
            this.checkBoxShadowEnabled.CheckedChanged += new System.EventHandler(this.checkBoxShadowEnabled_CheckedChanged);
            // 
            // checkBoxBackgroundColorEnabled
            // 
            this.checkBoxBackgroundColorEnabled.AutoSize = true;
            this.checkBoxBackgroundColorEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxBackgroundColorEnabled.Location = new System.Drawing.Point(135, 100);
            this.checkBoxBackgroundColorEnabled.Name = "checkBoxBackgroundColorEnabled";
            this.checkBoxBackgroundColorEnabled.Size = new System.Drawing.Size(65, 17);
            this.checkBoxBackgroundColorEnabled.TabIndex = 10;
            this.checkBoxBackgroundColorEnabled.Text = "Enabled";
            this.checkBoxBackgroundColorEnabled.UseVisualStyleBackColor = true;
            this.checkBoxBackgroundColorEnabled.CheckedChanged += new System.EventHandler(this.checkBoxBackgroundColorEnabled_CheckedChanged);
            // 
            // checkBoxColorEnabled
            // 
            this.checkBoxColorEnabled.AutoSize = true;
            this.checkBoxColorEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxColorEnabled.Location = new System.Drawing.Point(16, 100);
            this.checkBoxColorEnabled.Name = "checkBoxColorEnabled";
            this.checkBoxColorEnabled.Size = new System.Drawing.Size(65, 17);
            this.checkBoxColorEnabled.TabIndex = 9;
            this.checkBoxColorEnabled.Text = "Enabled";
            this.checkBoxColorEnabled.UseVisualStyleBackColor = true;
            this.checkBoxColorEnabled.CheckedChanged += new System.EventHandler(this.checkBoxColorEnabled_CheckedChanged);
            // 
            // panelShadowColor
            // 
            this.panelShadowColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelShadowColor.Location = new System.Drawing.Point(371, 124);
            this.panelShadowColor.Name = "panelShadowColor";
            this.panelShadowColor.Size = new System.Drawing.Size(21, 20);
            this.panelShadowColor.TabIndex = 7;
            this.panelShadowColor.Click += new System.EventHandler(this.panelShadowColor_Click);
            // 
            // buttonShadowColor
            // 
            this.buttonShadowColor.Location = new System.Drawing.Point(282, 123);
            this.buttonShadowColor.Name = "buttonShadowColor";
            this.buttonShadowColor.Size = new System.Drawing.Size(84, 23);
            this.buttonShadowColor.TabIndex = 6;
            this.buttonShadowColor.Text = "Shadow";
            this.buttonShadowColor.UseVisualStyleBackColor = true;
            // 
            // numericUpDownShadowWidth
            // 
            this.numericUpDownShadowWidth.DecimalPlaces = 1;
            this.numericUpDownShadowWidth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownShadowWidth.Location = new System.Drawing.Point(286, 152);
            this.numericUpDownShadowWidth.Name = "numericUpDownShadowWidth";
            this.numericUpDownShadowWidth.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownShadowWidth.TabIndex = 2;
            // 
            // panelBackgroundColor
            // 
            this.panelBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBackgroundColor.Location = new System.Drawing.Point(242, 124);
            this.panelBackgroundColor.Name = "panelBackgroundColor";
            this.panelBackgroundColor.Size = new System.Drawing.Size(21, 20);
            this.panelBackgroundColor.TabIndex = 3;
            this.panelBackgroundColor.Click += new System.EventHandler(this.panelBackgroundColor_Click);
            // 
            // labelShadow
            // 
            this.labelShadow.AutoSize = true;
            this.labelShadow.Location = new System.Drawing.Point(344, 154);
            this.labelShadow.Name = "labelShadow";
            this.labelShadow.Size = new System.Drawing.Size(46, 13);
            this.labelShadow.TabIndex = 2;
            this.labelShadow.Text = "Shadow";
            // 
            // buttonBackgroundColor
            // 
            this.buttonBackgroundColor.Location = new System.Drawing.Point(135, 123);
            this.buttonBackgroundColor.Name = "buttonBackgroundColor";
            this.buttonBackgroundColor.Size = new System.Drawing.Size(101, 23);
            this.buttonBackgroundColor.TabIndex = 2;
            this.buttonBackgroundColor.Text = "Background";
            this.buttonBackgroundColor.UseVisualStyleBackColor = true;
            this.buttonBackgroundColor.Click += new System.EventHandler(this.buttonBackgroundColor_Click);
            // 
            // panelPrimaryColor
            // 
            this.panelPrimaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPrimaryColor.Location = new System.Drawing.Point(100, 124);
            this.panelPrimaryColor.Name = "panelPrimaryColor";
            this.panelPrimaryColor.Size = new System.Drawing.Size(21, 20);
            this.panelPrimaryColor.TabIndex = 1;
            this.panelPrimaryColor.Click += new System.EventHandler(this.panelPrimaryColor_Click);
            // 
            // checkBoxStrikeout
            // 
            this.checkBoxStrikeout.AutoSize = true;
            this.checkBoxStrikeout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxStrikeout.Location = new System.Drawing.Point(265, 59);
            this.checkBoxStrikeout.Name = "checkBoxStrikeout";
            this.checkBoxStrikeout.Size = new System.Drawing.Size(68, 17);
            this.checkBoxStrikeout.TabIndex = 8;
            this.checkBoxStrikeout.Text = "Strikeout";
            this.checkBoxStrikeout.UseVisualStyleBackColor = true;
            this.checkBoxStrikeout.CheckedChanged += new System.EventHandler(this.checkBoxStrikeout_CheckedChanged);
            // 
            // buttonPrimaryColor
            // 
            this.buttonPrimaryColor.Location = new System.Drawing.Point(14, 123);
            this.buttonPrimaryColor.Name = "buttonPrimaryColor";
            this.buttonPrimaryColor.Size = new System.Drawing.Size(80, 23);
            this.buttonPrimaryColor.TabIndex = 0;
            this.buttonPrimaryColor.Text = "&Color";
            this.buttonPrimaryColor.UseVisualStyleBackColor = true;
            this.buttonPrimaryColor.Click += new System.EventHandler(this.buttonPrimaryColor_Click);
            // 
            // checkBoxFontUnderline
            // 
            this.checkBoxFontUnderline.AutoSize = true;
            this.checkBoxFontUnderline.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxFontUnderline.Location = new System.Drawing.Point(169, 59);
            this.checkBoxFontUnderline.Name = "checkBoxFontUnderline";
            this.checkBoxFontUnderline.Size = new System.Drawing.Size(71, 17);
            this.checkBoxFontUnderline.TabIndex = 7;
            this.checkBoxFontUnderline.Text = "Underline";
            this.checkBoxFontUnderline.UseVisualStyleBackColor = true;
            this.checkBoxFontUnderline.CheckedChanged += new System.EventHandler(this.checkBoxFontUnderline_CheckedChanged);
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
            this.numericUpDownFontSize.ValueChanged += new System.EventHandler(this.numericUpDownFontSize_ValueChanged);
            // 
            // checkBoxFontItalic
            // 
            this.checkBoxFontItalic.AutoSize = true;
            this.checkBoxFontItalic.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxFontItalic.Location = new System.Drawing.Point(90, 59);
            this.checkBoxFontItalic.Name = "checkBoxFontItalic";
            this.checkBoxFontItalic.Size = new System.Drawing.Size(48, 17);
            this.checkBoxFontItalic.TabIndex = 6;
            this.checkBoxFontItalic.Text = "Italic";
            this.checkBoxFontItalic.UseVisualStyleBackColor = true;
            this.checkBoxFontItalic.CheckedChanged += new System.EventHandler(this.checkBoxFontItalic_CheckedChanged);
            // 
            // checkBoxFontBold
            // 
            this.checkBoxFontBold.AutoSize = true;
            this.checkBoxFontBold.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxFontBold.Location = new System.Drawing.Point(13, 59);
            this.checkBoxFontBold.Name = "checkBoxFontBold";
            this.checkBoxFontBold.Size = new System.Drawing.Size(51, 17);
            this.checkBoxFontBold.TabIndex = 5;
            this.checkBoxFontBold.Text = "Bold";
            this.checkBoxFontBold.UseVisualStyleBackColor = true;
            this.checkBoxFontBold.CheckedChanged += new System.EventHandler(this.checkBoxFontBold_CheckedChanged);
            // 
            // comboBoxFontName
            // 
            this.comboBoxFontName.FormattingEnabled = true;
            this.comboBoxFontName.Location = new System.Drawing.Point(73, 17);
            this.comboBoxFontName.Name = "comboBoxFontName";
            this.comboBoxFontName.Size = new System.Drawing.Size(188, 21);
            this.comboBoxFontName.TabIndex = 1;
            this.comboBoxFontName.TextChanged += new System.EventHandler(this.comboBoxFontName_TextChanged);
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
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
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
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelDuplicateStyleNames
            // 
            this.labelDuplicateStyleNames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDuplicateStyleNames.AutoSize = true;
            this.labelDuplicateStyleNames.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDuplicateStyleNames.ForeColor = System.Drawing.Color.Firebrick;
            this.labelDuplicateStyleNames.Location = new System.Drawing.Point(5, 632);
            this.labelDuplicateStyleNames.Name = "labelDuplicateStyleNames";
            this.labelDuplicateStyleNames.Size = new System.Drawing.Size(154, 13);
            this.labelDuplicateStyleNames.TabIndex = 11;
            this.labelDuplicateStyleNames.Text = "labelDuplicateStyleNames";
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(8, 614);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(47, 13);
            this.labelInfo.TabIndex = 13;
            this.labelInfo.Text = "labelInfo";
            // 
            // WebVttStyleManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1110, 654);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelDuplicateStyleNames);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxProperties);
            this.Controls.Add(this.groupBoxStyles);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1100, 680);
            this.Name = "WebVttStyleManager";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WebVttStyleManager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WebVttStyleManager_FormClosing);
            this.groupBoxStyles.ResumeLayout(false);
            this.contextMenuStripStyles.ResumeLayout(false);
            this.groupBoxProperties.ResumeLayout(false);
            this.groupBoxProperties.PerformLayout();
            this.groupBoxAfter.ResumeLayout(false);
            this.groupBoxAfter.PerformLayout();
            this.groupBoxBefore.ResumeLayout(false);
            this.groupBoxBefore.PerformLayout();
            this.groupBoxPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.groupBoxFont.ResumeLayout(false);
            this.groupBoxFont.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.ColumnHeader columnHeaderForeColor;
        private System.Windows.Forms.ColumnHeader columnHeaderBackgroundColor;
        private System.Windows.Forms.GroupBox groupBoxProperties;
        private System.Windows.Forms.NumericUpDown numericUpDownShadowWidth;
        private System.Windows.Forms.Label labelShadow;
        private System.Windows.Forms.TextBox textBoxStyleName;
        private System.Windows.Forms.Label labelStyleName;
        private System.Windows.Forms.Panel panelShadowColor;
        private System.Windows.Forms.Button buttonShadowColor;
        private System.Windows.Forms.Panel panelBackgroundColor;
        private System.Windows.Forms.Button buttonBackgroundColor;
        private System.Windows.Forms.Panel panelPrimaryColor;
        private System.Windows.Forms.Button buttonPrimaryColor;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.GroupBox groupBoxFont;
        private System.Windows.Forms.CheckBox checkBoxStrikeout;
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
        private System.Windows.Forms.GroupBox groupBoxAfter;
        private System.Windows.Forms.GroupBox groupBoxBefore;
        private System.Windows.Forms.ColumnHeader columnHeaderItalic;
        private System.Windows.Forms.Label labelAfter;
        private System.Windows.Forms.Label labelBefore;
        private System.Windows.Forms.CheckBox checkBoxShadowEnabled;
        private System.Windows.Forms.Label labelDuplicateStyleNames;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripStyles;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveBottomToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItemNew;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItemCopy;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImport;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExport;
        private System.Windows.Forms.Label labelInfo;
    }
}