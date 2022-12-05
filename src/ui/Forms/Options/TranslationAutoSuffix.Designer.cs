namespace Nikse.SubtitleEdit.Forms.Options
{
    partial class TranslationAutoSuffix
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
            this.listViewNames = new System.Windows.Forms.ListView();
            this.columnHeaderNames = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonRemoveNameEtc = new System.Windows.Forms.Button();
            this.textBoxNameEtc = new System.Windows.Forms.TextBox();
            this.buttonAddNames = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(259, 221);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 18;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(178, 221);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 17;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // listViewNames
            // 
            this.listViewNames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderNames});
            this.listViewNames.FullRowSelect = true;
            this.listViewNames.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewNames.HideSelection = false;
            this.listViewNames.Location = new System.Drawing.Point(19, 21);
            this.listViewNames.Name = "listViewNames";
            this.listViewNames.Size = new System.Drawing.Size(151, 132);
            this.listViewNames.TabIndex = 31;
            this.listViewNames.UseCompatibleStateImageBehavior = false;
            this.listViewNames.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderNames
            // 
            this.columnHeaderNames.Width = 144;
            // 
            // buttonRemoveNameEtc
            // 
            this.buttonRemoveNameEtc.Location = new System.Drawing.Point(173, 19);
            this.buttonRemoveNameEtc.Name = "buttonRemoveNameEtc";
            this.buttonRemoveNameEtc.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveNameEtc.TabIndex = 28;
            this.buttonRemoveNameEtc.Text = "Remove";
            this.buttonRemoveNameEtc.UseVisualStyleBackColor = true;
            this.buttonRemoveNameEtc.Click += new System.EventHandler(this.buttonRemoveNameEtc_Click);
            // 
            // textBoxNameEtc
            // 
            this.textBoxNameEtc.Location = new System.Drawing.Point(19, 159);
            this.textBoxNameEtc.Name = "textBoxNameEtc";
            this.textBoxNameEtc.Size = new System.Drawing.Size(151, 20);
            this.textBoxNameEtc.TabIndex = 29;
            // 
            // buttonAddNames
            // 
            this.buttonAddNames.Location = new System.Drawing.Point(176, 156);
            this.buttonAddNames.Name = "buttonAddNames";
            this.buttonAddNames.Size = new System.Drawing.Size(75, 23);
            this.buttonAddNames.TabIndex = 30;
            this.buttonAddNames.Text = "Add name";
            this.buttonAddNames.UseVisualStyleBackColor = true;
            this.buttonAddNames.Click += new System.EventHandler(this.buttonAddNames_Click);
            // 
            // TranslationAutoSuffix
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 256);
            this.Controls.Add(this.listViewNames);
            this.Controls.Add(this.buttonRemoveNameEtc);
            this.Controls.Add(this.textBoxNameEtc);
            this.Controls.Add(this.buttonAddNames);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TranslationAutoSuffix";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TranslationAutoSuffix";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ListView listViewNames;
        private System.Windows.Forms.ColumnHeader columnHeaderNames;
        private System.Windows.Forms.Button buttonRemoveNameEtc;
        private System.Windows.Forms.TextBox textBoxNameEtc;
        private System.Windows.Forms.Button buttonAddNames;
    }
}