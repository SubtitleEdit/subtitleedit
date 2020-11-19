namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class EbuColorPicker
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
            this.buttonBlack = new System.Windows.Forms.Button();
            this.buttonRed = new System.Windows.Forms.Button();
            this.buttonGreen = new System.Windows.Forms.Button();
            this.buttonYellow = new System.Windows.Forms.Button();
            this.buttonBlue = new System.Windows.Forms.Button();
            this.buttonMagenta = new System.Windows.Forms.Button();
            this.buttonCyan = new System.Windows.Forms.Button();
            this.buttonWhite = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonBlack
            // 
            this.buttonBlack.BackColor = System.Drawing.Color.Black;
            this.buttonBlack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBlack.Location = new System.Drawing.Point(12, 12);
            this.buttonBlack.Name = "buttonBlack";
            this.buttonBlack.Size = new System.Drawing.Size(216, 30);
            this.buttonBlack.TabIndex = 0;
            this.buttonBlack.UseVisualStyleBackColor = false;
            this.buttonBlack.Click += new System.EventHandler(this.buttonBlack_Click);
            // 
            // buttonRed
            // 
            this.buttonRed.BackColor = System.Drawing.Color.Red;
            this.buttonRed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRed.Location = new System.Drawing.Point(12, 48);
            this.buttonRed.Name = "buttonRed";
            this.buttonRed.Size = new System.Drawing.Size(216, 30);
            this.buttonRed.TabIndex = 1;
            this.buttonRed.UseVisualStyleBackColor = false;
            this.buttonRed.Click += new System.EventHandler(this.buttonRed_Click);
            // 
            // buttonGreen
            // 
            this.buttonGreen.BackColor = System.Drawing.Color.Green;
            this.buttonGreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGreen.Location = new System.Drawing.Point(12, 84);
            this.buttonGreen.Name = "buttonGreen";
            this.buttonGreen.Size = new System.Drawing.Size(216, 30);
            this.buttonGreen.TabIndex = 2;
            this.buttonGreen.UseVisualStyleBackColor = false;
            this.buttonGreen.Click += new System.EventHandler(this.buttonGreen_Click);
            // 
            // buttonYellow
            // 
            this.buttonYellow.BackColor = System.Drawing.Color.Yellow;
            this.buttonYellow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonYellow.Location = new System.Drawing.Point(12, 120);
            this.buttonYellow.Name = "buttonYellow";
            this.buttonYellow.Size = new System.Drawing.Size(216, 30);
            this.buttonYellow.TabIndex = 3;
            this.buttonYellow.UseVisualStyleBackColor = false;
            this.buttonYellow.Click += new System.EventHandler(this.buttonYellow_Click);
            // 
            // buttonBlue
            // 
            this.buttonBlue.BackColor = System.Drawing.Color.Blue;
            this.buttonBlue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBlue.Location = new System.Drawing.Point(12, 156);
            this.buttonBlue.Name = "buttonBlue";
            this.buttonBlue.Size = new System.Drawing.Size(216, 30);
            this.buttonBlue.TabIndex = 4;
            this.buttonBlue.UseVisualStyleBackColor = false;
            this.buttonBlue.Click += new System.EventHandler(this.buttonBlue_Click);
            // 
            // buttonMagenta
            // 
            this.buttonMagenta.BackColor = System.Drawing.Color.Magenta;
            this.buttonMagenta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMagenta.Location = new System.Drawing.Point(12, 192);
            this.buttonMagenta.Name = "buttonMagenta";
            this.buttonMagenta.Size = new System.Drawing.Size(216, 30);
            this.buttonMagenta.TabIndex = 5;
            this.buttonMagenta.UseVisualStyleBackColor = false;
            this.buttonMagenta.Click += new System.EventHandler(this.buttonMagenta_Click);
            // 
            // buttonCyan
            // 
            this.buttonCyan.BackColor = System.Drawing.Color.Cyan;
            this.buttonCyan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCyan.Location = new System.Drawing.Point(12, 228);
            this.buttonCyan.Name = "buttonCyan";
            this.buttonCyan.Size = new System.Drawing.Size(216, 30);
            this.buttonCyan.TabIndex = 6;
            this.buttonCyan.UseVisualStyleBackColor = false;
            this.buttonCyan.Click += new System.EventHandler(this.buttonCyan_Click);
            // 
            // buttonWhite
            // 
            this.buttonWhite.BackColor = System.Drawing.Color.White;
            this.buttonWhite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonWhite.Location = new System.Drawing.Point(12, 264);
            this.buttonWhite.Name = "buttonWhite";
            this.buttonWhite.Size = new System.Drawing.Size(216, 30);
            this.buttonWhite.TabIndex = 7;
            this.buttonWhite.UseVisualStyleBackColor = false;
            this.buttonWhite.Click += new System.EventHandler(this.buttonWhite_Click);
            // 
            // EbuColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(243, 312);
            this.Controls.Add(this.buttonWhite);
            this.Controls.Add(this.buttonCyan);
            this.Controls.Add(this.buttonMagenta);
            this.Controls.Add(this.buttonBlue);
            this.Controls.Add(this.buttonYellow);
            this.Controls.Add(this.buttonGreen);
            this.Controls.Add(this.buttonRed);
            this.Controls.Add(this.buttonBlack);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EbuColorPicker";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "EbuColorPicker";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EbuColorPicker_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonBlack;
        private System.Windows.Forms.Button buttonRed;
        private System.Windows.Forms.Button buttonGreen;
        private System.Windows.Forms.Button buttonYellow;
        private System.Windows.Forms.Button buttonBlue;
        private System.Windows.Forms.Button buttonMagenta;
        private System.Windows.Forms.Button buttonCyan;
        private System.Windows.Forms.Button buttonWhite;
    }
}