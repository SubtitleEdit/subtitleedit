namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class EffectTypewriter
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
            this.labelPreview = new System.Windows.Forms.Label();
            this.buttonPreview = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelEndDelay = new System.Windows.Forms.Label();
            this.numericUpDownDelay = new System.Windows.Forms.NumericUpDown();
            this.labelTM = new System.Windows.Forms.Label();
            this.labelTotalMillisecs = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // labelPreview
            // 
            this.labelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPreview.BackColor = System.Drawing.Color.Black;
            this.labelPreview.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPreview.ForeColor = System.Drawing.Color.White;
            this.labelPreview.Location = new System.Drawing.Point(12, 72);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(409, 40);
            this.labelPreview.TabIndex = 25;
            this.labelPreview.Text = "labelPreview";
            this.labelPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonPreview
            // 
            this.buttonPreview.Location = new System.Drawing.Point(12, 119);
            this.buttonPreview.Name = "buttonPreview";
            this.buttonPreview.Size = new System.Drawing.Size(100, 21);
            this.buttonPreview.TabIndex = 26;
            this.buttonPreview.Text = "Preview";
            this.buttonPreview.UseVisualStyleBackColor = true;
            this.buttonPreview.Click += new System.EventHandler(this.ButtonPreviewClick);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(265, 148);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 30;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(346, 148);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 31;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer1Tick);
            // 
            // labelEndDelay
            // 
            this.labelEndDelay.Location = new System.Drawing.Point(16, 45);
            this.labelEndDelay.Name = "labelEndDelay";
            this.labelEndDelay.Size = new System.Drawing.Size(140, 13);
            this.labelEndDelay.TabIndex = 32;
            this.labelEndDelay.Text = "End delay in millisecs.:";
            this.labelEndDelay.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericUpDownDelay
            // 
            this.numericUpDownDelay.DecimalPlaces = 3;
            this.numericUpDownDelay.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownDelay.Location = new System.Drawing.Point(159, 43);
            this.numericUpDownDelay.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numericUpDownDelay.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this.numericUpDownDelay.Name = "numericUpDownDelay";
            this.numericUpDownDelay.Size = new System.Drawing.Size(54, 21);
            this.numericUpDownDelay.TabIndex = 35;
            // 
            // labelTM
            // 
            this.labelTM.Location = new System.Drawing.Point(16, 23);
            this.labelTM.Name = "labelTM";
            this.labelTM.Size = new System.Drawing.Size(140, 13);
            this.labelTM.TabIndex = 36;
            this.labelTM.Text = "Total millisecs.:";
            this.labelTM.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelTotalMillisecs
            // 
            this.labelTotalMillisecs.AutoSize = true;
            this.labelTotalMillisecs.Location = new System.Drawing.Point(159, 23);
            this.labelTotalMillisecs.Name = "labelTotalMillisecs";
            this.labelTotalMillisecs.Size = new System.Drawing.Size(90, 13);
            this.labelTotalMillisecs.TabIndex = 37;
            this.labelTotalMillisecs.Text = "labelTotalMillisecs";
            // 
            // EffectTypewriter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 179);
            this.Controls.Add(this.labelTotalMillisecs);
            this.Controls.Add(this.labelTM);
            this.Controls.Add(this.numericUpDownDelay);
            this.Controls.Add(this.labelEndDelay);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonPreview);
            this.Controls.Add(this.labelPreview);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EffectTypewriter";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Typewriter effect";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormEffectTypewriter_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.Button buttonPreview;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labelEndDelay;
        private System.Windows.Forms.NumericUpDown numericUpDownDelay;
        private System.Windows.Forms.Label labelTM;
        private System.Windows.Forms.Label labelTotalMillisecs;
    }
}