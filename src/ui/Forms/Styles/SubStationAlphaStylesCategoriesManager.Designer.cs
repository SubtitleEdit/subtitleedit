namespace Nikse.SubtitleEdit.Forms.Styles
{
    sealed partial class SubStationAlphaStylesCategoriesManager
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
            this.groupBoxCategories = new System.Windows.Forms.GroupBox();
            this.buttonExportCategories = new System.Windows.Forms.Button();
            this.buttonImportCategories = new System.Windows.Forms.Button();
            this.buttonSetDefaultCategory = new System.Windows.Forms.Button();
            this.buttonRemoveCategory = new System.Windows.Forms.Button();
            this.buttonNewCategory = new System.Windows.Forms.Button();
            this.listViewCategories = new System.Windows.Forms.ListView();
            this.columnHeaderForName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderForStyles = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderForDefault = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripCategories = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRename = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveToTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveToBottomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorCategoryImportExport = new System.Windows.Forms.ToolStripSeparator();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialogImport = new System.Windows.Forms.OpenFileDialog();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxCategories.SuspendLayout();
            this.contextMenuStripCategories.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxCategories
            // 
            this.groupBoxCategories.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right));
            this.groupBoxCategories.Controls.Add(this.buttonExportCategories);
            this.groupBoxCategories.Controls.Add(this.buttonImportCategories);
            this.groupBoxCategories.Controls.Add(this.buttonSetDefaultCategory);
            this.groupBoxCategories.Controls.Add(this.buttonRemoveCategory);
            this.groupBoxCategories.Controls.Add(this.buttonNewCategory);
            this.groupBoxCategories.Controls.Add(this.listViewCategories);
            this.groupBoxCategories.Location = new System.Drawing.Point(6, 3);
            this.groupBoxCategories.Name = "groupBoxCategories";
            this.groupBoxCategories.Size = new System.Drawing.Size(350, 410);
            this.groupBoxCategories.TabIndex = 0;
            this.groupBoxCategories.TabStop = false;
            this.groupBoxCategories.Text = "Categories";
            // 
            // buttonExportCategories
            // 
            this.buttonExportCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExportCategories.Location = new System.Drawing.Point(178, 380);
            this.buttonExportCategories.Name = "buttonExportCategories";
            this.buttonExportCategories.Size = new System.Drawing.Size(164, 23);
            this.buttonExportCategories.TabIndex = 5;
            this.buttonExportCategories.Text = "Export";
            this.buttonExportCategories.UseVisualStyleBackColor = true;
            this.buttonExportCategories.Click += new System.EventHandler(this.ButtonExportCategories_Click);
            // 
            // buttonImportCategories
            // 
            this.buttonImportCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonImportCategories.Location = new System.Drawing.Point(8, 380);
            this.buttonImportCategories.Name = "buttonImportCategories";
            this.buttonImportCategories.Size = new System.Drawing.Size(164, 23);
            this.buttonImportCategories.TabIndex = 4;
            this.buttonImportCategories.Text = "Import";
            this.buttonImportCategories.UseVisualStyleBackColor = true;
            this.buttonImportCategories.Click += new System.EventHandler(this.ButtonImportCategories_Click);
            // 
            // buttonSetDefaultCategory
            // 
            this.buttonSetDefaultCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSetDefaultCategory.Location = new System.Drawing.Point(8, 352);
            this.buttonSetDefaultCategory.Name = "buttonSetDefaultCategory";
            this.buttonSetDefaultCategory.Size = new System.Drawing.Size(335, 23);
            this.buttonSetDefaultCategory.TabIndex = 3;
            this.buttonSetDefaultCategory.Text = "Set as default";
            this.buttonSetDefaultCategory.UseVisualStyleBackColor = true;
            this.buttonSetDefaultCategory.Click += new System.EventHandler(this.ButtonSetDefaultCategory_Click);
            // 
            // buttonRemoveCategory
            // 
            this.buttonRemoveCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveCategory.Location = new System.Drawing.Point(178, 324);
            this.buttonRemoveCategory.Name = "buttonRemoveCategory";
            this.buttonRemoveCategory.Size = new System.Drawing.Size(164, 23);
            this.buttonRemoveCategory.TabIndex = 2;
            this.buttonRemoveCategory.Text = "Remove";
            this.buttonRemoveCategory.UseVisualStyleBackColor = true;
            this.buttonRemoveCategory.Click += new System.EventHandler(this.ButtonRemoveCategory_Click);
            // 
            // buttonNewCategory
            // 
            this.buttonNewCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonNewCategory.Location = new System.Drawing.Point(8, 324);
            this.buttonNewCategory.Name = "buttonNewCategory";
            this.buttonNewCategory.Size = new System.Drawing.Size(164, 23);
            this.buttonNewCategory.TabIndex = 1;
            this.buttonNewCategory.Text = "New";
            this.buttonNewCategory.UseVisualStyleBackColor = true;
            this.buttonNewCategory.Click += new System.EventHandler(this.ButtonNewCategory_Click);
            // 
            // listViewCategories
            // 
            this.listViewCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewCategories.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderForName,
            this.columnHeaderForStyles,
            this.columnHeaderForDefault});
            this.listViewCategories.ContextMenuStrip = this.contextMenuStripCategories;
            this.listViewCategories.FullRowSelect = true;
            this.listViewCategories.GridLines = true;
            this.listViewCategories.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewCategories.HideSelection = false;
            this.listViewCategories.Location = new System.Drawing.Point(8, 20);
            this.listViewCategories.MultiSelect = true;
            this.listViewCategories.Name = "listViewCategories";
            this.listViewCategories.Size = new System.Drawing.Size(335, 299);
            this.listViewCategories.TabIndex = 0;
            this.listViewCategories.UseCompatibleStateImageBehavior = false;
            this.listViewCategories.View = System.Windows.Forms.View.Details;
            this.listViewCategories.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListViewCategories_KeyDown);
            this.listViewCategories.SelectedIndexChanged += new System.EventHandler(this.ListViewCategories_SelectedIndexChanged);
            // 
            // columnHeaderForName
            // 
            this.columnHeaderForName.Text = "Category";
            this.columnHeaderForName.Width = 150;
            // 
            // columnHeaderForStyles
            // 
            this.columnHeaderForStyles.Text = "Number of styles";
            this.columnHeaderForStyles.Width = 100;
            // 
            // columnHeaderForDefault
            // 
            this.columnHeaderForDefault.Name = "Default";
            this.columnHeaderForDefault.Text = "Default";
            this.columnHeaderForDefault.Width = 100;
            // 
            // contextMenuStripCategories
            // 
            this.contextMenuStripCategories.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.toolStripMenuItemRename,
            this.deleteToolStripMenuItem,
            this.deleteAllToolStripMenuItem,
            this.toolStripSeparator,
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem,
            this.moveToTopToolStripMenuItem,
            this.moveToBottomToolStripMenuItem,
            this.toolStripSeparatorCategoryImportExport,
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.contextMenuStripCategories.Name = "contextMenuStripCategories";
            this.contextMenuStripCategories.Size = new System.Drawing.Size(216, 214);
            this.contextMenuStripCategories.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripCategories_Opening);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
            | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.newToolStripMenuItem.Text = "New...";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItem_Click);
            // 
            // toolStripMenuItemRename
            // 
            this.toolStripMenuItemRename.Name = "toolStripMenuItemRename";
            this.toolStripMenuItemRename.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.toolStripMenuItemRename.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemRename.Text = "Rename...";
            this.toolStripMenuItemRename.Click += new System.EventHandler(this.ToolStripMenuItemRenameClick);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.deleteToolStripMenuItem.Text = "Delete...";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // deleteAllToolStripMenuItem
            // 
            this.deleteAllToolStripMenuItem.Name = "deleteAllToolStripMenuItem";
            this.deleteAllToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.deleteAllToolStripMenuItem.Text = "Delete all...";
            this.deleteAllToolStripMenuItem.Click += new System.EventHandler(this.DeleteAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(212, 6);
            // 
            // moveUpToolStripMenuItem
            // 
            this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            this.moveUpToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.moveUpToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveUpToolStripMenuItem.Text = "Move up";
            this.moveUpToolStripMenuItem.Click += new System.EventHandler(this.MoveUpToolStripMenuItem_Click);
            // 
            // moveDownToolStripMenuItem
            // 
            this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            this.moveDownToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.moveDownToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveDownToolStripMenuItem.Text = "Move down";
            this.moveDownToolStripMenuItem.Click += new System.EventHandler(this.MoveDownToolStripMenuItem_Click);
            // 
            // moveToTopToolStripMenuItem
            // 
            this.moveToTopToolStripMenuItem.Name = "moveToTopToolStripMenuItem";
            this.moveToTopToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Home)));
            this.moveToTopToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveToTopToolStripMenuItem.Text = "Move to top";
            this.moveToTopToolStripMenuItem.Click += new System.EventHandler(this.MoveToTopToolStripMenuItem_Click);
            // 
            // moveToBottomToolStripMenuItem
            // 
            this.moveToBottomToolStripMenuItem.Name = "moveToBottomToolStripMenuItem";
            this.moveToBottomToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.End)));
            this.moveToBottomToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveToBottomToolStripMenuItem.Text = "Move to bottom";
            this.moveToBottomToolStripMenuItem.Click += new System.EventHandler(this.MoveToBottomToolStripMenuItem_Click);
            // 
            // toolStripSeparatorCategoryImportExport
            // 
            this.toolStripSeparatorCategoryImportExport.Name = "toolStripSeparatorCategoryImportExport";
            this.toolStripSeparatorCategoryImportExport.Size = new System.Drawing.Size(212, 6);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.importToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.importToolStripMenuItem.Text = "Import...";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.ImportToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.exportToolStripMenuItem.Text = "Export...";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.ExportToolStripMenuItem_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(200, 418);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(280, 418);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // SubStationAlphaStylesCategoriesManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 452);
            this.Controls.Add(this.groupBoxCategories);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(200, 300);
            this.Name = "SubStationAlphaStylesCategoriesManager";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Assa Categories Manager";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SubStationAlphaStylesCategoriesManager_KeyDown);
            this.ResizeEnd += new System.EventHandler(this.SubStationAlphaStylesCategoriesManager_ResizeEnd);
            this.groupBoxCategories.ResumeLayout(false);
            this.contextMenuStripCategories.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxCategories;
        private System.Windows.Forms.Button buttonExportCategories;
        private System.Windows.Forms.Button buttonImportCategories;
        private System.Windows.Forms.Button buttonSetDefaultCategory;
        private System.Windows.Forms.Button buttonRemoveCategory;
        private System.Windows.Forms.Button buttonNewCategory;
        private System.Windows.Forms.ListView listViewCategories;
        private System.Windows.Forms.ColumnHeader columnHeaderForName;
        private System.Windows.Forms.ColumnHeader columnHeaderForStyles;
        private System.Windows.Forms.ColumnHeader columnHeaderForDefault;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripCategories;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRename;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveToTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveToBottomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorCategoryImportExport;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.OpenFileDialog openFileDialogImport;
    }
}