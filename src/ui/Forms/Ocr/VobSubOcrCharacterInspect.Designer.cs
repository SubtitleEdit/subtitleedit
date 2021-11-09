namespace Nikse.SubtitleEdit.Forms.Ocr
{
    partial class VobSubOcrCharacterInspect
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxCurrentCompareImage = new System.Windows.Forms.GroupBox();
            this.buttonAddBetterMatch = new System.Windows.Forms.Button();
            this.checkBoxItalic = new System.Windows.Forms.CheckBox();
            this.labelDoubleSize = new System.Windows.Forms.Label();
            this.pictureBoxCompareBitmapDouble = new System.Windows.Forms.PictureBox();
            this.labelTextAssociatedWithImage = new System.Windows.Forms.Label();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.labelImageInfo = new System.Windows.Forms.Label();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.contextMenuStripLetters = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.pictureBoxCompareBitmap = new System.Windows.Forms.PictureBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.listBoxInspectItems = new System.Windows.Forms.ListBox();
            this.contextMenuStripAddBetterMultiMatch = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addBetterMultiMatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxInspectItems = new System.Windows.Forms.GroupBox();
            this.buttonDetectFont = new System.Windows.Forms.Button();
            this.labelImageSize = new System.Windows.Forms.Label();
            this.labelExpandCount = new System.Windows.Forms.Label();
            this.pictureBoxInspectItem = new System.Windows.Forms.PictureBox();
            this.labelCount = new System.Windows.Forms.Label();
            this.groupBoxCurrentCompareImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCompareBitmapDouble)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCompareBitmap)).BeginInit();
            this.contextMenuStripAddBetterMultiMatch.SuspendLayout();
            this.groupBoxInspectItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxInspectItem)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(644, 342);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // groupBoxCurrentCompareImage
            // 
            this.groupBoxCurrentCompareImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonAddBetterMatch);
            this.groupBoxCurrentCompareImage.Controls.Add(this.checkBoxItalic);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelDoubleSize);
            this.groupBoxCurrentCompareImage.Controls.Add(this.pictureBoxCompareBitmapDouble);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelTextAssociatedWithImage);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonDelete);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonUpdate);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelImageInfo);
            this.groupBoxCurrentCompareImage.Controls.Add(this.textBoxText);
            this.groupBoxCurrentCompareImage.Controls.Add(this.pictureBoxCompareBitmap);
            this.groupBoxCurrentCompareImage.Location = new System.Drawing.Point(443, 12);
            this.groupBoxCurrentCompareImage.Name = "groupBoxCurrentCompareImage";
            this.groupBoxCurrentCompareImage.Size = new System.Drawing.Size(276, 324);
            this.groupBoxCurrentCompareImage.TabIndex = 11;
            this.groupBoxCurrentCompareImage.TabStop = false;
            this.groupBoxCurrentCompareImage.Text = "Current compare image";
            // 
            // buttonAddBetterMatch
            // 
            this.buttonAddBetterMatch.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAddBetterMatch.Location = new System.Drawing.Point(120, 93);
            this.buttonAddBetterMatch.Name = "buttonAddBetterMatch";
            this.buttonAddBetterMatch.Size = new System.Drawing.Size(140, 23);
            this.buttonAddBetterMatch.TabIndex = 28;
            this.buttonAddBetterMatch.Text = "Add better match";
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
            // labelDoubleSize
            // 
            this.labelDoubleSize.AutoSize = true;
            this.labelDoubleSize.Location = new System.Drawing.Point(14, 174);
            this.labelDoubleSize.Name = "labelDoubleSize";
            this.labelDoubleSize.Size = new System.Drawing.Size(92, 13);
            this.labelDoubleSize.TabIndex = 6;
            this.labelDoubleSize.Text = "Image double size";
            // 
            // pictureBoxCompareBitmapDouble
            // 
            this.pictureBoxCompareBitmapDouble.BackColor = System.Drawing.Color.Red;
            this.pictureBoxCompareBitmapDouble.Location = new System.Drawing.Point(17, 190);
            this.pictureBoxCompareBitmapDouble.Name = "pictureBoxCompareBitmapDouble";
            this.pictureBoxCompareBitmapDouble.Size = new System.Drawing.Size(66, 73);
            this.pictureBoxCompareBitmapDouble.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxCompareBitmapDouble.TabIndex = 27;
            this.pictureBoxCompareBitmapDouble.TabStop = false;
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
            this.buttonDelete.Location = new System.Drawing.Point(120, 64);
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
            // labelImageInfo
            // 
            this.labelImageInfo.AutoSize = true;
            this.labelImageInfo.Location = new System.Drawing.Point(12, 89);
            this.labelImageInfo.Name = "labelImageInfo";
            this.labelImageInfo.Size = new System.Drawing.Size(76, 13);
            this.labelImageInfo.TabIndex = 5;
            this.labelImageInfo.Text = "labelImageInfo";
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
            // pictureBoxCompareBitmap
            // 
            this.pictureBoxCompareBitmap.BackColor = System.Drawing.Color.Red;
            this.pictureBoxCompareBitmap.Location = new System.Drawing.Point(17, 106);
            this.pictureBoxCompareBitmap.Name = "pictureBoxCompareBitmap";
            this.pictureBoxCompareBitmap.Size = new System.Drawing.Size(52, 52);
            this.pictureBoxCompareBitmap.TabIndex = 22;
            this.pictureBoxCompareBitmap.TabStop = false;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(563, 342);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // listBoxInspectItems
            // 
            this.listBoxInspectItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxInspectItems.ContextMenuStrip = this.contextMenuStripAddBetterMultiMatch;
            this.listBoxInspectItems.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxInspectItems.FormattingEnabled = true;
            this.listBoxInspectItems.Location = new System.Drawing.Point(6, 19);
            this.listBoxInspectItems.Name = "listBoxInspectItems";
            this.listBoxInspectItems.Size = new System.Drawing.Size(240, 290);
            this.listBoxInspectItems.TabIndex = 12;
            this.listBoxInspectItems.SelectedIndexChanged += new System.EventHandler(this.listBoxInspectItems_SelectedIndexChanged);
            // 
            // contextMenuStripAddBetterMultiMatch
            // 
            this.contextMenuStripAddBetterMultiMatch.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addBetterMultiMatchToolStripMenuItem});
            this.contextMenuStripAddBetterMultiMatch.Name = "contextMenuStripAddBetterMultiMatch";
            this.contextMenuStripAddBetterMultiMatch.Size = new System.Drawing.Size(199, 26);
            this.contextMenuStripAddBetterMultiMatch.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripAddBetterMultiMatch_Opening);
            // 
            // addBetterMultiMatchToolStripMenuItem
            // 
            this.addBetterMultiMatchToolStripMenuItem.Name = "addBetterMultiMatchToolStripMenuItem";
            this.addBetterMultiMatchToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.addBetterMultiMatchToolStripMenuItem.Text = "Add better multi match";
            this.addBetterMultiMatchToolStripMenuItem.Click += new System.EventHandler(this.addBetterMultiMatchToolStripMenuItem_Click);
            // 
            // groupBoxInspectItems
            // 
            this.groupBoxInspectItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxInspectItems.Controls.Add(this.buttonDetectFont);
            this.groupBoxInspectItems.Controls.Add(this.labelImageSize);
            this.groupBoxInspectItems.Controls.Add(this.labelExpandCount);
            this.groupBoxInspectItems.Controls.Add(this.pictureBoxInspectItem);
            this.groupBoxInspectItems.Controls.Add(this.listBoxInspectItems);
            this.groupBoxInspectItems.Location = new System.Drawing.Point(12, 12);
            this.groupBoxInspectItems.Name = "groupBoxInspectItems";
            this.groupBoxInspectItems.Size = new System.Drawing.Size(425, 324);
            this.groupBoxInspectItems.TabIndex = 13;
            this.groupBoxInspectItems.TabStop = false;
            this.groupBoxInspectItems.Text = "Inspect items";
            // 
            // buttonDetectFont
            // 
            this.buttonDetectFont.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDetectFont.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDetectFont.Location = new System.Drawing.Point(255, 286);
            this.buttonDetectFont.Name = "buttonDetectFont";
            this.buttonDetectFont.Size = new System.Drawing.Size(164, 23);
            this.buttonDetectFont.TabIndex = 31;
            this.buttonDetectFont.Text = "Detect font";
            this.buttonDetectFont.UseVisualStyleBackColor = true;
            this.buttonDetectFont.Click += new System.EventHandler(this.buttonDetectFont_Click);
            // 
            // labelImageSize
            // 
            this.labelImageSize.AutoSize = true;
            this.labelImageSize.Location = new System.Drawing.Point(252, 174);
            this.labelImageSize.Name = "labelImageSize";
            this.labelImageSize.Size = new System.Drawing.Size(49, 13);
            this.labelImageSize.TabIndex = 30;
            this.labelImageSize.Text = "labelSize";
            // 
            // labelExpandCount
            // 
            this.labelExpandCount.AutoSize = true;
            this.labelExpandCount.Location = new System.Drawing.Point(252, 98);
            this.labelExpandCount.Name = "labelExpandCount";
            this.labelExpandCount.Size = new System.Drawing.Size(93, 13);
            this.labelExpandCount.TabIndex = 29;
            this.labelExpandCount.Text = "labelExpandCount";
            // 
            // pictureBoxInspectItem
            // 
            this.pictureBoxInspectItem.BackColor = System.Drawing.Color.Red;
            this.pictureBoxInspectItem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxInspectItem.Location = new System.Drawing.Point(252, 114);
            this.pictureBoxInspectItem.Name = "pictureBoxInspectItem";
            this.pictureBoxInspectItem.Size = new System.Drawing.Size(52, 52);
            this.pictureBoxInspectItem.TabIndex = 23;
            this.pictureBoxInspectItem.TabStop = false;
            // 
            // labelCount
            // 
            this.labelCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelCount.AutoSize = true;
            this.labelCount.Location = new System.Drawing.Point(12, 339);
            this.labelCount.Name = "labelCount";
            this.labelCount.Size = new System.Drawing.Size(57, 13);
            this.labelCount.TabIndex = 30;
            this.labelCount.Text = "labelCount";
            // 
            // VobSubOcrCharacterInspect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(732, 379);
            this.Controls.Add(this.labelCount);
            this.Controls.Add(this.groupBoxInspectItems);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxCurrentCompareImage);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VobSubOcrCharacterInspect";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " ";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VobSubOcrCharacterInspect_KeyDown);
            this.groupBoxCurrentCompareImage.ResumeLayout(false);
            this.groupBoxCurrentCompareImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCompareBitmapDouble)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCompareBitmap)).EndInit();
            this.contextMenuStripAddBetterMultiMatch.ResumeLayout(false);
            this.groupBoxInspectItems.ResumeLayout(false);
            this.groupBoxInspectItems.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxInspectItem)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxCurrentCompareImage;
        private System.Windows.Forms.CheckBox checkBoxItalic;
        private System.Windows.Forms.Label labelDoubleSize;
        private System.Windows.Forms.PictureBox pictureBoxCompareBitmapDouble;
        private System.Windows.Forms.Label labelTextAssociatedWithImage;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Label labelImageInfo;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.PictureBox pictureBoxCompareBitmap;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ListBox listBoxInspectItems;
        private System.Windows.Forms.GroupBox groupBoxInspectItems;
        private System.Windows.Forms.PictureBox pictureBoxInspectItem;
        private System.Windows.Forms.Button buttonAddBetterMatch;
        private System.Windows.Forms.Label labelExpandCount;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAddBetterMultiMatch;
        private System.Windows.Forms.ToolStripMenuItem addBetterMultiMatchToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLetters;
        private System.Windows.Forms.Label labelImageSize;
        private System.Windows.Forms.Button buttonDetectFont;
    }
}