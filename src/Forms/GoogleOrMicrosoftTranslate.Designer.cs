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
            buttonMicrosoft = new System.Windows.Forms.Button();
            buttonGoogle = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            labelGoogleTranslate = new System.Windows.Forms.Label();
            labelMicrosoftTranslate = new System.Windows.Forms.Label();
            comboBoxFrom = new System.Windows.Forms.ComboBox();
            buttonTranslate = new System.Windows.Forms.Button();
            labelTo = new System.Windows.Forms.Label();
            comboBoxTo = new System.Windows.Forms.ComboBox();
            labelFrom = new System.Windows.Forms.Label();
            textBoxSourceText = new System.Windows.Forms.TextBox();
            labelSourceText = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // buttonMicrosoft
            // 
            buttonMicrosoft.Location = new System.Drawing.Point(322, 164);
            buttonMicrosoft.Name = "buttonMicrosoft";
            buttonMicrosoft.Size = new System.Drawing.Size(304, 55);
            buttonMicrosoft.TabIndex = 5;
            buttonMicrosoft.UseVisualStyleBackColor = true;
            buttonMicrosoft.Click += new System.EventHandler(buttonMicrosoft_Click);
            // 
            // buttonGoogle
            // 
            buttonGoogle.Location = new System.Drawing.Point(12, 164);
            buttonGoogle.Name = "buttonGoogle";
            buttonGoogle.Size = new System.Drawing.Size(304, 55);
            buttonGoogle.TabIndex = 4;
            buttonGoogle.UseVisualStyleBackColor = true;
            buttonGoogle.Click += new System.EventHandler(buttonGoogle_Click);
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(551, 227);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(75, 21);
            buttonCancel.TabIndex = 6;
            buttonCancel.Text = "C&ancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelGoogleTranslate
            // 
            labelGoogleTranslate.AutoSize = true;
            labelGoogleTranslate.Location = new System.Drawing.Point(14, 148);
            labelGoogleTranslate.Name = "labelGoogleTranslate";
            labelGoogleTranslate.Size = new System.Drawing.Size(84, 13);
            labelGoogleTranslate.TabIndex = 20;
            labelGoogleTranslate.Text = "Google translate";
            // 
            // labelMicrosoftTranslate
            // 
            labelMicrosoftTranslate.AutoSize = true;
            labelMicrosoftTranslate.Location = new System.Drawing.Point(319, 148);
            labelMicrosoftTranslate.Name = "labelMicrosoftTranslate";
            labelMicrosoftTranslate.Size = new System.Drawing.Size(93, 13);
            labelMicrosoftTranslate.TabIndex = 21;
            labelMicrosoftTranslate.Text = "Microsoft translate";
            // 
            // comboBoxFrom
            // 
            comboBoxFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxFrom.FormattingEnabled = true;
            comboBoxFrom.Location = new System.Drawing.Point(256, 12);
            comboBoxFrom.Name = "comboBoxFrom";
            comboBoxFrom.Size = new System.Drawing.Size(121, 21);
            comboBoxFrom.TabIndex = 0;
            // 
            // buttonTranslate
            // 
            buttonTranslate.Location = new System.Drawing.Point(553, 12);
            buttonTranslate.Name = "buttonTranslate";
            buttonTranslate.Size = new System.Drawing.Size(75, 21);
            buttonTranslate.TabIndex = 2;
            buttonTranslate.Text = "Translate";
            buttonTranslate.UseVisualStyleBackColor = true;
            buttonTranslate.Click += new System.EventHandler(buttonTranslate_Click);
            // 
            // labelTo
            // 
            labelTo.AutoSize = true;
            labelTo.Location = new System.Drawing.Point(397, 15);
            labelTo.Name = "labelTo";
            labelTo.Size = new System.Drawing.Size(23, 13);
            labelTo.TabIndex = 25;
            labelTo.Text = "To:";
            // 
            // comboBoxTo
            // 
            comboBoxTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxTo.FormattingEnabled = true;
            comboBoxTo.Location = new System.Drawing.Point(426, 12);
            comboBoxTo.Name = "comboBoxTo";
            comboBoxTo.Size = new System.Drawing.Size(121, 21);
            comboBoxTo.TabIndex = 1;
            // 
            // labelFrom
            // 
            labelFrom.AutoSize = true;
            labelFrom.Location = new System.Drawing.Point(214, 19);
            labelFrom.Name = "labelFrom";
            labelFrom.Size = new System.Drawing.Size(33, 13);
            labelFrom.TabIndex = 23;
            labelFrom.Text = "From:";
            // 
            // textBoxSourceText
            // 
            textBoxSourceText.Location = new System.Drawing.Point(17, 71);
            textBoxSourceText.Multiline = true;
            textBoxSourceText.Name = "textBoxSourceText";
            textBoxSourceText.Size = new System.Drawing.Size(299, 53);
            textBoxSourceText.TabIndex = 3;
            // 
            // labelSourceText
            // 
            labelSourceText.AutoSize = true;
            labelSourceText.Location = new System.Drawing.Point(14, 55);
            labelSourceText.Name = "labelSourceText";
            labelSourceText.Size = new System.Drawing.Size(61, 13);
            labelSourceText.TabIndex = 28;
            labelSourceText.Text = "Source text";
            // 
            // GoogleOrMicrosoftTranslate
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(638, 260);
            Controls.Add(labelSourceText);
            Controls.Add(textBoxSourceText);
            Controls.Add(comboBoxFrom);
            Controls.Add(buttonTranslate);
            Controls.Add(labelTo);
            Controls.Add(comboBoxTo);
            Controls.Add(labelFrom);
            Controls.Add(labelMicrosoftTranslate);
            Controls.Add(labelGoogleTranslate);
            Controls.Add(buttonCancel);
            Controls.Add(buttonGoogle);
            Controls.Add(buttonMicrosoft);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GoogleOrMicrosoftTranslate";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "GoogleOrMicrosoftTranslate";
            Shown += new System.EventHandler(GoogleOrMicrosoftTranslate_Shown);
            KeyDown += new System.Windows.Forms.KeyEventHandler(GoogleOrMicrosoftTranslate_KeyDown);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonMicrosoft;
        private System.Windows.Forms.Button buttonGoogle;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelGoogleTranslate;
        private System.Windows.Forms.Label labelMicrosoftTranslate;
        private System.Windows.Forms.ComboBox comboBoxFrom;
        private System.Windows.Forms.Button buttonTranslate;
        private System.Windows.Forms.Label labelTo;
        private System.Windows.Forms.ComboBox comboBoxTo;
        private System.Windows.Forms.Label labelFrom;
        private System.Windows.Forms.TextBox textBoxSourceText;
        private System.Windows.Forms.Label labelSourceText;
    }
}