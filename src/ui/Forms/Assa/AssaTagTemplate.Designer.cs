
namespace Nikse.SubtitleEdit.Forms.Assa
{
    partial class AssaTagTemplate
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
            this.columnHeaderTag = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderHint = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewTemplates = new System.Windows.Forms.ListView();
            this.contextMenuStripFile = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveBottomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.newToolStripMenuItemNew = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonRemoveAll = new System.Windows.Forms.Button();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.textBoxTag = new System.Windows.Forms.TextBox();
            this.labelTag = new System.Windows.Forms.Label();
            this.groupBoxTemplateItem = new System.Windows.Forms.GroupBox();
            this.textBoxHint = new System.Windows.Forms.TextBox();
            this.labelHint = new System.Windows.Forms.Label();
            this.contextMenuStripFile.SuspendLayout();
            this.groupBoxTemplateItem.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(757, 484);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 214;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(838, 484);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 215;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // columnHeaderTag
            // 
            this.columnHeaderTag.Text = "Tag";
            this.columnHeaderTag.Width = 250;
            // 
            // columnHeaderHint
            // 
            this.columnHeaderHint.Text = "Hint";
            this.columnHeaderHint.Width = 250;
            // 
            // listViewTemplates
            // 
            this.listViewTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewTemplates.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderTag,
            this.columnHeaderHint});
            this.listViewTemplates.ContextMenuStrip = this.contextMenuStripFile;
            this.listViewTemplates.FullRowSelect = true;
            this.listViewTemplates.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewTemplates.HideSelection = false;
            this.listViewTemplates.Location = new System.Drawing.Point(12, 12);
            this.listViewTemplates.MultiSelect = false;
            this.listViewTemplates.Name = "listViewTemplates";
            this.listViewTemplates.Size = new System.Drawing.Size(535, 432);
            this.listViewTemplates.TabIndex = 206;
            this.listViewTemplates.UseCompatibleStateImageBehavior = false;
            this.listViewTemplates.View = System.Windows.Forms.View.Details;
            this.listViewTemplates.SelectedIndexChanged += new System.EventHandler(this.listViewTemplates_SelectedIndexChanged);
            // 
            // contextMenuStripFile
            // 
            this.contextMenuStripFile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.toolStripMenuItemRemoveAll,
            this.toolStripSeparator8,
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem,
            this.moveTopToolStripMenuItem,
            this.moveBottomToolStripMenuItem,
            this.toolStripSeparator3,
            this.newToolStripMenuItemNew,
            this.copyToolStripMenuItemCopy});
            this.contextMenuStripFile.Name = "contextMenuStrip1";
            this.contextMenuStripFile.Size = new System.Drawing.Size(216, 192);
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
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(212, 6);
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
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemove.Location = new System.Drawing.Point(367, 450);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(82, 23);
            this.buttonRemove.TabIndex = 210;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAdd.Location = new System.Drawing.Point(279, 450);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(82, 23);
            this.buttonAdd.TabIndex = 208;
            this.buttonAdd.Text = "New";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonRemoveAll
            // 
            this.buttonRemoveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemoveAll.Location = new System.Drawing.Point(455, 450);
            this.buttonRemoveAll.Name = "buttonRemoveAll";
            this.buttonRemoveAll.Size = new System.Drawing.Size(92, 23);
            this.buttonRemoveAll.TabIndex = 211;
            this.buttonRemoveAll.Text = "Remove all";
            this.buttonRemoveAll.UseVisualStyleBackColor = true;
            this.buttonRemoveAll.Click += new System.EventHandler(this.buttonRemoveAll_Click);
            // 
            // buttonCopy
            // 
            this.buttonCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCopy.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCopy.Location = new System.Drawing.Point(191, 450);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(82, 23);
            this.buttonCopy.TabIndex = 207;
            this.buttonCopy.Text = "Copy";
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // textBoxTag
            // 
            this.textBoxTag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTag.Location = new System.Drawing.Point(82, 15);
            this.textBoxTag.Name = "textBoxTag";
            this.textBoxTag.Size = new System.Drawing.Size(265, 20);
            this.textBoxTag.TabIndex = 212;
            this.textBoxTag.TextChanged += new System.EventHandler(this.textBoxTag_TextChanged);
            // 
            // labelTag
            // 
            this.labelTag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTag.AutoSize = true;
            this.labelTag.Location = new System.Drawing.Point(10, 18);
            this.labelTag.Name = "labelTag";
            this.labelTag.Size = new System.Drawing.Size(26, 13);
            this.labelTag.TabIndex = 209;
            this.labelTag.Text = "Tag";
            // 
            // groupBoxTemplateItem
            // 
            this.groupBoxTemplateItem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTemplateItem.Controls.Add(this.textBoxHint);
            this.groupBoxTemplateItem.Controls.Add(this.labelHint);
            this.groupBoxTemplateItem.Controls.Add(this.textBoxTag);
            this.groupBoxTemplateItem.Controls.Add(this.labelTag);
            this.groupBoxTemplateItem.Location = new System.Drawing.Point(553, 12);
            this.groupBoxTemplateItem.Name = "groupBoxTemplateItem";
            this.groupBoxTemplateItem.Size = new System.Drawing.Size(360, 432);
            this.groupBoxTemplateItem.TabIndex = 213;
            this.groupBoxTemplateItem.TabStop = false;
            // 
            // textBoxHint
            // 
            this.textBoxHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHint.Location = new System.Drawing.Point(82, 41);
            this.textBoxHint.Name = "textBoxHint";
            this.textBoxHint.Size = new System.Drawing.Size(265, 20);
            this.textBoxHint.TabIndex = 214;
            this.textBoxHint.TextChanged += new System.EventHandler(this.textBoxHint_TextChanged);
            // 
            // labelHint
            // 
            this.labelHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelHint.AutoSize = true;
            this.labelHint.Location = new System.Drawing.Point(10, 44);
            this.labelHint.Name = "labelHint";
            this.labelHint.Size = new System.Drawing.Size(26, 13);
            this.labelHint.TabIndex = 213;
            this.labelHint.Text = "Hint";
            // 
            // AssaTagTemplate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(925, 519);
            this.Controls.Add(this.groupBoxTemplateItem);
            this.Controls.Add(this.buttonCopy);
            this.Controls.Add(this.buttonRemoveAll);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.listViewTemplates);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "AssaTagTemplate";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AssaTagTemplate";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AssaTagTemplate_KeyDown);
            this.contextMenuStripFile.ResumeLayout(false);
            this.groupBoxTemplateItem.ResumeLayout(false);
            this.groupBoxTemplateItem.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ColumnHeader columnHeaderTag;
        private System.Windows.Forms.ColumnHeader columnHeaderHint;
        private System.Windows.Forms.ListView listViewTemplates;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonRemoveAll;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.TextBox textBoxTag;
        private System.Windows.Forms.Label labelTag;
        private System.Windows.Forms.GroupBox groupBoxTemplateItem;
        private System.Windows.Forms.TextBox textBoxHint;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveBottomToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItemNew;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItemCopy;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFile;
    }
}