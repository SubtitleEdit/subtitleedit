namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ApplyDurationLimits
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
            this.numericUpDownDurationMax = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownDurationMin = new System.Windows.Forms.NumericUpDown();
            this.labelNote = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxFixesAvailable = new System.Windows.Forms.GroupBox();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxUnfixable = new System.Windows.Forms.GroupBox();
            this.subtitleListView1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.checkBoxMinDuration = new System.Windows.Forms.CheckBox();
            this.checkBoxMaxDuration = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMin)).BeginInit();
            this.groupBoxFixesAvailable.SuspendLayout();
            this.groupBoxUnfixable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // numericUpDownDurationMax
            // 
            this.numericUpDownDurationMax.Location = new System.Drawing.Point(191, 38);
            this.numericUpDownDurationMax.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownDurationMax.Minimum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.numericUpDownDurationMax.Name = "numericUpDownDurationMax";
            this.numericUpDownDurationMax.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownDurationMax.TabIndex = 4;
            this.numericUpDownDurationMax.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownDurationMax.ValueChanged += new System.EventHandler(this.numericUpDownDurationMax_ValueChanged);
            this.numericUpDownDurationMax.KeyUp += new System.Windows.Forms.KeyEventHandler(this.numericUpDownDurationMax_KeyUp);
            this.numericUpDownDurationMax.MouseUp += new System.Windows.Forms.MouseEventHandler(this.numericUpDownDurationMax_MouseUp);
            // 
            // numericUpDownDurationMin
            // 
            this.numericUpDownDurationMin.Location = new System.Drawing.Point(191, 12);
            this.numericUpDownDurationMin.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.numericUpDownDurationMin.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDurationMin.Name = "numericUpDownDurationMin";
            this.numericUpDownDurationMin.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownDurationMin.TabIndex = 2;
            this.numericUpDownDurationMin.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDurationMin.ValueChanged += new System.EventHandler(this.numericUpDownDurationMin_ValueChanged);
            this.numericUpDownDurationMin.KeyUp += new System.Windows.Forms.KeyEventHandler(this.numericUpDownDurationMin_KeyUp);
            this.numericUpDownDurationMin.MouseUp += new System.Windows.Forms.MouseEventHandler(this.numericUpDownDurationMin_MouseUp);
            // 
            // labelNote
            // 
            this.labelNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelNote.AutoSize = true;
            this.labelNote.Location = new System.Drawing.Point(19, 542);
            this.labelNote.Name = "labelNote";
            this.labelNote.Size = new System.Drawing.Size(265, 13);
            this.labelNote.TabIndex = 53;
            this.labelNote.Text = "Note: Display time will not overlap start time of next text";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(874, 538);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 110;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(793, 538);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 100;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxFixesAvailable
            // 
            this.groupBoxFixesAvailable.Controls.Add(this.listViewFixes);
            this.groupBoxFixesAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxFixesAvailable.Location = new System.Drawing.Point(0, 0);
            this.groupBoxFixesAvailable.Name = "groupBoxFixesAvailable";
            this.groupBoxFixesAvailable.Size = new System.Drawing.Size(931, 302);
            this.groupBoxFixesAvailable.TabIndex = 5;
            this.groupBoxFixesAvailable.TabStop = false;
            this.groupBoxFixesAvailable.Text = "Fixes available: {0}";
            // 
            // listViewFixes
            // 
            this.listViewFixes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader7,
            this.columnHeader8});
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(6, 23);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(919, 273);
            this.listViewFixes.TabIndex = 6;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            this.listViewFixes.SelectedIndexChanged += new System.EventHandler(this.listViewFixes_SelectedIndexChanged);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Apply";
            this.columnHeader4.Width = 50;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Line#";
            this.columnHeader5.Width = 61;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Before";
            this.columnHeader7.Width = 390;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "After";
            this.columnHeader8.Width = 390;
            // 
            // groupBoxUnfixable
            // 
            this.groupBoxUnfixable.Controls.Add(this.subtitleListView1);
            this.groupBoxUnfixable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxUnfixable.Location = new System.Drawing.Point(0, 0);
            this.groupBoxUnfixable.Name = "groupBoxUnfixable";
            this.groupBoxUnfixable.Size = new System.Drawing.Size(931, 154);
            this.groupBoxUnfixable.TabIndex = 51;
            this.groupBoxUnfixable.TabStop = false;
            this.groupBoxUnfixable.Text = "Unable to fix min duration: {0}";
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.AllowColumnReorder = true;
            this.subtitleListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.subtitleListView1.FirstVisibleIndex = -1;
            this.subtitleListView1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListView1.HideSelection = false;
            this.subtitleListView1.Location = new System.Drawing.Point(6, 19);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.OwnerDraw = true;
            this.subtitleListView1.Size = new System.Drawing.Size(919, 129);
            this.subtitleListView1.SubtitleFontBold = false;
            this.subtitleListView1.SubtitleFontName = "Tahoma";
            this.subtitleListView1.SubtitleFontSize = 8;
            this.subtitleListView1.TabIndex = 52;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.UseSyntaxColoring = true;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(16, 72);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBoxFixesAvailable);
            this.splitContainer1.Panel1MinSize = 100;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBoxUnfixable);
            this.splitContainer1.Panel2MinSize = 100;
            this.splitContainer1.Size = new System.Drawing.Size(931, 460);
            this.splitContainer1.SplitterDistance = 302;
            this.splitContainer1.TabIndex = 54;
            this.splitContainer1.TabStop = false;
            // 
            // checkBoxMinDuration
            // 
            this.checkBoxMinDuration.AutoSize = true;
            this.checkBoxMinDuration.Location = new System.Drawing.Point(16, 13);
            this.checkBoxMinDuration.Name = "checkBoxMinDuration";
            this.checkBoxMinDuration.Size = new System.Drawing.Size(149, 17);
            this.checkBoxMinDuration.TabIndex = 1;
            this.checkBoxMinDuration.Text = "Min. duration, milliseconds";
            this.checkBoxMinDuration.UseVisualStyleBackColor = true;
            this.checkBoxMinDuration.CheckedChanged += new System.EventHandler(this.checkBoxMinDuration_CheckedChanged);
            // 
            // checkBoxMaxDuration
            // 
            this.checkBoxMaxDuration.AutoSize = true;
            this.checkBoxMaxDuration.Location = new System.Drawing.Point(16, 39);
            this.checkBoxMaxDuration.Name = "checkBoxMaxDuration";
            this.checkBoxMaxDuration.Size = new System.Drawing.Size(152, 17);
            this.checkBoxMaxDuration.TabIndex = 3;
            this.checkBoxMaxDuration.Text = "Max. duration, milliseconds";
            this.checkBoxMaxDuration.UseVisualStyleBackColor = true;
            this.checkBoxMaxDuration.CheckedChanged += new System.EventHandler(this.checkBoxMaxDuration_CheckedChanged);
            // 
            // ApplyDurationLimits
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(961, 571);
            this.Controls.Add(this.checkBoxMaxDuration);
            this.Controls.Add(this.checkBoxMinDuration);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelNote);
            this.Controls.Add(this.numericUpDownDurationMax);
            this.Controls.Add(this.numericUpDownDurationMin);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(620, 440);
            this.Name = "ApplyDurationLimits";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Apply duration limits";
            this.Shown += new System.EventHandler(this.ApplyDurationLimits_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ApplyDurationLimits_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMin)).EndInit();
            this.groupBoxFixesAvailable.ResumeLayout(false);
            this.groupBoxUnfixable.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.NumericUpDown numericUpDownDurationMax;
        private System.Windows.Forms.NumericUpDown numericUpDownDurationMin;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxFixesAvailable;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.GroupBox groupBoxUnfixable;
        private Controls.SubtitleListView subtitleListView1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox checkBoxMinDuration;
        private System.Windows.Forms.CheckBox checkBoxMaxDuration;
    }
}