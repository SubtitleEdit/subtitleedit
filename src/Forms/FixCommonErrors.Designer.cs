using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class FixCommonErrors
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
            Nikse.SubtitleEdit.Core.TimeCode timeCode2 = new Nikse.SubtitleEdit.Core.TimeCode();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonNextFinish = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.buttonBack = new System.Windows.Forms.Button();
            this.groupBoxStep1 = new System.Windows.Forms.GroupBox();
            this.buttonResetDefault = new System.Windows.Forms.Button();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.buttonInverseSelection = new System.Windows.Forms.Button();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageFixes = new System.Windows.Forms.TabPage();
            this.splitContainerStep2 = new System.Windows.Forms.SplitContainer();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonFixesApply = new System.Windows.Forms.Button();
            this.buttonRefreshFixes = new System.Windows.Forms.Button();
            this.buttonFixesSelectAll = new System.Windows.Forms.Button();
            this.buttonFixesInverse = new System.Windows.Forms.Button();
            this.subtitleListView1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.contextMenuStripListview = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mergeSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxEditPanel = new System.Windows.Forms.GroupBox();
            this.buttonSplitLine = new System.Windows.Forms.Button();
            this.labelSingleLine = new System.Windows.Forms.Label();
            this.buttonUnBreak = new System.Windows.Forms.Button();
            this.buttonAutoBreak = new System.Windows.Forms.Button();
            this.labelStartTimeWarning = new System.Windows.Forms.Label();
            this.labelDurationWarning = new System.Windows.Forms.Label();
            this.timeUpDownStartTime = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.numericUpDownDuration = new System.Windows.Forms.NumericUpDown();
            this.labelDuration = new System.Windows.Forms.Label();
            this.labelStartTime = new System.Windows.Forms.Label();
            this.labelTextLineTotal = new System.Windows.Forms.Label();
            this.labelTextLineLengths = new System.Windows.Forms.Label();
            this.textBoxListViewText = new System.Windows.Forms.TextBox();
            this.tabPageLog = new System.Windows.Forms.TabPage();
            this.textBoxFixedIssues = new System.Windows.Forms.TextBox();
            this.labelNumberOfImportantLogMessages = new System.Windows.Forms.Label();
            this.contextMenuStripFixes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInverseSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxStep1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageFixes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerStep2)).BeginInit();
            this.splitContainerStep2.Panel1.SuspendLayout();
            this.splitContainerStep2.Panel2.SuspendLayout();
            this.splitContainerStep2.SuspendLayout();
            this.contextMenuStripListview.SuspendLayout();
            this.groupBoxEditPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDuration)).BeginInit();
            this.tabPageLog.SuspendLayout();
            this.contextMenuStripFixes.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(759, 575);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // buttonNextFinish
            // 
            this.buttonNextFinish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNextFinish.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonNextFinish.Location = new System.Drawing.Point(678, 575);
            this.buttonNextFinish.Name = "buttonNextFinish";
            this.buttonNextFinish.Size = new System.Drawing.Size(75, 23);
            this.buttonNextFinish.TabIndex = 9;
            this.buttonNextFinish.Text = "&Next >";
            this.buttonNextFinish.UseVisualStyleBackColor = true;
            this.buttonNextFinish.Click += new System.EventHandler(this.ButtonFixClick);
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 577);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(60, 13);
            this.labelStatus.TabIndex = 5;
            this.labelStatus.Text = "labelStatus";
            // 
            // buttonBack
            // 
            this.buttonBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBack.Enabled = false;
            this.buttonBack.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonBack.Location = new System.Drawing.Point(597, 575);
            this.buttonBack.Name = "buttonBack";
            this.buttonBack.Size = new System.Drawing.Size(75, 23);
            this.buttonBack.TabIndex = 8;
            this.buttonBack.Text = "< &Back";
            this.buttonBack.UseVisualStyleBackColor = true;
            this.buttonBack.Click += new System.EventHandler(this.ButtonBackClick);
            // 
            // groupBoxStep1
            // 
            this.groupBoxStep1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxStep1.Controls.Add(this.buttonResetDefault);
            this.groupBoxStep1.Controls.Add(this.comboBoxLanguage);
            this.groupBoxStep1.Controls.Add(this.labelLanguage);
            this.groupBoxStep1.Controls.Add(this.buttonInverseSelection);
            this.groupBoxStep1.Controls.Add(this.buttonSelectAll);
            this.groupBoxStep1.Controls.Add(this.listView1);
            this.groupBoxStep1.Location = new System.Drawing.Point(12, 12);
            this.groupBoxStep1.Name = "groupBoxStep1";
            this.groupBoxStep1.Size = new System.Drawing.Size(822, 557);
            this.groupBoxStep1.TabIndex = 9;
            this.groupBoxStep1.TabStop = false;
            this.groupBoxStep1.Text = "Step 1/2 - Choose which errors to fix";
            // 
            // buttonResetDefault
            // 
            this.buttonResetDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonResetDefault.Location = new System.Drawing.Point(193, 530);
            this.buttonResetDefault.Name = "buttonResetDefault";
            this.buttonResetDefault.Size = new System.Drawing.Size(100, 23);
            this.buttonResetDefault.TabIndex = 12;
            this.buttonResetDefault.Text = "Select default";
            this.buttonResetDefault.UseVisualStyleBackColor = true;
            this.buttonResetDefault.Click += new System.EventHandler(this.buttonResetDefault_Click);
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(619, 17);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(196, 21);
            this.comboBoxLanguage.TabIndex = 0;
            this.comboBoxLanguage.SelectedIndexChanged += new System.EventHandler(this.comboBoxLanguage_SelectedIndexChanged);
            this.comboBoxLanguage.Enter += new System.EventHandler(this.comboBoxLanguage_Enter);
            // 
            // labelLanguage
            // 
            this.labelLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLanguage.Location = new System.Drawing.Point(376, 20);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(237, 25);
            this.labelLanguage.TabIndex = 11;
            this.labelLanguage.Text = "Language";
            this.labelLanguage.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // buttonInverseSelection
            // 
            this.buttonInverseSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonInverseSelection.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonInverseSelection.Location = new System.Drawing.Point(87, 530);
            this.buttonInverseSelection.Name = "buttonInverseSelection";
            this.buttonInverseSelection.Size = new System.Drawing.Size(100, 23);
            this.buttonInverseSelection.TabIndex = 3;
            this.buttonInverseSelection.Text = "Inverse selection";
            this.buttonInverseSelection.UseVisualStyleBackColor = true;
            this.buttonInverseSelection.Click += new System.EventHandler(this.ButtonInverseSelectionClick);
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSelectAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSelectAll.Location = new System.Drawing.Point(6, 530);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectAll.TabIndex = 2;
            this.buttonSelectAll.Text = "Select all";
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.ButtonSelectAllClick);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.CheckBoxes = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(6, 48);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(810, 477);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Apply";
            this.columnHeader1.Width = 53;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "What to fix";
            this.columnHeader2.Width = 99;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Example";
            this.columnHeader3.Width = 158;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.tabControl1);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(822, 557);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Step 2/2 - Verify fixes";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageFixes);
            this.tabControl1.Controls.Add(this.tabPageLog);
            this.tabControl1.Location = new System.Drawing.Point(6, 20);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(810, 531);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPageFixes
            // 
            this.tabPageFixes.Controls.Add(this.splitContainerStep2);
            this.tabPageFixes.Location = new System.Drawing.Point(4, 22);
            this.tabPageFixes.Name = "tabPageFixes";
            this.tabPageFixes.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFixes.Size = new System.Drawing.Size(802, 505);
            this.tabPageFixes.TabIndex = 1;
            this.tabPageFixes.Text = "Fixes";
            this.tabPageFixes.UseVisualStyleBackColor = true;
            // 
            // splitContainerStep2
            // 
            this.splitContainerStep2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerStep2.Location = new System.Drawing.Point(3, 3);
            this.splitContainerStep2.Name = "splitContainerStep2";
            this.splitContainerStep2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerStep2.Panel1
            // 
            this.splitContainerStep2.Panel1.Controls.Add(this.listViewFixes);
            this.splitContainerStep2.Panel1.Controls.Add(this.buttonFixesApply);
            this.splitContainerStep2.Panel1.Controls.Add(this.buttonRefreshFixes);
            this.splitContainerStep2.Panel1.Controls.Add(this.buttonFixesSelectAll);
            this.splitContainerStep2.Panel1.Controls.Add(this.buttonFixesInverse);
            // 
            // splitContainerStep2.Panel2
            // 
            this.splitContainerStep2.Panel2.Controls.Add(this.subtitleListView1);
            this.splitContainerStep2.Panel2.Controls.Add(this.groupBoxEditPanel);
            this.splitContainerStep2.Size = new System.Drawing.Size(796, 499);
            this.splitContainerStep2.SplitterDistance = 231;
            this.splitContainerStep2.TabIndex = 112;
            // 
            // listViewFixes
            // 
            this.listViewFixes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.listViewFixes.ContextMenuStrip = this.contextMenuStripFixes;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(3, 3);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(790, 198);
            this.listViewFixes.TabIndex = 100;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            this.listViewFixes.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListViewFixesColumnClick);
            this.listViewFixes.SelectedIndexChanged += new System.EventHandler(this.ListViewFixesSelectedIndexChanged);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Apply";
            this.columnHeader4.Width = 50;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Line#";
            this.columnHeader5.Width = 61;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Function";
            this.columnHeader6.Width = 134;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Before";
            this.columnHeader7.Width = 281;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "After";
            this.columnHeader8.Width = 244;
            // 
            // buttonFixesApply
            // 
            this.buttonFixesApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFixesApply.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonFixesApply.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonFixesApply.Location = new System.Drawing.Point(628, 206);
            this.buttonFixesApply.Name = "buttonFixesApply";
            this.buttonFixesApply.Size = new System.Drawing.Size(165, 23);
            this.buttonFixesApply.TabIndex = 108;
            this.buttonFixesApply.Text = "Apply &selected fixes";
            this.buttonFixesApply.UseVisualStyleBackColor = true;
            this.buttonFixesApply.Click += new System.EventHandler(this.ButtonFixesApplyClick);
            // 
            // buttonRefreshFixes
            // 
            this.buttonRefreshFixes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefreshFixes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRefreshFixes.Location = new System.Drawing.Point(457, 206);
            this.buttonRefreshFixes.Name = "buttonRefreshFixes";
            this.buttonRefreshFixes.Size = new System.Drawing.Size(165, 23);
            this.buttonRefreshFixes.TabIndex = 106;
            this.buttonRefreshFixes.Text = "&Refresh available fixes";
            this.buttonRefreshFixes.UseVisualStyleBackColor = true;
            this.buttonRefreshFixes.Click += new System.EventHandler(this.ButtonRefreshFixesClick);
            // 
            // buttonFixesSelectAll
            // 
            this.buttonFixesSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonFixesSelectAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonFixesSelectAll.Location = new System.Drawing.Point(3, 206);
            this.buttonFixesSelectAll.Name = "buttonFixesSelectAll";
            this.buttonFixesSelectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonFixesSelectAll.TabIndex = 102;
            this.buttonFixesSelectAll.Text = "Select &all";
            this.buttonFixesSelectAll.UseVisualStyleBackColor = true;
            this.buttonFixesSelectAll.Click += new System.EventHandler(this.ButtonFixesSelectAllClick);
            // 
            // buttonFixesInverse
            // 
            this.buttonFixesInverse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonFixesInverse.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonFixesInverse.Location = new System.Drawing.Point(84, 206);
            this.buttonFixesInverse.Name = "buttonFixesInverse";
            this.buttonFixesInverse.Size = new System.Drawing.Size(100, 23);
            this.buttonFixesInverse.TabIndex = 104;
            this.buttonFixesInverse.Text = "&Inverse selection";
            this.buttonFixesInverse.UseVisualStyleBackColor = true;
            this.buttonFixesInverse.Click += new System.EventHandler(this.ButtonFixesInverseClick);
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.AllowColumnReorder = true;
            this.subtitleListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.subtitleListView1.ContextMenuStrip = this.contextMenuStripListview;
            this.subtitleListView1.FirstVisibleIndex = -1;
            this.subtitleListView1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListView1.HideSelection = false;
            this.subtitleListView1.Location = new System.Drawing.Point(3, 5);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.OwnerDraw = true;
            this.subtitleListView1.Size = new System.Drawing.Size(785, 158);
            this.subtitleListView1.SubtitleFontBold = false;
            this.subtitleListView1.SubtitleFontName = "Tahoma";
            this.subtitleListView1.SubtitleFontSize = 8;
            this.subtitleListView1.TabIndex = 110;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.UseSyntaxColoring = true;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            this.subtitleListView1.SelectedIndexChanged += new System.EventHandler(this.SubtitleListView1SelectedIndexChanged);
            this.subtitleListView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.subtitleListView1_KeyDown);
            // 
            // contextMenuStripListview
            // 
            this.contextMenuStripListview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemDelete,
            this.toolStripSeparator1,
            this.mergeSelectedLinesToolStripMenuItem});
            this.contextMenuStripListview.Name = "contextMenuStripListview";
            this.contextMenuStripListview.Size = new System.Drawing.Size(182, 54);
            this.contextMenuStripListview.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripListviewOpening);
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            this.toolStripMenuItemDelete.Size = new System.Drawing.Size(181, 22);
            this.toolStripMenuItemDelete.Text = "Delete";
            this.toolStripMenuItemDelete.Click += new System.EventHandler(this.ToolStripMenuItemDeleteClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(178, 6);
            // 
            // mergeSelectedLinesToolStripMenuItem
            // 
            this.mergeSelectedLinesToolStripMenuItem.Name = "mergeSelectedLinesToolStripMenuItem";
            this.mergeSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.mergeSelectedLinesToolStripMenuItem.Text = "Merge selected lines";
            this.mergeSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.MergeSelectedLinesToolStripMenuItemClick);
            // 
            // groupBoxEditPanel
            // 
            this.groupBoxEditPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxEditPanel.Controls.Add(this.buttonSplitLine);
            this.groupBoxEditPanel.Controls.Add(this.labelSingleLine);
            this.groupBoxEditPanel.Controls.Add(this.buttonUnBreak);
            this.groupBoxEditPanel.Controls.Add(this.buttonAutoBreak);
            this.groupBoxEditPanel.Controls.Add(this.labelStartTimeWarning);
            this.groupBoxEditPanel.Controls.Add(this.labelDurationWarning);
            this.groupBoxEditPanel.Controls.Add(this.timeUpDownStartTime);
            this.groupBoxEditPanel.Controls.Add(this.numericUpDownDuration);
            this.groupBoxEditPanel.Controls.Add(this.labelDuration);
            this.groupBoxEditPanel.Controls.Add(this.labelStartTime);
            this.groupBoxEditPanel.Controls.Add(this.labelTextLineTotal);
            this.groupBoxEditPanel.Controls.Add(this.labelTextLineLengths);
            this.groupBoxEditPanel.Controls.Add(this.textBoxListViewText);
            this.groupBoxEditPanel.Location = new System.Drawing.Point(8, 160);
            this.groupBoxEditPanel.Name = "groupBoxEditPanel";
            this.groupBoxEditPanel.Size = new System.Drawing.Size(780, 101);
            this.groupBoxEditPanel.TabIndex = 111;
            this.groupBoxEditPanel.TabStop = false;
            // 
            // buttonSplitLine
            // 
            this.buttonSplitLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSplitLine.ForeColor = System.Drawing.Color.Red;
            this.buttonSplitLine.Location = new System.Drawing.Point(627, 62);
            this.buttonSplitLine.Name = "buttonSplitLine";
            this.buttonSplitLine.Size = new System.Drawing.Size(96, 23);
            this.buttonSplitLine.TabIndex = 124;
            this.buttonSplitLine.Text = "Split line";
            this.buttonSplitLine.UseVisualStyleBackColor = true;
            this.buttonSplitLine.Click += new System.EventHandler(this.ButtonSplitLineClick);
            // 
            // labelSingleLine
            // 
            this.labelSingleLine.AutoSize = true;
            this.labelSingleLine.Location = new System.Drawing.Point(296, 79);
            this.labelSingleLine.Name = "labelSingleLine";
            this.labelSingleLine.Size = new System.Drawing.Size(23, 13);
            this.labelSingleLine.TabIndex = 123;
            this.labelSingleLine.Text = "1/1";
            // 
            // buttonUnBreak
            // 
            this.buttonUnBreak.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUnBreak.Location = new System.Drawing.Point(627, 37);
            this.buttonUnBreak.Name = "buttonUnBreak";
            this.buttonUnBreak.Size = new System.Drawing.Size(96, 23);
            this.buttonUnBreak.TabIndex = 122;
            this.buttonUnBreak.Text = "&Unbreak";
            this.buttonUnBreak.UseVisualStyleBackColor = true;
            this.buttonUnBreak.Click += new System.EventHandler(this.ButtonUnBreakClick);
            // 
            // buttonAutoBreak
            // 
            this.buttonAutoBreak.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAutoBreak.Location = new System.Drawing.Point(627, 12);
            this.buttonAutoBreak.Name = "buttonAutoBreak";
            this.buttonAutoBreak.Size = new System.Drawing.Size(96, 23);
            this.buttonAutoBreak.TabIndex = 120;
            this.buttonAutoBreak.Text = "Auto &br";
            this.buttonAutoBreak.UseVisualStyleBackColor = true;
            this.buttonAutoBreak.Click += new System.EventHandler(this.ButtonAutoBreakClick);
            // 
            // labelStartTimeWarning
            // 
            this.labelStartTimeWarning.AutoSize = true;
            this.labelStartTimeWarning.ForeColor = System.Drawing.Color.Red;
            this.labelStartTimeWarning.Location = new System.Drawing.Point(6, 51);
            this.labelStartTimeWarning.Name = "labelStartTimeWarning";
            this.labelStartTimeWarning.Size = new System.Drawing.Size(115, 13);
            this.labelStartTimeWarning.TabIndex = 32;
            this.labelStartTimeWarning.Text = "labelStartTimeWarning";
            // 
            // labelDurationWarning
            // 
            this.labelDurationWarning.AutoSize = true;
            this.labelDurationWarning.ForeColor = System.Drawing.Color.Red;
            this.labelDurationWarning.Location = new System.Drawing.Point(57, 64);
            this.labelDurationWarning.Name = "labelDurationWarning";
            this.labelDurationWarning.Size = new System.Drawing.Size(110, 13);
            this.labelDurationWarning.TabIndex = 31;
            this.labelDurationWarning.Text = "labelDurationWarning";
            // 
            // timeUpDownStartTime
            // 
            this.timeUpDownStartTime.AutoSize = true;
            this.timeUpDownStartTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownStartTime.Location = new System.Drawing.Point(8, 27);
            this.timeUpDownStartTime.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownStartTime.Name = "timeUpDownStartTime";
            this.timeUpDownStartTime.Size = new System.Drawing.Size(111, 27);
            this.timeUpDownStartTime.TabIndex = 112;
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
            // numericUpDownDuration
            // 
            this.numericUpDownDuration.DecimalPlaces = 3;
            this.numericUpDownDuration.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownDuration.Location = new System.Drawing.Point(99, 28);
            this.numericUpDownDuration.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numericUpDownDuration.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this.numericUpDownDuration.Name = "numericUpDownDuration";
            this.numericUpDownDuration.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownDuration.TabIndex = 114;
            this.numericUpDownDuration.ValueChanged += new System.EventHandler(this.NumericUpDownDurationValueChanged);
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(97, 12);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(48, 13);
            this.labelDuration.TabIndex = 28;
            this.labelDuration.Text = "Duration";
            // 
            // labelStartTime
            // 
            this.labelStartTime.AutoSize = true;
            this.labelStartTime.Location = new System.Drawing.Point(6, 12);
            this.labelStartTime.Name = "labelStartTime";
            this.labelStartTime.Size = new System.Drawing.Size(54, 13);
            this.labelStartTime.TabIndex = 27;
            this.labelStartTime.Text = "Start time";
            // 
            // labelTextLineTotal
            // 
            this.labelTextLineTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTextLineTotal.Location = new System.Drawing.Point(444, 79);
            this.labelTextLineTotal.Name = "labelTextLineTotal";
            this.labelTextLineTotal.Size = new System.Drawing.Size(177, 16);
            this.labelTextLineTotal.TabIndex = 26;
            this.labelTextLineTotal.Text = "labelTextLineTotal";
            this.labelTextLineTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelTextLineLengths
            // 
            this.labelTextLineLengths.AutoSize = true;
            this.labelTextLineLengths.Location = new System.Drawing.Point(191, 79);
            this.labelTextLineLengths.Name = "labelTextLineLengths";
            this.labelTextLineLengths.Size = new System.Drawing.Size(108, 13);
            this.labelTextLineLengths.TabIndex = 25;
            this.labelTextLineLengths.Text = "labelTextLineLengths";
            // 
            // textBoxListViewText
            // 
            this.textBoxListViewText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxListViewText.HideSelection = false;
            this.textBoxListViewText.Location = new System.Drawing.Point(194, 12);
            this.textBoxListViewText.Multiline = true;
            this.textBoxListViewText.Name = "textBoxListViewText";
            this.textBoxListViewText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxListViewText.Size = new System.Drawing.Size(427, 64);
            this.textBoxListViewText.TabIndex = 118;
            this.textBoxListViewText.TextChanged += new System.EventHandler(this.TextBoxListViewTextTextChanged);
            this.textBoxListViewText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxListViewTextKeyDown);
            // 
            // tabPageLog
            // 
            this.tabPageLog.Controls.Add(this.textBoxFixedIssues);
            this.tabPageLog.Location = new System.Drawing.Point(4, 22);
            this.tabPageLog.Name = "tabPageLog";
            this.tabPageLog.Size = new System.Drawing.Size(802, 505);
            this.tabPageLog.TabIndex = 2;
            this.tabPageLog.Text = "Log";
            this.tabPageLog.UseVisualStyleBackColor = true;
            // 
            // textBoxFixedIssues
            // 
            this.textBoxFixedIssues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFixedIssues.Location = new System.Drawing.Point(3, 3);
            this.textBoxFixedIssues.MaxLength = 65767;
            this.textBoxFixedIssues.Multiline = true;
            this.textBoxFixedIssues.Name = "textBoxFixedIssues";
            this.textBoxFixedIssues.ReadOnly = true;
            this.textBoxFixedIssues.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxFixedIssues.Size = new System.Drawing.Size(796, 499);
            this.textBoxFixedIssues.TabIndex = 5;
            this.textBoxFixedIssues.WordWrap = false;
            // 
            // labelNumberOfImportantLogMessages
            // 
            this.labelNumberOfImportantLogMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelNumberOfImportantLogMessages.AutoSize = true;
            this.labelNumberOfImportantLogMessages.ForeColor = System.Drawing.Color.Red;
            this.labelNumberOfImportantLogMessages.Location = new System.Drawing.Point(12, 593);
            this.labelNumberOfImportantLogMessages.Name = "labelNumberOfImportantLogMessages";
            this.labelNumberOfImportantLogMessages.Size = new System.Drawing.Size(190, 13);
            this.labelNumberOfImportantLogMessages.TabIndex = 11;
            this.labelNumberOfImportantLogMessages.Text = "labelNumberOfImportantLogMessages";
            // 
            // contextMenuStripFixes
            // 
            this.contextMenuStripFixes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSelectAll,
            this.toolStripMenuItemInverseSelection});
            this.contextMenuStripFixes.Name = "contextMenuStrip1";
            this.contextMenuStripFixes.Size = new System.Drawing.Size(181, 70);
            // 
            // toolStripMenuItemSelectAll
            // 
            this.toolStripMenuItemSelectAll.Name = "toolStripMenuItemSelectAll";
            this.toolStripMenuItemSelectAll.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItemSelectAll.Text = "Select all";
            this.toolStripMenuItemSelectAll.Click += new System.EventHandler(this.toolStripMenuItemSelectAll_Click);
            // 
            // toolStripMenuItemInverseSelection
            // 
            this.toolStripMenuItemInverseSelection.Name = "toolStripMenuItemInverseSelection";
            this.toolStripMenuItemInverseSelection.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItemInverseSelection.Text = "Inverse selection";
            this.toolStripMenuItemInverseSelection.Click += new System.EventHandler(this.toolStripMenuItemInverseSelection_Click);
            // 
            // FixCommonErrors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(846, 608);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelNumberOfImportantLogMessages);
            this.Controls.Add(this.buttonBack);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonNextFinish);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBoxStep1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(840, 580);
            this.Name = "FixCommonErrors";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Fix common errors";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FixCommonErrorsFormClosing);
            this.Shown += new System.EventHandler(this.FixCommonErrorsShown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormFixKeyDown);
            this.Resize += new System.EventHandler(this.FixCommonErrorsResize);
            this.groupBoxStep1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageFixes.ResumeLayout(false);
            this.splitContainerStep2.Panel1.ResumeLayout(false);
            this.splitContainerStep2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerStep2)).EndInit();
            this.splitContainerStep2.ResumeLayout(false);
            this.contextMenuStripListview.ResumeLayout(false);
            this.groupBoxEditPanel.ResumeLayout(false);
            this.groupBoxEditPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDuration)).EndInit();
            this.tabPageLog.ResumeLayout(false);
            this.tabPageLog.PerformLayout();
            this.contextMenuStripFixes.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonNextFinish;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button buttonBack;
        private System.Windows.Forms.GroupBox groupBoxStep1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageFixes;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.TabPage tabPageLog;
        private System.Windows.Forms.TextBox textBoxFixedIssues;
        private System.Windows.Forms.Button buttonInverseSelection;
        private System.Windows.Forms.Button buttonSelectAll;
        private System.Windows.Forms.GroupBox groupBoxEditPanel;
        private SubtitleListView subtitleListView1;
        private System.Windows.Forms.Label labelTextLineTotal;
        private System.Windows.Forms.Label labelTextLineLengths;
        private System.Windows.Forms.TextBox textBoxListViewText;
        private System.Windows.Forms.Button buttonFixesApply;
        private System.Windows.Forms.Button buttonFixesInverse;
        private System.Windows.Forms.Button buttonFixesSelectAll;
        private System.Windows.Forms.Label labelStartTimeWarning;
        private System.Windows.Forms.Label labelDurationWarning;
        private Nikse.SubtitleEdit.Controls.TimeUpDown timeUpDownStartTime;
        private System.Windows.Forms.NumericUpDown numericUpDownDuration;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.Label labelStartTime;
        private System.Windows.Forms.Button buttonRefreshFixes;
        private System.Windows.Forms.Button buttonUnBreak;
        private System.Windows.Forms.Button buttonAutoBreak;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListview;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripMenuItem mergeSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Label labelNumberOfImportantLogMessages;
        private System.Windows.Forms.SplitContainer splitContainerStep2;
        private System.Windows.Forms.Label labelSingleLine;
        private System.Windows.Forms.Button buttonSplitLine;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.Button buttonResetDefault;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFixes;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInverseSelection;
    }
}