namespace Nikse.SubtitleEdit.Forms
{
    partial class AddToOcrReplaceList
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
            this.labelLanguage = new System.Windows.Forms.Label();
            this.labelDescription = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxOcrFixValue = new System.Windows.Forms.TextBox();
            this.textBoxOcrFixKey = new System.Windows.Forms.TextBox();
            this.buttonAddOcrFix = new System.Windows.Forms.Button();
            this.textBoxLanguage = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(16, 71);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(55, 13);
            this.labelLanguage.TabIndex = 28;
            this.labelLanguage.Text = "Language";
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(16, 15);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(109, 13);
            this.labelDescription.TabIndex = 27;
            this.labelDescription.Text = "Add to user dictionary";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(204, 145);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 24;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // textBoxOcrFixValue
            // 
            this.textBoxOcrFixValue.Location = new System.Drawing.Point(113, 31);
            this.textBoxOcrFixValue.Name = "textBoxOcrFixValue";
            this.textBoxOcrFixValue.Size = new System.Drawing.Size(85, 20);
            this.textBoxOcrFixValue.TabIndex = 51;
            // 
            // textBoxOcrFixKey
            // 
            this.textBoxOcrFixKey.Location = new System.Drawing.Point(19, 31);
            this.textBoxOcrFixKey.Name = "textBoxOcrFixKey";
            this.textBoxOcrFixKey.Size = new System.Drawing.Size(88, 20);
            this.textBoxOcrFixKey.TabIndex = 50;
            // 
            // buttonAddOcrFix
            // 
            this.buttonAddOcrFix.Location = new System.Drawing.Point(204, 29);
            this.buttonAddOcrFix.Name = "buttonAddOcrFix";
            this.buttonAddOcrFix.Size = new System.Drawing.Size(75, 23);
            this.buttonAddOcrFix.TabIndex = 52;
            this.buttonAddOcrFix.Text = "Add pair";
            this.buttonAddOcrFix.UseVisualStyleBackColor = true;
            this.buttonAddOcrFix.Click += new System.EventHandler(this.buttonAddOcrFix_Click);
            // 
            // textBoxLanguage
            // 
            this.textBoxLanguage.Enabled = false;
            this.textBoxLanguage.Location = new System.Drawing.Point(19, 87);
            this.textBoxLanguage.Name = "textBoxLanguage";
            this.textBoxLanguage.ReadOnly = true;
            this.textBoxLanguage.Size = new System.Drawing.Size(179, 20);
            this.textBoxLanguage.TabIndex = 53;
            // 
            // AddToOcrReplaceList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 178);
            this.Controls.Add(this.textBoxLanguage);
            this.Controls.Add(this.textBoxOcrFixValue);
            this.Controls.Add(this.textBoxOcrFixKey);
            this.Controls.Add(this.buttonAddOcrFix);
            this.Controls.Add(this.labelLanguage);
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddToOcrReplaceList";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AddToOcrReplaceList";
            this.Shown += new System.EventHandler(this.AddToOcrReplaceList_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxOcrFixValue;
        private System.Windows.Forms.TextBox textBoxOcrFixKey;
        private System.Windows.Forms.Button buttonAddOcrFix;
        private System.Windows.Forms.TextBox textBoxLanguage;
    }
}