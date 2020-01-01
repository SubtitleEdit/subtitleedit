namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ChooseFontName
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
            this.buttonSearchClear = new System.Windows.Forms.Button();
            this.labelShortcutsSearch = new System.Windows.Forms.Label();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.labelPreview3 = new System.Windows.Forms.Label();
            this.labelPreview2 = new System.Windows.Forms.Label();
            this.labelPreview1 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.groupBoxPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(251, 411);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 60;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(170, 411);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 50;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonSearchClear
            // 
            this.buttonSearchClear.Enabled = false;
            this.buttonSearchClear.Location = new System.Drawing.Point(217, 14);
            this.buttonSearchClear.Name = "buttonSearchClear";
            this.buttonSearchClear.Size = new System.Drawing.Size(111, 23);
            this.buttonSearchClear.TabIndex = 10;
            this.buttonSearchClear.Text = "Clear";
            this.buttonSearchClear.UseVisualStyleBackColor = true;
            this.buttonSearchClear.Click += new System.EventHandler(this.buttonSearchClear_Click);
            // 
            // labelShortcutsSearch
            // 
            this.labelShortcutsSearch.AutoSize = true;
            this.labelShortcutsSearch.Location = new System.Drawing.Point(14, 19);
            this.labelShortcutsSearch.Name = "labelShortcutsSearch";
            this.labelShortcutsSearch.Size = new System.Drawing.Size(41, 13);
            this.labelShortcutsSearch.TabIndex = 48;
            this.labelShortcutsSearch.Text = "Search";
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Location = new System.Drawing.Point(60, 16);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(151, 20);
            this.textBoxSearch.TabIndex = 0;
            this.textBoxSearch.TextChanged += new System.EventHandler(this.textBoxSearch_TextChanged);
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.labelPreview3);
            this.groupBoxPreview.Controls.Add(this.labelPreview2);
            this.groupBoxPreview.Controls.Add(this.labelPreview1);
            this.groupBoxPreview.Location = new System.Drawing.Point(12, 274);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(314, 131);
            this.groupBoxPreview.TabIndex = 49;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // labelPreview3
            // 
            this.labelPreview3.AutoSize = true;
            this.labelPreview3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPreview3.Location = new System.Drawing.Point(22, 85);
            this.labelPreview3.Name = "labelPreview3";
            this.labelPreview3.Size = new System.Drawing.Size(211, 25);
            this.labelPreview3.TabIndex = 51;
            this.labelPreview3.Text = "Example of current font";
            // 
            // labelPreview2
            // 
            this.labelPreview2.AutoSize = true;
            this.labelPreview2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPreview2.Location = new System.Drawing.Point(22, 50);
            this.labelPreview2.Name = "labelPreview2";
            this.labelPreview2.Size = new System.Drawing.Size(174, 20);
            this.labelPreview2.TabIndex = 50;
            this.labelPreview2.Text = "Example of current font";
            // 
            // labelPreview1
            // 
            this.labelPreview1.AutoSize = true;
            this.labelPreview1.Location = new System.Drawing.Point(22, 25);
            this.labelPreview1.Name = "labelPreview1";
            this.labelPreview1.Size = new System.Drawing.Size(116, 13);
            this.labelPreview1.TabIndex = 49;
            this.labelPreview1.Text = "Example of current font";
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(17, 42);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(309, 225);
            this.listBox1.TabIndex = 61;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // ChooseFontName
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(338, 444);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonSearchClear);
            this.Controls.Add(this.labelShortcutsSearch);
            this.Controls.Add(this.textBoxSearch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChooseFontName";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ChooseFontName";
            this.Load += new System.EventHandler(this.ChooseFontName_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChooseFontName_KeyDown);
            this.groupBoxPreview.ResumeLayout(false);
            this.groupBoxPreview.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonSearchClear;
        private System.Windows.Forms.Label labelShortcutsSearch;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.Label labelPreview3;
        private System.Windows.Forms.Label labelPreview2;
        private System.Windows.Forms.Label labelPreview1;
        private System.Windows.Forms.ListBox listBox1;
    }
}