namespace Nikse.SubtitleEdit.Forms
{
    partial class ChangeCasingNames
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxNames = new System.Windows.Forms.GroupBox();
            this.buttonAddCustomNames = new System.Windows.Forms.Button();
            this.textBoxExtraNames = new System.Windows.Forms.TextBox();
            this.labelExtraNames = new System.Windows.Forms.Label();
            this.buttonInverseSelection = new System.Windows.Forms.Button();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.listViewNames = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxLinesFound = new System.Windows.Forms.GroupBox();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInverseSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.labelXLinesSelected = new System.Windows.Forms.Label();
            this.contextMenuStrip2SelectAll = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1SelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2InverseSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxNames.SuspendLayout();
            this.groupBoxLinesFound.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2SelectAll.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(591, 602);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 14;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(672, 602);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 15;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // groupBoxNames
            // 
            this.groupBoxNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNames.Controls.Add(this.buttonAddCustomNames);
            this.groupBoxNames.Controls.Add(this.textBoxExtraNames);
            this.groupBoxNames.Controls.Add(this.labelExtraNames);
            this.groupBoxNames.Controls.Add(this.buttonInverseSelection);
            this.groupBoxNames.Controls.Add(this.buttonSelectAll);
            this.groupBoxNames.Controls.Add(this.listViewNames);
            this.groupBoxNames.Location = new System.Drawing.Point(5, 9);
            this.groupBoxNames.Name = "groupBoxNames";
            this.groupBoxNames.Size = new System.Drawing.Size(747, 293);
            this.groupBoxNames.TabIndex = 12;
            this.groupBoxNames.TabStop = false;
            this.groupBoxNames.Text = "Names found in subtitle";
            // 
            // buttonAddCustomNames
            // 
            this.buttonAddCustomNames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddCustomNames.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAddCustomNames.Location = new System.Drawing.Point(640, 262);
            this.buttonAddCustomNames.Name = "buttonAddCustomNames";
            this.buttonAddCustomNames.Size = new System.Drawing.Size(100, 23);
            this.buttonAddCustomNames.TabIndex = 11;
            this.buttonAddCustomNames.Text = "Add";
            this.buttonAddCustomNames.UseVisualStyleBackColor = true;
            this.buttonAddCustomNames.Click += new System.EventHandler(this.buttonAddCustomNames_Click);
            // 
            // textBoxExtraNames
            // 
            this.textBoxExtraNames.Location = new System.Drawing.Point(10, 263);
            this.textBoxExtraNames.Name = "textBoxExtraNames";
            this.textBoxExtraNames.Size = new System.Drawing.Size(624, 21);
            this.textBoxExtraNames.TabIndex = 10;
            // 
            // labelExtraNames
            // 
            this.labelExtraNames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelExtraNames.AutoSize = true;
            this.labelExtraNames.Location = new System.Drawing.Point(7, 246);
            this.labelExtraNames.Name = "labelExtraNames";
            this.labelExtraNames.Size = new System.Drawing.Size(172, 13);
            this.labelExtraNames.TabIndex = 17;
            this.labelExtraNames.Text = "Extra names (separate by comma)";
            // 
            // buttonInverseSelection
            // 
            this.buttonInverseSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonInverseSelection.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonInverseSelection.Location = new System.Drawing.Point(88, 202);
            this.buttonInverseSelection.Name = "buttonInverseSelection";
            this.buttonInverseSelection.Size = new System.Drawing.Size(100, 23);
            this.buttonInverseSelection.TabIndex = 9;
            this.buttonInverseSelection.Text = "Inverse selection";
            this.buttonInverseSelection.UseVisualStyleBackColor = true;
            this.buttonInverseSelection.Click += new System.EventHandler(this.buttonInverseSelection_Click);
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSelectAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSelectAll.Location = new System.Drawing.Point(7, 202);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectAll.TabIndex = 8;
            this.buttonSelectAll.Text = "Select all";
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // listViewNames
            // 
            this.listViewNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewNames.CheckBoxes = true;
            this.listViewNames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewNames.ContextMenuStrip = this.contextMenuStrip2SelectAll;
            this.listViewNames.FullRowSelect = true;
            this.listViewNames.HideSelection = false;
            this.listViewNames.Location = new System.Drawing.Point(6, 19);
            this.listViewNames.MultiSelect = false;
            this.listViewNames.Name = "listViewNames";
            this.listViewNames.Size = new System.Drawing.Size(735, 177);
            this.listViewNames.TabIndex = 7;
            this.listViewNames.UseCompatibleStateImageBehavior = false;
            this.listViewNames.View = System.Windows.Forms.View.Details;
            this.listViewNames.SelectedIndexChanged += new System.EventHandler(this.ListViewNamesSelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Enabled";
            this.columnHeader1.Width = 70;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 620;
            // 
            // groupBoxLinesFound
            // 
            this.groupBoxLinesFound.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLinesFound.Controls.Add(this.listViewFixes);
            this.groupBoxLinesFound.Location = new System.Drawing.Point(5, 308);
            this.groupBoxLinesFound.Name = "groupBoxLinesFound";
            this.groupBoxLinesFound.Size = new System.Drawing.Size(747, 286);
            this.groupBoxLinesFound.TabIndex = 13;
            this.groupBoxLinesFound.TabStop = false;
            this.groupBoxLinesFound.Text = "Lines found: {0}";
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
            this.listViewFixes.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(6, 23);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(735, 257);
            this.listViewFixes.TabIndex = 9;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            this.listViewFixes.SelectedIndexChanged += new System.EventHandler(this.listViewFixes_SelectedIndexChanged);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Apply";
            this.columnHeader4.Width = 45;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Line#";
            this.columnHeader5.Width = 61;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Before";
            this.columnHeader7.Width = 292;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "After";
            this.columnHeader8.Width = 292;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSelectAll,
            this.toolStripMenuItemInverseSelection});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(162, 48);
            // 
            // toolStripMenuItemSelectAll
            // 
            this.toolStripMenuItemSelectAll.Name = "toolStripMenuItemSelectAll";
            this.toolStripMenuItemSelectAll.Size = new System.Drawing.Size(161, 22);
            this.toolStripMenuItemSelectAll.Text = "Select all";
            this.toolStripMenuItemSelectAll.Click += new System.EventHandler(this.toolStripMenuItemSelectAll_Click);
            // 
            // toolStripMenuItemInverseSelection
            // 
            this.toolStripMenuItemInverseSelection.Name = "toolStripMenuItemInverseSelection";
            this.toolStripMenuItemInverseSelection.Size = new System.Drawing.Size(161, 22);
            this.toolStripMenuItemInverseSelection.Text = "Inverse selection";
            this.toolStripMenuItemInverseSelection.Click += new System.EventHandler(this.toolStripMenuItemInverseSelection_Click);
            // 
            // labelXLinesSelected
            // 
            this.labelXLinesSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelXLinesSelected.AutoSize = true;
            this.labelXLinesSelected.Location = new System.Drawing.Point(5, 602);
            this.labelXLinesSelected.Name = "labelXLinesSelected";
            this.labelXLinesSelected.Size = new System.Drawing.Size(78, 13);
            this.labelXLinesSelected.TabIndex = 16;
            this.labelXLinesSelected.Text = "XLinesSelected";
            // 
            // contextMenuStrip2SelectAll
            // 
            this.contextMenuStrip2SelectAll.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1SelectAll,
            this.toolStripMenuItem2InverseSelection});
            this.contextMenuStrip2SelectAll.Name = "contextMenuStrip1";
            this.contextMenuStrip2SelectAll.Size = new System.Drawing.Size(181, 70);
            // 
            // toolStripMenuItem1SelectAll
            // 
            this.toolStripMenuItem1SelectAll.Name = "toolStripMenuItem1SelectAll";
            this.toolStripMenuItem1SelectAll.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem1SelectAll.Text = "Select all";
            this.toolStripMenuItem1SelectAll.Click += new System.EventHandler(this.toolStripMenuItem1SelectAll_Click);
            // 
            // toolStripMenuItem2InverseSelection
            // 
            this.toolStripMenuItem2InverseSelection.Name = "toolStripMenuItem2InverseSelection";
            this.toolStripMenuItem2InverseSelection.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem2InverseSelection.Text = "Inverse selection";
            this.toolStripMenuItem2InverseSelection.Click += new System.EventHandler(this.toolStripMenuItem2InverseSelection_Click);
            // 
            // ChangeCasingNames
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(757, 632);
            this.Controls.Add(this.labelXLinesSelected);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxNames);
            this.Controls.Add(this.groupBoxLinesFound);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 480);
            this.Name = "ChangeCasingNames";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Change casing - Names";
            this.Shown += new System.EventHandler(this.ChangeCasingNames_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChangeCasingNames_KeyDown);
            this.groupBoxNames.ResumeLayout(false);
            this.groupBoxNames.PerformLayout();
            this.groupBoxLinesFound.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2SelectAll.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxNames;
        private System.Windows.Forms.ListView listViewNames;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.GroupBox groupBoxLinesFound;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.Label labelXLinesSelected;
        private System.Windows.Forms.Button buttonInverseSelection;
        private System.Windows.Forms.Button buttonSelectAll;
        private System.Windows.Forms.Button buttonAddCustomNames;
        private System.Windows.Forms.TextBox textBoxExtraNames;
        private System.Windows.Forms.Label labelExtraNames;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInverseSelection;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2SelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1SelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2InverseSelection;
    }
}