
namespace Nikse.SubtitleEdit.Forms.Assa
{
    sealed partial class Attachments
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
            this.toolStripMenuItemStorageAttach = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStorageExport = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonAttachFile = new System.Windows.Forms.Button();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonExport = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.labelInfo = new System.Windows.Forms.Label();
            this.textBoxInfo = new System.Windows.Forms.TextBox();
            this.labelImageResizedToFit = new System.Windows.Forms.Label();
            this.contextMenuStripPreview = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setPreviewTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonImport = new System.Windows.Forms.Button();
            this.labelFontsAndImages = new System.Windows.Forms.Label();
            this.contextMenuStripAttachments.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.contextMenuStripPreview.SuspendLayout();
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
            this.toolStripMenuItemStorageAttach,
            this.importToolStripMenuItem,
            this.toolStripMenuItemStorageExport});
            this.contextMenuStripAttachments.Name = "contextMenuStrip1";
            this.contextMenuStripAttachments.Size = new System.Drawing.Size(216, 236);
            this.contextMenuStripAttachments.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripAttachments_Opening);
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
            // toolStripMenuItemStorageAttach
            // 
            this.toolStripMenuItemStorageAttach.Name = "toolStripMenuItemStorageAttach";
            this.toolStripMenuItemStorageAttach.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.toolStripMenuItemStorageAttach.Size = new System.Drawing.Size(215, 22);
            this.toolStripMenuItemStorageAttach.Text = "Attach file...";
            this.toolStripMenuItemStorageAttach.Click += new System.EventHandler(this.toolStripMenuItemStorageImport_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.importToolStripMenuItem.Text = "Import...";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.attachGraphicsToolStripMenuItem_Click);
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
            // buttonAttachFile
            // 
            this.buttonAttachFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAttachFile.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAttachFile.Location = new System.Drawing.Point(11, 460);
            this.buttonAttachFile.Name = "buttonAttachFile";
            this.buttonAttachFile.Size = new System.Drawing.Size(133, 23);
            this.buttonAttachFile.TabIndex = 2;
            this.buttonAttachFile.Text = "Attach file";
            this.buttonAttachFile.UseVisualStyleBackColor = true;
            this.buttonAttachFile.Click += new System.EventHandler(this.buttonAttachFile_Click);
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxPreview.Location = new System.Drawing.Point(480, 38);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(402, 416);
            this.pictureBoxPreview.TabIndex = 10;
            this.pictureBoxPreview.TabStop = false;
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
            this.buttonExport.TabIndex = 4;
            this.buttonExport.Text = "Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(483, 19);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(47, 13);
            this.labelInfo.TabIndex = 13;
            this.labelInfo.Text = "labelInfo";
            // 
            // textBoxInfo
            // 
            this.textBoxInfo.Location = new System.Drawing.Point(537, 14);
            this.textBoxInfo.Name = "textBoxInfo";
            this.textBoxInfo.Size = new System.Drawing.Size(194, 20);
            this.textBoxInfo.TabIndex = 1;
            // 
            // labelImageResizedToFit
            // 
            this.labelImageResizedToFit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelImageResizedToFit.AutoSize = true;
            this.labelImageResizedToFit.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelImageResizedToFit.Location = new System.Drawing.Point(483, 460);
            this.labelImageResizedToFit.Name = "labelImageResizedToFit";
            this.labelImageResizedToFit.Size = new System.Drawing.Size(120, 13);
            this.labelImageResizedToFit.TabIndex = 5;
            this.labelImageResizedToFit.Text = "labelImageResizedToFit";
            // 
            // contextMenuStripPreview
            // 
            this.contextMenuStripPreview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setPreviewTextToolStripMenuItem});
            this.contextMenuStripPreview.Name = "contextMenuStripPreview";
            this.contextMenuStripPreview.Size = new System.Drawing.Size(167, 26);
            this.contextMenuStripPreview.Click += new System.EventHandler(this.contextMenuStripPreview_Click);
            // 
            // setPreviewTextToolStripMenuItem
            // 
            this.setPreviewTextToolStripMenuItem.Name = "setPreviewTextToolStripMenuItem";
            this.setPreviewTextToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.setPreviewTextToolStripMenuItem.Text = "Set preview text...";
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonImport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonImport.Location = new System.Drawing.Point(150, 460);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(133, 23);
            this.buttonImport.TabIndex = 3;
            this.buttonImport.Text = "Import";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // labelFontsAndImages
            // 
            this.labelFontsAndImages.AutoSize = true;
            this.labelFontsAndImages.Location = new System.Drawing.Point(12, 19);
            this.labelFontsAndImages.Name = "labelFontsAndImages";
            this.labelFontsAndImages.Size = new System.Drawing.Size(108, 13);
            this.labelFontsAndImages.TabIndex = 14;
            this.labelFontsAndImages.Text = "labelFontsAndImages";
            // 
            // Attachments
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(894, 495);
            this.Controls.Add(this.labelFontsAndImages);
            this.Controls.Add(this.labelImageResizedToFit);
            this.Controls.Add(this.textBoxInfo);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.pictureBoxPreview);
            this.Controls.Add(this.buttonImport);
            this.Controls.Add(this.buttonAttachFile);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listViewAttachments);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(910, 534);
            this.Name = "Attachments";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Attachments";
            this.Shown += new System.EventHandler(this.Attachments_Shown);
            this.ResizeEnd += new System.EventHandler(this.Attachments_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Attachments_KeyDown);
            this.contextMenuStripAttachments.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.contextMenuStripPreview.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewAttachments;
        private System.Windows.Forms.ColumnHeader columnHeaderFileName;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ColumnHeader columnHeaderFileSize;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonAttachFile;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
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
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageAttach;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageExport;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.TextBox textBoxInfo;
        private System.Windows.Forms.Label labelImageResizedToFit;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPreview;
        private System.Windows.Forms.ToolStripMenuItem setPreviewTextToolStripMenuItem;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.Label labelFontsAndImages;
    }
}