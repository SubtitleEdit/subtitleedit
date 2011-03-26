namespace Nikse.SubtitleEdit.Forms
{
    partial class TranscriptImporter
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
            this.textBoxListViewText = new System.Windows.Forms.TextBox();
            this.labelText = new System.Windows.Forms.Label();
            this.buttonInsert = new System.Windows.Forms.Button();
            this.labelTranslateTip = new System.Windows.Forms.Label();
            this.buttonStartThreeSecondsBack = new System.Windows.Forms.Button();
            this.buttonStartHalfASecondBack = new System.Windows.Forms.Button();
            this.buttonPlayPause = new System.Windows.Forms.Button();
            this.SubtitleListview1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.SuspendLayout();
            // 
            // textBoxListViewText
            // 
            this.textBoxListViewText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxListViewText.Enabled = false;
            this.textBoxListViewText.HideSelection = false;
            this.textBoxListViewText.Location = new System.Drawing.Point(12, 294);
            this.textBoxListViewText.Multiline = true;
            this.textBoxListViewText.Name = "textBoxListViewText";
            this.textBoxListViewText.Size = new System.Drawing.Size(373, 56);
            this.textBoxListViewText.TabIndex = 7;
            // 
            // labelText
            // 
            this.labelText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelText.AutoSize = true;
            this.labelText.Location = new System.Drawing.Point(12, 278);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(28, 13);
            this.labelText.TabIndex = 8;
            this.labelText.Text = "Text";
            // 
            // buttonInsert
            // 
            this.buttonInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonInsert.Location = new System.Drawing.Point(257, 362);
            this.buttonInsert.Name = "buttonInsert";
            this.buttonInsert.Size = new System.Drawing.Size(189, 21);
            this.buttonInsert.TabIndex = 9;
            this.buttonInsert.Text = "Insert text (end time=video pos)";
            this.buttonInsert.UseVisualStyleBackColor = true;
            this.buttonInsert.Click += new System.EventHandler(this.buttonInsert_Click);
            // 
            // labelTranslateTip
            // 
            this.labelTranslateTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTranslateTip.AutoSize = true;
            this.labelTranslateTip.ForeColor = System.Drawing.Color.Gray;
            this.labelTranslateTip.Location = new System.Drawing.Point(452, 370);
            this.labelTranslateTip.Name = "labelTranslateTip";
            this.labelTranslateTip.Size = new System.Drawing.Size(161, 13);
            this.labelTranslateTip.TabIndex = 10;
            this.labelTranslateTip.Text = "Tip: Shortcut for insert is [Space]";
            // 
            // buttonStartThreeSecondsBack
            // 
            this.buttonStartThreeSecondsBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonStartThreeSecondsBack.Location = new System.Drawing.Point(15, 362);
            this.buttonStartThreeSecondsBack.Name = "buttonStartThreeSecondsBack";
            this.buttonStartThreeSecondsBack.Size = new System.Drawing.Size(60, 21);
            this.buttonStartThreeSecondsBack.TabIndex = 11;
            this.buttonStartThreeSecondsBack.Text = "< 3 secs";
            this.buttonStartThreeSecondsBack.UseVisualStyleBackColor = true;
            this.buttonStartThreeSecondsBack.Click += new System.EventHandler(this.buttonStartThreeSecondsBack_Click);
            // 
            // buttonStartHalfASecondBack
            // 
            this.buttonStartHalfASecondBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonStartHalfASecondBack.Location = new System.Drawing.Point(81, 362);
            this.buttonStartHalfASecondBack.Name = "buttonStartHalfASecondBack";
            this.buttonStartHalfASecondBack.Size = new System.Drawing.Size(60, 21);
            this.buttonStartHalfASecondBack.TabIndex = 12;
            this.buttonStartHalfASecondBack.Text = "< ½ sec";
            this.buttonStartHalfASecondBack.UseVisualStyleBackColor = true;
            this.buttonStartHalfASecondBack.Click += new System.EventHandler(this.buttonStartHalfASecondBack_Click);
            // 
            // buttonPlayPause
            // 
            this.buttonPlayPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPlayPause.Location = new System.Drawing.Point(147, 362);
            this.buttonPlayPause.Name = "buttonPlayPause";
            this.buttonPlayPause.Size = new System.Drawing.Size(104, 21);
            this.buttonPlayPause.TabIndex = 13;
            this.buttonPlayPause.Text = "Play/pause";
            this.buttonPlayPause.UseVisualStyleBackColor = true;
            this.buttonPlayPause.Click += new System.EventHandler(this.buttonPlayPause_Click);
            // 
            // SubtitleListview1
            // 
            this.SubtitleListview1.AllowDrop = true;
            this.SubtitleListview1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SubtitleListview1.FirstVisibleIndex = -1;
            this.SubtitleListview1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubtitleListview1.FullRowSelect = true;
            this.SubtitleListview1.GridLines = true;
            this.SubtitleListview1.HideSelection = false;
            this.SubtitleListview1.Location = new System.Drawing.Point(12, 12);
            this.SubtitleListview1.Name = "SubtitleListview1";
            this.SubtitleListview1.Size = new System.Drawing.Size(637, 258);
            this.SubtitleListview1.TabIndex = 6;
            this.SubtitleListview1.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview1.View = System.Windows.Forms.View.Details;
            // 
            // TranscriptImporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(661, 392);
            this.Controls.Add(this.buttonPlayPause);
            this.Controls.Add(this.buttonStartThreeSecondsBack);
            this.Controls.Add(this.buttonStartHalfASecondBack);
            this.Controls.Add(this.labelTranslateTip);
            this.Controls.Add(this.buttonInsert);
            this.Controls.Add(this.labelText);
            this.Controls.Add(this.textBoxListViewText);
            this.Controls.Add(this.SubtitleListview1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(533, 376);
            this.Name = "TranscriptImporter";
            this.Text = "Transcript importer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxListViewText;
        private Controls.SubtitleListView SubtitleListview1;
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.Button buttonInsert;
        private System.Windows.Forms.Label labelTranslateTip;
        private System.Windows.Forms.Button buttonStartThreeSecondsBack;
        private System.Windows.Forms.Button buttonStartHalfASecondBack;
        private System.Windows.Forms.Button buttonPlayPause;
    }
}