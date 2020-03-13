namespace Nikse.SubtitleEdit.Forms
{
    partial class SetSyncPoint
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
            Nikse.SubtitleEdit.Core.TimeCode timeCode1 = new Nikse.SubtitleEdit.Core.TimeCode();
            this.groupBoxSyncPointTimeCode = new System.Windows.Forms.GroupBox();
            this.timeUpDownLine = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.subtitleListView1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSetSyncPoint = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonOpenMovie = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelVideoFileName = new System.Windows.Forms.Label();
            this.buttonHalfASecondBack = new System.Windows.Forms.Button();
            this.buttonThreeSecondsBack = new System.Windows.Forms.Button();
            this.buttonThreeSecondsAhead = new System.Windows.Forms.Button();
            this.buttonHalfASecondAhead = new System.Windows.Forms.Button();
            this.buttonVerify = new System.Windows.Forms.Button();
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.videoPlayerContainer1 = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            this.buttonFindTextEnd = new System.Windows.Forms.Button();
            this.groupBoxSyncPointTimeCode.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxSyncPointTimeCode
            // 
            this.groupBoxSyncPointTimeCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxSyncPointTimeCode.Controls.Add(this.timeUpDownLine);
            this.groupBoxSyncPointTimeCode.Location = new System.Drawing.Point(12, 12);
            this.groupBoxSyncPointTimeCode.Name = "groupBoxSyncPointTimeCode";
            this.groupBoxSyncPointTimeCode.Size = new System.Drawing.Size(172, 224);
            this.groupBoxSyncPointTimeCode.TabIndex = 1;
            this.groupBoxSyncPointTimeCode.TabStop = false;
            this.groupBoxSyncPointTimeCode.Text = "Sync point time code";
            // 
            // timeUpDownLine
            // 
            this.timeUpDownLine.AutoSize = true;
            this.timeUpDownLine.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownLine.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.timeUpDownLine.Location = new System.Drawing.Point(18, 19);
            this.timeUpDownLine.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownLine.Name = "timeUpDownLine";
            this.timeUpDownLine.Size = new System.Drawing.Size(111, 27);
            this.timeUpDownLine.TabIndex = 1;
            timeCode1.Hours = 0;
            timeCode1.Milliseconds = 0;
            timeCode1.Minutes = 0;
            timeCode1.Seconds = 0;
            timeCode1.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode1.TotalMilliseconds = 0D;
            timeCode1.TotalSeconds = 0D;
            this.timeUpDownLine.TimeCode = timeCode1;
            this.timeUpDownLine.UseVideoOffset = false;
            this.timeUpDownLine.KeyDown += new System.Windows.Forms.KeyEventHandler(this.timeUpDownLine_KeyDown);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.subtitleListView1);
            this.groupBox2.Location = new System.Drawing.Point(190, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(605, 223);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
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
            this.subtitleListView1.Location = new System.Drawing.Point(6, 18);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.OwnerDraw = true;
            this.subtitleListView1.Size = new System.Drawing.Size(593, 199);
            this.subtitleListView1.SubtitleFontBold = false;
            this.subtitleListView1.SubtitleFontName = "Tahoma";
            this.subtitleListView1.SubtitleFontSize = 8;
            this.subtitleListView1.TabIndex = 11;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.UseSyntaxColoring = true;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            this.subtitleListView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.SubtitleListView1MouseDoubleClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(713, 584);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 31;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSetSyncPoint
            // 
            this.buttonSetSyncPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSetSyncPoint.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSetSyncPoint.Location = new System.Drawing.Point(584, 584);
            this.buttonSetSyncPoint.Name = "buttonSetSyncPoint";
            this.buttonSetSyncPoint.Size = new System.Drawing.Size(123, 23);
            this.buttonSetSyncPoint.TabIndex = 30;
            this.buttonSetSyncPoint.Text = "&Set sync point";
            this.buttonSetSyncPoint.UseVisualStyleBackColor = true;
            this.buttonSetSyncPoint.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonOpenMovie
            // 
            this.buttonOpenMovie.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenMovie.Location = new System.Drawing.Point(682, 241);
            this.buttonOpenMovie.Name = "buttonOpenMovie";
            this.buttonOpenMovie.Size = new System.Drawing.Size(109, 23);
            this.buttonOpenMovie.TabIndex = 12;
            this.buttonOpenMovie.Text = "Open movie...";
            this.buttonOpenMovie.UseVisualStyleBackColor = true;
            this.buttonOpenMovie.Click += new System.EventHandler(this.buttonOpenMovie_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // labelVideoFileName
            // 
            this.labelVideoFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelVideoFileName.AutoSize = true;
            this.labelVideoFileName.Location = new System.Drawing.Point(12, 250);
            this.labelVideoFileName.Name = "labelVideoFileName";
            this.labelVideoFileName.Size = new System.Drawing.Size(98, 13);
            this.labelVideoFileName.TabIndex = 21;
            this.labelVideoFileName.Text = "labelVideoFileName";
            // 
            // buttonHalfASecondBack
            // 
            this.buttonHalfASecondBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonHalfASecondBack.Location = new System.Drawing.Point(12, 560);
            this.buttonHalfASecondBack.Name = "buttonHalfASecondBack";
            this.buttonHalfASecondBack.Size = new System.Drawing.Size(77, 23);
            this.buttonHalfASecondBack.TabIndex = 20;
            this.buttonHalfASecondBack.Text = "&½ second <";
            this.buttonHalfASecondBack.UseVisualStyleBackColor = true;
            this.buttonHalfASecondBack.Click += new System.EventHandler(this.buttonStartHalfASecondBack_Click);
            // 
            // buttonThreeSecondsBack
            // 
            this.buttonThreeSecondsBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonThreeSecondsBack.Location = new System.Drawing.Point(95, 560);
            this.buttonThreeSecondsBack.Name = "buttonThreeSecondsBack";
            this.buttonThreeSecondsBack.Size = new System.Drawing.Size(77, 23);
            this.buttonThreeSecondsBack.TabIndex = 21;
            this.buttonThreeSecondsBack.Text = "&3 seconds <";
            this.buttonThreeSecondsBack.UseVisualStyleBackColor = true;
            this.buttonThreeSecondsBack.Click += new System.EventHandler(this.buttonStartThreeSecondsBack_Click);
            // 
            // buttonThreeSecondsAhead
            // 
            this.buttonThreeSecondsAhead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonThreeSecondsAhead.Location = new System.Drawing.Point(303, 560);
            this.buttonThreeSecondsAhead.Name = "buttonThreeSecondsAhead";
            this.buttonThreeSecondsAhead.Size = new System.Drawing.Size(77, 23);
            this.buttonThreeSecondsAhead.TabIndex = 23;
            this.buttonThreeSecondsAhead.Text = "3 seconds >";
            this.buttonThreeSecondsAhead.UseVisualStyleBackColor = true;
            this.buttonThreeSecondsAhead.Click += new System.EventHandler(this.buttonStartThreeSecondsAhead_Click);
            // 
            // buttonHalfASecondAhead
            // 
            this.buttonHalfASecondAhead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonHalfASecondAhead.Location = new System.Drawing.Point(386, 560);
            this.buttonHalfASecondAhead.Name = "buttonHalfASecondAhead";
            this.buttonHalfASecondAhead.Size = new System.Drawing.Size(77, 23);
            this.buttonHalfASecondAhead.TabIndex = 24;
            this.buttonHalfASecondAhead.Text = "½ second >";
            this.buttonHalfASecondAhead.UseVisualStyleBackColor = true;
            this.buttonHalfASecondAhead.Click += new System.EventHandler(this.buttonStartHalfASecondAhead_Click);
            // 
            // buttonVerify
            // 
            this.buttonVerify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonVerify.Location = new System.Drawing.Point(178, 560);
            this.buttonVerify.Name = "buttonVerify";
            this.buttonVerify.Size = new System.Drawing.Size(119, 23);
            this.buttonVerify.TabIndex = 22;
            this.buttonVerify.Text = "Play 2 s and back";
            this.buttonVerify.UseVisualStyleBackColor = true;
            this.buttonVerify.Click += new System.EventHandler(this.buttonStartVerify_Click);
            // 
            // labelSubtitle
            // 
            this.labelSubtitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSubtitle.BackColor = System.Drawing.Color.Black;
            this.labelSubtitle.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSubtitle.ForeColor = System.Drawing.Color.White;
            this.labelSubtitle.Location = new System.Drawing.Point(12, 529);
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Size = new System.Drawing.Size(779, 28);
            this.labelSubtitle.TabIndex = 27;
            this.labelSubtitle.Text = "labelSubtitle";
            this.labelSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // videoPlayerContainer1
            // 
            this.videoPlayerContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.videoPlayerContainer1.BackColor = System.Drawing.Color.Black;
            this.videoPlayerContainer1.CurrentPosition = 0D;
            this.videoPlayerContainer1.FontSizeFactor = 1F;
            this.videoPlayerContainer1.LastParagraph = null;
            this.videoPlayerContainer1.Location = new System.Drawing.Point(12, 267);
            this.videoPlayerContainer1.Name = "videoPlayerContainer1";
            this.videoPlayerContainer1.ShowFullscreenButton = true;
            this.videoPlayerContainer1.ShowMuteButton = true;
            this.videoPlayerContainer1.ShowStopButton = true;
            this.videoPlayerContainer1.Size = new System.Drawing.Size(779, 263);
            this.videoPlayerContainer1.SmpteMode = false;
            this.videoPlayerContainer1.SubtitleText = "";
            this.videoPlayerContainer1.TabIndex = 13;
            this.videoPlayerContainer1.TextRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.videoPlayerContainer1.VideoHeight = 0;
            this.videoPlayerContainer1.VideoPlayer = null;
            this.videoPlayerContainer1.VideoWidth = 0;
            this.videoPlayerContainer1.Volume = 0D;
            // 
            // buttonFindTextEnd
            // 
            this.buttonFindTextEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonFindTextEnd.Location = new System.Drawing.Point(469, 560);
            this.buttonFindTextEnd.Name = "buttonFindTextEnd";
            this.buttonFindTextEnd.Size = new System.Drawing.Size(106, 23);
            this.buttonFindTextEnd.TabIndex = 25;
            this.buttonFindTextEnd.Text = "Find text...";
            this.buttonFindTextEnd.UseVisualStyleBackColor = true;
            this.buttonFindTextEnd.Click += new System.EventHandler(this.ButtonFindTextEndClick);
            // 
            // SetSyncPoint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 609);
            this.Controls.Add(this.buttonFindTextEnd);
            this.Controls.Add(this.labelSubtitle);
            this.Controls.Add(this.buttonVerify);
            this.Controls.Add(this.buttonHalfASecondBack);
            this.Controls.Add(this.buttonThreeSecondsBack);
            this.Controls.Add(this.buttonThreeSecondsAhead);
            this.Controls.Add(this.buttonHalfASecondAhead);
            this.Controls.Add(this.labelVideoFileName);
            this.Controls.Add(this.buttonOpenMovie);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSetSyncPoint);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBoxSyncPointTimeCode);
            this.Controls.Add(this.videoPlayerContainer1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(700, 600);
            this.Name = "SetSyncPoint";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set sync point";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GetTime_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GetTime_FormClosed);
            this.Load += new System.EventHandler(this.GetTimeLoad);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GetTime_KeyDown);
            this.groupBoxSyncPointTimeCode.ResumeLayout(false);
            this.groupBoxSyncPointTimeCode.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.VideoPlayerContainer videoPlayerContainer1;
        private Controls.TimeUpDown timeUpDownLine;
        private System.Windows.Forms.GroupBox groupBoxSyncPointTimeCode;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSetSyncPoint;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonOpenMovie;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labelVideoFileName;
        private System.Windows.Forms.Button buttonHalfASecondBack;
        private System.Windows.Forms.Button buttonThreeSecondsBack;
        private System.Windows.Forms.Button buttonThreeSecondsAhead;
        private System.Windows.Forms.Button buttonHalfASecondAhead;
        private System.Windows.Forms.Button buttonVerify;
        private System.Windows.Forms.Label labelSubtitle;
        private Controls.SubtitleListView subtitleListView1;
        private System.Windows.Forms.Button buttonFindTextEnd;
    }
}