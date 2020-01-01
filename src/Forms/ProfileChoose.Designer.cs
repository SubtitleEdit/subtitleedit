namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ProfileChoose
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
            this.listViewProfiles = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMaxLength = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderOptimalCps = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMaxCps = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMinGap = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listViewProfiles
            // 
            this.listViewProfiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewProfiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderMaxLength,
            this.columnHeaderOptimalCps,
            this.columnHeaderMaxCps,
            this.columnHeaderMinGap});
            this.listViewProfiles.FullRowSelect = true;
            this.listViewProfiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewProfiles.HideSelection = false;
            this.listViewProfiles.Location = new System.Drawing.Point(12, 12);
            this.listViewProfiles.MultiSelect = false;
            this.listViewProfiles.Name = "listViewProfiles";
            this.listViewProfiles.Size = new System.Drawing.Size(661, 197);
            this.listViewProfiles.TabIndex = 11;
            this.listViewProfiles.UseCompatibleStateImageBehavior = false;
            this.listViewProfiles.View = System.Windows.Forms.View.Details;
            this.listViewProfiles.DoubleClick += new System.EventHandler(this.listViewProfiles_DoubleClick);
            this.listViewProfiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewProfiles_KeyDown);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 150;
            // 
            // columnHeaderMaxLength
            // 
            this.columnHeaderMaxLength.Text = "Single line max. length";
            this.columnHeaderMaxLength.Width = 135;
            // 
            // columnHeaderOptimalCps
            // 
            this.columnHeaderOptimalCps.Text = "Optimal CPS";
            this.columnHeaderOptimalCps.Width = 110;
            // 
            // columnHeaderMaxCps
            // 
            this.columnHeaderMaxCps.Text = "Max. CPS";
            this.columnHeaderMaxCps.Width = 90;
            // 
            // columnHeaderMinGap
            // 
            this.columnHeaderMinGap.Text = "Min. gap (ms)";
            this.columnHeaderMinGap.Width = 145;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(598, 215);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 207;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(517, 215);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 206;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // ProfileChoose
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(685, 248);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listViewProfiles);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProfileChoose";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ProfileChoose";
            this.Shown += new System.EventHandler(this.ProfileChoose_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ProfileChoose_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewProfiles;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderMaxLength;
        private System.Windows.Forms.ColumnHeader columnHeaderOptimalCps;
        private System.Windows.Forms.ColumnHeader columnHeaderMaxCps;
        private System.Windows.Forms.ColumnHeader columnHeaderMinGap;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
    }
}