namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class RestoreAutoBackup
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.listViewBackups = new System.Windows.Forms.ListView();
            this.columnHeaderDateTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderExtension = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelInfo = new System.Windows.Forms.Label();
            this.linkLabelOpenContainingFolder = new System.Windows.Forms.LinkLabel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(552, 383);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(471, 383);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // listViewBackups
            // 
            this.listViewBackups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewBackups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderDateTime,
            this.columnHeaderFileName,
            this.columnHeaderExtension,
            this.columnHeaderSize});
            this.listViewBackups.FullRowSelect = true;
            this.listViewBackups.HideSelection = false;
            this.listViewBackups.Location = new System.Drawing.Point(12, 37);
            this.listViewBackups.MultiSelect = false;
            this.listViewBackups.Name = "listViewBackups";
            this.listViewBackups.Size = new System.Drawing.Size(615, 333);
            this.listViewBackups.TabIndex = 1;
            this.listViewBackups.UseCompatibleStateImageBehavior = false;
            this.listViewBackups.View = System.Windows.Forms.View.Details;
            this.listViewBackups.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewBackups_MouseDoubleClick);
            // 
            // columnHeaderDateTime
            // 
            this.columnHeaderDateTime.Text = "Date and time";
            this.columnHeaderDateTime.Width = 130;
            // 
            // columnHeaderFileName
            // 
            this.columnHeaderFileName.Text = "File name";
            this.columnHeaderFileName.Width = 300;
            // 
            // columnHeaderExtension
            // 
            this.columnHeaderExtension.Text = "Extension";
            this.columnHeaderExtension.Width = 80;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderSize.Width = 101;
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(13, 13);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(122, 13);
            this.labelInfo.TabIndex = 0;
            this.labelInfo.Text = "Open auto save backup";
            // 
            // linkLabelOpenContainingFolder
            // 
            this.linkLabelOpenContainingFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelOpenContainingFolder.AutoSize = true;
            this.linkLabelOpenContainingFolder.Location = new System.Drawing.Point(13, 387);
            this.linkLabelOpenContainingFolder.Name = "linkLabelOpenContainingFolder";
            this.linkLabelOpenContainingFolder.Size = new System.Drawing.Size(124, 13);
            this.linkLabelOpenContainingFolder.TabIndex = 2;
            this.linkLabelOpenContainingFolder.TabStop = true;
            this.linkLabelOpenContainingFolder.Text = "Open \'Dictionaries\' folder";
            this.linkLabelOpenContainingFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenContainingFolder_LinkClicked);
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(186, 387);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "labelStatus";
            // 
            // RestoreAutoBackup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 414);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.linkLabelOpenContainingFolder);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listViewBackups);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(500, 400);
            this.Name = "RestoreAutoBackup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Restore auto-backup";
            this.Shown += new System.EventHandler(this.RestoreAutoBackup_Shown);
            this.ResizeEnd += new System.EventHandler(this.RestoreAutoBackup_ResizeEnd);
            this.SizeChanged += new System.EventHandler(this.RestoreAutoBackup_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RestoreAutoBackup_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ListView listViewBackups;
        private System.Windows.Forms.ColumnHeader columnHeaderDateTime;
        private System.Windows.Forms.ColumnHeader columnHeaderFileName;
        private System.Windows.Forms.ColumnHeader columnHeaderExtension;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.LinkLabel linkLabelOpenContainingFolder;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
    }
}