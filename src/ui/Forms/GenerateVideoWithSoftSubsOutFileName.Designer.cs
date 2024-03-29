namespace Nikse.SubtitleEdit.Forms
{
    partial class GenerateVideoWithSoftSubsOutFileName
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
            this.textBoxSuffix = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelSuffix = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.textBoxReplaceList = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.SuspendLayout();
            // 
            // textBoxSuffix
            // 
            this.textBoxSuffix.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSuffix.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxSuffix.Location = new System.Drawing.Point(12, 42);
            this.textBoxSuffix.Name = "textBoxSuffix";
            this.textBoxSuffix.Size = new System.Drawing.Size(354, 20);
            this.textBoxSuffix.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 98);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Remove from file name";
            // 
            // labelSuffix
            // 
            this.labelSuffix.AutoSize = true;
            this.labelSuffix.Location = new System.Drawing.Point(12, 26);
            this.labelSuffix.Name = "labelSuffix";
            this.labelSuffix.Size = new System.Drawing.Size(79, 13);
            this.labelSuffix.TabIndex = 3;
            this.labelSuffix.Text = "File name suffix";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(291, 334);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 103;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(210, 334);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 102;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // textBoxReplaceList
            // 
            this.textBoxReplaceList.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxReplaceList.CurrentLanguage = "";
            this.textBoxReplaceList.CurrentLineIndex = 0;
            this.textBoxReplaceList.HideSelection = true;
            this.textBoxReplaceList.IsDictionaryDownloaded = true;
            this.textBoxReplaceList.IsSpellCheckerInitialized = false;
            this.textBoxReplaceList.IsSpellCheckRequested = false;
            this.textBoxReplaceList.IsWrongWord = false;
            this.textBoxReplaceList.LanguageChanged = false;
            this.textBoxReplaceList.Location = new System.Drawing.Point(15, 115);
            this.textBoxReplaceList.MaxLength = 32767;
            this.textBoxReplaceList.Multiline = true;
            this.textBoxReplaceList.Name = "textBoxReplaceList";
            this.textBoxReplaceList.Padding = new System.Windows.Forms.Padding(1);
            this.textBoxReplaceList.ReadOnly = false;
            this.textBoxReplaceList.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.textBoxReplaceList.SelectedText = "";
            this.textBoxReplaceList.SelectionLength = 0;
            this.textBoxReplaceList.SelectionStart = 0;
            this.textBoxReplaceList.Size = new System.Drawing.Size(351, 213);
            this.textBoxReplaceList.TabIndex = 20;
            this.textBoxReplaceList.TextBoxFont = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.textBoxReplaceList.UseSystemPasswordChar = false;
            // 
            // GenerateVideoWithSoftSubsOutFileName
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 369);
            this.Controls.Add(this.textBoxReplaceList);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelSuffix);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxSuffix);
            this.KeyPreview = true;
            this.MaximumSize = new System.Drawing.Size(394, 408);
            this.Name = "GenerateVideoWithSoftSubsOutFileName";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Output file name settings";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenerateVideoWithSoftSubsOutFileName_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.NikseTextBox textBoxSuffix;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelSuffix;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private Controls.SETextBox textBoxReplaceList;
    }
}