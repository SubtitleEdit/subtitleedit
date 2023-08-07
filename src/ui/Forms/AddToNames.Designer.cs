namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class AddToNameList
    {
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
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
            this.textBoxAddName = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.comboBoxDictionaries = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(121, 161);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(202, 161);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 30;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxAddName
            // 
            this.textBoxAddName.Location = new System.Drawing.Point(21, 106);
            this.textBoxAddName.Name = "textBoxAddName";
            this.textBoxAddName.Size = new System.Drawing.Size(221, 21);
            this.textBoxAddName.TabIndex = 2;
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(18, 88);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(196, 13);
            this.labelDescription.TabIndex = 14;
            this.labelDescription.Text = "Add to names/noise list (case sensitive)";
            // 
            // comboBoxDictionaries
            // 
            this.comboBoxDictionaries.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxDictionaries.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxDictionaries.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxDictionaries.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxDictionaries.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxDictionaries.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxDictionaries.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxDictionaries.DropDownHeight = 400;
            this.comboBoxDictionaries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDictionaries.DropDownWidth = 221;
            this.comboBoxDictionaries.FormattingEnabled = true;
            this.comboBoxDictionaries.Location = new System.Drawing.Point(18, 26);
            this.comboBoxDictionaries.MaxLength = 32767;
            this.comboBoxDictionaries.Name = "comboBoxDictionaries";
            this.comboBoxDictionaries.SelectedIndex = -1;
            this.comboBoxDictionaries.SelectedItem = null;
            this.comboBoxDictionaries.SelectedText = "";
            this.comboBoxDictionaries.Size = new System.Drawing.Size(221, 21);
            this.comboBoxDictionaries.TabIndex = 1;
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(18, 10);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(54, 13);
            this.labelLanguage.TabIndex = 18;
            this.labelLanguage.Text = "Language";
            // 
            // AddToNameList
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(289, 196);
            this.Controls.Add(this.comboBoxDictionaries);
            this.Controls.Add(this.labelLanguage);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.textBoxAddName);
            this.Controls.Add(this.labelDescription);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddToNameList";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add to Names List";
            this.Shown += new System.EventHandler(this.AddToNameList_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxAddName;
        private System.Windows.Forms.Label labelDescription;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxDictionaries;
        private System.Windows.Forms.Label labelLanguage;
    }
}