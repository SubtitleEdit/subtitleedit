namespace Nikse.SubtitleEdit.Forms
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonJoin = new System.Windows.Forms.Button();
            this.listViewParts = new System.Windows.Forms.ListView();
            this.columnHeaderLines = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStartTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderEndTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.labelTotalLines = new System.Windows.Forms.Label();
            this.buttonClear = new System.Windows.Forms.Button();
            this.ButtonRemoveVob = new System.Windows.Forms.Button();
            this.buttonAddVobFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.labelNote = new System.Windows.Forms.Label();
            this.groupBoxPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(668, 269);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 29;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonJoin
            // 
            this.buttonJoin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonJoin.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonJoin.Location = new System.Drawing.Point(587, 269);
            this.buttonJoin.Name = "buttonJoin";
            this.buttonJoin.Size = new System.Drawing.Size(75, 21);
            this.buttonJoin.TabIndex = 28;
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
            this.listViewParts.FullRowSelect = true;
            this.listViewParts.HideSelection = false;
            this.listViewParts.Location = new System.Drawing.Point(6, 19);
            this.listViewParts.Name = "listViewParts";
            this.listViewParts.Size = new System.Drawing.Size(628, 207);
            this.listViewParts.TabIndex = 101;
            this.listViewParts.UseCompatibleStateImageBehavior = false;
            this.listViewParts.View = System.Windows.Forms.View.Details;
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
            this.columnHeaderFileName.Width = 463;
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.labelTotalLines);
            this.groupBoxPreview.Controls.Add(this.buttonClear);
            this.groupBoxPreview.Controls.Add(this.ButtonRemoveVob);
            this.groupBoxPreview.Controls.Add(this.buttonAddVobFile);
            this.groupBoxPreview.Controls.Add(this.listViewParts);
            this.groupBoxPreview.Location = new System.Drawing.Point(11, 12);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(732, 251);
            this.groupBoxPreview.TabIndex = 27;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Add subtitles to join (drop also supported)";
            // 
            // labelTotalLines
            // 
            this.labelTotalLines.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotalLines.AutoSize = true;
            this.labelTotalLines.Location = new System.Drawing.Point(7, 232);
            this.labelTotalLines.Name = "labelTotalLines";
            this.labelTotalLines.Size = new System.Drawing.Size(78, 13);
            this.labelTotalLines.TabIndex = 105;
            this.labelTotalLines.Text = "labelTotalLines";
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.Location = new System.Drawing.Point(641, 73);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(74, 21);
            this.buttonClear.TabIndex = 104;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // ButtonRemoveVob
            // 
            this.ButtonRemoveVob.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonRemoveVob.Location = new System.Drawing.Point(642, 46);
            this.ButtonRemoveVob.Name = "ButtonRemoveVob";
            this.ButtonRemoveVob.Size = new System.Drawing.Size(74, 21);
            this.ButtonRemoveVob.TabIndex = 103;
            this.ButtonRemoveVob.Text = "Remove";
            this.ButtonRemoveVob.UseVisualStyleBackColor = true;
            this.ButtonRemoveVob.Click += new System.EventHandler(this.ButtonRemoveVob_Click);
            // 
            // buttonAddVobFile
            // 
            this.buttonAddVobFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddVobFile.Location = new System.Drawing.Point(642, 19);
            this.buttonAddVobFile.Name = "buttonAddVobFile";
            this.buttonAddVobFile.Size = new System.Drawing.Size(73, 21);
            this.buttonAddVobFile.TabIndex = 102;
            this.buttonAddVobFile.Text = "Add...";
            this.buttonAddVobFile.UseVisualStyleBackColor = true;
            this.buttonAddVobFile.Click += new System.EventHandler(this.ButtonAddSubtitleClick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // labelNote
            // 
            this.labelNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelNote.AutoSize = true;
            this.labelNote.ForeColor = System.Drawing.Color.Gray;
            this.labelNote.Location = new System.Drawing.Point(12, 273);
            this.labelNote.Name = "labelNote";
            this.labelNote.Size = new System.Drawing.Size(236, 13);
            this.labelNote.TabIndex = 30;
            this.labelNote.Text = "Note: Files must already have correct time codes";
            // 
            // JoinSubtitles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(755, 302);
            this.Controls.Add(this.labelNote);
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
            this.Shown += new System.EventHandler(this.JoinSubtitles_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoinSubtitles_KeyDown);
            this.Resize += new System.EventHandler(this.JoinSubtitles_Resize);
            this.groupBoxPreview.ResumeLayout(false);
            this.groupBoxPreview.PerformLayout();
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
        private System.Windows.Forms.Button ButtonRemoveVob;
        private System.Windows.Forms.Button buttonAddVobFile;
        private System.Windows.Forms.ColumnHeader columnHeaderEndTime;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label labelTotalLines;
        private System.Windows.Forms.Label labelNote;
    }
}