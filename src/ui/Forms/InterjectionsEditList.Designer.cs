namespace Nikse.SubtitleEdit.Forms
{
    partial class InterjectionsEditList
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.TextBoxInterjections = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.buttonSort = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.buttonEditSkipList = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(395, 460);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(316, 460);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // TextBoxInterjections
            // 
            this.TextBoxInterjections.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBoxInterjections.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.TextBoxInterjections.Location = new System.Drawing.Point(23, 42);
            this.TextBoxInterjections.Multiline = true;
            this.TextBoxInterjections.Name = "TextBoxInterjections";
            this.TextBoxInterjections.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextBoxInterjections.Size = new System.Drawing.Size(290, 406);
            this.TextBoxInterjections.TabIndex = 6;
            // 
            // buttonSort
            // 
            this.buttonSort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSort.Location = new System.Drawing.Point(319, 42);
            this.buttonSort.Name = "buttonSort";
            this.buttonSort.Size = new System.Drawing.Size(154, 23);
            this.buttonSort.TabIndex = 23;
            this.buttonSort.Text = "Sort";
            this.buttonSort.UseVisualStyleBackColor = true;
            this.buttonSort.Click += new System.EventHandler(this.buttonSort_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(23, 18);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(239, 13);
            this.labelInfo.TabIndex = 24;
            this.labelInfo.Text = "Edit all interjections (one interjection on each line)";
            // 
            // buttonEditSkipList
            // 
            this.buttonEditSkipList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEditSkipList.Location = new System.Drawing.Point(319, 71);
            this.buttonEditSkipList.Name = "buttonEditSkipList";
            this.buttonEditSkipList.Size = new System.Drawing.Size(154, 23);
            this.buttonEditSkipList.TabIndex = 27;
            this.buttonEditSkipList.Text = "Edit skip list...";
            this.buttonEditSkipList.UseVisualStyleBackColor = true;
            this.buttonEditSkipList.Click += new System.EventHandler(this.EditSkipListStartsWithToolStripMenuItem_Click);
            // 
            // InterjectionsEditList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 494);
            this.Controls.Add(this.buttonEditSkipList);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonSort);
            this.Controls.Add(this.TextBoxInterjections);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(445, 533);
            this.Name = "InterjectionsEditList";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Interjections";
            this.Shown += new System.EventHandler(this.InterjectionsEditList_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Interjections_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private Controls.NikseTextBox TextBoxInterjections;
        private System.Windows.Forms.Button buttonSort;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Button buttonEditSkipList;
    }
}