namespace Nikse.SubtitleEdit.Forms
{
    partial class HardSubExtract
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numericUpDownFont2Diff = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDownFont1Diff = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownInterval = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBoxColor2 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBoxColor1 = new System.Windows.Forms.PictureBox();
            this.numericUpDownPixelsBottom = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonStart = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.labelClickOnTextColor = new System.Windows.Forms.Label();
            this.buttonStop = new System.Windows.Forms.Button();
            this.openFileDialogVideo = new System.Windows.Forms.OpenFileDialog();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonShowImg = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.timerRefreshProgressbar = new System.Windows.Forms.Timer(this.components);
            this.mediaPlayer = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFont2Diff)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFont1Diff)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxColor2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxColor1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPixelsBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.numericUpDownFont2Diff);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.numericUpDownFont1Diff);
            this.groupBox1.Controls.Add(this.numericUpDownInterval);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.pictureBoxColor2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.pictureBoxColor1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(341, 225);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Detection options";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(183, 182);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 28;
            this.button1.Text = "Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(112, 13);
            this.label7.TabIndex = 25;
            this.label7.Text = "Hardcoded font colors";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(205, 89);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(86, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "color diff allowed";
            // 
            // numericUpDownFont2Diff
            // 
            this.numericUpDownFont2Diff.Location = new System.Drawing.Point(133, 87);
            this.numericUpDownFont2Diff.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.numericUpDownFont2Diff.Name = "numericUpDownFont2Diff";
            this.numericUpDownFont2Diff.Size = new System.Drawing.Size(66, 20);
            this.numericUpDownFont2Diff.TabIndex = 23;
            this.numericUpDownFont2Diff.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(205, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "color diff allowed";
            // 
            // numericUpDownFont1Diff
            // 
            this.numericUpDownFont1Diff.Location = new System.Drawing.Point(133, 58);
            this.numericUpDownFont1Diff.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.numericUpDownFont1Diff.Name = "numericUpDownFont1Diff";
            this.numericUpDownFont1Diff.Size = new System.Drawing.Size(66, 20);
            this.numericUpDownFont1Diff.TabIndex = 21;
            this.numericUpDownFont1Diff.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // numericUpDownInterval
            // 
            this.numericUpDownInterval.Location = new System.Drawing.Point(16, 182);
            this.numericUpDownInterval.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDownInterval.Name = "numericUpDownInterval";
            this.numericUpDownInterval.Size = new System.Drawing.Size(59, 20);
            this.numericUpDownInterval.TabIndex = 20;
            this.numericUpDownInterval.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 166);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Scan interval in milliseconds";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Font color2";
            // 
            // pictureBoxColor2
            // 
            this.pictureBoxColor2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxColor2.Location = new System.Drawing.Point(16, 84);
            this.pictureBoxColor2.Name = "pictureBoxColor2";
            this.pictureBoxColor2.Size = new System.Drawing.Size(21, 21);
            this.pictureBoxColor2.TabIndex = 10;
            this.pictureBoxColor2.TabStop = false;
            this.pictureBoxColor2.Click += new System.EventHandler(this.pictureBoxColor2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Font color1";
            // 
            // pictureBoxColor1
            // 
            this.pictureBoxColor1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxColor1.Location = new System.Drawing.Point(16, 52);
            this.pictureBoxColor1.Name = "pictureBoxColor1";
            this.pictureBoxColor1.Size = new System.Drawing.Size(21, 21);
            this.pictureBoxColor1.TabIndex = 8;
            this.pictureBoxColor1.TabStop = false;
            this.pictureBoxColor1.Click += new System.EventHandler(this.pictureBoxBackground_Click);
            // 
            // numericUpDownPixelsBottom
            // 
            this.numericUpDownPixelsBottom.Location = new System.Drawing.Point(192, 16);
            this.numericUpDownPixelsBottom.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDownPixelsBottom.Name = "numericUpDownPixelsBottom";
            this.numericUpDownPixelsBottom.Size = new System.Drawing.Size(66, 20);
            this.numericUpDownPixelsBottom.TabIndex = 19;
            this.numericUpDownPixelsBottom.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownPixelsBottom.ValueChanged += new System.EventHandler(this.numericUpDownPixelsBottom_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(178, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Search number of pixels from bottom";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(12, 243);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(118, 23);
            this.buttonStart.TabIndex = 1;
            this.buttonStart.Text = "Start detect";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStartClick);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.Location = new System.Drawing.Point(11, 37);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(867, 373);
            this.pictureBox2.TabIndex = 4;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox2_Paint);
            this.pictureBox2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox2_MouseClick);
            // 
            // labelClickOnTextColor
            // 
            this.labelClickOnTextColor.AutoSize = true;
            this.labelClickOnTextColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelClickOnTextColor.ForeColor = System.Drawing.Color.Red;
            this.labelClickOnTextColor.Location = new System.Drawing.Point(325, 59);
            this.labelClickOnTextColor.Name = "labelClickOnTextColor";
            this.labelClickOnTextColor.Size = new System.Drawing.Size(110, 13);
            this.labelClickOnTextColor.TabIndex = 19;
            this.labelClickOnTextColor.Text = "Click on text color";
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(136, 243);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 21;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(821, 695);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 25;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(740, 695);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 24;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonShowImg
            // 
            this.buttonShowImg.Location = new System.Drawing.Point(235, 243);
            this.buttonShowImg.Name = "buttonShowImg";
            this.buttonShowImg.Size = new System.Drawing.Size(118, 23);
            this.buttonShowImg.TabIndex = 26;
            this.buttonShowImg.Text = "Update subtitle area";
            this.buttonShowImg.UseVisualStyleBackColor = true;
            this.buttonShowImg.Click += new System.EventHandler(this.buttonShowImg_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(16, 699);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 21;
            this.labelStatus.Text = "labelStatus";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.numericUpDownPixelsBottom);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.labelClickOnTextColor);
            this.groupBox2.Controls.Add(this.pictureBox2);
            this.groupBox2.Location = new System.Drawing.Point(12, 273);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(884, 416);
            this.groupBox2.TabIndex = 27;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Subtitle area (red rectangle)";
            // 
            // timerRefreshProgressbar
            // 
            this.timerRefreshProgressbar.Tick += new System.EventHandler(this.TimerRefreshProgressbarTick);
            // 
            // mediaPlayer
            // 
            this.mediaPlayer.AllowDrop = true;
            this.mediaPlayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mediaPlayer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.mediaPlayer.CurrentPosition = 0D;
            this.mediaPlayer.FontSizeFactor = 1F;
            this.mediaPlayer.Location = new System.Drawing.Point(369, 19);
            this.mediaPlayer.Margin = new System.Windows.Forms.Padding(0);
            this.mediaPlayer.Name = "mediaPlayer";
            this.mediaPlayer.ShowFullscreenButton = true;
            this.mediaPlayer.ShowMuteButton = true;
            this.mediaPlayer.ShowStopButton = true;
            this.mediaPlayer.Size = new System.Drawing.Size(527, 247);
            this.mediaPlayer.SubtitleText = "";
            this.mediaPlayer.TabIndex = 23;
            this.mediaPlayer.TextRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mediaPlayer.VideoHeight = 0;
            this.mediaPlayer.VideoPlayer = null;
            this.mediaPlayer.VideoWidth = 0;
            this.mediaPlayer.Volume = 0D;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(264, 182);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 29;
            this.button2.Text = "Test";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // HardSubExtract
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(908, 728);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonShowImg);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.mediaPlayer);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(900, 700);
            this.Name = "HardSubExtract";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import/OCR burned-in subtitles from video file";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HardSubExtract_FormClosing);
            this.Shown += new System.EventHandler(this.HardSubExtract_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFont2Diff)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFont1Diff)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxColor2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxColor1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPixelsBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBoxColor2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBoxColor1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label labelClickOnTextColor;
        private System.Windows.Forms.NumericUpDown numericUpDownPixelsBottom;
        private System.Windows.Forms.NumericUpDown numericUpDownInterval;
        private System.Windows.Forms.Button buttonStop;
        private Controls.VideoPlayerContainer mediaPlayer;
        private System.Windows.Forms.OpenFileDialog openFileDialogVideo;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonShowImg;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDownFont1Diff;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericUpDownFont2Diff;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Timer timerRefreshProgressbar;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}