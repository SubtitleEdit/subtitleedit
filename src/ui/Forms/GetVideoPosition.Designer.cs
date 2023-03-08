using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;

namespace Nikse.SubtitleEdit.Forms
{
    partial class GetVideoPosition
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetVideoPosition));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelVideoFileName = new System.Windows.Forms.Label();
            this.buttonHalfASecondBack = new System.Windows.Forms.Button();
            this.buttonThreeSecondsBack = new System.Windows.Forms.Button();
            this.buttonThreeSecondsAhead = new System.Windows.Forms.Button();
            this.buttonHalfASecondAhead = new System.Windows.Forms.Button();
            this.buttonVerify = new System.Windows.Forms.Button();
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.videoPlayerContainer1 = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(750, 431);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(86, 23);
            this.buttonCancel.TabIndex = 31;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(658, 431);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(86, 23);
            this.buttonOK.TabIndex = 30;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
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
            this.labelVideoFileName.Location = new System.Drawing.Point(12, -134);
            this.labelVideoFileName.Name = "labelVideoFileName";
            this.labelVideoFileName.Size = new System.Drawing.Size(98, 13);
            this.labelVideoFileName.TabIndex = 21;
            this.labelVideoFileName.Text = "labelVideoFileName";
            // 
            // buttonHalfASecondBack
            // 
            this.buttonHalfASecondBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonHalfASecondBack.Location = new System.Drawing.Point(12, 417);
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
            this.buttonThreeSecondsBack.Location = new System.Drawing.Point(95, 417);
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
            this.buttonThreeSecondsAhead.Location = new System.Drawing.Point(303, 417);
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
            this.buttonHalfASecondAhead.Location = new System.Drawing.Point(386, 417);
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
            this.buttonVerify.Location = new System.Drawing.Point(178, 417);
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
            this.labelSubtitle.Location = new System.Drawing.Point(12, 386);
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Size = new System.Drawing.Size(827, 28);
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
            this.videoPlayerContainer1.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.videoPlayerContainer1.FontSizeFactor = 1F;
            this.videoPlayerContainer1.LastParagraph = null;
            this.videoPlayerContainer1.Location = new System.Drawing.Point(12, -118);
            this.videoPlayerContainer1.Name = "videoPlayerContainer1";
            this.videoPlayerContainer1.ShowFullscreenButton = true;
            this.videoPlayerContainer1.ShowMuteButton = true;
            this.videoPlayerContainer1.ShowStopButton = true;
            this.videoPlayerContainer1.Size = new System.Drawing.Size(827, 505);
            this.videoPlayerContainer1.SubtitleText = "";
            this.videoPlayerContainer1.TabIndex = 13;
            this.videoPlayerContainer1.TextRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.videoPlayerContainer1.UsingFrontCenterAudioChannelOnly = false;
            this.videoPlayerContainer1.VideoHeight = 0;
            this.videoPlayerContainer1.VideoPlayer = null;
            this.videoPlayerContainer1.VideoWidth = 0;
            this.videoPlayerContainer1.Volume = 0D;
            // 
            // GetVideoPosition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 466);
            this.Controls.Add(this.labelSubtitle);
            this.Controls.Add(this.buttonVerify);
            this.Controls.Add(this.buttonHalfASecondBack);
            this.Controls.Add(this.buttonThreeSecondsBack);
            this.Controls.Add(this.buttonThreeSecondsAhead);
            this.Controls.Add(this.buttonHalfASecondAhead);
            this.Controls.Add(this.labelVideoFileName);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.videoPlayerContainer1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 300);
            this.Name = "GetVideoPosition";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Get video position";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GetTime_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GetTime_FormClosed);
            this.Shown += new System.EventHandler(this.GetVideoPosition_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GetTime_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.VideoPlayerContainer videoPlayerContainer1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labelVideoFileName;
        private System.Windows.Forms.Button buttonHalfASecondBack;
        private System.Windows.Forms.Button buttonThreeSecondsBack;
        private System.Windows.Forms.Button buttonThreeSecondsAhead;
        private System.Windows.Forms.Button buttonHalfASecondAhead;
        private System.Windows.Forms.Button buttonVerify;
        private System.Windows.Forms.Label labelSubtitle;
    }
}