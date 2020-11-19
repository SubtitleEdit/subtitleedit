namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class AlignmentPickerJapanese
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
            this.button2 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(108, 268);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(282, 60);
            this.button2.TabIndex = 17;
            this.button2.Text = "bottom-left-justified\r\n(normal text)";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(396, 12);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(90, 314);
            this.button6.TabIndex = 15;
            this.button6.Text = "R\r\ni\r\ng\r\nh\r\nt";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(108, 138);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(282, 60);
            this.button5.TabIndex = 14;
            this.button5.Text = "force-narrative-example-region \r\n(exceptional cases for forced narrative events)";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(12, 12);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(90, 314);
            this.button4.TabIndex = 13;
            this.button4.Text = "L\r\ne\r\nf\r\nt";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(108, 12);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(282, 60);
            this.button8.TabIndex = 11;
            this.button8.Text = "top-center-justified \r\n(English text only)";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // AlignmentPickerJapanese
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 340);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button8);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AlignmentPickerJapanese";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AlignmentPickerJapanese";
            this.Shown += new System.EventHandler(this.AlignmentPickerJapanese_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AlignmentPickerJapanese_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button8;
    }
}