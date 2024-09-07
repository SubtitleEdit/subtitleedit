namespace Nikse.SubtitleEdit.Forms.Tts
{
    sealed partial class ReviewAudioClips
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
            this.listViewAudioClips = new System.Windows.Forms.ListView();
            this.columnHeaderInclude = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVoice = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderCps = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAdjustSpeed = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportListAsCsvToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.labelParagraphInfo = new System.Windows.Forms.Label();
            this.checkBoxContinuePlay = new System.Windows.Forms.CheckBox();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonEdit = new System.Windows.Forms.Button();
            this.contextMenuStripListView.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewAudioClips
            // 
            this.listViewAudioClips.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewAudioClips.CheckBoxes = true;
            this.listViewAudioClips.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderInclude,
            this.columnHeaderNo,
            this.columnHeaderVoice,
            this.columnHeaderCps,
            this.columnHeaderAdjustSpeed,
            this.columnHeaderText});
            this.listViewAudioClips.ContextMenuStrip = this.contextMenuStripListView;
            this.listViewAudioClips.FullRowSelect = true;
            this.listViewAudioClips.HideSelection = false;
            this.listViewAudioClips.Location = new System.Drawing.Point(12, 52);
            this.listViewAudioClips.MultiSelect = false;
            this.listViewAudioClips.Name = "listViewAudioClips";
            this.listViewAudioClips.Size = new System.Drawing.Size(684, 319);
            this.listViewAudioClips.TabIndex = 0;
            this.listViewAudioClips.UseCompatibleStateImageBehavior = false;
            this.listViewAudioClips.View = System.Windows.Forms.View.Details;
            this.listViewAudioClips.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listViewAudioClips.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView1_KeyDown);
            // 
            // columnHeaderInclude
            // 
            this.columnHeaderInclude.Text = "Include";
            // 
            // columnHeaderNo
            // 
            this.columnHeaderNo.Text = "#";
            this.columnHeaderNo.Width = 50;
            // 
            // columnHeaderVoice
            // 
            this.columnHeaderVoice.Text = "Voice";
            this.columnHeaderVoice.Width = 120;
            // 
            // columnHeaderCps
            // 
            this.columnHeaderCps.Text = "CPS";
            // 
            // columnHeaderAdjustSpeed
            // 
            this.columnHeaderAdjustSpeed.Text = "Speed";
            // 
            // columnHeaderText
            // 
            this.columnHeaderText.Text = "Text";
            this.columnHeaderText.Width = 460;
            // 
            // contextMenuStripListView
            // 
            this.contextMenuStripListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportListAsCsvToolStripMenuItem});
            this.contextMenuStripListView.Name = "contextMenuStripListView";
            this.contextMenuStripListView.Size = new System.Drawing.Size(170, 26);
            // 
            // exportListAsCsvToolStripMenuItem
            // 
            this.exportListAsCsvToolStripMenuItem.Name = "exportListAsCsvToolStripMenuItem";
            this.exportListAsCsvToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.exportListAsCsvToolStripMenuItem.Text = "Export list as csv...";
            this.exportListAsCsvToolStripMenuItem.Click += new System.EventHandler(this.exportListAsCsvToolStripMenuItem_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(795, 413);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(87, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(12, 33);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(219, 13);
            this.labelInfo.TabIndex = 8;
            this.labelInfo.Text = "Review and un-include unwanted audio clips";
            // 
            // buttonPlay
            // 
            this.buttonPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPlay.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonPlay.Location = new System.Drawing.Point(702, 294);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(180, 23);
            this.buttonPlay.TabIndex = 9;
            this.buttonPlay.Text = "Play";
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // labelParagraphInfo
            // 
            this.labelParagraphInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelParagraphInfo.AutoSize = true;
            this.labelParagraphInfo.Location = new System.Drawing.Point(12, 383);
            this.labelParagraphInfo.Name = "labelParagraphInfo";
            this.labelParagraphInfo.Size = new System.Drawing.Size(76, 13);
            this.labelParagraphInfo.TabIndex = 10;
            this.labelParagraphInfo.Text = "Paragraph info";
            // 
            // checkBoxContinuePlay
            // 
            this.checkBoxContinuePlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxContinuePlay.AutoSize = true;
            this.checkBoxContinuePlay.Checked = true;
            this.checkBoxContinuePlay.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxContinuePlay.Location = new System.Drawing.Point(717, 354);
            this.checkBoxContinuePlay.Name = "checkBoxContinuePlay";
            this.checkBoxContinuePlay.Size = new System.Drawing.Size(92, 17);
            this.checkBoxContinuePlay.TabIndex = 11;
            this.checkBoxContinuePlay.Text = "Auto continue";
            this.checkBoxContinuePlay.UseVisualStyleBackColor = true;
            // 
            // buttonStop
            // 
            this.buttonStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStop.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonStop.Location = new System.Drawing.Point(703, 324);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(179, 23);
            this.buttonStop.TabIndex = 12;
            this.buttonStop.Text = "Stop ";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonEdit
            // 
            this.buttonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEdit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonEdit.Location = new System.Drawing.Point(703, 246);
            this.buttonEdit.Name = "buttonEdit";
            this.buttonEdit.Size = new System.Drawing.Size(180, 23);
            this.buttonEdit.TabIndex = 13;
            this.buttonEdit.Text = "Edit";
            this.buttonEdit.UseVisualStyleBackColor = true;
            this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
            // 
            // ReviewAudioClips
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(894, 448);
            this.Controls.Add(this.buttonEdit);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.checkBoxContinuePlay);
            this.Controls.Add(this.labelParagraphInfo);
            this.Controls.Add(this.buttonPlay);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listViewAudioClips);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(800, 450);
            this.Name = "ReviewAudioClips";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Review audio clips";
            this.Load += new System.EventHandler(this.VoicePreviewList_Load);
            this.Shown += new System.EventHandler(this.VoicePreviewList_Shown);
            this.ResizeEnd += new System.EventHandler(this.VoicePreviewList_ResizeEnd);
            this.Resize += new System.EventHandler(this.VoicePreviewList_Resize);
            this.contextMenuStripListView.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewAudioClips;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.ColumnHeader columnHeaderInclude;
        private System.Windows.Forms.ColumnHeader columnHeaderNo;
        private System.Windows.Forms.ColumnHeader columnHeaderVoice;
        private System.Windows.Forms.ColumnHeader columnHeaderCps;
        private System.Windows.Forms.ColumnHeader columnHeaderText;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.Label labelParagraphInfo;
        private System.Windows.Forms.CheckBox checkBoxContinuePlay;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonEdit;
        private System.Windows.Forms.ColumnHeader columnHeaderAdjustSpeed;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListView;
        private System.Windows.Forms.ToolStripMenuItem exportListAsCsvToolStripMenuItem;
    }
}