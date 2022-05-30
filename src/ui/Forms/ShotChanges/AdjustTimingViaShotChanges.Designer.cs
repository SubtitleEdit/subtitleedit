using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;

namespace Nikse.SubtitleEdit.Forms.ShotChanges
{
    partial class AdjustTimingViaShotChanges
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdjustTimingViaShotChanges));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.audioVisualizer = new Nikse.SubtitleEdit.Controls.AudioVisualizer();
            this.numericUpDownFixedMilliseconds = new System.Windows.Forms.NumericUpDown();
            this.labelMillisecondsFixed = new System.Windows.Forms.Label();
            this.groupBoxStartTime = new System.Windows.Forms.GroupBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxEndTime = new System.Windows.Forms.GroupBox();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.videoPlayerContainer1 = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFixedMilliseconds)).BeginInit();
            this.groupBoxStartTime.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.groupBoxEndTime.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(830, 533);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(87, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(737, 533);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(87, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(13, 13);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(904, 144);
            this.listView1.TabIndex = 7;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // audioVisualizer
            // 
            this.audioVisualizer.AllowNewSelection = true;
            this.audioVisualizer.AllowOverlap = false;
            this.audioVisualizer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.audioVisualizer.BackColor = System.Drawing.Color.Black;
            this.audioVisualizer.BackgroundColor = System.Drawing.Color.Black;
            this.audioVisualizer.Chapters = ((System.Collections.Generic.List<Nikse.SubtitleEdit.Core.ContainerFormats.Matroska.MatroskaChapter>)(resources.GetObject("audioVisualizer.Chapters")));
            this.audioVisualizer.ChaptersColor = System.Drawing.Color.Empty;
            this.audioVisualizer.ClosenessForBorderSelection = 15;
            this.audioVisualizer.Color = System.Drawing.Color.GreenYellow;
            this.audioVisualizer.CursorColor = System.Drawing.Color.Empty;
            this.audioVisualizer.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.audioVisualizer.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(18)))));
            this.audioVisualizer.InsertAtVideoPositionShortcut = System.Windows.Forms.Keys.Insert;
            this.audioVisualizer.Location = new System.Drawing.Point(12, 405);
            this.audioVisualizer.MouseWheelScrollUpIsForward = true;
            this.audioVisualizer.Move100MsLeft = System.Windows.Forms.Keys.None;
            this.audioVisualizer.Move100MsRight = System.Windows.Forms.Keys.None;
            this.audioVisualizer.MoveOneSecondLeft = System.Windows.Forms.Keys.None;
            this.audioVisualizer.MoveOneSecondRight = System.Windows.Forms.Keys.None;
            this.audioVisualizer.Name = "audioVisualizer";
            this.audioVisualizer.NewSelectionParagraph = null;
            this.audioVisualizer.ParagraphColor = System.Drawing.Color.LimeGreen;
            this.audioVisualizer.SelectedColor = System.Drawing.Color.Red;
            this.audioVisualizer.ShotChanges = null;
            this.audioVisualizer.ShowGridLines = true;
            this.audioVisualizer.ShowSpectrogram = true;
            this.audioVisualizer.ShowWaveform = true;
            this.audioVisualizer.Size = new System.Drawing.Size(905, 122);
            this.audioVisualizer.StartPositionSeconds = 0D;
            this.audioVisualizer.TabIndex = 0;
            this.audioVisualizer.TextBold = true;
            this.audioVisualizer.TextColor = System.Drawing.Color.Gray;
            this.audioVisualizer.TextSize = 9F;
            this.audioVisualizer.VerticalZoomFactor = 1D;
            this.audioVisualizer.WaveformNotLoadedText = "Click to add waveform/spectrogram";
            this.audioVisualizer.WavePeaks = null;
            this.audioVisualizer.ZoomFactor = 1D;
            // 
            // numericUpDownFixedMilliseconds
            // 
            this.numericUpDownFixedMilliseconds.Enabled = false;
            this.numericUpDownFixedMilliseconds.Location = new System.Drawing.Point(88, 19);
            this.numericUpDownFixedMilliseconds.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numericUpDownFixedMilliseconds.Name = "numericUpDownFixedMilliseconds";
            this.numericUpDownFixedMilliseconds.Size = new System.Drawing.Size(61, 20);
            this.numericUpDownFixedMilliseconds.TabIndex = 14;
            this.numericUpDownFixedMilliseconds.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // labelMillisecondsFixed
            // 
            this.labelMillisecondsFixed.AutoSize = true;
            this.labelMillisecondsFixed.Location = new System.Drawing.Point(155, 22);
            this.labelMillisecondsFixed.Name = "labelMillisecondsFixed";
            this.labelMillisecondsFixed.Size = new System.Drawing.Size(158, 13);
            this.labelMillisecondsFixed.TabIndex = 15;
            this.labelMillisecondsFixed.Text = "milliseconds before shot change";
            // 
            // groupBoxStartTime
            // 
            this.groupBoxStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxStartTime.Controls.Add(this.numericUpDown1);
            this.groupBoxStartTime.Controls.Add(this.label2);
            this.groupBoxStartTime.Controls.Add(this.label3);
            this.groupBoxStartTime.Controls.Add(this.numericUpDownFixedMilliseconds);
            this.groupBoxStartTime.Controls.Add(this.label1);
            this.groupBoxStartTime.Controls.Add(this.labelMillisecondsFixed);
            this.groupBoxStartTime.Location = new System.Drawing.Point(508, 163);
            this.groupBoxStartTime.Name = "groupBoxStartTime";
            this.groupBoxStartTime.Size = new System.Drawing.Size(387, 70);
            this.groupBoxStartTime.TabIndex = 13;
            this.groupBoxStartTime.TabStop = false;
            this.groupBoxStartTime.Text = "Start time - move to shot change if";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Enabled = false;
            this.numericUpDown1.Location = new System.Drawing.Point(88, 41);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(61, 20);
            this.numericUpDown1.TabIndex = 17;
            this.numericUpDown1.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "Begins at most";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(155, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(149, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "milliseconds after shot change";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Begins at most";
            // 
            // groupBoxEndTime
            // 
            this.groupBoxEndTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxEndTime.Controls.Add(this.numericUpDown2);
            this.groupBoxEndTime.Controls.Add(this.label4);
            this.groupBoxEndTime.Controls.Add(this.label5);
            this.groupBoxEndTime.Controls.Add(this.numericUpDown3);
            this.groupBoxEndTime.Controls.Add(this.label6);
            this.groupBoxEndTime.Controls.Add(this.label7);
            this.groupBoxEndTime.Location = new System.Drawing.Point(508, 239);
            this.groupBoxEndTime.Name = "groupBoxEndTime";
            this.groupBoxEndTime.Size = new System.Drawing.Size(387, 70);
            this.groupBoxEndTime.TabIndex = 14;
            this.groupBoxEndTime.TabStop = false;
            this.groupBoxEndTime.Text = "End time - move to shot change if";
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Enabled = false;
            this.numericUpDown2.Location = new System.Drawing.Point(88, 44);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(61, 20);
            this.numericUpDown2.TabIndex = 17;
            this.numericUpDown2.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "Begins at most";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(155, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(149, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "milliseconds after shot change";
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.Enabled = false;
            this.numericUpDown3.Location = new System.Drawing.Point(88, 19);
            this.numericUpDown3.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(61, 20);
            this.numericUpDown3.TabIndex = 14;
            this.numericUpDown3.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Begins at most";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(155, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(158, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "milliseconds before shot change";
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(785, 376);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(132, 23);
            this.button3.TabIndex = 17;
            this.button3.Text = "Go to next issue";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button4.Location = new System.Drawing.Point(171, 533);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(156, 23);
            this.button4.TabIndex = 18;
            this.button4.Text = "One frame forward >";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button5.Location = new System.Drawing.Point(9, 533);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(156, 23);
            this.button5.TabIndex = 19;
            this.button5.Text = "< One frame back";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(647, 376);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(132, 23);
            this.button1.TabIndex = 20;
            this.button1.Text = "Auto fix current";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(509, 376);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(132, 23);
            this.button2.TabIndex = 21;
            this.button2.Text = "Auto fix all";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // videoPlayerContainer1
            // 
            this.videoPlayerContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.videoPlayerContainer1.Chapters = ((System.Collections.Generic.List<Nikse.SubtitleEdit.Core.ContainerFormats.Matroska.MatroskaChapter>)(resources.GetObject("videoPlayerContainer1.Chapters")));
            this.videoPlayerContainer1.CurrentPosition = 0D;
            this.videoPlayerContainer1.FontSizeFactor = 1F;
            this.videoPlayerContainer1.LastParagraph = null;
            this.videoPlayerContainer1.Location = new System.Drawing.Point(13, 163);
            this.videoPlayerContainer1.Name = "videoPlayerContainer1";
            this.videoPlayerContainer1.ShowFullscreenButton = true;
            this.videoPlayerContainer1.ShowMuteButton = true;
            this.videoPlayerContainer1.ShowStopButton = true;
            this.videoPlayerContainer1.Size = new System.Drawing.Size(474, 236);
            this.videoPlayerContainer1.SubtitleText = "";
            this.videoPlayerContainer1.TabIndex = 22;
            this.videoPlayerContainer1.TextRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.videoPlayerContainer1.VideoHeight = 0;
            this.videoPlayerContainer1.VideoPlayer = null;
            this.videoPlayerContainer1.VideoWidth = 0;
            this.videoPlayerContainer1.Volume = 0D;
            // 
            // AdjustTimingViaShotChanges
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(929, 568);
            this.Controls.Add(this.videoPlayerContainer1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.groupBoxEndTime);
            this.Controls.Add(this.groupBoxStartTime);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.audioVisualizer);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(940, 600);
            this.Name = "AdjustTimingViaShotChanges";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Adjust timing via shot changes";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AdjustTimingViaShotChanges_FormClosing);
            this.ResizeEnd += new System.EventHandler(this.AdjustTimingViaShotChanges_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AdjustTimingViaShotChanges_KeyDown);
            this.Resize += new System.EventHandler(this.AdjustTimingViaShotChanges_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFixedMilliseconds)).EndInit();
            this.groupBoxStartTime.ResumeLayout(false);
            this.groupBoxStartTime.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.groupBoxEndTime.ResumeLayout(false);
            this.groupBoxEndTime.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.AudioVisualizer audioVisualizer;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.NumericUpDown numericUpDownFixedMilliseconds;
        private System.Windows.Forms.Label labelMillisecondsFixed;
        private System.Windows.Forms.GroupBox groupBoxStartTime;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBoxEndTime;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private Controls.VideoPlayerContainer videoPlayerContainer1;
    }
}