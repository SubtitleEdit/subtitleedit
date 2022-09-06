namespace Nikse.SubtitleEdit.Forms.Assa
{
    sealed partial class ImageColorPicker
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
            this.contextMenuStripCopy = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyHexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyAssaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRgbToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBoxImage = new System.Windows.Forms.PictureBox();
            this.panelMouseOverColor = new System.Windows.Forms.Panel();
            this.labelInfo = new System.Windows.Forms.Label();
            this.contextMenuStripCopy.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStripCopy
            // 
            this.contextMenuStripCopy.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyHexToolStripMenuItem,
            this.copyAssaToolStripMenuItem,
            this.copyRgbToolStripMenuItem});
            this.contextMenuStripCopy.Name = "contextMenuStripCopy";
            this.contextMenuStripCopy.Size = new System.Drawing.Size(133, 48);
            this.contextMenuStripCopy.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripCopy_Opening);
            // 
            // copyHexToolStripMenuItem
            // 
            this.copyHexToolStripMenuItem.Name = "copyHexToolStripMenuItem";
            this.copyHexToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.copyHexToolStripMenuItem.Text = "Copy Hex";
            this.copyHexToolStripMenuItem.Click += new System.EventHandler(this.copyHexToolStripMenuItem_Click);
            // 
            // copyASSAToolStripMenuItem
            // 
            this.copyAssaToolStripMenuItem.Name = "copyASSAToolStripMenuItem";
            this.copyAssaToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.copyAssaToolStripMenuItem.Text = "Copy ASSA";
            this.copyAssaToolStripMenuItem.Click += new System.EventHandler(this.copyAssaToolStripMenuItem_Click);
            // 
            // copyRgbToolStripMenuItem
            // 
            this.copyRgbToolStripMenuItem.Name = "copyRgbToolStripMenuItem";
            this.copyRgbToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.copyRgbToolStripMenuItem.Text = "Copy RGB";
            this.copyRgbToolStripMenuItem.Click += new System.EventHandler(this.copyRgbToolStripMenuItem_Click);
            // 
            // pictureBoxImage
            // 
            this.pictureBoxImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxImage.ContextMenuStrip = this.contextMenuStripCopy;
            this.pictureBoxImage.Location = new System.Drawing.Point(-7, -2);
            this.pictureBoxImage.Name = "pictureBoxImage";
            this.pictureBoxImage.Size = new System.Drawing.Size(808, 441);
            this.pictureBoxImage.TabIndex = 1;
            this.pictureBoxImage.TabStop = false;
            this.pictureBoxImage.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxImage_MouseDoubleClick);
            this.pictureBoxImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxImage_MouseMove);
            // 
            // panelMouseOverColor
            // 
            this.panelMouseOverColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelMouseOverColor.Location = new System.Drawing.Point(5, 445);
            this.panelMouseOverColor.Name = "panelMouseOverColor";
            this.panelMouseOverColor.Size = new System.Drawing.Size(113, 15);
            this.panelMouseOverColor.TabIndex = 28;
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(125, 445);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(60, 13);
            this.labelInfo.TabIndex = 29;
            this.labelInfo.Text = "labelInfo";
            // 
            // ImageColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 466);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.panelMouseOverColor);
            this.Controls.Add(this.pictureBoxImage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImageColorPicker";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Image color picker";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImageColorPicker_KeyDown);
            this.contextMenuStripCopy.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStripCopy;
        private System.Windows.Forms.ToolStripMenuItem copyHexToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyAssaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyRgbToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBoxImage;
        private System.Windows.Forms.Panel panelMouseOverColor;
        private System.Windows.Forms.Label labelInfo;
    }
}