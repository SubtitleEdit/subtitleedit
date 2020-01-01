namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ExportPngXmlDialogOpenFolder
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelText = new System.Windows.Forms.Label();
            this.linkLabelOpenFolder = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(493, 266);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelText
            // 
            this.labelText.AutoSize = true;
            this.labelText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelText.Location = new System.Drawing.Point(10, 21);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(35, 15);
            this.labelText.TabIndex = 5;
            this.labelText.Text = "text...";
            // 
            // linkLabelOpenFolder
            // 
            this.linkLabelOpenFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelOpenFolder.AutoSize = true;
            this.linkLabelOpenFolder.Location = new System.Drawing.Point(12, 271);
            this.linkLabelOpenFolder.Name = "linkLabelOpenFolder";
            this.linkLabelOpenFolder.Size = new System.Drawing.Size(62, 13);
            this.linkLabelOpenFolder.TabIndex = 7;
            this.linkLabelOpenFolder.TabStop = true;
            this.linkLabelOpenFolder.Text = "Open folder";
            this.linkLabelOpenFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenFolder_LinkClicked);
            // 
            // ExportPngXmlDialogOpenFolder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 300);
            this.Controls.Add(this.linkLabelOpenFolder);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportPngXmlDialogOpenFolder";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ExportPngXmlDialogOpenFolder";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExportPngXmlDialogOpenFolder_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.LinkLabel linkLabelOpenFolder;
    }
}