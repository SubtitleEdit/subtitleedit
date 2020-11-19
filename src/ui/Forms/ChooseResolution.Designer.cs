namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ChooseResolution
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.labelVideoResolution = new System.Windows.Forms.Label();
            this.numericUpDownVideoHeight = new System.Windows.Forms.NumericUpDown();
            this.button1 = new System.Windows.Forms.Button();
            this.labelX = new System.Windows.Forms.Label();
            this.numericUpDownVideoWidth = new System.Windows.Forms.NumericUpDown();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownVideoHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownVideoWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // labelVideoResolution
            // 
            this.labelVideoResolution.AutoSize = true;
            this.labelVideoResolution.Location = new System.Drawing.Point(12, 32);
            this.labelVideoResolution.Name = "labelVideoResolution";
            this.labelVideoResolution.Size = new System.Drawing.Size(82, 13);
            this.labelVideoResolution.TabIndex = 0;
            this.labelVideoResolution.Text = "Video resolution";
            // 
            // numericUpDownVideoHeight
            // 
            this.numericUpDownVideoHeight.Location = new System.Drawing.Point(205, 29);
            this.numericUpDownVideoHeight.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownVideoHeight.Name = "numericUpDownVideoHeight";
            this.numericUpDownVideoHeight.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownVideoHeight.TabIndex = 3;
            this.numericUpDownVideoHeight.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(258, 27);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(27, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(185, 31);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(14, 13);
            this.labelX.TabIndex = 2;
            this.labelX.Text = "X";
            // 
            // numericUpDownVideoWidth
            // 
            this.numericUpDownVideoWidth.Location = new System.Drawing.Point(132, 29);
            this.numericUpDownVideoWidth.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownVideoWidth.Name = "numericUpDownVideoWidth";
            this.numericUpDownVideoWidth.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownVideoWidth.TabIndex = 1;
            this.numericUpDownVideoWidth.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(213, 81);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(132, 81);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // ChooseResolution
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 114);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelVideoResolution);
            this.Controls.Add(this.numericUpDownVideoHeight);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelX);
            this.Controls.Add(this.numericUpDownVideoWidth);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChooseResolution";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose video resolution";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChooseResolution_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownVideoHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownVideoWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label labelVideoResolution;
        private System.Windows.Forms.NumericUpDown numericUpDownVideoHeight;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.NumericUpDown numericUpDownVideoWidth;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
    }
}