namespace Nikse.SubtitleEdit.Forms
{
    partial class PreviewVideo
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
            this.videoPlayerContainer1 = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            this.timerSubtitleOnVideo = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // videoPlayerContainer1
            // 
            this.videoPlayerContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.videoPlayerContainer1.Chapters = null;
            this.videoPlayerContainer1.CurrentPosition = 0D;
            this.videoPlayerContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoPlayerContainer1.FontSizeFactor = 1F;
            this.videoPlayerContainer1.LastParagraph = null;
            this.videoPlayerContainer1.Location = new System.Drawing.Point(0, 0);
            this.videoPlayerContainer1.Name = "videoPlayerContainer1";
            this.videoPlayerContainer1.ShowFullscreenButton = true;
            this.videoPlayerContainer1.ShowMuteButton = true;
            this.videoPlayerContainer1.ShowStopButton = true;
            this.videoPlayerContainer1.Size = new System.Drawing.Size(800, 450);
            this.videoPlayerContainer1.SubtitleText = "";
            this.videoPlayerContainer1.TabIndex = 17;
            this.videoPlayerContainer1.TextRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.videoPlayerContainer1.UsingFrontCenterAudioChannelOnly = false;
            this.videoPlayerContainer1.VideoHeight = 0;
            this.videoPlayerContainer1.VideoPlayer = null;
            this.videoPlayerContainer1.VideoWidth = 0;
            this.videoPlayerContainer1.Volume = 0D;
            // 
            // timerSubtitleOnVideo
            // 
            this.timerSubtitleOnVideo.Interval = 25;
            this.timerSubtitleOnVideo.Tick += new System.EventHandler(this.timerSubtitleOnVideo_Tick);
            // 
            // PreviewVideo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.videoPlayerContainer1);
            this.KeyPreview = true;
            this.Name = "PreviewVideo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PreviewVideo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PreviewVideo_FormClosing);
            this.Shown += new System.EventHandler(this.PreviewVideo_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PreviewVideo_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.VideoPlayerContainer videoPlayerContainer1;
        private System.Windows.Forms.Timer timerSubtitleOnVideo;
    }
}