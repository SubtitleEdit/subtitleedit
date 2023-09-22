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
            this.components = new System.ComponentModel.Container();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSelAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInvertSel = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonSearchClear = new System.Windows.Forms.Button();
            this.labelSearch = new System.Windows.Forms.Label();
            this.labelDefaultLanguagesList = new System.Windows.Forms.Label();
            this.textBoxSearch = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
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
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSelAll,
            this.toolStripMenuItemInvertSel});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(155, 48);
            // 
            // toolStripMenuItemSelAll
            // 
            this.toolStripMenuItemSelAll.Name = "toolStripMenuItemSelAll";
            this.toolStripMenuItemSelAll.Size = new System.Drawing.Size(154, 22);
            this.toolStripMenuItemSelAll.Text = "Select all";
            this.toolStripMenuItemSelAll.Click += new System.EventHandler(this.toolStripMenuItemSelAll_Click);
            // 
            // toolStripMenuItemInvertSel
            // 
            this.toolStripMenuItemInvertSel.Name = "toolStripMenuItemInvertSel";
            this.toolStripMenuItemInvertSel.Size = new System.Drawing.Size(154, 22);
            this.toolStripMenuItemInvertSel.Text = "Invert selection";
            this.toolStripMenuItemInvertSel.Click += new System.EventHandler(this.toolStripMenuItemInvertSel_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(335, 537);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 24);
            this.buttonCancel.TabIndex = 51;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(254, 537);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 24);
            this.buttonOK.TabIndex = 50;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonShortcutsClear
            // 
            this.buttonSearchClear.Enabled = false;
            this.buttonSearchClear.Location = new System.Drawing.Point(215, 17);
            this.buttonSearchClear.Name = "buttonSearchClear";
            this.buttonSearchClear.Size = new System.Drawing.Size(111, 23);
            this.buttonSearchClear.TabIndex = 10;
            this.buttonSearchClear.Text = "Clear";
            this.buttonSearchClear.UseVisualStyleBackColor = true;
            this.buttonSearchClear.Click += new System.EventHandler(this.buttonShortcutsClear_Click);
            // 
            // labelShortcutsSearch
            // 
            this.labelSearch.AutoSize = true;
            this.labelSearch.Location = new System.Drawing.Point(12, 21);
            this.labelSearch.Name = "labelSearch";
            this.labelSearch.Size = new System.Drawing.Size(41, 13);
            this.labelSearch.TabIndex = 40;
            this.labelSearch.Text = "Search";
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
            this.textBoxSearch.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxSearch.Location = new System.Drawing.Point(59, 17);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(149, 20);
            this.textBoxSearch.TabIndex = 1;
            this.textBoxSearch.TextChanged += new System.EventHandler(this.textBoxShortcutSearch_TextChanged);
            // 
            // DefaultLanguagesChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(422, 573);
            this.Controls.Add(this.labelDefaultLanguagesList);
            this.Controls.Add(this.textBoxSearch);
            this.Controls.Add(this.buttonSearchClear);
            this.Controls.Add(this.labelSearch);
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
            this.ResizeEnd += new System.EventHandler(this.DefaultLanguagesChooser_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DefaultLanguagesChooser_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button buttonSearchClear;
        private System.Windows.Forms.Label labelSearch;
        private Controls.NikseTextBox textBoxSearch;
        private System.Windows.Forms.Label labelDefaultLanguagesList;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInvertSel;
    }
}