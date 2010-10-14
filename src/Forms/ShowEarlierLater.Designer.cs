using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ShowEarlierLater
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
            this.labelHoursMinSecsMilliSecs = new System.Windows.Forms.Label();
            this.buttonShowLater = new System.Windows.Forms.Button();
            this.buttonShowEarlier = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.subtitleListView1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.labelTotalAdjustment = new System.Windows.Forms.Label();
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.mediaPlayer = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timeUpDownAdjust = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.SuspendLayout();
            // 
            // labelHoursMinSecsMilliSecs
            // 
            this.labelHoursMinSecsMilliSecs.AutoSize = true;
            this.labelHoursMinSecsMilliSecs.Location = new System.Drawing.Point(11, 383);
            this.labelHoursMinSecsMilliSecs.Name = "labelHoursMinSecsMilliSecs";
            this.labelHoursMinSecsMilliSecs.Size = new System.Drawing.Size(108, 13);
            this.labelHoursMinSecsMilliSecs.TabIndex = 18;
            this.labelHoursMinSecsMilliSecs.Text = "Hours:min:sec.msecs";
            // 
            // buttonShowLater
            // 
            this.buttonShowLater.Location = new System.Drawing.Point(14, 456);
            this.buttonShowLater.Name = "buttonShowLater";
            this.buttonShowLater.Size = new System.Drawing.Size(119, 21);
            this.buttonShowLater.TabIndex = 20;
            this.buttonShowLater.Text = "Show later";
            this.buttonShowLater.UseVisualStyleBackColor = true;
            this.buttonShowLater.Click += new System.EventHandler(this.ButtonShowLaterClick);
            // 
            // buttonShowEarlier
            // 
            this.buttonShowEarlier.Location = new System.Drawing.Point(14, 429);
            this.buttonShowEarlier.Name = "buttonShowEarlier";
            this.buttonShowEarlier.Size = new System.Drawing.Size(120, 21);
            this.buttonShowEarlier.TabIndex = 19;
            this.buttonShowEarlier.Text = "Show earlier";
            this.buttonShowEarlier.UseVisualStyleBackColor = true;
            this.buttonShowEarlier.Click += new System.EventHandler(this.ButtonShowEarlierClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(715, 630);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 37;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(634, 630);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 36;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // subtitleListView1
            // 
            this.subtitleListView1.FirstVisibleIndex = -1;
            this.subtitleListView1.FullRowSelect = true;
            this.subtitleListView1.GridLines = true;
            this.subtitleListView1.HideSelection = false;
            this.subtitleListView1.Location = new System.Drawing.Point(14, 13);
            this.subtitleListView1.Name = "subtitleListView1";
            this.subtitleListView1.Size = new System.Drawing.Size(776, 329);
            this.subtitleListView1.TabIndex = 35;
            this.subtitleListView1.UseCompatibleStateImageBehavior = false;
            this.subtitleListView1.View = System.Windows.Forms.View.Details;
            this.subtitleListView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.subtitleListView1_MouseDoubleClick);
            // 
            // labelTotalAdjustment
            // 
            this.labelTotalAdjustment.AutoSize = true;
            this.labelTotalAdjustment.Location = new System.Drawing.Point(11, 348);
            this.labelTotalAdjustment.Name = "labelTotalAdjustment";
            this.labelTotalAdjustment.Size = new System.Drawing.Size(108, 13);
            this.labelTotalAdjustment.TabIndex = 38;
            this.labelTotalAdjustment.Text = "labelTotalAdjustment";
            // 
            // labelSubtitle
            // 
            this.labelSubtitle.BackColor = System.Drawing.Color.Black;
            this.labelSubtitle.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSubtitle.ForeColor = System.Drawing.Color.White;
            this.labelSubtitle.Location = new System.Drawing.Point(387, 582);
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Size = new System.Drawing.Size(403, 28);
            this.labelSubtitle.TabIndex = 39;
            this.labelSubtitle.Text = "labelSubtitle";
            this.labelSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mediaPlayer
            // 
            this.mediaPlayer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.mediaPlayer.CurrentPosition = 0D;
            this.mediaPlayer.Location = new System.Drawing.Point(387, 348);
            this.mediaPlayer.Name = "mediaPlayer";
            this.mediaPlayer.ShowStopButton = true;
            this.mediaPlayer.Size = new System.Drawing.Size(403, 234);
            this.mediaPlayer.TabIndex = 40;
            this.mediaPlayer.VideoPlayer = null;
            this.mediaPlayer.Volume = 0D;
            // 
            // timer1
            // 
            this.timer1.Interval = 250;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timeUpDownAdjust
            // 
            this.timeUpDownAdjust.AutoSize = true;
            this.timeUpDownAdjust.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownAdjust.Location = new System.Drawing.Point(12, 399);
            this.timeUpDownAdjust.Name = "timeUpDownAdjust";
            this.timeUpDownAdjust.Size = new System.Drawing.Size(89, 25);
            this.timeUpDownAdjust.TabIndex = 21;
            // 
            // ShowEarlierLater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 663);
            this.Controls.Add(this.labelSubtitle);
            this.Controls.Add(this.mediaPlayer);
            this.Controls.Add(this.labelTotalAdjustment);
            this.Controls.Add(this.subtitleListView1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.timeUpDownAdjust);
            this.Controls.Add(this.labelHoursMinSecsMilliSecs);
            this.Controls.Add(this.buttonShowLater);
            this.Controls.Add(this.buttonShowEarlier);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShowEarlierLater";
            this.Text = "Show selected lines earlier/later";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShowEarlierLater_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ShowEarlierLater_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Nikse.SubtitleEdit.Controls.TimeUpDown timeUpDownAdjust;
        private System.Windows.Forms.Label labelHoursMinSecsMilliSecs;
        private System.Windows.Forms.Button buttonShowLater;
        private System.Windows.Forms.Button buttonShowEarlier;
        private SubtitleListView subtitleListView1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelTotalAdjustment;
        private System.Windows.Forms.Label labelSubtitle;
        private VideoPlayerContainer mediaPlayer;
        private System.Windows.Forms.Timer timer1;
    }
}