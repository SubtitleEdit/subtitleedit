namespace Nikse.SubtitleEdit.Forms
{
    partial class About
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.okButton = new System.Windows.Forms.Button();
            this.labelProduct = new System.Windows.Forms.Label();
            this.pictureBoxSE = new System.Windows.Forms.PictureBox();
            this.richTextBoxAbout1 = new System.Windows.Forms.RichTextBox();
            this.buttonDonate = new System.Windows.Forms.Button();
            this.linkLabelGitBuildHash = new System.Windows.Forms.LinkLabel();
            this.tooltip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSE)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Location = new System.Drawing.Point(381, 337);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(83, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "&OK";
            this.okButton.Click += new System.EventHandler(this.OkButtonClick);
            // 
            // labelProduct
            // 
            this.labelProduct.AutoSize = true;
            this.labelProduct.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProduct.Location = new System.Drawing.Point(13, 16);
            this.labelProduct.Name = "labelProduct";
            this.labelProduct.Size = new System.Drawing.Size(140, 19);
            this.labelProduct.TabIndex = 26;
            this.labelProduct.Text = "Subtitle Edit 3.2";
            // 
            // pictureBoxSE
            // 
            this.pictureBoxSE.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxSE.Location = new System.Drawing.Point(432, 12);
            this.pictureBoxSE.Name = "pictureBoxSE";
            this.pictureBoxSE.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxSE.TabIndex = 27;
            this.pictureBoxSE.TabStop = false;
            // 
            // richTextBoxAbout1
            // 
            this.richTextBoxAbout1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxAbout1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxAbout1.Location = new System.Drawing.Point(16, 43);
            this.richTextBoxAbout1.Name = "richTextBoxAbout1";
            this.richTextBoxAbout1.ReadOnly = true;
            this.richTextBoxAbout1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBoxAbout1.Size = new System.Drawing.Size(448, 266);
            this.richTextBoxAbout1.TabIndex = 40;
            this.richTextBoxAbout1.TabStop = false;
            this.richTextBoxAbout1.Text = "About...";
            this.richTextBoxAbout1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.RichTextBoxAbout1LinkClicked);
            // 
            // buttonDonate
            // 
            this.buttonDonate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDonate.AutoSize = true;
            this.buttonDonate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonDonate.FlatAppearance.BorderSize = 0;
            this.buttonDonate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDonate.ForeColor = System.Drawing.Color.Transparent;
            this.buttonDonate.Image = global::Nikse.SubtitleEdit.Properties.Resources.Donate;
            this.buttonDonate.Location = new System.Drawing.Point(16, 328);
            this.buttonDonate.Name = "buttonDonate";
            this.buttonDonate.Size = new System.Drawing.Size(98, 32);
            this.buttonDonate.TabIndex = 42;
            this.buttonDonate.UseVisualStyleBackColor = false;
            this.buttonDonate.Click += new System.EventHandler(this.buttonDonate_Click);
            // 
            // linkLabelGitBuildHash
            // 
            this.linkLabelGitBuildHash.AutoSize = true;
            this.linkLabelGitBuildHash.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelGitBuildHash.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.linkLabelGitBuildHash.Location = new System.Drawing.Point(148, 15);
            this.linkLabelGitBuildHash.Name = "linkLabelGitBuildHash";
            this.linkLabelGitBuildHash.Size = new System.Drawing.Size(90, 20);
            this.linkLabelGitBuildHash.TabIndex = 43;
            this.linkLabelGitBuildHash.TabStop = true;
            this.linkLabelGitBuildHash.Text = "linkLabel1";
            this.linkLabelGitBuildHash.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.linkLabelGitBuildHash.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelGitBuildHash_LinkClicked);
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 372);
            this.Controls.Add(this.linkLabelGitBuildHash);
            this.Controls.Add(this.buttonDonate);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.pictureBoxSE);
            this.Controls.Add(this.richTextBoxAbout1);
            this.Controls.Add(this.labelProduct);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "About";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Subtitle Edit";
            this.Shown += new System.EventHandler(this.About_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.About_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSE)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label labelProduct;
        private System.Windows.Forms.PictureBox pictureBoxSE;
        private System.Windows.Forms.RichTextBox richTextBoxAbout1;
        private System.Windows.Forms.Button buttonDonate;
        private System.Windows.Forms.LinkLabel linkLabelGitBuildHash;
        private System.Windows.Forms.ToolTip tooltip;

    }
}
