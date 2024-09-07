﻿namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class JoinSubtitles
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
            this.buttonJoin = new System.Windows.Forms.Button();
            this.listViewParts = new System.Windows.Forms.ListView();
            this.columnHeaderLines = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStartTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderEndTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.labelTotalLines = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonRemoveFile = new System.Windows.Forms.Button();
            this.buttonAddFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.radioButtonJoinPlain = new System.Windows.Forms.RadioButton();
            this.numericUpDownAddMs = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.radioButtonJoinAddTime = new System.Windows.Forms.RadioButton();
            this.labelAddTime = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.contextMenuStripParts = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveBottomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxPreview.SuspendLayout();
            this.contextMenuStripParts.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(694, 385);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(90, 23);
            this.buttonCancel.TabIndex = 45;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonJoin
            // 
            this.buttonJoin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonJoin.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonJoin.Location = new System.Drawing.Point(573, 385);
            this.buttonJoin.Name = "buttonJoin";
            this.buttonJoin.Size = new System.Drawing.Size(115, 23);
            this.buttonJoin.TabIndex = 40;
            this.buttonJoin.Text = "&Join";
            this.buttonJoin.UseVisualStyleBackColor = true;
            this.buttonJoin.Click += new System.EventHandler(this.buttonSplit_Click);
            // 
            // listViewParts
            // 
            this.listViewParts.AllowDrop = true;
            this.listViewParts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewParts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderLines,
            this.columnHeaderStartTime,
            this.columnHeaderEndTime,
            this.columnHeaderFileName});
            this.listViewParts.ContextMenuStrip = this.contextMenuStripParts;
            this.listViewParts.FullRowSelect = true;
            this.listViewParts.HideSelection = false;
            this.listViewParts.Location = new System.Drawing.Point(6, 19);
            this.listViewParts.Name = "listViewParts";
            this.listViewParts.Size = new System.Drawing.Size(669, 273);
            this.listViewParts.TabIndex = 101;
            this.listViewParts.UseCompatibleStateImageBehavior = false;
            this.listViewParts.View = System.Windows.Forms.View.Details;
            this.listViewParts.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewParts_ColumnClick);
            this.listViewParts.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewParts_DragDrop);
            this.listViewParts.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewParts_DragEnter);
            // 
            // columnHeaderLines
            // 
            this.columnHeaderLines.Text = "#Lines";
            this.columnHeaderLines.Width = 50;
            // 
            // columnHeaderStartTime
            // 
            this.columnHeaderStartTime.Text = "Start time";
            this.columnHeaderStartTime.Width = 75;
            // 
            // columnHeaderEndTime
            // 
            this.columnHeaderEndTime.Text = "End time";
            this.columnHeaderEndTime.Width = 75;
            // 
            // columnHeaderFileName
            // 
            this.columnHeaderFileName.Text = "File name";
            this.columnHeaderFileName.Width = 455;
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.labelTotalLines);
            this.groupBoxPreview.Controls.Add(this.buttonClear);
            this.groupBoxPreview.Controls.Add(this.buttonRemoveFile);
            this.groupBoxPreview.Controls.Add(this.buttonAddFile);
            this.groupBoxPreview.Controls.Add(this.listViewParts);
            this.groupBoxPreview.Location = new System.Drawing.Point(11, 12);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(773, 314);
            this.groupBoxPreview.TabIndex = 27;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Add subtitles to join (drop also supported)";
            // 
            // labelTotalLines
            // 
            this.labelTotalLines.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotalLines.AutoSize = true;
            this.labelTotalLines.Location = new System.Drawing.Point(7, 295);
            this.labelTotalLines.Name = "labelTotalLines";
            this.labelTotalLines.Size = new System.Drawing.Size(78, 13);
            this.labelTotalLines.TabIndex = 105;
            this.labelTotalLines.Text = "labelTotalLines";
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.Location = new System.Drawing.Point(682, 73);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(74, 23);
            this.buttonClear.TabIndex = 104;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonRemoveFile
            // 
            this.buttonRemoveFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveFile.Location = new System.Drawing.Point(683, 46);
            this.buttonRemoveFile.Name = "buttonRemoveFile";
            this.buttonRemoveFile.Size = new System.Drawing.Size(74, 23);
            this.buttonRemoveFile.TabIndex = 103;
            this.buttonRemoveFile.Text = "Remove";
            this.buttonRemoveFile.UseVisualStyleBackColor = true;
            this.buttonRemoveFile.Click += new System.EventHandler(this.ButtonRemoveSubtitle_Click);
            // 
            // buttonAddFile
            // 
            this.buttonAddFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddFile.Location = new System.Drawing.Point(683, 19);
            this.buttonAddFile.Name = "buttonAddFile";
            this.buttonAddFile.Size = new System.Drawing.Size(73, 23);
            this.buttonAddFile.TabIndex = 102;
            this.buttonAddFile.Text = "Add...";
            this.buttonAddFile.UseVisualStyleBackColor = true;
            this.buttonAddFile.Click += new System.EventHandler(this.ButtonAddSubtitleClick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // radioButtonJoinPlain
            // 
            this.radioButtonJoinPlain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButtonJoinPlain.AutoSize = true;
            this.radioButtonJoinPlain.Location = new System.Drawing.Point(14, 334);
            this.radioButtonJoinPlain.Name = "radioButtonJoinPlain";
            this.radioButtonJoinPlain.Size = new System.Drawing.Size(200, 17);
            this.radioButtonJoinPlain.TabIndex = 30;
            this.radioButtonJoinPlain.TabStop = true;
            this.radioButtonJoinPlain.Text = "Files already have correct time codes";
            this.radioButtonJoinPlain.UseVisualStyleBackColor = true;
            this.radioButtonJoinPlain.CheckedChanged += new System.EventHandler(this.RadioButtonJoinPlain_CheckedChanged);
            // 
            // numericUpDownAddMs
            // 
            this.numericUpDownAddMs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.numericUpDownAddMs.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownAddMs.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownAddMs.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownAddMs.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownAddMs.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownAddMs.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownAddMs.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownAddMs.DecimalPlaces = 0;
            this.numericUpDownAddMs.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownAddMs.Location = new System.Drawing.Point(385, 357);
            this.numericUpDownAddMs.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownAddMs.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownAddMs.Name = "numericUpDownAddMs";
            this.numericUpDownAddMs.Size = new System.Drawing.Size(60, 23);
            this.numericUpDownAddMs.TabIndex = 34;
            this.numericUpDownAddMs.TabStop = false;
            this.numericUpDownAddMs.ThousandsSeparator = false;
            this.numericUpDownAddMs.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // radioButtonJoinAddTime
            // 
            this.radioButtonJoinAddTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButtonJoinAddTime.AutoSize = true;
            this.radioButtonJoinAddTime.Location = new System.Drawing.Point(14, 357);
            this.radioButtonJoinAddTime.Name = "radioButtonJoinAddTime";
            this.radioButtonJoinAddTime.Size = new System.Drawing.Size(161, 17);
            this.radioButtonJoinAddTime.TabIndex = 32;
            this.radioButtonJoinAddTime.TabStop = true;
            this.radioButtonJoinAddTime.Text = "Add end time of previous  file";
            this.radioButtonJoinAddTime.UseVisualStyleBackColor = true;
            this.radioButtonJoinAddTime.CheckedChanged += new System.EventHandler(this.RadioButtonJoinAddTime_CheckedChanged);
            // 
            // labelAddTime
            // 
            this.labelAddTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelAddTime.AutoSize = true;
            this.labelAddTime.Location = new System.Drawing.Point(227, 359);
            this.labelAddTime.Name = "labelAddTime";
            this.labelAddTime.Size = new System.Drawing.Size(152, 13);
            this.labelAddTime.TabIndex = 34;
            this.labelAddTime.Text = "Add milliseconds after each file";
            // 
            // contextMenuStripParts
            // 
            this.contextMenuStripParts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem,
            this.moveTopToolStripMenuItem,
            this.moveBottomToolStripMenuItem});
            this.contextMenuStripParts.Name = "contextMenuStrip1";
            this.contextMenuStripParts.Size = new System.Drawing.Size(216, 92);
            this.contextMenuStripParts.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripParts_Opening);
            // 
            // moveUpToolStripMenuItem
            // 
            this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            this.moveUpToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.moveUpToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveUpToolStripMenuItem.Text = "Move up";
            this.moveUpToolStripMenuItem.Click += new System.EventHandler(this.moveUpToolStripMenuItem_Click);
            // 
            // moveDownToolStripMenuItem
            // 
            this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            this.moveDownToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.moveDownToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveDownToolStripMenuItem.Text = "Move down";
            this.moveDownToolStripMenuItem.Click += new System.EventHandler(this.moveDownToolStripMenuItem_Click);
            // 
            // moveTopToolStripMenuItem
            // 
            this.moveTopToolStripMenuItem.Name = "moveTopToolStripMenuItem";
            this.moveTopToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Home)));
            this.moveTopToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveTopToolStripMenuItem.Text = "Move to top";
            this.moveTopToolStripMenuItem.Click += new System.EventHandler(this.moveTopToolStripMenuItem_Click);
            // 
            // moveBottomToolStripMenuItem
            // 
            this.moveBottomToolStripMenuItem.Name = "moveBottomToolStripMenuItem";
            this.moveBottomToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.End)));
            this.moveBottomToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveBottomToolStripMenuItem.Text = "Move to bottom";
            this.moveBottomToolStripMenuItem.Click += new System.EventHandler(this.moveBottomToolStripMenuItem_Click);
            // 
            // JoinSubtitles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(796, 420);
            this.Controls.Add(this.labelAddTime);
            this.Controls.Add(this.radioButtonJoinAddTime);
            this.Controls.Add(this.numericUpDownAddMs);
            this.Controls.Add(this.radioButtonJoinPlain);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonJoin);
            this.Controls.Add(this.groupBoxPreview);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(640, 320);
            this.Name = "JoinSubtitles";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Join subtitles";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.JoinSubtitles_FormClosing);
            this.Shown += new System.EventHandler(this.JoinSubtitles_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoinSubtitles_KeyDown);
            this.Resize += new System.EventHandler(this.JoinSubtitles_Resize);
            this.groupBoxPreview.ResumeLayout(false);
            this.groupBoxPreview.PerformLayout();
            this.contextMenuStripParts.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonJoin;
        private System.Windows.Forms.ListView listViewParts;
        private System.Windows.Forms.ColumnHeader columnHeaderLines;
        private System.Windows.Forms.ColumnHeader columnHeaderStartTime;
        private System.Windows.Forms.ColumnHeader columnHeaderFileName;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonRemoveFile;
        private System.Windows.Forms.Button buttonAddFile;
        private System.Windows.Forms.ColumnHeader columnHeaderEndTime;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelTotalLines;
        private System.Windows.Forms.RadioButton radioButtonJoinPlain;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownAddMs;
        private System.Windows.Forms.RadioButton radioButtonJoinAddTime;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelAddTime;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripParts;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveBottomToolStripMenuItem;
    }
}