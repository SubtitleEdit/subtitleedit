namespace Nikse.SubtitleEdit.Forms
{
    partial class GoogleOrMicrosoftTranslate
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
            this.buttonMicrosoft = new System.Windows.Forms.Button();
            this.buttonGoogle = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxFrom = new System.Windows.Forms.ComboBox();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.labelTo = new System.Windows.Forms.Label();
            this.comboBoxTo = new System.Windows.Forms.ComboBox();
            this.labelFrom = new System.Windows.Forms.Label();
            this.textBoxSourceText = new System.Windows.Forms.TextBox();
            this.labelSourceText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonMicrosoft
            // 
            this.buttonMicrosoft.Location = new System.Drawing.Point(322, 164);
            this.buttonMicrosoft.Name = "buttonMicrosoft";
            this.buttonMicrosoft.Size = new System.Drawing.Size(304, 55);
            this.buttonMicrosoft.TabIndex = 5;
            this.buttonMicrosoft.UseVisualStyleBackColor = true;
            this.buttonMicrosoft.Click += new System.EventHandler(this.buttonMicrosoft_Click);
            // 
            // buttonGoogle
            // 
            this.buttonGoogle.Location = new System.Drawing.Point(12, 164);
            this.buttonGoogle.Name = "buttonGoogle";
            this.buttonGoogle.Size = new System.Drawing.Size(304, 55);
            this.buttonGoogle.TabIndex = 4;
            this.buttonGoogle.UseVisualStyleBackColor = true;
            this.buttonGoogle.Click += new System.EventHandler(this.buttonGoogle_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(551, 227);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 148);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Google translate";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(319, 148);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Microsoft translate";
            // 
            // comboBoxFrom
            // 
            this.comboBoxFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFrom.FormattingEnabled = true;
            this.comboBoxFrom.Location = new System.Drawing.Point(256, 12);
            this.comboBoxFrom.Name = "comboBoxFrom";
            this.comboBoxFrom.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFrom.TabIndex = 0;
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.Location = new System.Drawing.Point(553, 12);
            this.buttonTranslate.Name = "buttonTranslate";
            this.buttonTranslate.Size = new System.Drawing.Size(75, 21);
            this.buttonTranslate.TabIndex = 2;
            this.buttonTranslate.Text = "Translate";
            this.buttonTranslate.UseVisualStyleBackColor = true;
            this.buttonTranslate.Click += new System.EventHandler(this.buttonTranslate_Click);
            // 
            // labelTo
            // 
            this.labelTo.AutoSize = true;
            this.labelTo.Location = new System.Drawing.Point(397, 15);
            this.labelTo.Name = "labelTo";
            this.labelTo.Size = new System.Drawing.Size(23, 13);
            this.labelTo.TabIndex = 25;
            this.labelTo.Text = "To:";
            // 
            // comboBoxTo
            // 
            this.comboBoxTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTo.FormattingEnabled = true;
            this.comboBoxTo.Location = new System.Drawing.Point(426, 12);
            this.comboBoxTo.Name = "comboBoxTo";
            this.comboBoxTo.Size = new System.Drawing.Size(121, 21);
            this.comboBoxTo.TabIndex = 1;
            // 
            // labelFrom
            // 
            this.labelFrom.AutoSize = true;
            this.labelFrom.Location = new System.Drawing.Point(214, 19);
            this.labelFrom.Name = "labelFrom";
            this.labelFrom.Size = new System.Drawing.Size(33, 13);
            this.labelFrom.TabIndex = 23;
            this.labelFrom.Text = "From:";
            // 
            // textBoxSourceText
            // 
            this.textBoxSourceText.Location = new System.Drawing.Point(17, 71);
            this.textBoxSourceText.Multiline = true;
            this.textBoxSourceText.Name = "textBoxSourceText";
            this.textBoxSourceText.Size = new System.Drawing.Size(299, 53);
            this.textBoxSourceText.TabIndex = 3;
            // 
            // labelSourceText
            // 
            this.labelSourceText.AutoSize = true;
            this.labelSourceText.Location = new System.Drawing.Point(14, 55);
            this.labelSourceText.Name = "labelSourceText";
            this.labelSourceText.Size = new System.Drawing.Size(61, 13);
            this.labelSourceText.TabIndex = 28;
            this.labelSourceText.Text = "Source text";
            // 
            // GoogleOrMicrosoftTranslate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(638, 260);
            this.Controls.Add(this.labelSourceText);
            this.Controls.Add(this.textBoxSourceText);
            this.Controls.Add(this.comboBoxFrom);
            this.Controls.Add(this.buttonTranslate);
            this.Controls.Add(this.labelTo);
            this.Controls.Add(this.comboBoxTo);
            this.Controls.Add(this.labelFrom);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonGoogle);
            this.Controls.Add(this.buttonMicrosoft);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GoogleOrMicrosoftTranslate";
            this.Text = "GoogleOrMicrosoftTranslate";
            this.Shown += new System.EventHandler(this.GoogleOrMicrosoftTranslate_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonMicrosoft;
        private System.Windows.Forms.Button buttonGoogle;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxFrom;
        private System.Windows.Forms.Button buttonTranslate;
        private System.Windows.Forms.Label labelTo;
        private System.Windows.Forms.ComboBox comboBoxTo;
        private System.Windows.Forms.Label labelFrom;
        private System.Windows.Forms.TextBox textBoxSourceText;
        private System.Windows.Forms.Label labelSourceText;
    }
}