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
            this.buttonPreview = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelEndDelay = new System.Windows.Forms.Label();
            this.numericUpDownDelay = new System.Windows.Forms.NumericUpDown();
            this.labelTM = new System.Windows.Forms.Label();
            this.labelTotalMilliseconds = new System.Windows.Forms.Label();
            this.richTextBoxPreview = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonPreview
            // 
            this.buttonPreview.Location = new System.Drawing.Point(12, 119);
            this.buttonPreview.Name = "buttonPreview";
            this.buttonPreview.Size = new System.Drawing.Size(100, 23);
            this.buttonPreview.TabIndex = 26;
            this.buttonPreview.Text = "Preview";
            this.buttonPreview.UseVisualStyleBackColor = true;
            this.buttonPreview.Click += new System.EventHandler(this.ButtonPreviewClick);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(265, 144);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 30;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(346, 144);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
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
            this.labelEndDelay.Location = new System.Drawing.Point(5, 45);
            this.labelEndDelay.Name = "labelEndDelay";
            this.labelEndDelay.Size = new System.Drawing.Size(161, 19);
            this.labelEndDelay.TabIndex = 32;
            this.labelEndDelay.Text = "End delay in milliseconds:";
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
            this.numericUpDownDelay.Location = new System.Drawing.Point(169, 43);
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
            this.labelTM.Location = new System.Drawing.Point(2, 23);
            this.labelTM.Name = "labelTM";
            this.labelTM.Size = new System.Drawing.Size(164, 13);
            this.labelTM.TabIndex = 36;
            this.labelTM.Text = "Total milliseconds:";
            this.labelTM.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelTotalMilliseconds
            // 
            this.labelTotalMilliseconds.AutoSize = true;
            this.labelTotalMilliseconds.Location = new System.Drawing.Point(169, 23);
            this.labelTotalMilliseconds.Name = "labelTotalMilliseconds";
            this.labelTotalMilliseconds.Size = new System.Drawing.Size(108, 13);
            this.labelTotalMilliseconds.TabIndex = 37;
            this.labelTotalMilliseconds.Text = "labelTotalMilliseconds";
            // 
            // richTextBoxPreview
            // 
            this.richTextBoxPreview.BackColor = System.Drawing.Color.Black;
            this.richTextBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxPreview.DetectUrls = false;
            this.richTextBoxPreview.Location = new System.Drawing.Point(12, 69);
            this.richTextBoxPreview.Name = "richTextBoxPreview";
            this.richTextBoxPreview.ReadOnly = true;
            this.richTextBoxPreview.Size = new System.Drawing.Size(409, 44);
            this.richTextBoxPreview.TabIndex = 51;
            this.richTextBoxPreview.TabStop = false;
            this.richTextBoxPreview.Text = "";
            // 
            // EffectTypewriter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 179);
            this.Controls.Add(this.richTextBoxPreview);
            this.Controls.Add(this.labelTotalMilliseconds);
            this.Controls.Add(this.labelTM);
            this.Controls.Add(this.numericUpDownDelay);
            this.Controls.Add(this.labelEndDelay);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonPreview);
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

        private System.Windows.Forms.Button buttonPreview;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labelEndDelay;
        private System.Windows.Forms.NumericUpDown numericUpDownDelay;
        private System.Windows.Forms.Label labelTM;
        private System.Windows.Forms.Label labelTotalMilliseconds;
        private System.Windows.Forms.RichTextBox richTextBoxPreview;
    }
}