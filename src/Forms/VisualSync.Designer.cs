using Nikse.SubtitleEdit.Logic;
namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class VisualSync
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
            this.buttonOpenMovie = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxStartScene = new System.Windows.Forms.GroupBox();
            this.MediaPlayerStart = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            this.panelControlsStart = new System.Windows.Forms.Panel();
            this.buttonStartVerify = new System.Windows.Forms.Button();
            this.buttonGotoStartSubtitlePosition = new System.Windows.Forms.Button();
            this.buttonFindTextStart = new System.Windows.Forms.Button();
            this.buttonStartThreeSecondsBack = new System.Windows.Forms.Button();
            this.buttonStartHalfASecondBack = new System.Windows.Forms.Button();
            this.comboBoxStartTexts = new System.Windows.Forms.ComboBox();
            this.groupBoxEndScene = new System.Windows.Forms.GroupBox();
            this.MediaPlayerEnd = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            this.panelControlsEnd = new System.Windows.Forms.Panel();
            this.buttonEndVerify = new System.Windows.Forms.Button();
            this.buttonGotoEndSubtitlePosition = new System.Windows.Forms.Button();
            this.buttonFindTextEnd = new System.Windows.Forms.Button();
            this.buttonThreeSecondsBack = new System.Windows.Forms.Button();
            this.buttonEndHalfASecondBack = new System.Windows.Forms.Button();
            this.comboBoxEndTexts = new System.Windows.Forms.ComboBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelVideoInfo = new System.Windows.Forms.Label();
            this.groupBoxMovieInfo = new System.Windows.Forms.GroupBox();
            this.buttonSync = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.labelTip = new System.Windows.Forms.Label();
            this.timerProgressBarRefresh = new System.Windows.Forms.Timer(this.components);
            this.labelSyncDone = new System.Windows.Forms.Label();
            this.groupBoxStartScene.SuspendLayout();
            this.panelControlsStart.SuspendLayout();
            this.groupBoxEndScene.SuspendLayout();
            this.panelControlsEnd.SuspendLayout();
            this.groupBoxMovieInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOpenMovie
            // 
            this.buttonOpenMovie.Location = new System.Drawing.Point(12, 12);
            this.buttonOpenMovie.Name = "buttonOpenMovie";
            this.buttonOpenMovie.Size = new System.Drawing.Size(100, 23);
            this.buttonOpenMovie.TabIndex = 5;
            this.buttonOpenMovie.Text = "Open movie...";
            this.buttonOpenMovie.UseVisualStyleBackColor = true;
            this.buttonOpenMovie.Click += new System.EventHandler(this.ButtonOpenMovieClick);
            // 
            // timer1
            // 
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.Timer1Tick);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(754, 490);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxStartScene
            // 
            this.groupBoxStartScene.Controls.Add(this.MediaPlayerStart);
            this.groupBoxStartScene.Controls.Add(this.panelControlsStart);
            this.groupBoxStartScene.Location = new System.Drawing.Point(12, 65);
            this.groupBoxStartScene.Name = "groupBoxStartScene";
            this.groupBoxStartScene.Size = new System.Drawing.Size(450, 399);
            this.groupBoxStartScene.TabIndex = 0;
            this.groupBoxStartScene.TabStop = false;
            this.groupBoxStartScene.Text = "Start scene";
            this.groupBoxStartScene.Enter += new System.EventHandler(this.GroupBoxStartSceneEnter);
            // 
            // MediaPlayerStart
            // 
            this.MediaPlayerStart.BackColor = System.Drawing.Color.Black;
            this.MediaPlayerStart.CurrentPosition = 0D;
            this.MediaPlayerStart.FontSizeFactor = 1F;
            this.MediaPlayerStart.LastParagraph = null;
            this.MediaPlayerStart.Location = new System.Drawing.Point(6, 34);
            this.MediaPlayerStart.Name = "MediaPlayerStart";
            this.MediaPlayerStart.ShowFullscreenButton = true;
            this.MediaPlayerStart.ShowMuteButton = true;
            this.MediaPlayerStart.ShowStopButton = true;
            this.MediaPlayerStart.Size = new System.Drawing.Size(450, 287);
            this.MediaPlayerStart.SmpteMode = false;
            this.MediaPlayerStart.SubtitleText = "";
            this.MediaPlayerStart.TabIndex = 13;
            this.MediaPlayerStart.TextRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.MediaPlayerStart.VideoHeight = 0;
            this.MediaPlayerStart.VideoPlayer = null;
            this.MediaPlayerStart.VideoWidth = 0;
            this.MediaPlayerStart.Volume = 0D;
            this.MediaPlayerStart.OnButtonClicked += new System.EventHandler(this.MediaPlayerStart_OnButtonClicked);
            // 
            // panelControlsStart
            // 
            this.panelControlsStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelControlsStart.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelControlsStart.Controls.Add(this.buttonStartVerify);
            this.panelControlsStart.Controls.Add(this.buttonGotoStartSubtitlePosition);
            this.panelControlsStart.Controls.Add(this.buttonFindTextStart);
            this.panelControlsStart.Controls.Add(this.buttonStartThreeSecondsBack);
            this.panelControlsStart.Controls.Add(this.buttonStartHalfASecondBack);
            this.panelControlsStart.Controls.Add(this.comboBoxStartTexts);
            this.panelControlsStart.Location = new System.Drawing.Point(6, 327);
            this.panelControlsStart.Name = "panelControlsStart";
            this.panelControlsStart.Size = new System.Drawing.Size(438, 68);
            this.panelControlsStart.TabIndex = 1;
            // 
            // buttonStartVerify
            // 
            this.buttonStartVerify.Location = new System.Drawing.Point(138, 37);
            this.buttonStartVerify.Name = "buttonStartVerify";
            this.buttonStartVerify.Size = new System.Drawing.Size(119, 23);
            this.buttonStartVerify.TabIndex = 3;
            this.buttonStartVerify.Text = "Play 2 s and back";
            this.buttonStartVerify.UseVisualStyleBackColor = true;
            this.buttonStartVerify.Click += new System.EventHandler(this.ButtonStartVerifyClick);
            // 
            // buttonGotoStartSubtitlePosition
            // 
            this.buttonGotoStartSubtitlePosition.Location = new System.Drawing.Point(263, 37);
            this.buttonGotoStartSubtitlePosition.Name = "buttonGotoStartSubtitlePosition";
            this.buttonGotoStartSubtitlePosition.Size = new System.Drawing.Size(90, 23);
            this.buttonGotoStartSubtitlePosition.TabIndex = 4;
            this.buttonGotoStartSubtitlePosition.Text = "Goto sub Position";
            this.buttonGotoStartSubtitlePosition.UseVisualStyleBackColor = true;
            this.buttonGotoStartSubtitlePosition.Click += new System.EventHandler(this.ButtonGotoStartSubtitlePositionClick);
            // 
            // buttonFindTextStart
            // 
            this.buttonFindTextStart.Location = new System.Drawing.Point(359, 37);
            this.buttonFindTextStart.Name = "buttonFindTextStart";
            this.buttonFindTextStart.Size = new System.Drawing.Size(70, 23);
            this.buttonFindTextStart.TabIndex = 5;
            this.buttonFindTextStart.Text = "Find text...";
            this.buttonFindTextStart.UseVisualStyleBackColor = true;
            this.buttonFindTextStart.Click += new System.EventHandler(this.ButtonFindTextStartClick);
            // 
            // buttonStartThreeSecondsBack
            // 
            this.buttonStartThreeSecondsBack.Location = new System.Drawing.Point(6, 37);
            this.buttonStartThreeSecondsBack.Name = "buttonStartThreeSecondsBack";
            this.buttonStartThreeSecondsBack.Size = new System.Drawing.Size(60, 23);
            this.buttonStartThreeSecondsBack.TabIndex = 1;
            this.buttonStartThreeSecondsBack.Text = "< 3 s";
            this.buttonStartThreeSecondsBack.UseVisualStyleBackColor = true;
            this.buttonStartThreeSecondsBack.Click += new System.EventHandler(this.ButtonStartThreeSecondsBackClick);
            // 
            // buttonStartHalfASecondBack
            // 
            this.buttonStartHalfASecondBack.Location = new System.Drawing.Point(72, 37);
            this.buttonStartHalfASecondBack.Name = "buttonStartHalfASecondBack";
            this.buttonStartHalfASecondBack.Size = new System.Drawing.Size(60, 23);
            this.buttonStartHalfASecondBack.TabIndex = 2;
            this.buttonStartHalfASecondBack.Text = "< ½ s";
            this.buttonStartHalfASecondBack.UseVisualStyleBackColor = true;
            this.buttonStartHalfASecondBack.Click += new System.EventHandler(this.ButtonStartHalfASecondBackClick);
            // 
            // comboBoxStartTexts
            // 
            this.comboBoxStartTexts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStartTexts.FormattingEnabled = true;
            this.comboBoxStartTexts.Location = new System.Drawing.Point(7, 10);
            this.comboBoxStartTexts.Name = "comboBoxStartTexts";
            this.comboBoxStartTexts.Size = new System.Drawing.Size(422, 21);
            this.comboBoxStartTexts.TabIndex = 0;
            // 
            // groupBoxEndScene
            // 
            this.groupBoxEndScene.Controls.Add(this.MediaPlayerEnd);
            this.groupBoxEndScene.Controls.Add(this.panelControlsEnd);
            this.groupBoxEndScene.Location = new System.Drawing.Point(468, 65);
            this.groupBoxEndScene.Name = "groupBoxEndScene";
            this.groupBoxEndScene.Size = new System.Drawing.Size(450, 399);
            this.groupBoxEndScene.TabIndex = 1;
            this.groupBoxEndScene.TabStop = false;
            this.groupBoxEndScene.Text = "End scene";
            this.groupBoxEndScene.Enter += new System.EventHandler(this.GroupBoxEndSceneEnter);
            // 
            // MediaPlayerEnd
            // 
            this.MediaPlayerEnd.BackColor = System.Drawing.Color.Black;
            this.MediaPlayerEnd.CurrentPosition = 0D;
            this.MediaPlayerEnd.FontSizeFactor = 1F;
            this.MediaPlayerEnd.LastParagraph = null;
            this.MediaPlayerEnd.Location = new System.Drawing.Point(6, 34);
            this.MediaPlayerEnd.Name = "MediaPlayerEnd";
            this.MediaPlayerEnd.ShowFullscreenButton = true;
            this.MediaPlayerEnd.ShowMuteButton = true;
            this.MediaPlayerEnd.ShowStopButton = true;
            this.MediaPlayerEnd.Size = new System.Drawing.Size(438, 287);
            this.MediaPlayerEnd.SmpteMode = false;
            this.MediaPlayerEnd.SubtitleText = "";
            this.MediaPlayerEnd.TabIndex = 14;
            this.MediaPlayerEnd.TextRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.MediaPlayerEnd.VideoHeight = 0;
            this.MediaPlayerEnd.VideoPlayer = null;
            this.MediaPlayerEnd.VideoWidth = 0;
            this.MediaPlayerEnd.Volume = 0D;
            this.MediaPlayerEnd.OnButtonClicked += new System.EventHandler(this.MediaPlayerEnd_OnButtonClicked);
            // 
            // panelControlsEnd
            // 
            this.panelControlsEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelControlsEnd.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelControlsEnd.Controls.Add(this.buttonEndVerify);
            this.panelControlsEnd.Controls.Add(this.buttonGotoEndSubtitlePosition);
            this.panelControlsEnd.Controls.Add(this.buttonFindTextEnd);
            this.panelControlsEnd.Controls.Add(this.buttonThreeSecondsBack);
            this.panelControlsEnd.Controls.Add(this.buttonEndHalfASecondBack);
            this.panelControlsEnd.Controls.Add(this.comboBoxEndTexts);
            this.panelControlsEnd.Location = new System.Drawing.Point(7, 327);
            this.panelControlsEnd.Name = "panelControlsEnd";
            this.panelControlsEnd.Size = new System.Drawing.Size(438, 68);
            this.panelControlsEnd.TabIndex = 1;
            // 
            // buttonEndVerify
            // 
            this.buttonEndVerify.Location = new System.Drawing.Point(139, 37);
            this.buttonEndVerify.Name = "buttonEndVerify";
            this.buttonEndVerify.Size = new System.Drawing.Size(118, 23);
            this.buttonEndVerify.TabIndex = 3;
            this.buttonEndVerify.Text = "Play 2 s and back";
            this.buttonEndVerify.UseVisualStyleBackColor = true;
            this.buttonEndVerify.Click += new System.EventHandler(this.ButtonEndVerifyClick);
            // 
            // buttonGotoEndSubtitlePosition
            // 
            this.buttonGotoEndSubtitlePosition.Location = new System.Drawing.Point(263, 37);
            this.buttonGotoEndSubtitlePosition.Name = "buttonGotoEndSubtitlePosition";
            this.buttonGotoEndSubtitlePosition.Size = new System.Drawing.Size(90, 23);
            this.buttonGotoEndSubtitlePosition.TabIndex = 4;
            this.buttonGotoEndSubtitlePosition.Text = "Goto sub Position";
            this.buttonGotoEndSubtitlePosition.UseVisualStyleBackColor = true;
            this.buttonGotoEndSubtitlePosition.Click += new System.EventHandler(this.ButtonGotoEndSubtitlePositionClick);
            // 
            // buttonFindTextEnd
            // 
            this.buttonFindTextEnd.Location = new System.Drawing.Point(359, 37);
            this.buttonFindTextEnd.Name = "buttonFindTextEnd";
            this.buttonFindTextEnd.Size = new System.Drawing.Size(70, 23);
            this.buttonFindTextEnd.TabIndex = 5;
            this.buttonFindTextEnd.Text = "Find text...";
            this.buttonFindTextEnd.UseVisualStyleBackColor = true;
            this.buttonFindTextEnd.Click += new System.EventHandler(this.ButtonFindTextEndClick);
            // 
            // buttonThreeSecondsBack
            // 
            this.buttonThreeSecondsBack.Location = new System.Drawing.Point(7, 37);
            this.buttonThreeSecondsBack.Name = "buttonThreeSecondsBack";
            this.buttonThreeSecondsBack.Size = new System.Drawing.Size(60, 23);
            this.buttonThreeSecondsBack.TabIndex = 1;
            this.buttonThreeSecondsBack.Text = "< 3 s";
            this.buttonThreeSecondsBack.UseVisualStyleBackColor = true;
            this.buttonThreeSecondsBack.Click += new System.EventHandler(this.ButtonThreeSecondsBackClick);
            // 
            // buttonEndHalfASecondBack
            // 
            this.buttonEndHalfASecondBack.Location = new System.Drawing.Point(73, 37);
            this.buttonEndHalfASecondBack.Name = "buttonEndHalfASecondBack";
            this.buttonEndHalfASecondBack.Size = new System.Drawing.Size(60, 23);
            this.buttonEndHalfASecondBack.TabIndex = 2;
            this.buttonEndHalfASecondBack.Text = "< ½ s";
            this.buttonEndHalfASecondBack.UseVisualStyleBackColor = true;
            this.buttonEndHalfASecondBack.Click += new System.EventHandler(this.ButtonEndHalfASecondBackClick);
            // 
            // comboBoxEndTexts
            // 
            this.comboBoxEndTexts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEndTexts.FormattingEnabled = true;
            this.comboBoxEndTexts.Location = new System.Drawing.Point(7, 10);
            this.comboBoxEndTexts.Name = "comboBoxEndTexts";
            this.comboBoxEndTexts.Size = new System.Drawing.Size(422, 21);
            this.comboBoxEndTexts.TabIndex = 0;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(835, 490);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelVideoInfo
            // 
            this.labelVideoInfo.AutoSize = true;
            this.labelVideoInfo.Location = new System.Drawing.Point(6, 16);
            this.labelVideoInfo.Name = "labelVideoInfo";
            this.labelVideoInfo.Size = new System.Drawing.Size(84, 13);
            this.labelVideoInfo.TabIndex = 13;
            this.labelVideoInfo.Text = "No video loaded";
            // 
            // groupBoxMovieInfo
            // 
            this.groupBoxMovieInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMovieInfo.Controls.Add(this.labelVideoInfo);
            this.groupBoxMovieInfo.Location = new System.Drawing.Point(132, 8);
            this.groupBoxMovieInfo.Name = "groupBoxMovieInfo";
            this.groupBoxMovieInfo.Size = new System.Drawing.Size(786, 53);
            this.groupBoxMovieInfo.TabIndex = 14;
            this.groupBoxMovieInfo.TabStop = false;
            this.groupBoxMovieInfo.Text = "Movie info";
            // 
            // buttonSync
            // 
            this.buttonSync.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonSync.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSync.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSync.Location = new System.Drawing.Point(395, 488);
            this.buttonSync.Name = "buttonSync";
            this.buttonSync.Size = new System.Drawing.Size(148, 31);
            this.buttonSync.TabIndex = 2;
            this.buttonSync.Text = "Sync!";
            this.buttonSync.UseVisualStyleBackColor = true;
            this.buttonSync.Click += new System.EventHandler(this.ButtonSyncClick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // labelTip
            // 
            this.labelTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTip.AutoSize = true;
            this.labelTip.ForeColor = System.Drawing.Color.Gray;
            this.labelTip.Location = new System.Drawing.Point(12, 473);
            this.labelTip.Name = "labelTip";
            this.labelTip.Size = new System.Drawing.Size(332, 13);
            this.labelTip.TabIndex = 15;
            this.labelTip.Text = "Tip: Use <ctrl+arrow left/right> keys to move 100 ms back/forward";
            // 
            // timerProgressBarRefresh
            // 
            this.timerProgressBarRefresh.Tick += new System.EventHandler(this.timerProgressBarRefresh_Tick);
            // 
            // labelSyncDone
            // 
            this.labelSyncDone.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.labelSyncDone.AutoSize = true;
            this.labelSyncDone.ForeColor = System.Drawing.Color.Gray;
            this.labelSyncDone.Location = new System.Drawing.Point(546, 502);
            this.labelSyncDone.Name = "labelSyncDone";
            this.labelSyncDone.Size = new System.Drawing.Size(77, 13);
            this.labelSyncDone.TabIndex = 16;
            this.labelSyncDone.Text = "labelSyncDone";
            // 
            // VisualSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(927, 525);
            this.Controls.Add(this.labelSyncDone);
            this.Controls.Add(this.buttonSync);
            this.Controls.Add(this.labelTip);
            this.Controls.Add(this.groupBoxMovieInfo);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxEndScene);
            this.Controls.Add(this.groupBoxStartScene);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonOpenMovie);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(935, 540);
            this.Name = "VisualSync";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Visual Sync";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormVisualSync_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.VisualSync_FormClosed);
            this.Load += new System.EventHandler(this.VisualSync_Load);
            this.Shown += new System.EventHandler(this.VisualSync_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VisualSync_KeyDown);
            this.Resize += new System.EventHandler(this.FormVisualSync_Resize);
            this.groupBoxStartScene.ResumeLayout(false);
            this.panelControlsStart.ResumeLayout(false);
            this.groupBoxEndScene.ResumeLayout(false);
            this.panelControlsEnd.ResumeLayout(false);
            this.groupBoxMovieInfo.ResumeLayout(false);
            this.groupBoxMovieInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOpenMovie;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxStartScene;
        private System.Windows.Forms.GroupBox groupBoxEndScene;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Panel panelControlsStart;
        private System.Windows.Forms.ComboBox comboBoxStartTexts;
        private System.Windows.Forms.Button buttonGotoStartSubtitlePosition;
        private System.Windows.Forms.Button buttonFindTextStart;
        private System.Windows.Forms.Button buttonStartThreeSecondsBack;
        private System.Windows.Forms.Button buttonStartHalfASecondBack;
        private System.Windows.Forms.Label labelVideoInfo;
        private System.Windows.Forms.GroupBox groupBoxMovieInfo;
        private System.Windows.Forms.Panel panelControlsEnd;
        private System.Windows.Forms.Button buttonGotoEndSubtitlePosition;
        private System.Windows.Forms.Button buttonFindTextEnd;
        private System.Windows.Forms.Button buttonThreeSecondsBack;
        private System.Windows.Forms.Button buttonEndHalfASecondBack;
        private System.Windows.Forms.ComboBox comboBoxEndTexts;
        private System.Windows.Forms.Button buttonSync;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonStartVerify;
        private System.Windows.Forms.Button buttonEndVerify;
        private Nikse.SubtitleEdit.Controls.VideoPlayerContainer MediaPlayerStart;
        private Nikse.SubtitleEdit.Controls.VideoPlayerContainer MediaPlayerEnd;
        private System.Windows.Forms.Label labelTip;
        private System.Windows.Forms.Timer timerProgressBarRefresh;
        private System.Windows.Forms.Label labelSyncDone;
    }
}