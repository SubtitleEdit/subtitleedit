
namespace Nikse.SubtitleEdit.Forms.Assa
{
    sealed partial class TagHistory
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
            this.seTextBoxOverrideStyle = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.listBoxHistory = new System.Windows.Forms.ListBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // seTextBoxOverrideStyle
            // 
            this.seTextBoxOverrideStyle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.seTextBoxOverrideStyle.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.seTextBoxOverrideStyle.CurrentLanguage = "";
            this.seTextBoxOverrideStyle.CurrentLineIndex = 0;
            this.seTextBoxOverrideStyle.HideSelection = true;
            this.seTextBoxOverrideStyle.IsDictionaryDownloaded = true;
            this.seTextBoxOverrideStyle.IsSpellCheckerInitialized = false;
            this.seTextBoxOverrideStyle.IsSpellCheckRequested = false;
            this.seTextBoxOverrideStyle.IsWrongWord = false;
            this.seTextBoxOverrideStyle.LanguageChanged = false;
            this.seTextBoxOverrideStyle.Location = new System.Drawing.Point(12, 254);
            this.seTextBoxOverrideStyle.Multiline = true;
            this.seTextBoxOverrideStyle.Name = "seTextBoxOverrideStyle";
            this.seTextBoxOverrideStyle.Padding = new System.Windows.Forms.Padding(1);
            this.seTextBoxOverrideStyle.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.seTextBoxOverrideStyle.SelectedText = "";
            this.seTextBoxOverrideStyle.SelectionLength = 0;
            this.seTextBoxOverrideStyle.SelectionStart = 0;
            this.seTextBoxOverrideStyle.Size = new System.Drawing.Size(776, 152);
            this.seTextBoxOverrideStyle.TabIndex = 0;
            // 
            // listBoxHistory
            // 
            this.listBoxHistory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxHistory.FormattingEnabled = true;
            this.listBoxHistory.Location = new System.Drawing.Point(12, 36);
            this.listBoxHistory.Name = "listBoxHistory";
            this.listBoxHistory.Size = new System.Drawing.Size(776, 212);
            this.listBoxHistory.TabIndex = 1;
            this.listBoxHistory.SelectedIndexChanged += new System.EventHandler(this.listBoxHistory_SelectedIndexChanged);
            this.listBoxHistory.DoubleClick += new System.EventHandler(this.listBoxHistory_DoubleClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(713, 415);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 217;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(632, 415);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 216;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // TagHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listBoxHistory);
            this.Controls.Add(this.seTextBoxOverrideStyle);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(816, 489);
            this.Name = "TagHistory";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TagHistory";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TagHistory_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.SETextBox seTextBoxOverrideStyle;
        private System.Windows.Forms.ListBox listBoxHistory;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
    }
}