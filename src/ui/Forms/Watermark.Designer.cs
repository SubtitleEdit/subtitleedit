namespace Nikse.SubtitleEdit.Forms
{
    partial class Watermark
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
            this.labelWatermark = new System.Windows.Forms.Label();
            this.groupBoxGenerate = new System.Windows.Forms.GroupBox();
            this.textBoxWatermark = new System.Windows.Forms.TextBox();
            this.radioButtonSpread = new System.Windows.Forms.RadioButton();
            this.radioButtonCurrentLine = new System.Windows.Forms.RadioButton();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxGenerate.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelWatermark
            // 
            this.labelWatermark.AutoSize = true;
            this.labelWatermark.Location = new System.Drawing.Point(4, 19);
            this.labelWatermark.Name = "labelWatermark";
            this.labelWatermark.Size = new System.Drawing.Size(62, 13);
            this.labelWatermark.TabIndex = 0;
            this.labelWatermark.Text = "Watermark:";
            // 
            // groupBoxGenerate
            // 
            this.groupBoxGenerate.Controls.Add(this.textBoxWatermark);
            this.groupBoxGenerate.Controls.Add(this.radioButtonSpread);
            this.groupBoxGenerate.Controls.Add(this.radioButtonCurrentLine);
            this.groupBoxGenerate.Controls.Add(this.buttonGenerate);
            this.groupBoxGenerate.Location = new System.Drawing.Point(7, 69);
            this.groupBoxGenerate.Name = "groupBoxGenerate";
            this.groupBoxGenerate.Size = new System.Drawing.Size(344, 100);
            this.groupBoxGenerate.TabIndex = 1;
            this.groupBoxGenerate.TabStop = false;
            this.groupBoxGenerate.Text = "Generate watermark";
            // 
            // textBoxWatermark
            // 
            this.textBoxWatermark.Location = new System.Drawing.Point(87, 68);
            this.textBoxWatermark.MaxLength = 50;
            this.textBoxWatermark.Name = "textBoxWatermark";
            this.textBoxWatermark.Size = new System.Drawing.Size(195, 20);
            this.textBoxWatermark.TabIndex = 1;
            // 
            // radioButtonSpread
            // 
            this.radioButtonSpread.AutoSize = true;
            this.radioButtonSpread.Checked = true;
            this.radioButtonSpread.Location = new System.Drawing.Point(6, 19);
            this.radioButtonSpread.Name = "radioButtonSpread";
            this.radioButtonSpread.Size = new System.Drawing.Size(148, 17);
            this.radioButtonSpread.TabIndex = 2;
            this.radioButtonSpread.TabStop = true;
            this.radioButtonSpread.Text = "Spread over entire subtitle";
            this.radioButtonSpread.UseVisualStyleBackColor = true;
            // 
            // radioButtonCurrentLine
            // 
            this.radioButtonCurrentLine.AutoSize = true;
            this.radioButtonCurrentLine.Location = new System.Drawing.Point(6, 42);
            this.radioButtonCurrentLine.Name = "radioButtonCurrentLine";
            this.radioButtonCurrentLine.Size = new System.Drawing.Size(119, 17);
            this.radioButtonCurrentLine.TabIndex = 1;
            this.radioButtonCurrentLine.Text = "Only on current line:";
            this.radioButtonCurrentLine.UseVisualStyleBackColor = true;
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Location = new System.Drawing.Point(6, 65);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(75, 23);
            this.buttonGenerate.TabIndex = 0;
            this.buttonGenerate.Text = "Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Location = new System.Drawing.Point(7, 38);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonRemove.TabIndex = 0;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(276, 175);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // Watermark
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 202);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.groupBoxGenerate);
            this.Controls.Add(this.labelWatermark);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Watermark";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Watermark";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Watermark_KeyDown);
            this.groupBoxGenerate.ResumeLayout(false);
            this.groupBoxGenerate.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelWatermark;
        private System.Windows.Forms.GroupBox groupBoxGenerate;
        private System.Windows.Forms.RadioButton radioButtonSpread;
        private System.Windows.Forms.RadioButton radioButtonCurrentLine;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TextBox textBoxWatermark;
    }
}