namespace Nikse.SubtitleEdit.Forms
{
    partial class HardSubExtract
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelCustomRgb = new System.Windows.Forms.Label();
            this.pictureBoxCustomColor = new System.Windows.Forms.PictureBox();
            this.numericUpDownCustomMaxDiff = new System.Windows.Forms.NumericUpDown();
            this.checkBoxCustomColor = new System.Windows.Forms.CheckBox();
            this.checkBoxYellow = new System.Windows.Forms.CheckBox();
            this.numericUpDownPixelsBottom = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.contextMenuStripScreenshot = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveImageAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelClickOnTextColor = new System.Windows.Forms.Label();
            this.openFileDialogVideo = new System.Windows.Forms.OpenFileDialog();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.timerRefreshProgressbar = new System.Windows.Forms.Timer(this.components);
            this.label8 = new System.Windows.Forms.Label();
            this.tbBlacks = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbFrameNum = new System.Windows.Forms.TextBox();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.StartStop = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.buttonStop = new System.Windows.Forms.Button();
            this.mediaPlayer = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCustomColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCustomMaxDiff)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPixelsBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.contextMenuStripScreenshot.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.labelCustomRgb);
            this.groupBox1.Controls.Add(this.pictureBoxCustomColor);
            this.groupBox1.Controls.Add(this.numericUpDownCustomMaxDiff);
            this.groupBox1.Controls.Add(this.checkBoxCustomColor);
            this.groupBox1.Controls.Add(this.checkBoxYellow);
            this.groupBox1.Location = new System.Drawing.Point(12, 85);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(341, 117);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Detection options";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 16);
            this.label1.TabIndex = 31;
            this.label1.Text = "Allow diff per RGB";
            // 
            // labelCustomRgb
            // 
            this.labelCustomRgb.AutoSize = true;
            this.labelCustomRgb.Location = new System.Drawing.Point(177, 56);
            this.labelCustomRgb.Name = "labelCustomRgb";
            this.labelCustomRgb.Size = new System.Drawing.Size(30, 13);
            this.labelCustomRgb.TabIndex = 37;
            this.labelCustomRgb.Text = "RGB";
            // 
            // pictureBoxCustomColor
            // 
            this.pictureBoxCustomColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxCustomColor.Location = new System.Drawing.Point(152, 53);
            this.pictureBoxCustomColor.Name = "pictureBoxCustomColor";
            this.pictureBoxCustomColor.Size = new System.Drawing.Size(21, 21);
            this.pictureBoxCustomColor.TabIndex = 8;
            this.pictureBoxCustomColor.TabStop = false;
            this.pictureBoxCustomColor.Click += new System.EventHandler(this.pictureBoxCustomColor_Click);
            // 
            // numericUpDownCustomMaxDiff
            // 
            this.numericUpDownCustomMaxDiff.Location = new System.Drawing.Point(116, 76);
            this.numericUpDownCustomMaxDiff.Name = "numericUpDownCustomMaxDiff";
            this.numericUpDownCustomMaxDiff.Size = new System.Drawing.Size(38, 20);
            this.numericUpDownCustomMaxDiff.TabIndex = 2;
            this.numericUpDownCustomMaxDiff.Value = new decimal(new int[] {
            14,
            0,
            0,
            0});
            // 
            // checkBoxCustomColor
            // 
            this.checkBoxCustomColor.AutoSize = true;
            this.checkBoxCustomColor.Location = new System.Drawing.Point(11, 55);
            this.checkBoxCustomColor.Name = "checkBoxCustomColor";
            this.checkBoxCustomColor.Size = new System.Drawing.Size(135, 17);
            this.checkBoxCustomColor.TabIndex = 1;
            this.checkBoxCustomColor.Text = "Check for custom color";
            this.checkBoxCustomColor.UseVisualStyleBackColor = true;
            // 
            // checkBoxYellow
            // 
            this.checkBoxYellow.AutoSize = true;
            this.checkBoxYellow.Checked = true;
            this.checkBoxYellow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxYellow.Location = new System.Drawing.Point(9, 19);
            this.checkBoxYellow.Name = "checkBoxYellow";
            this.checkBoxYellow.Size = new System.Drawing.Size(124, 17);
            this.checkBoxYellow.TabIndex = 0;
            this.checkBoxYellow.Text = "Check for yellow text";
            this.checkBoxYellow.UseVisualStyleBackColor = true;
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
            127,
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
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.ContextMenuStrip = this.contextMenuStripScreenshot;
            this.pictureBox2.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureBox2.Location = new System.Drawing.Point(11, 37);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(843, 373);
            this.pictureBox2.TabIndex = 4;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox2_Paint);
            this.pictureBox2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox2_MouseClick);
            // 
            // contextMenuStripScreenshot
            // 
            this.contextMenuStripScreenshot.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveImageAsToolStripMenuItem});
            this.contextMenuStripScreenshot.Name = "contextMenuStripScreenshot";
            this.contextMenuStripScreenshot.Size = new System.Drawing.Size(158, 26);
            // 
            // saveImageAsToolStripMenuItem
            // 
            this.saveImageAsToolStripMenuItem.Name = "saveImageAsToolStripMenuItem";
            this.saveImageAsToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.saveImageAsToolStripMenuItem.Text = "Save image as...";
            this.saveImageAsToolStripMenuItem.Click += new System.EventHandler(this.saveImageAsToolStripMenuItem_Click);
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
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(797, 695);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 25;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(716, 695);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 24;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
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
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.pictureBox2);
            this.groupBox2.Controls.Add(this.numericUpDownPixelsBottom);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.labelClickOnTextColor);
            this.groupBox2.Location = new System.Drawing.Point(12, 273);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(860, 416);
            this.groupBox2.TabIndex = 27;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Subtitle area (red rectangle)";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(357, 11);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(246, 26);
            this.button1.TabIndex = 37;
            this.button1.Text = "Take screenshot from video player";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // timerRefreshProgressbar
            // 
            this.timerRefreshProgressbar.Tick += new System.EventHandler(this.TimerRefreshProgressbarTick);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(125, 236);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(96, 23);
            this.label8.TabIndex = 34;
            this.label8.Text = "# of subtitles";
            // 
            // tbBlacks
            // 
            this.tbBlacks.Location = new System.Drawing.Point(229, 236);
            this.tbBlacks.Name = "tbBlacks";
            this.tbBlacks.ReadOnly = true;
            this.tbBlacks.Size = new System.Drawing.Size(48, 20);
            this.tbBlacks.TabIndex = 33;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(125, 209);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(100, 23);
            this.label9.TabIndex = 32;
            this.label9.Text = "Scanning Frame #";
            // 
            // tbFrameNum
            // 
            this.tbFrameNum.Location = new System.Drawing.Point(229, 209);
            this.tbFrameNum.Name = "tbFrameNum";
            this.tbFrameNum.ReadOnly = true;
            this.tbFrameNum.Size = new System.Drawing.Size(48, 20);
            this.tbFrameNum.TabIndex = 31;
            // 
            // tbFileName
            // 
            this.tbFileName.Location = new System.Drawing.Point(6, 35);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(285, 20);
            this.tbFileName.TabIndex = 30;
            this.tbFileName.Text = "c:\\foo.mpg";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(6, 16);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(72, 16);
            this.label10.TabIndex = 29;
            this.label10.Text = "Source file";
            // 
            // StartStop
            // 
            this.StartStop.Location = new System.Drawing.Point(16, 209);
            this.StartStop.Name = "StartStop";
            this.StartStop.Size = new System.Drawing.Size(75, 26);
            this.StartStop.TabIndex = 28;
            this.StartStop.Text = "Start";
            this.StartStop.Click += new System.EventHandler(this.StartStop_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.tbFileName);
            this.groupBox3.Location = new System.Drawing.Point(12, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(341, 67);
            this.groupBox3.TabIndex = 35;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Input";
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(18, 240);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 26);
            this.buttonStop.TabIndex = 36;
            this.buttonStop.Text = "Stop";
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // mediaPlayer
            // 
            this.mediaPlayer.AllowDrop = true;
            this.mediaPlayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mediaPlayer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.mediaPlayer.CurrentPosition = 0D;
            this.mediaPlayer.FontSizeFactor = 1F;
            this.mediaPlayer.LastParagraph = null;
            this.mediaPlayer.Location = new System.Drawing.Point(369, 19);
            this.mediaPlayer.Margin = new System.Windows.Forms.Padding(0);
            this.mediaPlayer.Name = "mediaPlayer";
            this.mediaPlayer.ShowFullscreenButton = true;
            this.mediaPlayer.ShowMuteButton = true;
            this.mediaPlayer.ShowStopButton = true;
            this.mediaPlayer.Size = new System.Drawing.Size(503, 247);
            this.mediaPlayer.SmpteMode = false;
            this.mediaPlayer.SubtitleText = "";
            this.mediaPlayer.TabIndex = 23;
            this.mediaPlayer.TextRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mediaPlayer.VideoHeight = 0;
            this.mediaPlayer.VideoPlayer = null;
            this.mediaPlayer.VideoWidth = 0;
            this.mediaPlayer.Volume = 0D;
            // 
            // HardSubExtract
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 728);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.StartStop);
            this.Controls.Add(this.tbBlacks);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tbFrameNum);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.mediaPlayer);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(900, 700);
            this.Name = "HardSubExtract";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import/OCR burned-in subtitles from video file";
            this.Shown += new System.EventHandler(this.HardSubExtract_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCustomColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCustomMaxDiff)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPixelsBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.contextMenuStripScreenshot.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label labelClickOnTextColor;
        private System.Windows.Forms.NumericUpDown numericUpDownPixelsBottom;
        private Controls.VideoPlayerContainer mediaPlayer;
        private System.Windows.Forms.OpenFileDialog openFileDialogVideo;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Timer timerRefreshProgressbar;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbBlacks;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbFrameNum;
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button StartStop;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBoxCustomColor;
        private System.Windows.Forms.CheckBox checkBoxYellow;
        private System.Windows.Forms.NumericUpDown numericUpDownCustomMaxDiff;
        private System.Windows.Forms.PictureBox pictureBoxCustomColor;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labelCustomRgb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripScreenshot;
        private System.Windows.Forms.ToolStripMenuItem saveImageAsToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}