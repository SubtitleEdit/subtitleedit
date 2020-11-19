namespace Nikse.SubtitleEdit.Controls
{
    sealed partial class AudioVisualizer
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            //
            // Waveform
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Name = "Waveform";
            this.Size = new System.Drawing.Size(682, 355);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.WaveformPaint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.WaveformKeyDown);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.WaveformMouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.WaveformMouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WaveformMouseDown);
            this.MouseEnter += new System.EventHandler(this.WaveformMouseEnter);
            this.MouseLeave += new System.EventHandler(this.WaveformMouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WaveformMouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WaveformMouseUp);
            this.ResumeLayout(false);

        }

        #endregion

    }
}
