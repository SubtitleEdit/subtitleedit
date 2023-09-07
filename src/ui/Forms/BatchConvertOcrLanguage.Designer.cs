namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class BatchConvertOcrLanguage
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
            this.labelFileNameEnding = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxOcrMethod = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.buttonGetTesseractDictionaries = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelFileNameEnding
            // 
            this.labelFileNameEnding.AutoSize = true;
            this.labelFileNameEnding.Location = new System.Drawing.Point(12, 17);
            this.labelFileNameEnding.Name = "labelFileNameEnding";
            this.labelFileNameEnding.Size = new System.Drawing.Size(65, 13);
            this.labelFileNameEnding.TabIndex = 108;
            this.labelFileNameEnding.Text = "OCR engine";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(287, 123);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 333;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(368, 123);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 335;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 110;
            this.label2.Text = "Language";
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(83, 44);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(180, 21);
            this.comboBoxLanguage.TabIndex = 222;
            // 
            // comboBoxOcrMethod
            // 
            this.comboBoxOcrMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOcrMethod.FormattingEnabled = true;
            this.comboBoxOcrMethod.Items.AddRange(new object[] {
            "Tesseract",
            "nOCR"});
            this.comboBoxOcrMethod.Location = new System.Drawing.Point(83, 14);
            this.comboBoxOcrMethod.Name = "comboBoxOcrMethod";
            this.comboBoxOcrMethod.Size = new System.Drawing.Size(180, 21);
            this.comboBoxOcrMethod.TabIndex = 111;
            this.comboBoxOcrMethod.SelectedIndexChanged += new System.EventHandler(this.comboBoxOcrMethod_SelectedIndexChanged);
            // 
            // buttonGetTesseractDictionaries
            // 
            this.buttonGetTesseractDictionaries.Location = new System.Drawing.Point(269, 43);
            this.buttonGetTesseractDictionaries.Name = "buttonGetTesseractDictionaries";
            this.buttonGetTesseractDictionaries.Size = new System.Drawing.Size(29, 23);
            this.buttonGetTesseractDictionaries.TabIndex = 336;
            this.buttonGetTesseractDictionaries.Text = "...";
            this.buttonGetTesseractDictionaries.UseVisualStyleBackColor = true;
            this.buttonGetTesseractDictionaries.Click += new System.EventHandler(this.buttonGetTesseractDictionaries_Click);
            // 
            // BatchConvertOcrLanguage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 158);
            this.Controls.Add(this.buttonGetTesseractDictionaries);
            this.Controls.Add(this.comboBoxOcrMethod);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxLanguage);
            this.Controls.Add(this.labelFileNameEnding);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BatchConvertOcrLanguage";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "OCR medthod / language";
            this.Load += new System.EventHandler(this.BatchConvertMkvEnding_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BatchConvertMkvEnding_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelFileNameEnding;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label2;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLanguage;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxOcrMethod;
        private System.Windows.Forms.Button buttonGetTesseractDictionaries;
    }
}