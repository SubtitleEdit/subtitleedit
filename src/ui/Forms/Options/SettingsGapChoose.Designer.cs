namespace Nikse.SubtitleEdit.Forms.Options
{
    partial class SettingsGapChoose
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.comboBoxFrameRate = new System.Windows.Forms.ComboBox();
            this.labelFrameRate = new System.Windows.Forms.Label();
            this.numericUpDownFrames = new System.Windows.Forms.NumericUpDown();
            this.labelFrames = new System.Windows.Forms.Label();
            this.labelCalcInfo = new System.Windows.Forms.Label();
            this.labelUseCalc = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrames)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(385, 219);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(304, 219);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 9;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // comboBoxFrameRate
            // 
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Location = new System.Drawing.Point(15, 43);
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFrameRate.TabIndex = 11;
            this.comboBoxFrameRate.SelectedIndexChanged += new System.EventHandler(this.comboBoxFrameRate_SelectedIndexChanged);
            this.comboBoxFrameRate.TextChanged += new System.EventHandler(this.comboBoxFrameRate_TextChanged);
            // 
            // labelFrameRate
            // 
            this.labelFrameRate.AutoSize = true;
            this.labelFrameRate.Location = new System.Drawing.Point(12, 27);
            this.labelFrameRate.Name = "labelFrameRate";
            this.labelFrameRate.Size = new System.Drawing.Size(57, 13);
            this.labelFrameRate.TabIndex = 12;
            this.labelFrameRate.Text = "Frame rate";
            // 
            // numericUpDownFrames
            // 
            this.numericUpDownFrames.Location = new System.Drawing.Point(15, 94);
            this.numericUpDownFrames.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDownFrames.Name = "numericUpDownFrames";
            this.numericUpDownFrames.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownFrames.TabIndex = 52;
            this.numericUpDownFrames.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownFrames.ValueChanged += new System.EventHandler(this.numericUpDownFrames_ValueChanged);
            // 
            // labelFrames
            // 
            this.labelFrames.AutoSize = true;
            this.labelFrames.Location = new System.Drawing.Point(12, 78);
            this.labelFrames.Name = "labelFrames";
            this.labelFrames.Size = new System.Drawing.Size(196, 13);
            this.labelFrames.TabIndex = 51;
            this.labelFrames.Text = "Min. number of frames between subtitles";
            // 
            // labelCalcInfo
            // 
            this.labelCalcInfo.AutoSize = true;
            this.labelCalcInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCalcInfo.Location = new System.Drawing.Point(12, 144);
            this.labelCalcInfo.Name = "labelCalcInfo";
            this.labelCalcInfo.Size = new System.Drawing.Size(68, 13);
            this.labelCalcInfo.TabIndex = 53;
            this.labelCalcInfo.Text = "labelCalcInfo";
            // 
            // labelUseCalc
            // 
            this.labelUseCalc.AutoSize = true;
            this.labelUseCalc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUseCalc.Location = new System.Drawing.Point(12, 168);
            this.labelUseCalc.Name = "labelUseCalc";
            this.labelUseCalc.Size = new System.Drawing.Size(81, 13);
            this.labelUseCalc.TabIndex = 54;
            this.labelUseCalc.Text = "labelUseCalc";
            // 
            // SettingsGapChoose
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 254);
            this.Controls.Add(this.labelUseCalc);
            this.Controls.Add(this.labelCalcInfo);
            this.Controls.Add(this.numericUpDownFrames);
            this.Controls.Add(this.labelFrames);
            this.Controls.Add(this.labelFrameRate);
            this.Controls.Add(this.comboBoxFrameRate);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsGapChoose";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gap between subtitles";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SettingsGapChoose_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrames)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ComboBox comboBoxFrameRate;
        private System.Windows.Forms.Label labelFrameRate;
        private System.Windows.Forms.NumericUpDown numericUpDownFrames;
        private System.Windows.Forms.Label labelFrames;
        private System.Windows.Forms.Label labelCalcInfo;
        private System.Windows.Forms.Label labelUseCalc;
    }
}