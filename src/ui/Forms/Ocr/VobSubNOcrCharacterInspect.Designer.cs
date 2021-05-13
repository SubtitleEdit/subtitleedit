namespace Nikse.SubtitleEdit.Forms.Ocr
{
    partial class VobSubNOcrCharacterInspect
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
            this.groupBoxInspectItems = new System.Windows.Forms.GroupBox();
            this.labelExpandCount = new System.Windows.Forms.Label();
            this.labelImageSize = new System.Windows.Forms.Label();
            this.pictureBoxInspectItem = new System.Windows.Forms.PictureBox();
            this.listBoxInspectItems = new System.Windows.Forms.ListBox();
            this.contextMenuStripAddBetterMultiMatch = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addBetterMultiMatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxCurrentCompareImage = new System.Windows.Forms.GroupBox();
            this.buttonEditDB = new System.Windows.Forms.Button();
            this.buttonZoomOut = new System.Windows.Forms.Button();
            this.buttonZoomIn = new System.Windows.Forms.Button();
            this.labelCharacters = new System.Windows.Forms.Label();
            this.pictureBoxCharacter = new System.Windows.Forms.PictureBox();
            this.buttonAddBetterMatch = new System.Windows.Forms.Button();
            this.checkBoxItalic = new System.Windows.Forms.CheckBox();
            this.labelTextAssociatedWithImage = new System.Windows.Forms.Label();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.contextMenuStripLetters = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.labelStatus = new System.Windows.Forms.Label();
            this.groupBoxInspectItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxInspectItem)).BeginInit();
            this.contextMenuStripAddBetterMultiMatch.SuspendLayout();
            this.groupBoxCurrentCompareImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCharacter)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(658, 413);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(739, 413);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // groupBoxInspectItems
            // 
            this.groupBoxInspectItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxInspectItems.Controls.Add(this.labelExpandCount);
            this.groupBoxInspectItems.Controls.Add(this.labelImageSize);
            this.groupBoxInspectItems.Controls.Add(this.pictureBoxInspectItem);
            this.groupBoxInspectItems.Controls.Add(this.listBoxInspectItems);
            this.groupBoxInspectItems.Location = new System.Drawing.Point(12, 12);
            this.groupBoxInspectItems.Name = "groupBoxInspectItems";
            this.groupBoxInspectItems.Size = new System.Drawing.Size(425, 395);
            this.groupBoxInspectItems.TabIndex = 15;
            this.groupBoxInspectItems.TabStop = false;
            this.groupBoxInspectItems.Text = "Inspect items";
            // 
            // labelExpandCount
            // 
            this.labelExpandCount.AutoSize = true;
            this.labelExpandCount.Location = new System.Drawing.Point(252, 96);
            this.labelExpandCount.Name = "labelExpandCount";
            this.labelExpandCount.Size = new System.Drawing.Size(93, 13);
            this.labelExpandCount.TabIndex = 32;
            this.labelExpandCount.Text = "labelExpandCount";
            // 
            // labelImageSize
            // 
            this.labelImageSize.AutoSize = true;
            this.labelImageSize.Location = new System.Drawing.Point(252, 179);
            this.labelImageSize.Name = "labelImageSize";
            this.labelImageSize.Size = new System.Drawing.Size(49, 13);
            this.labelImageSize.TabIndex = 31;
            this.labelImageSize.Text = "labelSize";
            // 
            // pictureBoxInspectItem
            // 
            this.pictureBoxInspectItem.BackColor = System.Drawing.Color.Red;
            this.pictureBoxInspectItem.Location = new System.Drawing.Point(252, 114);
            this.pictureBoxInspectItem.Name = "pictureBoxInspectItem";
            this.pictureBoxInspectItem.Size = new System.Drawing.Size(52, 52);
            this.pictureBoxInspectItem.TabIndex = 23;
            this.pictureBoxInspectItem.TabStop = false;
            // 
            // listBoxInspectItems
            // 
            this.listBoxInspectItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxInspectItems.ContextMenuStrip = this.contextMenuStripAddBetterMultiMatch;
            this.listBoxInspectItems.FormattingEnabled = true;
            this.listBoxInspectItems.Location = new System.Drawing.Point(6, 19);
            this.listBoxInspectItems.Name = "listBoxInspectItems";
            this.listBoxInspectItems.Size = new System.Drawing.Size(240, 368);
            this.listBoxInspectItems.TabIndex = 12;
            this.listBoxInspectItems.SelectedIndexChanged += new System.EventHandler(this.listBoxInspectItems_SelectedIndexChanged);
            // 
            // contextMenuStripAddBetterMultiMatch
            // 
            this.contextMenuStripAddBetterMultiMatch.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addBetterMultiMatchToolStripMenuItem});
            this.contextMenuStripAddBetterMultiMatch.Name = "contextMenuStripAddBetterMultiMatch";
            this.contextMenuStripAddBetterMultiMatch.Size = new System.Drawing.Size(199, 26);
            // 
            // addBetterMultiMatchToolStripMenuItem
            // 
            this.addBetterMultiMatchToolStripMenuItem.Name = "addBetterMultiMatchToolStripMenuItem";
            this.addBetterMultiMatchToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.addBetterMultiMatchToolStripMenuItem.Text = "Add better multi match";
            this.addBetterMultiMatchToolStripMenuItem.Click += new System.EventHandler(this.addBetterMultiMatchToolStripMenuItem_Click);
            // 
            // groupBoxCurrentCompareImage
            // 
            this.groupBoxCurrentCompareImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonEditDB);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonZoomOut);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonZoomIn);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelCharacters);
            this.groupBoxCurrentCompareImage.Controls.Add(this.pictureBoxCharacter);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonAddBetterMatch);
            this.groupBoxCurrentCompareImage.Controls.Add(this.checkBoxItalic);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelTextAssociatedWithImage);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonDelete);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonUpdate);
            this.groupBoxCurrentCompareImage.Controls.Add(this.textBoxText);
            this.groupBoxCurrentCompareImage.Location = new System.Drawing.Point(443, 12);
            this.groupBoxCurrentCompareImage.Name = "groupBoxCurrentCompareImage";
            this.groupBoxCurrentCompareImage.Size = new System.Drawing.Size(371, 395);
            this.groupBoxCurrentCompareImage.TabIndex = 14;
            this.groupBoxCurrentCompareImage.TabStop = false;
            this.groupBoxCurrentCompareImage.Text = "Current compare image";
            // 
            // buttonEditDB
            // 
            this.buttonEditDB.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonEditDB.Location = new System.Drawing.Point(120, 125);
            this.buttonEditDB.Name = "buttonEditDB";
            this.buttonEditDB.Size = new System.Drawing.Size(140, 23);
            this.buttonEditDB.TabIndex = 37;
            this.buttonEditDB.Text = "Check other...";
            this.buttonEditDB.UseVisualStyleBackColor = true;
            this.buttonEditDB.Click += new System.EventHandler(this.buttonEditDB_Click);
            // 
            // buttonZoomOut
            // 
            this.buttonZoomOut.Location = new System.Drawing.Point(89, 174);
            this.buttonZoomOut.Name = "buttonZoomOut";
            this.buttonZoomOut.Size = new System.Drawing.Size(25, 23);
            this.buttonZoomOut.TabIndex = 36;
            this.buttonZoomOut.Text = "-";
            this.buttonZoomOut.UseVisualStyleBackColor = true;
            this.buttonZoomOut.Click += new System.EventHandler(this.buttonZoomOut_Click);
            // 
            // buttonZoomIn
            // 
            this.buttonZoomIn.Location = new System.Drawing.Point(120, 174);
            this.buttonZoomIn.Name = "buttonZoomIn";
            this.buttonZoomIn.Size = new System.Drawing.Size(25, 23);
            this.buttonZoomIn.TabIndex = 35;
            this.buttonZoomIn.Text = "+";
            this.buttonZoomIn.UseVisualStyleBackColor = true;
            this.buttonZoomIn.Click += new System.EventHandler(this.buttonZoomIn_Click);
            // 
            // labelCharacters
            // 
            this.labelCharacters.AutoSize = true;
            this.labelCharacters.Location = new System.Drawing.Point(19, 185);
            this.labelCharacters.Name = "labelCharacters";
            this.labelCharacters.Size = new System.Drawing.Size(64, 13);
            this.labelCharacters.TabIndex = 34;
            this.labelCharacters.Text = "Character(s)";
            // 
            // pictureBoxCharacter
            // 
            this.pictureBoxCharacter.BackColor = System.Drawing.Color.DimGray;
            this.pictureBoxCharacter.Location = new System.Drawing.Point(17, 206);
            this.pictureBoxCharacter.Name = "pictureBoxCharacter";
            this.pictureBoxCharacter.Size = new System.Drawing.Size(99, 47);
            this.pictureBoxCharacter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxCharacter.TabIndex = 33;
            this.pictureBoxCharacter.TabStop = false;
            this.pictureBoxCharacter.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxCharacter_Paint);
            // 
            // buttonAddBetterMatch
            // 
            this.buttonAddBetterMatch.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAddBetterMatch.Location = new System.Drawing.Point(120, 98);
            this.buttonAddBetterMatch.Name = "buttonAddBetterMatch";
            this.buttonAddBetterMatch.Size = new System.Drawing.Size(140, 23);
            this.buttonAddBetterMatch.TabIndex = 28;
            this.buttonAddBetterMatch.Text = "Add better match...";
            this.buttonAddBetterMatch.UseVisualStyleBackColor = true;
            this.buttonAddBetterMatch.Click += new System.EventHandler(this.buttonAddBetterMatch_Click);
            // 
            // checkBoxItalic
            // 
            this.checkBoxItalic.AutoSize = true;
            this.checkBoxItalic.Location = new System.Drawing.Point(15, 63);
            this.checkBoxItalic.Name = "checkBoxItalic";
            this.checkBoxItalic.Size = new System.Drawing.Size(58, 17);
            this.checkBoxItalic.TabIndex = 2;
            this.checkBoxItalic.Text = "Is &italic";
            this.checkBoxItalic.UseVisualStyleBackColor = true;
            this.checkBoxItalic.CheckedChanged += new System.EventHandler(this.checkBoxItalic_CheckedChanged);
            // 
            // labelTextAssociatedWithImage
            // 
            this.labelTextAssociatedWithImage.AutoSize = true;
            this.labelTextAssociatedWithImage.Location = new System.Drawing.Point(14, 19);
            this.labelTextAssociatedWithImage.Name = "labelTextAssociatedWithImage";
            this.labelTextAssociatedWithImage.Size = new System.Drawing.Size(135, 13);
            this.labelTextAssociatedWithImage.TabIndex = 0;
            this.labelTextAssociatedWithImage.Text = "Text associated with image";
            // 
            // buttonDelete
            // 
            this.buttonDelete.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDelete.Location = new System.Drawing.Point(120, 62);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(140, 23);
            this.buttonDelete.TabIndex = 4;
            this.buttonDelete.Text = "Delete ";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonUpdate.Location = new System.Drawing.Point(120, 35);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(140, 23);
            this.buttonUpdate.TabIndex = 3;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // textBoxText
            // 
            this.textBoxText.ContextMenuStrip = this.contextMenuStripLetters;
            this.textBoxText.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxText.Location = new System.Drawing.Point(14, 35);
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.Size = new System.Drawing.Size(100, 23);
            this.textBoxText.TabIndex = 1;
            // 
            // contextMenuStripLetters
            // 
            this.contextMenuStripLetters.Name = "contextMenuStripLetters";
            this.contextMenuStripLetters.Size = new System.Drawing.Size(181, 26);
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(15, 424);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 38;
            this.labelStatus.Text = "labelStatus";
            // 
            // VobSubNOcrCharacterInspect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(826, 446);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.groupBoxInspectItems);
            this.Controls.Add(this.groupBoxCurrentCompareImage);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(840, 460);
            this.Name = "VobSubNOcrCharacterInspect";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "nOCR character inspect";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VobSubNOcrCharacterInspect_KeyDown);
            this.groupBoxInspectItems.ResumeLayout(false);
            this.groupBoxInspectItems.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxInspectItem)).EndInit();
            this.contextMenuStripAddBetterMultiMatch.ResumeLayout(false);
            this.groupBoxCurrentCompareImage.ResumeLayout(false);
            this.groupBoxCurrentCompareImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCharacter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxInspectItems;
        private System.Windows.Forms.PictureBox pictureBoxInspectItem;
        private System.Windows.Forms.ListBox listBoxInspectItems;
        private System.Windows.Forms.GroupBox groupBoxCurrentCompareImage;
        private System.Windows.Forms.Button buttonAddBetterMatch;
        private System.Windows.Forms.CheckBox checkBoxItalic;
        private System.Windows.Forms.Label labelTextAssociatedWithImage;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.Button buttonZoomOut;
        private System.Windows.Forms.Button buttonZoomIn;
        private System.Windows.Forms.Label labelCharacters;
        private System.Windows.Forms.PictureBox pictureBoxCharacter;
        private System.Windows.Forms.Button buttonEditDB;
        private System.Windows.Forms.Label labelImageSize;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAddBetterMultiMatch;
        private System.Windows.Forms.ToolStripMenuItem addBetterMultiMatchToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLetters;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelExpandCount;
    }
}