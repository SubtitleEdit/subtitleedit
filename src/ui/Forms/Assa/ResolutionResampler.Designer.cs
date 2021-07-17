
namespace Nikse.SubtitleEdit.Forms.Assa
{
    partial class ResolutionResampler
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
            this.labelVideoResolution = new System.Windows.Forms.Label();
            this.numericUpDownTargetHeight = new System.Windows.Forms.NumericUpDown();
            this.buttonGetResolutionFromVideo = new System.Windows.Forms.Button();
            this.labelX = new System.Windows.Forms.Label();
            this.numericUpDownTargetWidth = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownSourceHeight = new System.Windows.Forms.NumericUpDown();
            this.buttonSourceRes = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownSourceWidth = new System.Windows.Forms.NumericUpDown();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.checkBoxKeepAspectRatio = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTargetHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTargetWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSourceHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSourceWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // labelVideoResolution
            // 
            this.labelVideoResolution.AutoSize = true;
            this.labelVideoResolution.Location = new System.Drawing.Point(12, 65);
            this.labelVideoResolution.Name = "labelVideoResolution";
            this.labelVideoResolution.Size = new System.Drawing.Size(115, 13);
            this.labelVideoResolution.TabIndex = 5;
            this.labelVideoResolution.Text = "Target video resolution";
            // 
            // numericUpDownTargetHeight
            // 
            this.numericUpDownTargetHeight.Location = new System.Drawing.Point(265, 63);
            this.numericUpDownTargetHeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownTargetHeight.Name = "numericUpDownTargetHeight";
            this.numericUpDownTargetHeight.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownTargetHeight.TabIndex = 8;
            // 
            // buttonGetResolutionFromVideo
            // 
            this.buttonGetResolutionFromVideo.Location = new System.Drawing.Point(316, 63);
            this.buttonGetResolutionFromVideo.Name = "buttonGetResolutionFromVideo";
            this.buttonGetResolutionFromVideo.Size = new System.Drawing.Size(27, 23);
            this.buttonGetResolutionFromVideo.TabIndex = 9;
            this.buttonGetResolutionFromVideo.Text = "...";
            this.buttonGetResolutionFromVideo.UseVisualStyleBackColor = true;
            this.buttonGetResolutionFromVideo.Click += new System.EventHandler(this.buttonGetResolutionFromVideo_Click);
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(247, 66);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(14, 13);
            this.labelX.TabIndex = 7;
            this.labelX.Text = "X";
            // 
            // numericUpDownTargetWidth
            // 
            this.numericUpDownTargetWidth.Location = new System.Drawing.Point(196, 63);
            this.numericUpDownTargetWidth.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownTargetWidth.Name = "numericUpDownTargetWidth";
            this.numericUpDownTargetWidth.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownTargetWidth.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Source video resolution";
            // 
            // numericUpDownSourceHeight
            // 
            this.numericUpDownSourceHeight.Location = new System.Drawing.Point(265, 37);
            this.numericUpDownSourceHeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownSourceHeight.Name = "numericUpDownSourceHeight";
            this.numericUpDownSourceHeight.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownSourceHeight.TabIndex = 13;
            // 
            // buttonSourceRes
            // 
            this.buttonSourceRes.Location = new System.Drawing.Point(316, 37);
            this.buttonSourceRes.Name = "buttonSourceRes";
            this.buttonSourceRes.Size = new System.Drawing.Size(27, 23);
            this.buttonSourceRes.TabIndex = 14;
            this.buttonSourceRes.Text = "...";
            this.buttonSourceRes.UseVisualStyleBackColor = true;
            this.buttonSourceRes.Click += new System.EventHandler(this.buttonSourceRes_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(247, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "X";
            // 
            // numericUpDownSourceWidth
            // 
            this.numericUpDownSourceWidth.Location = new System.Drawing.Point(196, 37);
            this.numericUpDownSourceWidth.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownSourceWidth.Name = "numericUpDownSourceWidth";
            this.numericUpDownSourceWidth.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownSourceWidth.TabIndex = 11;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(327, 143);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 16;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(246, 143);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 15;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // checkBoxKeepAspectRatio
            // 
            this.checkBoxKeepAspectRatio.AutoSize = true;
            this.checkBoxKeepAspectRatio.Location = new System.Drawing.Point(15, 99);
            this.checkBoxKeepAspectRatio.Name = "checkBoxKeepAspectRatio";
            this.checkBoxKeepAspectRatio.Size = new System.Drawing.Size(169, 17);
            this.checkBoxKeepAspectRatio.TabIndex = 17;
            this.checkBoxKeepAspectRatio.Text = "Keep aspect ration for margins";
            this.checkBoxKeepAspectRatio.UseVisualStyleBackColor = true;
            // 
            // ResolutionResampler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 178);
            this.Controls.Add(this.checkBoxKeepAspectRatio);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDownSourceHeight);
            this.Controls.Add(this.buttonSourceRes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericUpDownSourceWidth);
            this.Controls.Add(this.labelVideoResolution);
            this.Controls.Add(this.numericUpDownTargetHeight);
            this.Controls.Add(this.buttonGetResolutionFromVideo);
            this.Controls.Add(this.labelX);
            this.Controls.Add(this.numericUpDownTargetWidth);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResolutionResampler";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ResolutionResampler";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTargetHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTargetWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSourceHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSourceWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelVideoResolution;
        private System.Windows.Forms.NumericUpDown numericUpDownTargetHeight;
        private System.Windows.Forms.Button buttonGetResolutionFromVideo;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.NumericUpDown numericUpDownTargetWidth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownSourceHeight;
        private System.Windows.Forms.Button buttonSourceRes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownSourceWidth;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.CheckBox checkBoxKeepAspectRatio;
    }
}