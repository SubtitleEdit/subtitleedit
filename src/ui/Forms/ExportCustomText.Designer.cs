﻿namespace Nikse.SubtitleEdit.Forms
{
    partial class ExportCustomText
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
            this.listViewTemplates = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxFormats = new System.Windows.Forms.GroupBox();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonEdit = new System.Windows.Forms.Button();
            this.buttonNew = new System.Windows.Forms.Button();
            this.textBoxPreview = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.labelEncoding = new System.Windows.Forms.Label();
            this.comboBoxEncoding = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveBottomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBoxFormats.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewTemplates
            // 
            this.listViewTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewTemplates.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewTemplates.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewTemplates.FullRowSelect = true;
            this.listViewTemplates.HideSelection = false;
            this.listViewTemplates.Location = new System.Drawing.Point(6, 23);
            this.listViewTemplates.MultiSelect = false;
            this.listViewTemplates.Name = "listViewTemplates";
            this.listViewTemplates.Size = new System.Drawing.Size(450, 310);
            this.listViewTemplates.TabIndex = 0;
            this.listViewTemplates.UseCompatibleStateImageBehavior = false;
            this.listViewTemplates.View = System.Windows.Forms.View.Details;
            this.listViewTemplates.SelectedIndexChanged += new System.EventHandler(this.listViewTemplates_SelectedIndexChanged);
            this.listViewTemplates.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewTemplates_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 120;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Text";
            this.columnHeader2.Width = 326;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.toolStripMenuItem2,
            this.editToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator2,
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem,
            this.moveTopToolStripMenuItem,
            this.moveBottomToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(216, 192);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(212, 6);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // groupBoxFormats
            // 
            this.groupBoxFormats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxFormats.Controls.Add(this.buttonDelete);
            this.groupBoxFormats.Controls.Add(this.buttonEdit);
            this.groupBoxFormats.Controls.Add(this.buttonNew);
            this.groupBoxFormats.Controls.Add(this.listViewTemplates);
            this.groupBoxFormats.Location = new System.Drawing.Point(12, 12);
            this.groupBoxFormats.Name = "groupBoxFormats";
            this.groupBoxFormats.Size = new System.Drawing.Size(462, 366);
            this.groupBoxFormats.TabIndex = 0;
            this.groupBoxFormats.TabStop = false;
            this.groupBoxFormats.Text = "Formats";
            // 
            // buttonDelete
            // 
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDelete.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDelete.Location = new System.Drawing.Point(216, 339);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(117, 23);
            this.buttonDelete.TabIndex = 2;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonEdit
            // 
            this.buttonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEdit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonEdit.Location = new System.Drawing.Point(93, 339);
            this.buttonEdit.Name = "buttonEdit";
            this.buttonEdit.Size = new System.Drawing.Size(117, 23);
            this.buttonEdit.TabIndex = 1;
            this.buttonEdit.Text = "&Edit";
            this.buttonEdit.UseVisualStyleBackColor = true;
            this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
            // 
            // buttonNew
            // 
            this.buttonNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNew.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonNew.Location = new System.Drawing.Point(339, 339);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(117, 23);
            this.buttonNew.TabIndex = 3;
            this.buttonNew.Text = "&New";
            this.buttonNew.UseVisualStyleBackColor = true;
            this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
            // 
            // textBoxPreview
            // 
            this.textBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxPreview.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxPreview.Location = new System.Drawing.Point(3, 16);
            this.textBoxPreview.Multiline = true;
            this.textBoxPreview.Name = "textBoxPreview";
            this.textBoxPreview.ReadOnly = true;
            this.textBoxPreview.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxPreview.Size = new System.Drawing.Size(445, 346);
            this.textBoxPreview.TabIndex = 0;
            this.textBoxPreview.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxPreview_KeyDown);
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.textBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(480, 13);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(451, 365);
            this.groupBoxPreview.TabIndex = 1;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // labelEncoding
            // 
            this.labelEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelEncoding.AutoSize = true;
            this.labelEncoding.Location = new System.Drawing.Point(482, 415);
            this.labelEncoding.Name = "labelEncoding";
            this.labelEncoding.Size = new System.Drawing.Size(52, 13);
            this.labelEncoding.TabIndex = 2;
            this.labelEncoding.Text = "Encoding";
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxEncoding.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxEncoding.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxEncoding.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxEncoding.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxEncoding.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxEncoding.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxEncoding.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxEncoding.DropDownHeight = 400;
            this.comboBoxEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncoding.DropDownWidth = 183;
            this.comboBoxEncoding.FormattingEnabled = true;
            this.comboBoxEncoding.Location = new System.Drawing.Point(540, 411);
            this.comboBoxEncoding.MaxLength = 32767;
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.SelectedIndex = -1;
            this.comboBoxEncoding.SelectedItem = null;
            this.comboBoxEncoding.SelectedText = "";
            this.comboBoxEncoding.Size = new System.Drawing.Size(183, 21);
            this.comboBoxEncoding.TabIndex = 3;
            this.comboBoxEncoding.UsePopupWindow = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(852, 410);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSave.Location = new System.Drawing.Point(729, 410);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(117, 23);
            this.buttonSave.TabIndex = 4;
            this.buttonSave.Text = "&Save as...";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.deleteToolStripMenuItem.Text = "Remove";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(212, 6);
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
            // ExportCustomText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(939, 444);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.labelEncoding);
            this.Controls.Add(this.comboBoxEncoding);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.groupBoxFormats);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(830, 350);
            this.Name = "ExportCustomText";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export custom text format";
            this.Shown += new System.EventHandler(this.ExportCustomText_Shown);
            this.ResizeEnd += new System.EventHandler(this.ExportCustomText_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExportCustomText_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBoxFormats.ResumeLayout(false);
            this.groupBoxPreview.ResumeLayout(false);
            this.groupBoxPreview.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewTemplates;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.GroupBox groupBoxFormats;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxPreview;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.Label labelEncoding;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxEncoding;
        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonEdit;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveBottomToolStripMenuItem;
    }
}