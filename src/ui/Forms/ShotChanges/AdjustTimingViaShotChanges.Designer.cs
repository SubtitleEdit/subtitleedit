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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.audioVisualizer = new Nikse.SubtitleEdit.Controls.AudioVisualizer();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(787, 372);
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
            this.buttonOK.Location = new System.Drawing.Point(694, 372);
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
            this.listView1.Size = new System.Drawing.Size(861, 159);
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
            this.audioVisualizer.Chapters = null;
            this.audioVisualizer.ChaptersColor = System.Drawing.Color.Empty;
            this.audioVisualizer.ClosenessForBorderSelection = 15;
            this.audioVisualizer.Color = System.Drawing.Color.GreenYellow;
            this.audioVisualizer.CursorColor = System.Drawing.Color.Empty;
            this.audioVisualizer.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.audioVisualizer.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(18)))));
            this.audioVisualizer.InsertAtVideoPositionShortcut = System.Windows.Forms.Keys.Insert;
            this.audioVisualizer.Location = new System.Drawing.Point(12, 215);
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
            this.audioVisualizer.Size = new System.Drawing.Size(862, 151);
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
            // AdjustTimingViaShotChanges
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(886, 407);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.audioVisualizer);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(902, 446);
            this.Name = "AdjustTimingViaShotChanges";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Adjust timing via shot changes";
            this.ResizeEnd += new System.EventHandler(this.AdjustTimingViaShotChanges_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AdjustTimingViaShotChanges_KeyDown);
            this.Resize += new System.EventHandler(this.AdjustTimingViaShotChanges_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.AudioVisualizer audioVisualizer;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Timer timer1;
    }
}