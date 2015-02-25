namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class MultipleReplace
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
            this.groupBoxLinesFound = new System.Windows.Forms.GroupBox();
            this.buttonReplacesInverseSelection = new System.Windows.Forms.Button();
            this.buttonReplacesSelectAll = new System.Windows.Forms.Button();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxReplaces = new System.Windows.Forms.GroupBox();
            this.radioButtonRegEx = new System.Windows.Forms.RadioButton();
            this.radioButtonCaseSensitive = new System.Windows.Forms.RadioButton();
            this.buttonRemoveAll = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.textBoxFind = new System.Windows.Forms.TextBox();
            this.textBoxReplace = new System.Windows.Forms.TextBox();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.labelFindWhat = new System.Windows.Forms.Label();
            this.labelReplaceWith = new System.Windows.Forms.Label();
            this.listViewReplaceList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.radioButtonNormal = new System.Windows.Forms.RadioButton();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.moveTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveBottomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxLinesFound.SuspendLayout();
            this.groupBoxReplaces.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxLinesFound
            // 
            this.groupBoxLinesFound.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLinesFound.Controls.Add(this.buttonReplacesInverseSelection);
            this.groupBoxLinesFound.Controls.Add(this.buttonReplacesSelectAll);
            this.groupBoxLinesFound.Controls.Add(this.listViewFixes);
            this.groupBoxLinesFound.Location = new System.Drawing.Point(4, 14);
            this.groupBoxLinesFound.Name = "groupBoxLinesFound";
            this.groupBoxLinesFound.Size = new System.Drawing.Size(926, 289);
            this.groupBoxLinesFound.TabIndex = 8;
            this.groupBoxLinesFound.TabStop = false;
            this.groupBoxLinesFound.Text = "Lines found: {0}";
            // 
            // buttonReplacesInverseSelection
            // 
            this.buttonReplacesInverseSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonReplacesInverseSelection.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonReplacesInverseSelection.Location = new System.Drawing.Point(91, 261);
            this.buttonReplacesInverseSelection.Name = "buttonReplacesInverseSelection";
            this.buttonReplacesInverseSelection.Size = new System.Drawing.Size(100, 21);
            this.buttonReplacesInverseSelection.TabIndex = 106;
            this.buttonReplacesInverseSelection.Text = "&Inverse selection";
            this.buttonReplacesInverseSelection.UseVisualStyleBackColor = true;
            this.buttonReplacesInverseSelection.Click += new System.EventHandler(this.buttonReplacesInverseSelection_Click);
            // 
            // buttonReplacesSelectAll
            // 
            this.buttonReplacesSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonReplacesSelectAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonReplacesSelectAll.Location = new System.Drawing.Point(10, 261);
            this.buttonReplacesSelectAll.Name = "buttonReplacesSelectAll";
            this.buttonReplacesSelectAll.Size = new System.Drawing.Size(75, 21);
            this.buttonReplacesSelectAll.TabIndex = 105;
            this.buttonReplacesSelectAll.Text = "Select &all";
            this.buttonReplacesSelectAll.UseVisualStyleBackColor = true;
            this.buttonReplacesSelectAll.Click += new System.EventHandler(this.buttonReplacesSelectAll_Click);
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
            this.columnHeader7,
            this.columnHeader8});
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(10, 21);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(910, 234);
            this.listViewFixes.TabIndex = 9;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Apply";
            this.columnHeader4.Width = 48;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Line#";
            this.columnHeader5.Width = 61;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Before";
            this.columnHeader7.Width = 330;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "After";
            this.columnHeader8.Width = 440;
            // 
            // groupBoxReplaces
            // 
            this.groupBoxReplaces.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxReplaces.Controls.Add(this.radioButtonRegEx);
            this.groupBoxReplaces.Controls.Add(this.radioButtonCaseSensitive);
            this.groupBoxReplaces.Controls.Add(this.buttonRemoveAll);
            this.groupBoxReplaces.Controls.Add(this.buttonExport);
            this.groupBoxReplaces.Controls.Add(this.buttonImport);
            this.groupBoxReplaces.Controls.Add(this.textBoxFind);
            this.groupBoxReplaces.Controls.Add(this.textBoxReplace);
            this.groupBoxReplaces.Controls.Add(this.buttonUpdate);
            this.groupBoxReplaces.Controls.Add(this.buttonAdd);
            this.groupBoxReplaces.Controls.Add(this.labelFindWhat);
            this.groupBoxReplaces.Controls.Add(this.labelReplaceWith);
            this.groupBoxReplaces.Controls.Add(this.listViewReplaceList);
            this.groupBoxReplaces.Controls.Add(this.radioButtonNormal);
            this.groupBoxReplaces.Location = new System.Drawing.Point(8, 3);
            this.groupBoxReplaces.Name = "groupBoxReplaces";
            this.groupBoxReplaces.Size = new System.Drawing.Size(922, 312);
            this.groupBoxReplaces.TabIndex = 0;
            this.groupBoxReplaces.TabStop = false;
            // 
            // radioButtonRegEx
            // 
            this.radioButtonRegEx.AutoSize = true;
            this.radioButtonRegEx.Location = new System.Drawing.Point(286, 62);
            this.radioButtonRegEx.Name = "radioButtonRegEx";
            this.radioButtonRegEx.Size = new System.Drawing.Size(56, 17);
            this.radioButtonRegEx.TabIndex = 6;
            this.radioButtonRegEx.Text = "RegEx";
            this.radioButtonRegEx.UseVisualStyleBackColor = true;
            this.radioButtonRegEx.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // radioButtonCaseSensitive
            // 
            this.radioButtonCaseSensitive.AutoSize = true;
            this.radioButtonCaseSensitive.Location = new System.Drawing.Point(146, 62);
            this.radioButtonCaseSensitive.Name = "radioButtonCaseSensitive";
            this.radioButtonCaseSensitive.Size = new System.Drawing.Size(94, 17);
            this.radioButtonCaseSensitive.TabIndex = 5;
            this.radioButtonCaseSensitive.Text = "Case sensitive";
            this.radioButtonCaseSensitive.UseVisualStyleBackColor = true;
            this.radioButtonCaseSensitive.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // buttonRemoveAll
            // 
            this.buttonRemoveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveAll.Location = new System.Drawing.Point(541, 285);
            this.buttonRemoveAll.Name = "buttonRemoveAll";
            this.buttonRemoveAll.Size = new System.Drawing.Size(120, 21);
            this.buttonRemoveAll.TabIndex = 5;
            this.buttonRemoveAll.Text = "Remove all";
            this.buttonRemoveAll.UseVisualStyleBackColor = true;
            this.buttonRemoveAll.Click += new System.EventHandler(this.buttonRemoveAll_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.Location = new System.Drawing.Point(793, 285);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(120, 21);
            this.buttonExport.TabIndex = 7;
            this.buttonExport.Text = "Export...";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.ExportClick);
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImport.Location = new System.Drawing.Point(667, 285);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(120, 21);
            this.buttonImport.TabIndex = 6;
            this.buttonImport.Text = "Import...";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // textBoxFind
            // 
            this.textBoxFind.Location = new System.Drawing.Point(20, 35);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(224, 21);
            this.textBoxFind.TabIndex = 0;
            // 
            // textBoxReplace
            // 
            this.textBoxReplace.Location = new System.Drawing.Point(250, 35);
            this.textBoxReplace.Name = "textBoxReplace";
            this.textBoxReplace.Size = new System.Drawing.Size(227, 21);
            this.textBoxReplace.TabIndex = 1;
            this.textBoxReplace.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxReplaceKeyDown);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Location = new System.Drawing.Point(588, 35);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(99, 21);
            this.buttonUpdate.TabIndex = 3;
            this.buttonUpdate.Text = "&Update";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.ButtonUpdateClick);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(483, 35);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(99, 21);
            this.buttonAdd.TabIndex = 2;
            this.buttonAdd.Text = "&Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.ButtonAddClick);
            // 
            // labelFindWhat
            // 
            this.labelFindWhat.AutoSize = true;
            this.labelFindWhat.Location = new System.Drawing.Point(17, 16);
            this.labelFindWhat.Name = "labelFindWhat";
            this.labelFindWhat.Size = new System.Drawing.Size(58, 13);
            this.labelFindWhat.TabIndex = 22;
            this.labelFindWhat.Text = "Find what:";
            // 
            // labelReplaceWith
            // 
            this.labelReplaceWith.AutoSize = true;
            this.labelReplaceWith.Location = new System.Drawing.Point(247, 16);
            this.labelReplaceWith.Name = "labelReplaceWith";
            this.labelReplaceWith.Size = new System.Drawing.Size(72, 13);
            this.labelReplaceWith.TabIndex = 21;
            this.labelReplaceWith.Text = "Replace with:";
            // 
            // listViewReplaceList
            // 
            this.listViewReplaceList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewReplaceList.CheckBoxes = true;
            this.listViewReplaceList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader6});
            this.listViewReplaceList.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewReplaceList.FullRowSelect = true;
            this.listViewReplaceList.HideSelection = false;
            this.listViewReplaceList.Location = new System.Drawing.Point(6, 96);
            this.listViewReplaceList.Name = "listViewReplaceList";
            this.listViewReplaceList.Size = new System.Drawing.Size(910, 183);
            this.listViewReplaceList.TabIndex = 4;
            this.listViewReplaceList.UseCompatibleStateImageBehavior = false;
            this.listViewReplaceList.View = System.Windows.Forms.View.Details;
            this.listViewReplaceList.SelectedIndexChanged += new System.EventHandler(this.ListViewReplaceListSelectedIndexChanged);
            this.listViewReplaceList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListViewReplaceListKeyDown);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Enabled";
            this.columnHeader1.Width = 70;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Find what";
            this.columnHeader2.Width = 210;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Replace with";
            this.columnHeader3.Width = 210;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Search type";
            this.columnHeader6.Width = 200;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem,
            this.moveTopToolStripMenuItem,
            this.moveBottomToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(162, 136);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItemClick);
            // 
            // moveUpToolStripMenuItem
            // 
            this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            this.moveUpToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.moveUpToolStripMenuItem.Text = "Move up";
            this.moveUpToolStripMenuItem.Click += new System.EventHandler(this.moveUpToolStripMenuItem_Click);
            // 
            // moveDownToolStripMenuItem
            // 
            this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            this.moveDownToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.moveDownToolStripMenuItem.Text = "Move down";
            this.moveDownToolStripMenuItem.Click += new System.EventHandler(this.moveDownToolStripMenuItem_Click);
            // 
            // radioButtonNormal
            // 
            this.radioButtonNormal.AutoSize = true;
            this.radioButtonNormal.Checked = true;
            this.radioButtonNormal.Location = new System.Drawing.Point(22, 62);
            this.radioButtonNormal.Name = "radioButtonNormal";
            this.radioButtonNormal.Size = new System.Drawing.Size(58, 17);
            this.radioButtonNormal.TabIndex = 4;
            this.radioButtonNormal.TabStop = true;
            this.radioButtonNormal.Text = "Normal";
            this.radioButtonNormal.UseVisualStyleBackColor = true;
            this.radioButtonNormal.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(768, 308);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(849, 308);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBoxReplaces);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBoxLinesFound);
            this.splitContainer1.Panel2.Controls.Add(this.buttonOK);
            this.splitContainer1.Panel2.Controls.Add(this.buttonCancel);
            this.splitContainer1.Size = new System.Drawing.Size(933, 667);
            this.splitContainer1.SplitterDistance = 318;
            this.splitContainer1.TabIndex = 12;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // moveTopToolStripMenuItem
            // 
            this.moveTopToolStripMenuItem.Name = "moveTopToolStripMenuItem";
            this.moveTopToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.moveTopToolStripMenuItem.Text = "Move to top";
            this.moveTopToolStripMenuItem.Click += new System.EventHandler(this.moveTopToolStripMenuItem_Click);
            // 
            // moveBottomToolStripMenuItem
            // 
            this.moveBottomToolStripMenuItem.Name = "moveBottomToolStripMenuItem";
            this.moveBottomToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.moveBottomToolStripMenuItem.Text = "Move to bottom";
            this.moveBottomToolStripMenuItem.Click += new System.EventHandler(this.moveBottomToolStripMenuItem_Click);
            // 
            // MultipleReplace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 667);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(750, 500);
            this.Name = "MultipleReplace";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Multiple replace";
            this.Shown += new System.EventHandler(this.MultipleReplace_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MultipleReplace_KeyDown);
            this.groupBoxLinesFound.ResumeLayout(false);
            this.groupBoxReplaces.ResumeLayout(false);
            this.groupBoxReplaces.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxLinesFound;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.GroupBox groupBoxReplaces;
        private System.Windows.Forms.ListView listViewReplaceList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.Label labelFindWhat;
        private System.Windows.Forms.Label labelReplaceWith;
        private System.Windows.Forms.TextBox textBoxReplace;
        private System.Windows.Forms.RadioButton radioButtonRegEx;
        private System.Windows.Forms.RadioButton radioButtonCaseSensitive;
        private System.Windows.Forms.RadioButton radioButtonNormal;
        private System.Windows.Forms.TextBox textBoxFind;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button buttonReplacesSelectAll;
        private System.Windows.Forms.Button buttonReplacesInverseSelection;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button buttonRemoveAll;
        private System.Windows.Forms.ToolStripMenuItem moveTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveBottomToolStripMenuItem;
    }
}