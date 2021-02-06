
namespace Nikse.SubtitleEdit.Forms.Styles
{
    partial class Attachments
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
            this.listViewAttachments = new System.Windows.Forms.ListView();
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripAttachments = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemStorageRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStorageRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemStorageMoveUp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStorageMoveDown = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStorageMoveTop = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStorageMoveBottom = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemStorageImport = new System.Windows.Forms.ToolStripMenuItem();
            this.attachGraphicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStorageExport = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonAttachFont = new System.Windows.Forms.Button();
            this.buttonAttachGraphics = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonExport = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.contextMenuStripAttachments.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // listViewAttachments
            // 
            this.listViewAttachments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listViewAttachments.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFileName,
            this.columnHeaderType,
            this.columnHeaderFileSize});
            this.listViewAttachments.ContextMenuStrip = this.contextMenuStripAttachments;
            this.listViewAttachments.FullRowSelect = true;
            this.listViewAttachments.HideSelection = false;
            this.listViewAttachments.Location = new System.Drawing.Point(12, 38);
            this.listViewAttachments.Name = "listViewAttachments";
            this.listViewAttachments.Size = new System.Drawing.Size(461, 416);
            this.listViewAttachments.TabIndex = 0;
            this.listViewAttachments.UseCompatibleStateImageBehavior = false;
            this.listViewAttachments.View = System.Windows.Forms.View.Details;
            this.listViewAttachments.SelectedIndexChanged += new System.EventHandler(this.listViewAttachments_SelectedIndexChanged);
            this.listViewAttachments.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewAttachments_KeyDown);
            // 
            // columnHeaderFileName
            // 
            this.columnHeaderFileName.Text = "File name";
            this.columnHeaderFileName.Width = 200;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "Type";
            this.columnHeaderType.Width = 100;
            // 
            // columnHeaderFileSize
            // 
            this.columnHeaderFileSize.Text = "File size";
            this.columnHeaderFileSize.Width = 150;
            // 
            // contextMenuStripAttachments
            // 
            this.contextMenuStripAttachments.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemStorageRemove,
            this.toolStripMenuItemStorageRemoveAll,
            this.toolStripSeparator7,
            this.toolStripMenuItemStorageMoveUp,
            this.toolStripMenuItemStorageMoveDown,
            this.toolStripMenuItemStorageMoveTop,
            this.toolStripMenuItemStorageMoveBottom,
            this.toolStripSeparator5,
            this.toolStripMenuItemStorageImport,
            this.attachGraphicsToolStripMenuItem,
            this.toolStripMenuItemStorageExport});
            this.contextMenuStripAttachments.Name = "contextMenuStrip1";
            this.contextMenuStripAttachments.Size = new System.Drawing.Size(216, 214);
            // 
            // toolStripMenuItemStorageRemove
            // 
            this.toolStripMenuItemStorageRemove.Name = "toolStripMenuItemStorageRemove";
            this.toolStripMenuItemStorageRemove.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.toolStripMenuItemStorageRemove.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemStorageRemove.Text = "Remove";
            this.toolStripMenuItemStorageRemove.Click += new System.EventHandler(this.toolStripMenuItemStorageRemove_Click);
            // 
            // toolStripMenuItemStorageRemoveAll
            // 
            this.toolStripMenuItemStorageRemoveAll.Name = "toolStripMenuItemStorageRemoveAll";
            this.toolStripMenuItemStorageRemoveAll.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemStorageRemoveAll.Text = "Remove all";
            this.toolStripMenuItemStorageRemoveAll.Click += new System.EventHandler(this.toolStripMenuItemStorageRemoveAll_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(212, 6);
            // 
            // toolStripMenuItemStorageMoveUp
            // 
            this.toolStripMenuItemStorageMoveUp.Name = "toolStripMenuItemStorageMoveUp";
            this.toolStripMenuItemStorageMoveUp.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.toolStripMenuItemStorageMoveUp.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemStorageMoveUp.Text = "Move up";
            this.toolStripMenuItemStorageMoveUp.Click += new System.EventHandler(this.toolStripMenuItemStorageMoveUp_Click);
            // 
            // toolStripMenuItemStorageMoveDown
            // 
            this.toolStripMenuItemStorageMoveDown.Name = "toolStripMenuItemStorageMoveDown";
            this.toolStripMenuItemStorageMoveDown.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.toolStripMenuItemStorageMoveDown.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemStorageMoveDown.Text = "Move down";
            this.toolStripMenuItemStorageMoveDown.Click += new System.EventHandler(this.toolStripMenuItemStorageMoveDown_Click);
            // 
            // toolStripMenuItemStorageMoveTop
            // 
            this.toolStripMenuItemStorageMoveTop.Name = "toolStripMenuItemStorageMoveTop";
            this.toolStripMenuItemStorageMoveTop.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Home)));
            this.toolStripMenuItemStorageMoveTop.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemStorageMoveTop.Text = "Move to top";
            this.toolStripMenuItemStorageMoveTop.Click += new System.EventHandler(this.toolStripMenuItemStorageMoveTop_Click);
            // 
            // toolStripMenuItemStorageMoveBottom
            // 
            this.toolStripMenuItemStorageMoveBottom.Name = "toolStripMenuItemStorageMoveBottom";
            this.toolStripMenuItemStorageMoveBottom.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.End)));
            this.toolStripMenuItemStorageMoveBottom.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemStorageMoveBottom.Text = "Move to bottom";
            this.toolStripMenuItemStorageMoveBottom.Click += new System.EventHandler(this.toolStripMenuItemStorageMoveBottom_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(212, 6);
            // 
            // toolStripMenuItemStorageImport
            // 
            this.toolStripMenuItemStorageImport.Name = "toolStripMenuItemStorageImport";
            this.toolStripMenuItemStorageImport.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.toolStripMenuItemStorageImport.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemStorageImport.Text = "Attach font...";
            this.toolStripMenuItemStorageImport.Click += new System.EventHandler(this.toolStripMenuItemStorageImport_Click);
            // 
            // attachGraphicsToolStripMenuItem
            // 
            this.attachGraphicsToolStripMenuItem.Name = "attachGraphicsToolStripMenuItem";
            this.attachGraphicsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.attachGraphicsToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.attachGraphicsToolStripMenuItem.Text = "Attach graphics...";
            this.attachGraphicsToolStripMenuItem.Click += new System.EventHandler(this.attachGraphicsToolStripMenuItem_Click);
            // 
            // toolStripMenuItemStorageExport
            // 
            this.toolStripMenuItemStorageExport.Name = "toolStripMenuItemStorageExport";
            this.toolStripMenuItemStorageExport.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.toolStripMenuItemStorageExport.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemStorageExport.Text = "Export...";
            this.toolStripMenuItemStorageExport.Click += new System.EventHandler(this.toolStripMenuItemStorageExport_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(807, 460);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(726, 460);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonAttachFont
            // 
            this.buttonAttachFont.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAttachFont.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAttachFont.Location = new System.Drawing.Point(11, 460);
            this.buttonAttachFont.Name = "buttonAttachFont";
            this.buttonAttachFont.Size = new System.Drawing.Size(133, 23);
            this.buttonAttachFont.TabIndex = 8;
            this.buttonAttachFont.Text = "Attach font";
            this.buttonAttachFont.UseVisualStyleBackColor = true;
            this.buttonAttachFont.Click += new System.EventHandler(this.buttonAttachFont_Click);
            // 
            // buttonAttachGraphics
            // 
            this.buttonAttachGraphics.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAttachGraphics.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAttachGraphics.Location = new System.Drawing.Point(150, 460);
            this.buttonAttachGraphics.Name = "buttonAttachGraphics";
            this.buttonAttachGraphics.Size = new System.Drawing.Size(133, 23);
            this.buttonAttachGraphics.TabIndex = 9;
            this.buttonAttachGraphics.Text = "Attach graphics";
            this.buttonAttachGraphics.UseVisualStyleBackColor = true;
            this.buttonAttachGraphics.Click += new System.EventHandler(this.buttonAttachGraphics_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(480, 38);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(402, 416);
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonExport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonExport.Location = new System.Drawing.Point(289, 460);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(133, 23);
            this.buttonExport.TabIndex = 12;
            this.buttonExport.Text = "Export...";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // Attachments
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(894, 495);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonAttachGraphics);
            this.Controls.Add(this.buttonAttachFont);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listViewAttachments);
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "Attachments";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Attachments";
            this.contextMenuStripAttachments.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewAttachments;
        private System.Windows.Forms.ColumnHeader columnHeaderFileName;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ColumnHeader columnHeaderFileSize;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonAttachFont;
        private System.Windows.Forms.Button buttonAttachGraphics;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAttachments;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageRemove;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageRemoveAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageMoveUp;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageMoveDown;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageMoveTop;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageMoveBottom;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageImport;
        private System.Windows.Forms.ToolStripMenuItem attachGraphicsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageExport;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}