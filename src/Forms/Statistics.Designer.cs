namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class Statistics
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
            this.groupBoxGeneral = new System.Windows.Forms.GroupBox();
            this.textBoxGeneral = new System.Windows.Forms.TextBox();
            this.groupBoxMostUsed = new System.Windows.Forms.GroupBox();
            this.labelMostUsedLines = new System.Windows.Forms.Label();
            this.labelMostUsedWords = new System.Windows.Forms.Label();
            this.textBoxMostUsedLines = new System.Windows.Forms.TextBox();
            this.textBoxMostUsedWords = new System.Windows.Forms.TextBox();
            this.buttonExport = new System.Windows.Forms.Button();
            this.groupBoxGeneral.SuspendLayout();
            this.groupBoxMostUsed.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(642, 632);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxGeneral
            // 
            this.groupBoxGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxGeneral.Controls.Add(this.textBoxGeneral);
            this.groupBoxGeneral.Location = new System.Drawing.Point(15, 12);
            this.groupBoxGeneral.Name = "groupBoxGeneral";
            this.groupBoxGeneral.Size = new System.Drawing.Size(705, 376);
            this.groupBoxGeneral.TabIndex = 0;
            this.groupBoxGeneral.TabStop = false;
            this.groupBoxGeneral.Text = "General statistics";
            // 
            // textBoxGeneral
            // 
            this.textBoxGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGeneral.Location = new System.Drawing.Point(6, 19);
            this.textBoxGeneral.Multiline = true;
            this.textBoxGeneral.Name = "textBoxGeneral";
            this.textBoxGeneral.ReadOnly = true;
            this.textBoxGeneral.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxGeneral.Size = new System.Drawing.Size(694, 351);
            this.textBoxGeneral.TabIndex = 0;
            // 
            // groupBoxMostUsed
            // 
            this.groupBoxMostUsed.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMostUsed.Controls.Add(this.labelMostUsedLines);
            this.groupBoxMostUsed.Controls.Add(this.labelMostUsedWords);
            this.groupBoxMostUsed.Controls.Add(this.textBoxMostUsedLines);
            this.groupBoxMostUsed.Controls.Add(this.textBoxMostUsedWords);
            this.groupBoxMostUsed.Location = new System.Drawing.Point(12, 394);
            this.groupBoxMostUsed.Name = "groupBoxMostUsed";
            this.groupBoxMostUsed.Size = new System.Drawing.Size(705, 232);
            this.groupBoxMostUsed.TabIndex = 1;
            this.groupBoxMostUsed.TabStop = false;
            this.groupBoxMostUsed.Text = "Most used";
            // 
            // labelMostUsedLines
            // 
            this.labelMostUsedLines.AutoSize = true;
            this.labelMostUsedLines.Location = new System.Drawing.Point(329, 27);
            this.labelMostUsedLines.Name = "labelMostUsedLines";
            this.labelMostUsedLines.Size = new System.Drawing.Size(80, 13);
            this.labelMostUsedLines.TabIndex = 2;
            this.labelMostUsedLines.Text = "Most used lines";
            // 
            // labelMostUsedWords
            // 
            this.labelMostUsedWords.AutoSize = true;
            this.labelMostUsedWords.Location = new System.Drawing.Point(6, 27);
            this.labelMostUsedWords.Name = "labelMostUsedWords";
            this.labelMostUsedWords.Size = new System.Drawing.Size(87, 13);
            this.labelMostUsedWords.TabIndex = 0;
            this.labelMostUsedWords.Text = "Most used words";
            // 
            // textBoxMostUsedLines
            // 
            this.textBoxMostUsedLines.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMostUsedLines.Location = new System.Drawing.Point(332, 43);
            this.textBoxMostUsedLines.Multiline = true;
            this.textBoxMostUsedLines.Name = "textBoxMostUsedLines";
            this.textBoxMostUsedLines.ReadOnly = true;
            this.textBoxMostUsedLines.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxMostUsedLines.Size = new System.Drawing.Size(371, 183);
            this.textBoxMostUsedLines.TabIndex = 3;
            // 
            // textBoxMostUsedWords
            // 
            this.textBoxMostUsedWords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxMostUsedWords.Location = new System.Drawing.Point(9, 43);
            this.textBoxMostUsedWords.Multiline = true;
            this.textBoxMostUsedWords.Name = "textBoxMostUsedWords";
            this.textBoxMostUsedWords.ReadOnly = true;
            this.textBoxMostUsedWords.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxMostUsedWords.Size = new System.Drawing.Size(320, 183);
            this.textBoxMostUsedWords.TabIndex = 1;
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.Location = new System.Drawing.Point(532, 632);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(104, 23);
            this.buttonExport.TabIndex = 2;
            this.buttonExport.Text = "Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // Statistics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(732, 665);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.groupBoxMostUsed);
            this.Controls.Add(this.groupBoxGeneral);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(700, 600);
            this.Name = "Statistics";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Statistics";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Statistics_KeyDown);
            this.groupBoxGeneral.ResumeLayout(false);
            this.groupBoxGeneral.PerformLayout();
            this.groupBoxMostUsed.ResumeLayout(false);
            this.groupBoxMostUsed.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxGeneral;
        private System.Windows.Forms.TextBox textBoxGeneral;
        private System.Windows.Forms.GroupBox groupBoxMostUsed;
        private System.Windows.Forms.TextBox textBoxMostUsedWords;
        private System.Windows.Forms.TextBox textBoxMostUsedLines;
        private System.Windows.Forms.Label labelMostUsedLines;
        private System.Windows.Forms.Label labelMostUsedWords;
        private System.Windows.Forms.Button buttonExport;
    }
}