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
            this.numericUpDownMaxMillisecondsBetweenLines = new System.Windows.Forms.NumericUpDown();
            this.labelMaxDifferenceMS = new System.Windows.Forms.Label();
            this.checkBoxAutoBreakOn = new System.Windows.Forms.CheckBox();
            this.groupBoxLinesFound = new System.Windows.Forms.GroupBox();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SubtitleListview1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxMillisecondsBetweenLines)).BeginInit();
            this.groupBoxLinesFound.SuspendLayout();
            this.SuspendLayout();
            // 
            // numericUpDownMaxMillisecondsBetweenLines
            // 
            this.numericUpDownMaxMillisecondsBetweenLines.Location = new System.Drawing.Point(189, 12);
            this.numericUpDownMaxMillisecondsBetweenLines.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownMaxMillisecondsBetweenLines.Name = "numericUpDownMaxMillisecondsBetweenLines";
            this.numericUpDownMaxMillisecondsBetweenLines.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownMaxMillisecondsBetweenLines.TabIndex = 55;
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
            this.labelMaxDifferenceMS.TabIndex = 56;
            this.labelMaxDifferenceMS.Text = "Maximum difference in milliseconds";
            // 
            // checkBoxAutoBreakOn
            // 
            this.checkBoxAutoBreakOn.AutoSize = true;
            this.checkBoxAutoBreakOn.Location = new System.Drawing.Point(291, 15);
            this.checkBoxAutoBreakOn.Name = "checkBoxAutoBreakOn";
            this.checkBoxAutoBreakOn.Size = new System.Drawing.Size(90, 17);
            this.checkBoxAutoBreakOn.TabIndex = 54;
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
            this.groupBoxLinesFound.TabIndex = 53;
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
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.Location = new System.Drawing.Point(6, 19);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(971, 177);
            this.listViewFixes.TabIndex = 0;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
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
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(836, 501);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 51;
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
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 52;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
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
            this.SubtitleListview1.TabIndex = 50;
            this.SubtitleListview1.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview1.UseSyntaxColoring = true;
            this.SubtitleListview1.View = System.Windows.Forms.View.Details;
            // 
            // MergeTextWithSameTimeCodes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1004, 534);
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
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxMillisecondsBetweenLines)).EndInit();
            this.groupBoxLinesFound.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numericUpDownMaxMillisecondsBetweenLines;
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
    }
}