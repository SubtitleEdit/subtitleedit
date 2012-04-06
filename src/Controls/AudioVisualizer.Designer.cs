namespace Nikse.SubtitleEdit.Controls
{
    partial class AudioVisualizer
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
            // WaveForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Name = "WaveForm";
            this.Size = new System.Drawing.Size(682, 355);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.WaveFormPaint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.WaveFormKeyDown);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.WaveFormMouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.WaveFormMouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WaveFormMouseDown);
            this.MouseEnter += new System.EventHandler(this.WaveFormMouseEnter);
            this.MouseLeave += new System.EventHandler(this.WaveFormMouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WaveFormMouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WaveFormMouseUp);
            this.ResumeLayout(false);

        }

        #endregion

    }
}
