﻿namespace Nikse.SubtitleEdit.Forms.ShotChanges
{
    sealed partial class ImportShotChangesFromSe
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
            this.listViewShotChanges = new System.Windows.Forms.ListView();
            this.columnHeaderLastUpdated = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVideoFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderShotChanges = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(583, 308);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 40;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(502, 308);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 30;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // listViewShotChanges
            // 
            this.listViewShotChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewShotChanges.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderLastUpdated,
            this.columnHeaderVideoFileName,
            this.columnHeaderShotChanges});
            this.listViewShotChanges.FullRowSelect = true;
            this.listViewShotChanges.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewShotChanges.HideSelection = false;
            this.listViewShotChanges.Location = new System.Drawing.Point(13, 12);
            this.listViewShotChanges.Name = "listViewShotChanges";
            this.listViewShotChanges.Size = new System.Drawing.Size(645, 289);
            this.listViewShotChanges.TabIndex = 0;
            this.listViewShotChanges.UseCompatibleStateImageBehavior = false;
            this.listViewShotChanges.View = System.Windows.Forms.View.Details;
            this.listViewShotChanges.DoubleClick += new System.EventHandler(this.listViewShotChanges_DoubleClick);
            // 
            // columnHeaderLastUpdated
            // 
            this.columnHeaderLastUpdated.Width = 120;
            // 
            // columnHeaderVideoFileName
            // 
            this.columnHeaderVideoFileName.Width = 200;
            // 
            // columnHeaderShotChanges
            // 
            this.columnHeaderShotChanges.Width = 80;
            // 
            // ImportShotChangesFromSe
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 341);
            this.Controls.Add(this.listViewShotChanges);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(458, 380);
            this.Name = "ImportShotChangesFromSe";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Shot change list";
            this.Shown += new System.EventHandler(this.ShotChangesList_Shown);
            this.ResizeEnd += new System.EventHandler(this.ShotChangesList_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ShotChangesList_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ListView listViewShotChanges;
        private System.Windows.Forms.ColumnHeader columnHeaderLastUpdated;
        private System.Windows.Forms.ColumnHeader columnHeaderVideoFileName;
        private System.Windows.Forms.ColumnHeader columnHeaderShotChanges;
    }
}