namespace Nikse.SubtitleEdit.Forms
{
    partial class ChangeCasingNames
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxNames = new System.Windows.Forms.GroupBox();
            this.listViewNames = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxLinesFound = new System.Windows.Forms.GroupBox();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelXLinesSelected = new System.Windows.Forms.Label();
            this.groupBoxNames.SuspendLayout();
            this.groupBoxLinesFound.SuspendLayout();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(570, 559);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 14;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(651, 559);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 15;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            //
            // groupBoxNames
            //
            this.groupBoxNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNames.Controls.Add(this.listViewNames);
            this.groupBoxNames.Location = new System.Drawing.Point(5, 9);
            this.groupBoxNames.Name = "groupBoxNames";
            this.groupBoxNames.Size = new System.Drawing.Size(726, 268);
            this.groupBoxNames.TabIndex = 12;
            this.groupBoxNames.TabStop = false;
            this.groupBoxNames.Text = "Names found in subtitle";
            //
            // listViewNames
            //
            this.listViewNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewNames.CheckBoxes = true;
            this.listViewNames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewNames.FullRowSelect = true;
            this.listViewNames.HideSelection = false;
            this.listViewNames.Location = new System.Drawing.Point(6, 19);
            this.listViewNames.MultiSelect = false;
            this.listViewNames.Name = "listViewNames";
            this.listViewNames.Size = new System.Drawing.Size(714, 243);
            this.listViewNames.TabIndex = 7;
            this.listViewNames.UseCompatibleStateImageBehavior = false;
            this.listViewNames.View = System.Windows.Forms.View.Details;
            this.listViewNames.SelectedIndexChanged += new System.EventHandler(this.ListViewNamesSelectedIndexChanged);
            //
            // columnHeader1
            //
            this.columnHeader1.Text = "Enabled";
            this.columnHeader1.Width = 70;
            //
            // columnHeader2
            //
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 620;
            //
            // groupBoxLinesFound
            //
            this.groupBoxLinesFound.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLinesFound.Controls.Add(this.listViewFixes);
            this.groupBoxLinesFound.Location = new System.Drawing.Point(5, 283);
            this.groupBoxLinesFound.Name = "groupBoxLinesFound";
            this.groupBoxLinesFound.Size = new System.Drawing.Size(726, 268);
            this.groupBoxLinesFound.TabIndex = 13;
            this.groupBoxLinesFound.TabStop = false;
            this.groupBoxLinesFound.Text = "Lines found: {0}";
            //
            // listViewFixes
            //
            this.listViewFixes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader7,
            this.columnHeader8});
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(6, 23);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(714, 239);
            this.listViewFixes.TabIndex = 9;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            this.listViewFixes.SelectedIndexChanged += new System.EventHandler(this.listViewFixes_SelectedIndexChanged);
            //
            // columnHeader4
            //
            this.columnHeader4.Text = "Apply";
            this.columnHeader4.Width = 45;
            //
            // columnHeader5
            //
            this.columnHeader5.Text = "Line#";
            this.columnHeader5.Width = 61;
            //
            // columnHeader7
            //
            this.columnHeader7.Text = "Before";
            this.columnHeader7.Width = 292;
            //
            // columnHeader8
            //
            this.columnHeader8.Text = "After";
            this.columnHeader8.Width = 292;
            //
            // labelXLinesSelected
            //
            this.labelXLinesSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelXLinesSelected.AutoSize = true;
            this.labelXLinesSelected.Location = new System.Drawing.Point(5, 559);
            this.labelXLinesSelected.Name = "labelXLinesSelected";
            this.labelXLinesSelected.Size = new System.Drawing.Size(78, 13);
            this.labelXLinesSelected.TabIndex = 16;
            this.labelXLinesSelected.Text = "XLinesSelected";
            //
            // ChangeCasingNames
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(736, 589);
            this.Controls.Add(this.labelXLinesSelected);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxNames);
            this.Controls.Add(this.groupBoxLinesFound);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChangeCasingNames";
            this.ShowIcon = false;
            this.Text = "Change casing - Names";
            this.Shown += new System.EventHandler(this.ChangeCasingNames_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChangeCasingNames_KeyDown);
            this.groupBoxNames.ResumeLayout(false);
            this.groupBoxLinesFound.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxNames;
        private System.Windows.Forms.ListView listViewNames;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.GroupBox groupBoxLinesFound;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.Label labelXLinesSelected;
    }
}