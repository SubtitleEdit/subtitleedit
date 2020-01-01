namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class DialogDoNotShowAgain
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
            this.checkBoxDoNotDisplayAgain = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // checkBoxDoNotDisplayAgain
            // 
            this.checkBoxDoNotDisplayAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxDoNotDisplayAgain.AutoSize = true;
            this.checkBoxDoNotDisplayAgain.Location = new System.Drawing.Point(12, 283);
            this.checkBoxDoNotDisplayAgain.Name = "checkBoxDoNotDisplayAgain";
            this.checkBoxDoNotDisplayAgain.Size = new System.Drawing.Size(179, 17);
            this.checkBoxDoNotDisplayAgain.TabIndex = 0;
            this.checkBoxDoNotDisplayAgain.Text = "Don\'t display this message again";
            this.checkBoxDoNotDisplayAgain.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(497, 277);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelText
            // 
            this.labelText.AutoSize = true;
            this.labelText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelText.Location = new System.Drawing.Point(12, 24);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(35, 15);
            this.labelText.TabIndex = 2;
            this.labelText.Text = "text...";
            // 
            // DialogDoNotShowAgain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 312);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelText);
            this.Controls.Add(this.checkBoxDoNotDisplayAgain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DialogDoNotShowAgain";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SpellCheckCompleted";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DialogDoNotShowAgain_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SpellCheckCompleted_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxDoNotDisplayAgain;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelText;
    }
}