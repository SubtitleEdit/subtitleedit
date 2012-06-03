namespace Nikse.SubtitleEdit.Forms
{
    partial class ApplyDurationLimits
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
            this.labelMaxDuration = new System.Windows.Forms.Label();
            this.labelMinDuration = new System.Windows.Forms.Label();
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
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMin)).BeginInit();
            this.groupBoxFixesAvailable.SuspendLayout();
            this.groupBoxUnfixable.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelMaxDuration
            // 
            this.labelMaxDuration.AutoSize = true;
            this.labelMaxDuration.Location = new System.Drawing.Point(13, 48);
            this.labelMaxDuration.Name = "labelMaxDuration";
            this.labelMaxDuration.Size = new System.Drawing.Size(133, 13);
            this.labelMaxDuration.TabIndex = 48;
            this.labelMaxDuration.Text = "Max. duration, milliseconds";
            // 
            // labelMinDuration
            // 
            this.labelMinDuration.AutoSize = true;
            this.labelMinDuration.Location = new System.Drawing.Point(12, 21);
            this.labelMinDuration.Name = "labelMinDuration";
            this.labelMinDuration.Size = new System.Drawing.Size(130, 13);
            this.labelMinDuration.TabIndex = 47;
            this.labelMinDuration.Text = "Min. duration, milliseconds";
            // 
            // numericUpDownDurationMax
            // 
            this.numericUpDownDurationMax.Location = new System.Drawing.Point(191, 46);
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
            this.numericUpDownDurationMax.TabIndex = 46;
            this.numericUpDownDurationMax.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownDurationMax.ValueChanged += new System.EventHandler(this.numericUpDownDurationMax_ValueChanged);
            // 
            // numericUpDownDurationMin
            // 
            this.numericUpDownDurationMin.Location = new System.Drawing.Point(191, 19);
            this.numericUpDownDurationMin.Maximum = new decimal(new int[] {
            2000,
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
            this.numericUpDownDurationMin.TabIndex = 45;
            this.numericUpDownDurationMin.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDurationMin.ValueChanged += new System.EventHandler(this.numericUpDownDurationMin_ValueChanged);
            // 
            // labelNote
            // 
            this.labelNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelNote.AutoSize = true;
            this.labelNote.Location = new System.Drawing.Point(19, 548);
            this.labelNote.Name = "labelNote";
            this.labelNote.Size = new System.Drawing.Size(265, 13);
            this.labelNote.TabIndex = 49;
            this.labelNote.Text = "Note: Display time will not overlap start time of next text";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(858, 544);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 51;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(777, 544);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 50;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxFixesAvailable
            // 
            this.groupBoxFixesAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFixesAvailable.Controls.Add(this.listViewFixes);
            this.groupBoxFixesAvailable.Location = new System.Drawing.Point(16, 72);
            this.groupBoxFixesAvailable.Name = "groupBoxFixesAvailable";
            this.groupBoxFixesAvailable.Size = new System.Drawing.Size(917, 308);
            this.groupBoxFixesAvailable.TabIndex = 52;
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
            this.listViewFixes.Size = new System.Drawing.Size(905, 279);
            this.listViewFixes.TabIndex = 9;
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
            this.groupBoxUnfixable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxUnfixable.Controls.Add(this.subtitleListView1);
            this.groupBoxUnfixable.Location = new System.Drawing.Point(16, 386);
            this.groupBoxUnfixable.Name = "groupBoxUnfixable";
            this.groupBoxUnfixable.Size = new System.Drawing.Size(917, 152);
            this.groupBoxUnfixable.TabIndex = 53;
            this.groupBoxUnfixable.TabStop = false;
            this.groupBoxUnfixable.Text = "Unable to fix min duration: {0}";
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.subtitleListView1.DisplayExtraFromExtra = false;
            this.subtitleListView1.FirstVisibleIndex = -1;
            this.subtitleListView1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.HideSelection = false;
            this.subtitleListView1.Location = new System.Drawing.Point(6, 19);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.OwnerDraw = true;
            this.subtitleListView1.Size = new System.Drawing.Size(905, 127);
            this.subtitleListView1.TabIndex = 111;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            // 
            // ApplyDurationLimits
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(945, 577);
            this.Controls.Add(this.groupBoxUnfixable);
            this.Controls.Add(this.groupBoxFixesAvailable);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelNote);
            this.Controls.Add(this.labelMaxDuration);
            this.Controls.Add(this.labelMinDuration);
            this.Controls.Add(this.numericUpDownDurationMax);
            this.Controls.Add(this.numericUpDownDurationMin);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ApplyDurationLimits";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Apply duration limits";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ApplyDurationLimits_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMin)).EndInit();
            this.groupBoxFixesAvailable.ResumeLayout(false);
            this.groupBoxUnfixable.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelMaxDuration;
        private System.Windows.Forms.Label labelMinDuration;
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
    }
}