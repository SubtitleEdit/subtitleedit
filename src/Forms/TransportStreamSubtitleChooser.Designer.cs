namespace Nikse.SubtitleEdit.Forms
{
    partial class TransportStreamSubtitleChooser
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
            this.labelChoose = new System.Windows.Forms.Label();
            this.listBoxTracks = new System.Windows.Forms.ListBox();
            this.contextMenuStripListview = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemExport = new System.Windows.Forms.ToolStripMenuItem();
            this.bDNXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bluraySupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vobSubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dOSTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAllImagesWithHtmlIndexViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSubtitleAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.listBoxSubtitles = new System.Windows.Forms.ListBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.textBoxTeletext = new System.Windows.Forms.TextBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.contextMenuStripListview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelChoose
            // 
            this.labelChoose.AutoSize = true;
            this.labelChoose.Location = new System.Drawing.Point(9, 15);
            this.labelChoose.Name = "labelChoose";
            this.labelChoose.Size = new System.Drawing.Size(220, 13);
            this.labelChoose.TabIndex = 52;
            this.labelChoose.Text = "More than one subtitle found - please choose";
            // 
            // listBoxTracks
            // 
            this.listBoxTracks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxTracks.ContextMenuStrip = this.contextMenuStripListview;
            this.listBoxTracks.FormattingEnabled = true;
            this.listBoxTracks.Location = new System.Drawing.Point(13, 31);
            this.listBoxTracks.Name = "listBoxTracks";
            this.listBoxTracks.Size = new System.Drawing.Size(589, 134);
            this.listBoxTracks.TabIndex = 51;
            this.listBoxTracks.SelectedIndexChanged += new System.EventHandler(this.listBoxTracks_SelectedIndexChanged);
            // 
            // contextMenuStripListview
            // 
            this.contextMenuStripListview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemExport,
            this.saveAllImagesWithHtmlIndexViewToolStripMenuItem,
            this.saveSubtitleAsToolStripMenuItem});
            this.contextMenuStripListview.Name = "contextMenuStripListview";
            this.contextMenuStripListview.Size = new System.Drawing.Size(284, 70);
            this.contextMenuStripListview.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripListview_Opening);
            // 
            // toolStripMenuItemExport
            // 
            this.toolStripMenuItemExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bDNXMLToolStripMenuItem,
            this.bluraySupToolStripMenuItem,
            this.vobSubToolStripMenuItem,
            this.dOSTToolStripMenuItem});
            this.toolStripMenuItemExport.Name = "toolStripMenuItemExport";
            this.toolStripMenuItemExport.Size = new System.Drawing.Size(283, 22);
            this.toolStripMenuItemExport.Text = "Export all images as...";
            // 
            // bDNXMLToolStripMenuItem
            // 
            this.bDNXMLToolStripMenuItem.Name = "bDNXMLToolStripMenuItem";
            this.bDNXMLToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.bDNXMLToolStripMenuItem.Text = "BDN XML...";
            this.bDNXMLToolStripMenuItem.Click += new System.EventHandler(this.BDNXMLToolStripMenuItem_Click);
            // 
            // bluraySupToolStripMenuItem
            // 
            this.bluraySupToolStripMenuItem.Name = "bluraySupToolStripMenuItem";
            this.bluraySupToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.bluraySupToolStripMenuItem.Text = "Blu-ray sup...";
            this.bluraySupToolStripMenuItem.Click += new System.EventHandler(this.BluraySupToolStripMenuItem_Click);
            // 
            // vobSubToolStripMenuItem
            // 
            this.vobSubToolStripMenuItem.Name = "vobSubToolStripMenuItem";
            this.vobSubToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.vobSubToolStripMenuItem.Text = "VobSub...";
            this.vobSubToolStripMenuItem.Click += new System.EventHandler(this.VobSubToolStripMenuItem_Click);
            // 
            // dOSTToolStripMenuItem
            // 
            this.dOSTToolStripMenuItem.Name = "dOSTToolStripMenuItem";
            this.dOSTToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.dOSTToolStripMenuItem.Text = "DOST...";
            this.dOSTToolStripMenuItem.Click += new System.EventHandler(this.DOSTToolStripMenuItem_Click);
            // 
            // saveAllImagesWithHtmlIndexViewToolStripMenuItem
            // 
            this.saveAllImagesWithHtmlIndexViewToolStripMenuItem.Name = "saveAllImagesWithHtmlIndexViewToolStripMenuItem";
            this.saveAllImagesWithHtmlIndexViewToolStripMenuItem.Size = new System.Drawing.Size(283, 22);
            this.saveAllImagesWithHtmlIndexViewToolStripMenuItem.Text = "Save all images with HTML index view...";
            this.saveAllImagesWithHtmlIndexViewToolStripMenuItem.Click += new System.EventHandler(this.SaveAllImagesWithHtmlIndexViewToolStripMenuItem_Click);
            // 
            // saveSubtitleAsToolStripMenuItem
            // 
            this.saveSubtitleAsToolStripMenuItem.Name = "saveSubtitleAsToolStripMenuItem";
            this.saveSubtitleAsToolStripMenuItem.Size = new System.Drawing.Size(283, 22);
            this.saveSubtitleAsToolStripMenuItem.Text = "Save subtitle as...";
            this.saveSubtitleAsToolStripMenuItem.Click += new System.EventHandler(this.saveSubtitleAsToolStripMenuItem_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.BackColor = System.Drawing.SystemColors.Control;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(526, 450);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 60;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(445, 450);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 54;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // listBoxSubtitles
            // 
            this.listBoxSubtitles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxSubtitles.FormattingEnabled = true;
            this.listBoxSubtitles.Location = new System.Drawing.Point(12, 171);
            this.listBoxSubtitles.Name = "listBoxSubtitles";
            this.listBoxSubtitles.Size = new System.Drawing.Size(589, 147);
            this.listBoxSubtitles.TabIndex = 53;
            this.listBoxSubtitles.SelectedIndexChanged += new System.EventHandler(this.listBoxSubtitles_SelectedIndexChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(13, 324);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(588, 116);
            this.pictureBox1.TabIndex = 54;
            this.pictureBox1.TabStop = false;
            // 
            // textBoxTeletext
            // 
            this.textBoxTeletext.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTeletext.Location = new System.Drawing.Point(12, 167);
            this.textBoxTeletext.Multiline = true;
            this.textBoxTeletext.Name = "textBoxTeletext";
            this.textBoxTeletext.ReadOnly = true;
            this.textBoxTeletext.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxTeletext.Size = new System.Drawing.Size(589, 273);
            this.textBoxTeletext.TabIndex = 61;
            // 
            // TransportStreamSubtitleChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(613, 483);
            this.Controls.Add(this.textBoxTeletext);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.listBoxSubtitles);
            this.Controls.Add(this.labelChoose);
            this.Controls.Add(this.listBoxTracks);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 510);
            this.Name = "TransportStreamSubtitleChooser";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Transport stream subtitle chooser";
            this.Shown += new System.EventHandler(this.TransportStreamSubtitleChooser_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TransportStreamSubtitleChooser_KeyDown);
            this.contextMenuStripListview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelChoose;
        private System.Windows.Forms.ListBox listBoxTracks;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ListBox listBoxSubtitles;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListview;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExport;
        private System.Windows.Forms.ToolStripMenuItem bDNXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bluraySupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem vobSubToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dOSTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAllImagesWithHtmlIndexViewToolStripMenuItem;
        private System.Windows.Forms.TextBox textBoxTeletext;
        private System.Windows.Forms.ToolStripMenuItem saveSubtitleAsToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}