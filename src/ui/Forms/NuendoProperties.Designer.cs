namespace Nikse.SubtitleEdit.Forms
{
    partial class NuendoProperties
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.labelCharacterList = new System.Windows.Forms.Label();
            this.textBoxCharacterFile = new System.Windows.Forms.TextBox();
            this.buttonChooseCharacterFile = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(218, 109);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(299, 109);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // labelCharacterList
            // 
            this.labelCharacterList.AutoSize = true;
            this.labelCharacterList.Location = new System.Drawing.Point(13, 25);
            this.labelCharacterList.Name = "labelCharacterList";
            this.labelCharacterList.Size = new System.Drawing.Size(104, 13);
            this.labelCharacterList.TabIndex = 5;
            this.labelCharacterList.Text = "Character list csv file";
            // 
            // textBoxCharacterFile
            // 
            this.textBoxCharacterFile.Location = new System.Drawing.Point(13, 42);
            this.textBoxCharacterFile.Name = "textBoxCharacterFile";
            this.textBoxCharacterFile.Size = new System.Drawing.Size(330, 20);
            this.textBoxCharacterFile.TabIndex = 6;
            // 
            // buttonChooseCharacterFile
            // 
            this.buttonChooseCharacterFile.Location = new System.Drawing.Point(349, 42);
            this.buttonChooseCharacterFile.Name = "buttonChooseCharacterFile";
            this.buttonChooseCharacterFile.Size = new System.Drawing.Size(24, 21);
            this.buttonChooseCharacterFile.TabIndex = 7;
            this.buttonChooseCharacterFile.Text = "...";
            this.buttonChooseCharacterFile.UseVisualStyleBackColor = true;
            this.buttonChooseCharacterFile.Click += new System.EventHandler(this.ButtonChooseCharacter_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(13, 65);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 8;
            this.labelStatus.Text = "labelStatus";
            // 
            // NuendoProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 142);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonChooseCharacterFile);
            this.Controls.Add(this.textBoxCharacterFile);
            this.Controls.Add(this.labelCharacterList);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NuendoProperties";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Nuendo properties";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Csv2Properties_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label labelCharacterList;
        private System.Windows.Forms.TextBox textBoxCharacterFile;
        private System.Windows.Forms.Button buttonChooseCharacterFile;
        private System.Windows.Forms.Label labelStatus;
    }
}