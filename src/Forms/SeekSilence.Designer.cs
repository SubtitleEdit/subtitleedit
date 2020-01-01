namespace Nikse.SubtitleEdit.Forms
{
    partial class SeekSilence
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
            this.groupBoxSearchDirection = new System.Windows.Forms.GroupBox();
            this.radioButtonBack = new System.Windows.Forms.RadioButton();
            this.radioButtonForward = new System.Windows.Forms.RadioButton();
            this.numericUpDownSeconds = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownVolume = new System.Windows.Forms.NumericUpDown();
            this.labelDuration = new System.Windows.Forms.Label();
            this.labelVolumeBelow = new System.Windows.Forms.Label();
            this.groupBoxSearchDirection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownVolume)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(247, 152);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(166, 152);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxSearchDirection
            // 
            this.groupBoxSearchDirection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSearchDirection.Controls.Add(this.radioButtonBack);
            this.groupBoxSearchDirection.Controls.Add(this.radioButtonForward);
            this.groupBoxSearchDirection.Location = new System.Drawing.Point(12, 12);
            this.groupBoxSearchDirection.Name = "groupBoxSearchDirection";
            this.groupBoxSearchDirection.Size = new System.Drawing.Size(310, 73);
            this.groupBoxSearchDirection.TabIndex = 5;
            this.groupBoxSearchDirection.TabStop = false;
            this.groupBoxSearchDirection.Text = "Search direction";
            // 
            // radioButtonBack
            // 
            this.radioButtonBack.AutoSize = true;
            this.radioButtonBack.Location = new System.Drawing.Point(7, 44);
            this.radioButtonBack.Name = "radioButtonBack";
            this.radioButtonBack.Size = new System.Drawing.Size(50, 17);
            this.radioButtonBack.TabIndex = 1;
            this.radioButtonBack.Text = "Back";
            this.radioButtonBack.UseVisualStyleBackColor = true;
            // 
            // radioButtonForward
            // 
            this.radioButtonForward.AutoSize = true;
            this.radioButtonForward.Checked = true;
            this.radioButtonForward.Location = new System.Drawing.Point(7, 20);
            this.radioButtonForward.Name = "radioButtonForward";
            this.radioButtonForward.Size = new System.Drawing.Size(63, 17);
            this.radioButtonForward.TabIndex = 0;
            this.radioButtonForward.TabStop = true;
            this.radioButtonForward.Text = "Forward";
            this.radioButtonForward.UseVisualStyleBackColor = true;
            // 
            // numericUpDownSeconds
            // 
            this.numericUpDownSeconds.DecimalPlaces = 1;
            this.numericUpDownSeconds.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownSeconds.Location = new System.Drawing.Point(247, 91);
            this.numericUpDownSeconds.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownSeconds.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownSeconds.Name = "numericUpDownSeconds";
            this.numericUpDownSeconds.Size = new System.Drawing.Size(75, 20);
            this.numericUpDownSeconds.TabIndex = 6;
            this.numericUpDownSeconds.Value = new decimal(new int[] {
            3,
            0,
            0,
            65536});
            // 
            // numericUpDownVolume
            // 
            this.numericUpDownVolume.DecimalPlaces = 2;
            this.numericUpDownVolume.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownVolume.Location = new System.Drawing.Point(247, 117);
            this.numericUpDownVolume.Name = "numericUpDownVolume";
            this.numericUpDownVolume.Size = new System.Drawing.Size(75, 20);
            this.numericUpDownVolume.TabIndex = 7;
            this.numericUpDownVolume.Value = new decimal(new int[] {
            3,
            0,
            0,
            65536});
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(12, 93);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(180, 13);
            this.labelDuration.TabIndex = 8;
            this.labelDuration.Text = "Silence must be at at least (seconds)";
            // 
            // labelVolumeBelow
            // 
            this.labelVolumeBelow.AutoSize = true;
            this.labelVolumeBelow.Location = new System.Drawing.Point(12, 119);
            this.labelVolumeBelow.Name = "labelVolumeBelow";
            this.labelVolumeBelow.Size = new System.Drawing.Size(113, 13);
            this.labelVolumeBelow.TabIndex = 9;
            this.labelVolumeBelow.Text = "Volume must be below";
            // 
            // SeekSilence
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 185);
            this.Controls.Add(this.labelVolumeBelow);
            this.Controls.Add(this.labelDuration);
            this.Controls.Add(this.numericUpDownVolume);
            this.Controls.Add(this.numericUpDownSeconds);
            this.Controls.Add(this.groupBoxSearchDirection);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SeekSilence";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Seek silence";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SeekSilence_KeyDown);
            this.groupBoxSearchDirection.ResumeLayout(false);
            this.groupBoxSearchDirection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownVolume)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxSearchDirection;
        private System.Windows.Forms.RadioButton radioButtonBack;
        private System.Windows.Forms.RadioButton radioButtonForward;
        private System.Windows.Forms.NumericUpDown numericUpDownSeconds;
        private System.Windows.Forms.NumericUpDown numericUpDownVolume;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.Label labelVolumeBelow;
    }
}