using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class Compare
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
            this.labelSubtitle2 = new System.Windows.Forms.Label();
            this.labelSubtitle1 = new System.Windows.Forms.Label();
            this.buttonOpenSubtitle1 = new System.Windows.Forms.Button();
            this.buttonReloadSubtitle1 = new System.Windows.Forms.Button();
            this.buttonOpenSubtitle2 = new System.Windows.Forms.Button();
            this.buttonReloadSubtitle2 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonNextDifference = new System.Windows.Forms.Button();
            this.buttonPreviousDifference = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyTextToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxShowOnlyDifferences = new System.Windows.Forms.CheckBox();
            this.checkBoxOnlyListDifferencesInText = new System.Windows.Forms.CheckBox();
            this.checkBoxIgnoreLineBreaks = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.subtitleListView2 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.subtitleListView1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.checkBoxIgnoreFormatting = new System.Windows.Forms.CheckBox();
            this.buttonExport = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(884, 561);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(84, 23);
            this.buttonOK.TabIndex = 18;
            this.buttonOK.Text = "&Close";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // labelSubtitle2
            // 
            this.labelSubtitle2.AutoSize = true;
            this.labelSubtitle2.Location = new System.Drawing.Point(491, 36);
            this.labelSubtitle2.Name = "labelSubtitle2";
            this.labelSubtitle2.Size = new System.Drawing.Size(30, 13);
            this.labelSubtitle2.TabIndex = 5;
            this.labelSubtitle2.Text = "sub2";
            this.labelSubtitle2.MouseHover += new System.EventHandler(this.labelSubtitle2_MouseHover);
            // 
            // labelSubtitle1
            // 
            this.labelSubtitle1.AutoSize = true;
            this.labelSubtitle1.Location = new System.Drawing.Point(9, 36);
            this.labelSubtitle1.Name = "labelSubtitle1";
            this.labelSubtitle1.Size = new System.Drawing.Size(30, 13);
            this.labelSubtitle1.TabIndex = 4;
            this.labelSubtitle1.Text = "sub1";
            this.labelSubtitle1.MouseHover += new System.EventHandler(this.labelSubtitle1_MouseHover);
            // 
            // buttonOpenSubtitle1
            // 
            this.buttonOpenSubtitle1.Location = new System.Drawing.Point(9, 8);
            this.buttonOpenSubtitle1.Name = "buttonOpenSubtitle1";
            this.buttonOpenSubtitle1.Size = new System.Drawing.Size(28, 23);
            this.buttonOpenSubtitle1.TabIndex = 0;
            this.buttonOpenSubtitle1.Text = "...";
            this.buttonOpenSubtitle1.UseVisualStyleBackColor = true;
            this.buttonOpenSubtitle1.Click += new System.EventHandler(this.ButtonOpenSubtitle1Click);
            // 
            // buttonReloadSubtitle1
            // 
            this.buttonReloadSubtitle1.Enabled = false;
            this.buttonReloadSubtitle1.Location = new System.Drawing.Point(43, 8);
            this.buttonReloadSubtitle1.Name = "buttonReloadSubtitle1";
            this.buttonReloadSubtitle1.Size = new System.Drawing.Size(60, 23);
            this.buttonReloadSubtitle1.TabIndex = 1;
            this.buttonReloadSubtitle1.Text = "Reload";
            this.buttonReloadSubtitle1.UseVisualStyleBackColor = true;
            this.buttonReloadSubtitle1.Click += new System.EventHandler(this.ButtonReloadSubtitle1Click);
            // 
            // buttonOpenSubtitle2
            // 
            this.buttonOpenSubtitle2.Location = new System.Drawing.Point(491, 8);
            this.buttonOpenSubtitle2.Name = "buttonOpenSubtitle2";
            this.buttonOpenSubtitle2.Size = new System.Drawing.Size(28, 23);
            this.buttonOpenSubtitle2.TabIndex = 2;
            this.buttonOpenSubtitle2.Text = "...";
            this.buttonOpenSubtitle2.UseVisualStyleBackColor = true;
            this.buttonOpenSubtitle2.Click += new System.EventHandler(this.ButtonOpenSubtitle2Click);
            // 
            // buttonReloadSubtitle2
            // 
            this.buttonReloadSubtitle2.Enabled = false;
            this.buttonReloadSubtitle2.Location = new System.Drawing.Point(525, 8);
            this.buttonReloadSubtitle2.Name = "buttonReloadSubtitle2";
            this.buttonReloadSubtitle2.Size = new System.Drawing.Size(60, 23);
            this.buttonReloadSubtitle2.TabIndex = 3;
            this.buttonReloadSubtitle2.Text = "Reload";
            this.buttonReloadSubtitle2.UseVisualStyleBackColor = true;
            this.buttonReloadSubtitle2.Click += new System.EventHandler(this.ButtonReloadSubtitle2Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonNextDifference
            // 
            this.buttonNextDifference.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonNextDifference.Enabled = false;
            this.buttonNextDifference.Location = new System.Drawing.Point(168, 540);
            this.buttonNextDifference.Name = "buttonNextDifference";
            this.buttonNextDifference.Size = new System.Drawing.Size(156, 23);
            this.buttonNextDifference.TabIndex = 11;
            this.buttonNextDifference.Text = "&Next difference";
            this.buttonNextDifference.UseVisualStyleBackColor = true;
            this.buttonNextDifference.Click += new System.EventHandler(this.ButtonNextDifferenceClick);
            // 
            // buttonPreviousDifference
            // 
            this.buttonPreviousDifference.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPreviousDifference.Enabled = false;
            this.buttonPreviousDifference.Location = new System.Drawing.Point(8, 540);
            this.buttonPreviousDifference.Name = "buttonPreviousDifference";
            this.buttonPreviousDifference.Size = new System.Drawing.Size(156, 23);
            this.buttonPreviousDifference.TabIndex = 10;
            this.buttonPreviousDifference.Text = "&Previous difference";
            this.buttonPreviousDifference.UseVisualStyleBackColor = true;
            this.buttonPreviousDifference.Click += new System.EventHandler(this.ButtonPreviousDifferenceClick);
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(8, 572);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(37, 13);
            this.labelStatus.TabIndex = 16;
            this.labelStatus.Text = "status";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer1Tick);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.richTextBox1.Location = new System.Drawing.Point(8, 486);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(476, 43);
            this.richTextBox1.TabIndex = 8;
            this.richTextBox1.Text = "";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyTextToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(168, 26);
            // 
            // copyTextToolStripMenuItem
            // 
            this.copyTextToolStripMenuItem.Name = "copyTextToolStripMenuItem";
            this.copyTextToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.copyTextToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.copyTextToolStripMenuItem.Text = "Copy Text";
            this.copyTextToolStripMenuItem.Click += new System.EventHandler(this.copyTextToolStripMenuItem_Click);
            // 
            // richTextBox2
            // 
            this.richTextBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBox2.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBox2.ContextMenuStrip = this.contextMenuStrip2;
            this.richTextBox2.Location = new System.Drawing.Point(490, 485);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.ReadOnly = true;
            this.richTextBox2.Size = new System.Drawing.Size(478, 43);
            this.richTextBox2.TabIndex = 9;
            this.richTextBox2.Text = "";
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyTextToolStripMenuItem1});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(168, 26);
            // 
            // copyTextToolStripMenuItem1
            // 
            this.copyTextToolStripMenuItem1.Name = "copyTextToolStripMenuItem1";
            this.copyTextToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.copyTextToolStripMenuItem1.Size = new System.Drawing.Size(167, 22);
            this.copyTextToolStripMenuItem1.Text = "Copy Text";
            this.copyTextToolStripMenuItem1.Click += new System.EventHandler(this.copyTextToolStripMenuItem1_Click);
            // 
            // checkBoxShowOnlyDifferences
            // 
            this.checkBoxShowOnlyDifferences.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxShowOnlyDifferences.AutoSize = true;
            this.checkBoxShowOnlyDifferences.Location = new System.Drawing.Point(330, 534);
            this.checkBoxShowOnlyDifferences.Name = "checkBoxShowOnlyDifferences";
            this.checkBoxShowOnlyDifferences.Size = new System.Drawing.Size(132, 17);
            this.checkBoxShowOnlyDifferences.TabIndex = 12;
            this.checkBoxShowOnlyDifferences.Text = "Show only differences";
            this.checkBoxShowOnlyDifferences.UseVisualStyleBackColor = true;
            this.checkBoxShowOnlyDifferences.CheckedChanged += new System.EventHandler(this.checkBoxShowOnlyDifferences_CheckedChanged);
            // 
            // checkBoxOnlyListDifferencesInText
            // 
            this.checkBoxOnlyListDifferencesInText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxOnlyListDifferencesInText.AutoSize = true;
            this.checkBoxOnlyListDifferencesInText.Checked = true;
            this.checkBoxOnlyListDifferencesInText.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxOnlyListDifferencesInText.Location = new System.Drawing.Point(330, 552);
            this.checkBoxOnlyListDifferencesInText.Name = "checkBoxOnlyListDifferencesInText";
            this.checkBoxOnlyListDifferencesInText.Size = new System.Drawing.Size(197, 17);
            this.checkBoxOnlyListDifferencesInText.TabIndex = 13;
            this.checkBoxOnlyListDifferencesInText.Text = "Only look for differences in the text";
            this.checkBoxOnlyListDifferencesInText.UseVisualStyleBackColor = true;
            this.checkBoxOnlyListDifferencesInText.CheckedChanged += new System.EventHandler(this.checkBoxOnlyListDifferencesInText_CheckedChanged);
            // 
            // checkBoxIgnoreLineBreaks
            // 
            this.checkBoxIgnoreLineBreaks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxIgnoreLineBreaks.AutoSize = true;
            this.checkBoxIgnoreLineBreaks.Location = new System.Drawing.Point(582, 534);
            this.checkBoxIgnoreLineBreaks.Name = "checkBoxIgnoreLineBreaks";
            this.checkBoxIgnoreLineBreaks.Size = new System.Drawing.Size(112, 17);
            this.checkBoxIgnoreLineBreaks.TabIndex = 14;
            this.checkBoxIgnoreLineBreaks.Text = "Ignore line breaks";
            this.checkBoxIgnoreLineBreaks.UseVisualStyleBackColor = true;
            this.checkBoxIgnoreLineBreaks.CheckedChanged += new System.EventHandler(this.checkBoxIgnoreLineBreaks_CheckedChanged);
            // 
            // toolTip1
            // 
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip1.ToolTipTitle = "Subtitle name";
            // 
            // subtitleListView2
            // 
            this.subtitleListView2.AllowColumnReorder = true;
            this.subtitleListView2.AllowDrop = true;
            this.subtitleListView2.FirstVisibleIndex = -1;
            this.subtitleListView2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView2.FullRowSelect = true;
            this.subtitleListView2.GridLines = true;
            this.subtitleListView2.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListView2.HideSelection = false;
            this.subtitleListView2.Location = new System.Drawing.Point(490, 56);
            this.subtitleListView2.Name = "subtitleListView2";
            this.subtitleListView2.OwnerDraw = true;
            this.subtitleListView2.Size = new System.Drawing.Size(478, 422);
            this.subtitleListView2.SubtitleFontBold = false;
            this.subtitleListView2.SubtitleFontName = "Tahoma";
            this.subtitleListView2.SubtitleFontSize = 8;
            this.subtitleListView2.TabIndex = 7;
            this.subtitleListView2.UseCompatibleStateImageBehavior = false;
            this.subtitleListView2.UseSyntaxColoring = true;
            this.subtitleListView2.View = System.Windows.Forms.View.Details;
            this.subtitleListView2.SelectedIndexChanged += new System.EventHandler(this.SubtitleListView2SelectedIndexChanged);
            this.subtitleListView2.DragDrop += new System.Windows.Forms.DragEventHandler(this.subtitleListView2_DragDrop);
            this.subtitleListView2.DragEnter += new System.Windows.Forms.DragEventHandler(this.subtitleListView2_DragEnter);
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.AllowColumnReorder = true;
            this.subtitleListView1.AllowDrop = true;
            this.subtitleListView1.FirstVisibleIndex = -1;
            this.subtitleListView1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListView1.HideSelection = false;
            this.subtitleListView1.Location = new System.Drawing.Point(8, 56);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.OwnerDraw = true;
            this.subtitleListView1.Size = new System.Drawing.Size(476, 422);
            this.subtitleListView1.SubtitleFontBold = false;
            this.subtitleListView1.SubtitleFontName = "Tahoma";
            this.subtitleListView1.SubtitleFontSize = 8;
            this.subtitleListView1.TabIndex = 6;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.UseSyntaxColoring = true;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            this.subtitleListView1.SelectedIndexChanged += new System.EventHandler(this.SubtitleListView1SelectedIndexChanged);
            this.subtitleListView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.subtitleListView1_DragDrop);
            this.subtitleListView1.DragEnter += new System.Windows.Forms.DragEventHandler(this.subtitleListView1_DragEnter);
            // 
            // checkBoxIgnoreFormatting
            // 
            this.checkBoxIgnoreFormatting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxIgnoreFormatting.AutoSize = true;
            this.checkBoxIgnoreFormatting.Location = new System.Drawing.Point(582, 552);
            this.checkBoxIgnoreFormatting.Name = "checkBoxIgnoreFormatting";
            this.checkBoxIgnoreFormatting.Size = new System.Drawing.Size(111, 17);
            this.checkBoxIgnoreFormatting.TabIndex = 15;
            this.checkBoxIgnoreFormatting.Text = "Ignore formatting";
            this.checkBoxIgnoreFormatting.UseVisualStyleBackColor = true;
            this.checkBoxIgnoreFormatting.CheckedChanged += new System.EventHandler(this.checkBoxIgnoreFormatting_CheckedChanged);
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.Location = new System.Drawing.Point(774, 561);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(104, 23);
            this.buttonExport.TabIndex = 17;
            this.buttonExport.Text = "Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // Compare
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(974, 591);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.checkBoxIgnoreFormatting);
            this.Controls.Add(this.checkBoxIgnoreLineBreaks);
            this.Controls.Add(this.checkBoxOnlyListDifferencesInText);
            this.Controls.Add(this.checkBoxShowOnlyDifferences);
            this.Controls.Add(this.richTextBox2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonPreviousDifference);
            this.Controls.Add(this.buttonNextDifference);
            this.Controls.Add(this.buttonOpenSubtitle2);
            this.Controls.Add(this.buttonReloadSubtitle2);
            this.Controls.Add(this.buttonOpenSubtitle1);
            this.Controls.Add(this.buttonReloadSubtitle1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.subtitleListView2);
            this.Controls.Add(this.labelSubtitle2);
            this.Controls.Add(this.subtitleListView1);
            this.Controls.Add(this.labelSubtitle1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(800, 400);
            this.Name = "Compare";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Compare subtitles";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Compare_FormClosing);
            this.Shown += new System.EventHandler(this.Compare_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Compare_KeyDown);
            this.Resize += new System.EventHandler(this.Compare_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelSubtitle2;
        private SubtitleListView subtitleListView2;
        private SubtitleListView subtitleListView1;
        private System.Windows.Forms.Label labelSubtitle1;
        private System.Windows.Forms.Button buttonOpenSubtitle1;
        private System.Windows.Forms.Button buttonReloadSubtitle1;
        private System.Windows.Forms.Button buttonOpenSubtitle2;
        private System.Windows.Forms.Button buttonReloadSubtitle2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonNextDifference;
        private System.Windows.Forms.Button buttonPreviousDifference;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.CheckBox checkBoxShowOnlyDifferences;
        private System.Windows.Forms.CheckBox checkBoxOnlyListDifferencesInText;
        private System.Windows.Forms.CheckBox checkBoxIgnoreLineBreaks;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyTextToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem copyTextToolStripMenuItem1;
        private System.Windows.Forms.CheckBox checkBoxIgnoreFormatting;
        private System.Windows.Forms.Button buttonExport;
    }
}