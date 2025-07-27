namespace Nikse.SubtitleEdit.Forms.Ocr
{
    partial class DownloadPaddleGpuOrCpu
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonDownloadCpu = new System.Windows.Forms.Button();
            this.buttonDownloadGpu = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(553, 175);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 15;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonDownloadCpu
            // 
            this.buttonDownloadCpu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownloadCpu.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDownloadCpu.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDownloadCpu.Location = new System.Drawing.Point(306, 68);
            this.buttonDownloadCpu.Name = "buttonDownloadCpu";
            this.buttonDownloadCpu.Size = new System.Drawing.Size(193, 29);
            this.buttonDownloadCpu.TabIndex = 16;
            this.buttonDownloadCpu.Text = "Download &CPU version";
            this.buttonDownloadCpu.UseVisualStyleBackColor = true;
            this.buttonDownloadCpu.Click += new System.EventHandler(this.buttonDownloadCpu_Click);
            // 
            // buttonDownloadGpu
            // 
            this.buttonDownloadGpu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownloadGpu.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDownloadGpu.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDownloadGpu.Location = new System.Drawing.Point(107, 68);
            this.buttonDownloadGpu.Name = "buttonDownloadGpu";
            this.buttonDownloadGpu.Size = new System.Drawing.Size(193, 29);
            this.buttonDownloadGpu.TabIndex = 17;
            this.buttonDownloadGpu.Text = "Download GPU (nvidia) version";
            this.buttonDownloadGpu.UseVisualStyleBackColor = true;
            this.buttonDownloadGpu.Click += new System.EventHandler(this.buttonDownloadGpu_Click);
            // 
            // DownloadPaddleGpuOrCpu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 210);
            this.Controls.Add(this.buttonDownloadGpu);
            this.Controls.Add(this.buttonDownloadCpu);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.Name = "DownloadPaddleGpuOrCpu";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Download Paddle OCR";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DownloadPaddleGpuOrCpu_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonDownloadCpu;
        private System.Windows.Forms.Button buttonDownloadGpu;
    }
}