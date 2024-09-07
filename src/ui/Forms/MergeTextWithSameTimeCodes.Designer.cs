﻿namespace Nikse.SubtitleEdit.Forms
{
    partial class MergeTextWithSameTimeCodes
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
            this.numericUpDownMaxMillisecondsBetweenLines = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMaxDifferenceMS = new System.Windows.Forms.Label();
            this.checkBoxAutoBreakOn = new System.Windows.Forms.CheckBox();
            this.groupBoxLinesFound = new System.Windows.Forms.GroupBox();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInverseSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkBoxMakeDialog = new System.Windows.Forms.CheckBox();
            this.SubtitleListview1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.groupBoxLinesFound.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // numericUpDownMaxMillisecondsBetweenLines
            // 
            this.numericUpDownMaxMillisecondsBetweenLines.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMaxMillisecondsBetweenLines.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMaxMillisecondsBetweenLines.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMaxMillisecondsBetweenLines.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMaxMillisecondsBetweenLines.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMaxMillisecondsBetweenLines.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMaxMillisecondsBetweenLines.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMaxMillisecondsBetweenLines.DecimalPlaces = 0;
            this.numericUpDownMaxMillisecondsBetweenLines.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxMillisecondsBetweenLines.Location = new System.Drawing.Point(189, 12);
            this.numericUpDownMaxMillisecondsBetweenLines.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownMaxMillisecondsBetweenLines.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownMaxMillisecondsBetweenLines.Name = "numericUpDownMaxMillisecondsBetweenLines";
            this.numericUpDownMaxMillisecondsBetweenLines.Size = new System.Drawing.Size(64, 23);
            this.numericUpDownMaxMillisecondsBetweenLines.TabIndex = 2;
            this.numericUpDownMaxMillisecondsBetweenLines.TabStop = false;
            this.numericUpDownMaxMillisecondsBetweenLines.ThousandsSeparator = false;
            this.numericUpDownMaxMillisecondsBetweenLines.Value = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.numericUpDownMaxMillisecondsBetweenLines.ValueChanged += new System.EventHandler(this.numericUpDownMaxMillisecondsBetweenLines_ValueChanged);
            // 
            // labelMaxDifferenceMS
            // 
            this.labelMaxDifferenceMS.AutoSize = true;
            this.labelMaxDifferenceMS.Location = new System.Drawing.Point(12, 14);
            this.labelMaxDifferenceMS.Name = "labelMaxDifferenceMS";
            this.labelMaxDifferenceMS.Size = new System.Drawing.Size(171, 13);
            this.labelMaxDifferenceMS.TabIndex = 1;
            this.labelMaxDifferenceMS.Text = "Maximum difference in milliseconds";
            // 
            // checkBoxAutoBreakOn
            // 
            this.checkBoxAutoBreakOn.AutoSize = true;
            this.checkBoxAutoBreakOn.Location = new System.Drawing.Point(401, 14);
            this.checkBoxAutoBreakOn.Name = "checkBoxAutoBreakOn";
            this.checkBoxAutoBreakOn.Size = new System.Drawing.Size(90, 17);
            this.checkBoxAutoBreakOn.TabIndex = 4;
            this.checkBoxAutoBreakOn.Text = "Re-break text";
            this.checkBoxAutoBreakOn.UseVisualStyleBackColor = true;
            this.checkBoxAutoBreakOn.CheckedChanged += new System.EventHandler(this.checkBoxAutoBreakOn_CheckedChanged);
            // 
            // groupBoxLinesFound
            // 
            this.groupBoxLinesFound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLinesFound.Controls.Add(this.listViewFixes);
            this.groupBoxLinesFound.Location = new System.Drawing.Point(9, 41);
            this.groupBoxLinesFound.Name = "groupBoxLinesFound";
            this.groupBoxLinesFound.Size = new System.Drawing.Size(983, 207);
            this.groupBoxLinesFound.TabIndex = 5;
            this.groupBoxLinesFound.TabStop = false;
            this.groupBoxLinesFound.Text = "Lines that will be merged";
            // 
            // listViewFixes
            // 
            this.listViewFixes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeaderText});
            this.listViewFixes.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(6, 19);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(971, 177);
            this.listViewFixes.TabIndex = 0;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            this.listViewFixes.SelectedIndexChanged += new System.EventHandler(this.listViewFixes_SelectedIndexChanged);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Apply";
            this.columnHeader4.Width = 45;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Line#";
            this.columnHeader5.Width = 122;
            // 
            // columnHeaderText
            // 
            this.columnHeaderText.Text = "New text";
            this.columnHeaderText.Width = 500;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSelectAll,
            this.toolStripMenuItemInverseSelection});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(162, 48);
            // 
            // toolStripMenuItemSelectAll
            // 
            this.toolStripMenuItemSelectAll.Name = "toolStripMenuItemSelectAll";
            this.toolStripMenuItemSelectAll.Size = new System.Drawing.Size(161, 22);
            this.toolStripMenuItemSelectAll.Text = "Select all";
            this.toolStripMenuItemSelectAll.Click += new System.EventHandler(this.toolStripMenuItemSelectAll_Click);
            // 
            // toolStripMenuItemInverseSelection
            // 
            this.toolStripMenuItemInverseSelection.Name = "toolStripMenuItemInverseSelection";
            this.toolStripMenuItemInverseSelection.Size = new System.Drawing.Size(161, 22);
            this.toolStripMenuItemInverseSelection.Text = "Inverse selection";
            this.toolStripMenuItemInverseSelection.Click += new System.EventHandler(this.toolStripMenuItemInverseSelection_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(836, 501);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(917, 501);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // checkBoxMakeDialog
            // 
            this.checkBoxMakeDialog.AutoSize = true;
            this.checkBoxMakeDialog.Location = new System.Drawing.Point(286, 14);
            this.checkBoxMakeDialog.Name = "checkBoxMakeDialog";
            this.checkBoxMakeDialog.Size = new System.Drawing.Size(84, 17);
            this.checkBoxMakeDialog.TabIndex = 3;
            this.checkBoxMakeDialog.Text = "Make dialog";
            this.checkBoxMakeDialog.UseVisualStyleBackColor = true;
            this.checkBoxMakeDialog.CheckedChanged += new System.EventHandler(this.checkBoxMakeDialog_CheckedChanged);
            // 
            // SubtitleListview1
            // 
            this.SubtitleListview1.AllowColumnReorder = true;
            this.SubtitleListview1.AllowDrop = true;
            this.SubtitleListview1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SubtitleListview1.FirstVisibleIndex = -1;
            this.SubtitleListview1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubtitleListview1.FullRowSelect = true;
            this.SubtitleListview1.GridLines = true;
            this.SubtitleListview1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SubtitleListview1.HideSelection = false;
            this.SubtitleListview1.Location = new System.Drawing.Point(6, 254);
            this.SubtitleListview1.Name = "SubtitleListview1";
            this.SubtitleListview1.OwnerDraw = true;
            this.SubtitleListview1.Size = new System.Drawing.Size(980, 241);
            this.SubtitleListview1.SubtitleFontBold = false;
            this.SubtitleListview1.SubtitleFontName = "Tahoma";
            this.SubtitleListview1.SubtitleFontSize = 8;
            this.SubtitleListview1.TabIndex = 6;
            this.SubtitleListview1.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview1.UseSyntaxColoring = true;
            this.SubtitleListview1.View = System.Windows.Forms.View.Details;
            // 
            // MergeTextWithSameTimeCodes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1004, 534);
            this.Controls.Add(this.checkBoxMakeDialog);
            this.Controls.Add(this.numericUpDownMaxMillisecondsBetweenLines);
            this.Controls.Add(this.labelMaxDifferenceMS);
            this.Controls.Add(this.checkBoxAutoBreakOn);
            this.Controls.Add(this.groupBoxLinesFound);
            this.Controls.Add(this.SubtitleListview1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1000, 500);
            this.Name = "MergeTextWithSameTimeCodes";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MergeTextWithSameTimeCodes";
            this.Shown += new System.EventHandler(this.MergeTextWithSameTimeCodes_Shown);
            this.ResizeEnd += new System.EventHandler(this.MergeTextWithSameTimeCodes_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MergeTextWithSameTimeCodes_KeyDown);
            this.groupBoxLinesFound.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxMillisecondsBetweenLines;
        private System.Windows.Forms.Label labelMaxDifferenceMS;
        private System.Windows.Forms.CheckBox checkBoxAutoBreakOn;
        private System.Windows.Forms.GroupBox groupBoxLinesFound;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeaderText;
        private Controls.SubtitleListView SubtitleListview1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInverseSelection;
        private System.Windows.Forms.CheckBox checkBoxMakeDialog;
    }
}