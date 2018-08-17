namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class UnknownSubtitle
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
            this.labelTitle = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.richTextBoxMessage = new System.Windows.Forms.RichTextBox();
            this.buttonImportPlainText = new System.Windows.Forms.Button();
            this.LabelPreview = new System.Windows.Forms.Label();
            this.textBoxPreview = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(15, 25);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(114, 13);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "Unknown subtitle type";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(457, 272);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(100, 27);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // richTextBoxMessage
            // 
            this.richTextBoxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxMessage.Location = new System.Drawing.Point(18, 54);
            this.richTextBoxMessage.Name = "richTextBoxMessage";
            this.richTextBoxMessage.ReadOnly = true;
            this.richTextBoxMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBoxMessage.Size = new System.Drawing.Size(539, 47);
            this.richTextBoxMessage.TabIndex = 41;
            this.richTextBoxMessage.TabStop = false;
            this.richTextBoxMessage.Text = "If you want this fixed please send an email to mailto:niksedk@gmail.com and inclu" +
    "de a copy of the subtitle.";
            // 
            // buttonImportPlainText
            // 
            this.buttonImportPlainText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImportPlainText.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonImportPlainText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonImportPlainText.Location = new System.Drawing.Point(260, 272);
            this.buttonImportPlainText.Name = "buttonImportPlainText";
            this.buttonImportPlainText.Size = new System.Drawing.Size(191, 27);
            this.buttonImportPlainText.TabIndex = 42;
            this.buttonImportPlainText.Text = "Import plain text...";
            this.buttonImportPlainText.UseVisualStyleBackColor = true;
            this.buttonImportPlainText.Click += new System.EventHandler(this.buttonImportPlainText_Click);
            // 
            // LabelPreview
            // 
            this.LabelPreview.AutoSize = true;
            this.LabelPreview.Location = new System.Drawing.Point(17, 101);
            this.LabelPreview.Name = "LabelPreview";
            this.LabelPreview.Size = new System.Drawing.Size(45, 13);
            this.LabelPreview.TabIndex = 44;
            this.LabelPreview.Text = "Preview";
            // 
            // textBoxPreview
            // 
            this.textBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPreview.Location = new System.Drawing.Point(18, 118);
            this.textBoxPreview.Multiline = true;
            this.textBoxPreview.Name = "textBoxPreview";
            this.textBoxPreview.ReadOnly = true;
            this.textBoxPreview.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxPreview.Size = new System.Drawing.Size(539, 141);
            this.textBoxPreview.TabIndex = 43;
            // 
            // UnknownSubtitle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(569, 311);
            this.Controls.Add(this.LabelPreview);
            this.Controls.Add(this.textBoxPreview);
            this.Controls.Add(this.buttonImportPlainText);
            this.Controls.Add(this.richTextBoxMessage);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelTitle);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UnknownSubtitle";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Unknown subtitle";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormUnknownSubtitle_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.RichTextBox richTextBoxMessage;
        private System.Windows.Forms.Button buttonImportPlainText;
        private System.Windows.Forms.Label LabelPreview;
        private System.Windows.Forms.TextBox textBoxPreview;
    }
}