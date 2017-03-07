namespace Nikse.SubtitleEdit.Forms
{
    partial class NetflixQCResult
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
            this.btnOk = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.lnkLblText = new System.Windows.Forms.LinkLabel();
            this.lblText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(162, 64);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(46, 64);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(110, 23);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Open file location";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // lnkLblText
            // 
            this.lnkLblText.AutoSize = true;
            this.lnkLblText.Location = new System.Drawing.Point(12, 9);
            this.lnkLblText.Name = "lnkLblText";
            this.lnkLblText.Size = new System.Drawing.Size(55, 13);
            this.lnkLblText.TabIndex = 3;
            this.lnkLblText.TabStop = true;
            this.lnkLblText.Text = "linkLabel1";
            this.lnkLblText.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblText_LinkClicked);
            // 
            // lblText
            // 
            this.lblText.AutoSize = true;
            this.lblText.Location = new System.Drawing.Point(12, 9);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(35, 13);
            this.lblText.TabIndex = 4;
            this.lblText.Text = "label1";
            // 
            // NetflixQCResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(296, 99);
            this.Controls.Add(this.lblText);
            this.Controls.Add(this.lnkLblText);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "NetflixQCResult";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Netflix quality check";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.LinkLabel lnkLblText;
        private System.Windows.Forms.Label lblText;
    }
}