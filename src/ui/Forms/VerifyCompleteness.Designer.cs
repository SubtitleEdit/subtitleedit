namespace Nikse.SubtitleEdit.Forms
{
    partial class VerifyCompleteness
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelControlSubtitleFilename = new System.Windows.Forms.Label();
            this.buttonOpenControlSubtitle = new System.Windows.Forms.Button();
            this.buttonDismissAndNext = new System.Windows.Forms.Button();
            this.buttonInsertAndNext = new System.Windows.Forms.Button();
            this.buttonReload = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.subtitleListView = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(797, 601);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelControlSubtitleFilename
            // 
            this.labelControlSubtitleFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControlSubtitleFilename.Location = new System.Drawing.Point(12, 16);
            this.labelControlSubtitleFilename.Name = "labelControlSubtitleFilename";
            this.labelControlSubtitleFilename.Size = new System.Drawing.Size(824, 15);
            this.labelControlSubtitleFilename.TabIndex = 1;
            this.labelControlSubtitleFilename.Text = "Control subtitle:";
            // 
            // buttonOpenControlSubtitle
            // 
            this.buttonOpenControlSubtitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenControlSubtitle.Location = new System.Drawing.Point(842, 12);
            this.buttonOpenControlSubtitle.Name = "buttonOpenControlSubtitle";
            this.buttonOpenControlSubtitle.Size = new System.Drawing.Size(30, 23);
            this.buttonOpenControlSubtitle.TabIndex = 2;
            this.buttonOpenControlSubtitle.Text = "...";
            this.buttonOpenControlSubtitle.UseVisualStyleBackColor = true;
            this.buttonOpenControlSubtitle.Click += new System.EventHandler(this.buttonOpenControlSubtitle_Click);
            // 
            // buttonDismissAndNext
            // 
            this.buttonDismissAndNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDismissAndNext.Enabled = false;
            this.buttonDismissAndNext.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDismissAndNext.Location = new System.Drawing.Point(432, 551);
            this.buttonDismissAndNext.Name = "buttonDismissAndNext";
            this.buttonDismissAndNext.Size = new System.Drawing.Size(217, 36);
            this.buttonDismissAndNext.TabIndex = 5;
            this.buttonDismissAndNext.Text = "Dismiss and go to next";
            this.buttonDismissAndNext.UseVisualStyleBackColor = true;
            this.buttonDismissAndNext.Click += new System.EventHandler(this.buttonDismissAndNext_Click);
            // 
            // buttonInsertAndNext
            // 
            this.buttonInsertAndNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInsertAndNext.Enabled = false;
            this.buttonInsertAndNext.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonInsertAndNext.Location = new System.Drawing.Point(655, 551);
            this.buttonInsertAndNext.Name = "buttonInsertAndNext";
            this.buttonInsertAndNext.Size = new System.Drawing.Size(217, 36);
            this.buttonInsertAndNext.TabIndex = 6;
            this.buttonInsertAndNext.Text = "Insert and go to next";
            this.buttonInsertAndNext.UseVisualStyleBackColor = true;
            this.buttonInsertAndNext.Click += new System.EventHandler(this.buttonInsertAndNext_Click);
            // 
            // buttonReload
            // 
            this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonReload.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonReload.Location = new System.Drawing.Point(12, 551);
            this.buttonReload.Name = "buttonReload";
            this.buttonReload.Size = new System.Drawing.Size(132, 36);
            this.buttonReload.TabIndex = 4;
            this.buttonReload.Text = "Reload";
            this.buttonReload.UseVisualStyleBackColor = true;
            this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
            // 
            // subtitleListView
            // 
            this.subtitleListView.AllowColumnReorder = true;
            this.subtitleListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.subtitleListView.FirstVisibleIndex = -1;
            this.subtitleListView.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView.FullRowSelect = true;
            this.subtitleListView.GridLines = true;
            this.subtitleListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListView.HideSelection = false;
            this.subtitleListView.Location = new System.Drawing.Point(13, 41);
            this.subtitleListView.MultiSelect = false;
            this.subtitleListView.Name = "subtitleListView";
            this.subtitleListView.OwnerDraw = true;
            this.subtitleListView.Size = new System.Drawing.Size(858, 504);
            this.subtitleListView.SubtitleFontBold = false;
            this.subtitleListView.SubtitleFontName = "Tahoma";
            this.subtitleListView.SubtitleFontSize = 8;
            this.subtitleListView.TabIndex = 3;
            this.subtitleListView.UseCompatibleStateImageBehavior = false;
            this.subtitleListView.UseSyntaxColoring = false;
            this.subtitleListView.View = System.Windows.Forms.View.Details;
            this.subtitleListView.SelectedIndexChanged += new System.EventHandler(this.subtitleListView_SelectedIndexChanged);
            this.subtitleListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.subtitleListView_KeyDown);
            this.subtitleListView.Resize += new System.EventHandler(this.subtitleListView_Resize);
            // 
            // VerifyCompleteness
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 636);
            this.Controls.Add(this.buttonReload);
            this.Controls.Add(this.buttonInsertAndNext);
            this.Controls.Add(this.buttonDismissAndNext);
            this.Controls.Add(this.buttonOpenControlSubtitle);
            this.Controls.Add(this.labelControlSubtitleFilename);
            this.Controls.Add(this.subtitleListView);
            this.Controls.Add(this.buttonOK);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "VerifyCompleteness";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Verify completeness against other subtitle";
            this.Shown += new System.EventHandler(this.VerifyCompleteness_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VerifyCompleteness_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private Controls.SubtitleListView subtitleListView;
        private System.Windows.Forms.Label labelControlSubtitleFilename;
        private System.Windows.Forms.Button buttonOpenControlSubtitle;
        private System.Windows.Forms.Button buttonDismissAndNext;
        private System.Windows.Forms.Button buttonInsertAndNext;
        private System.Windows.Forms.Button buttonReload;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}