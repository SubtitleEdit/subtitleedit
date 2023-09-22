namespace Nikse.SubtitleEdit.Forms.Options
{
    sealed partial class DefaultLanguagesChooser
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonShortcutsClear = new System.Windows.Forms.Button();
            this.labelShortcutsSearch = new System.Windows.Forms.Label();
            this.labelDefaultLanguagesList = new System.Windows.Forms.Label();
            this.textBoxShortcutSearch = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 52);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(398, 471);
            this.listView1.TabIndex = 20;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView1_ItemChecked);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(335, 538);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 51;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(254, 538);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 50;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonShortcutsClear
            // 
            this.buttonShortcutsClear.Enabled = false;
            this.buttonShortcutsClear.Location = new System.Drawing.Point(215, 17);
            this.buttonShortcutsClear.Name = "buttonShortcutsClear";
            this.buttonShortcutsClear.Size = new System.Drawing.Size(111, 23);
            this.buttonShortcutsClear.TabIndex = 10;
            this.buttonShortcutsClear.Text = "Clear";
            this.buttonShortcutsClear.UseVisualStyleBackColor = true;
            this.buttonShortcutsClear.Click += new System.EventHandler(this.buttonShortcutsClear_Click);
            // 
            // labelShortcutsSearch
            // 
            this.labelShortcutsSearch.AutoSize = true;
            this.labelShortcutsSearch.Location = new System.Drawing.Point(12, 21);
            this.labelShortcutsSearch.Name = "labelShortcutsSearch";
            this.labelShortcutsSearch.Size = new System.Drawing.Size(41, 13);
            this.labelShortcutsSearch.TabIndex = 40;
            this.labelShortcutsSearch.Text = "Search";
            // 
            // labelDefaultLanguagesList
            // 
            this.labelDefaultLanguagesList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDefaultLanguagesList.AutoSize = true;
            this.labelDefaultLanguagesList.Location = new System.Drawing.Point(12, 538);
            this.labelDefaultLanguagesList.Name = "labelDefaultLanguagesList";
            this.labelDefaultLanguagesList.Size = new System.Drawing.Size(18, 13);
            this.labelDefaultLanguagesList.TabIndex = 42;
            this.labelDefaultLanguagesList.Text = "All";
            // 
            // textBoxShortcutSearch
            // 
            this.textBoxShortcutSearch.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxShortcutSearch.Location = new System.Drawing.Point(59, 17);
            this.textBoxShortcutSearch.Name = "textBoxShortcutSearch";
            this.textBoxShortcutSearch.Size = new System.Drawing.Size(149, 20);
            this.textBoxShortcutSearch.TabIndex = 1;
            this.textBoxShortcutSearch.TextChanged += new System.EventHandler(this.textBoxShortcutSearch_TextChanged);
            // 
            // DefaultLanguagesChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(422, 573);
            this.Controls.Add(this.labelDefaultLanguagesList);
            this.Controls.Add(this.textBoxShortcutSearch);
            this.Controls.Add(this.buttonShortcutsClear);
            this.Controls.Add(this.labelShortcutsSearch);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listView1);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(438, 612);
            this.Name = "DefaultLanguagesChooser";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DefaultLanguagesChooser";
            this.Shown += new System.EventHandler(this.DefaultLanguagesChooser_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DefaultLanguagesChooser_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button buttonShortcutsClear;
        private System.Windows.Forms.Label labelShortcutsSearch;
        private Controls.NikseTextBox textBoxShortcutSearch;
        private System.Windows.Forms.Label labelDefaultLanguagesList;
    }
}