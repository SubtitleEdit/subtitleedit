namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ChooseStyle
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
            this.groupBoxStyles = new System.Windows.Forms.GroupBox();
            this.listViewStyles = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFontName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFontSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderUseCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPrimaryColor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderOutline = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxStyles.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxStyles
            // 
            this.groupBoxStyles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxStyles.Controls.Add(this.listViewStyles);
            this.groupBoxStyles.Location = new System.Drawing.Point(12, 12);
            this.groupBoxStyles.Name = "groupBoxStyles";
            this.groupBoxStyles.Size = new System.Drawing.Size(700, 424);
            this.groupBoxStyles.TabIndex = 1;
            this.groupBoxStyles.TabStop = false;
            this.groupBoxStyles.Text = "Styles";
            // 
            // listViewStyles
            // 
            this.listViewStyles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewStyles.CheckBoxes = true;
            this.listViewStyles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderFontName,
            this.columnHeaderFontSize,
            this.columnHeaderUseCount,
            this.columnHeaderPrimaryColor,
            this.columnHeaderOutline});
            this.listViewStyles.FullRowSelect = true;
            this.listViewStyles.HideSelection = false;
            this.listViewStyles.Location = new System.Drawing.Point(6, 19);
            this.listViewStyles.MultiSelect = false;
            this.listViewStyles.Name = "listViewStyles";
            this.listViewStyles.Size = new System.Drawing.Size(688, 399);
            this.listViewStyles.TabIndex = 0;
            this.listViewStyles.UseCompatibleStateImageBehavior = false;
            this.listViewStyles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 130;
            // 
            // columnHeaderFontName
            // 
            this.columnHeaderFontName.Text = "Font name";
            this.columnHeaderFontName.Width = 128;
            // 
            // columnHeaderFontSize
            // 
            this.columnHeaderFontSize.Text = "Font size";
            this.columnHeaderFontSize.Width = 85;
            // 
            // columnHeaderUseCount
            // 
            this.columnHeaderUseCount.Text = "Used#";
            this.columnHeaderUseCount.Width = 65;
            // 
            // columnHeaderPrimaryColor
            // 
            this.columnHeaderPrimaryColor.Text = "Primary";
            // 
            // columnHeaderOutline
            // 
            this.columnHeaderOutline.Text = "Outline";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(637, 442);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(556, 442);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // ChooseStyle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(724, 475);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxStyles);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChooseStyle";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose style";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChooseStyle_KeyDown);
            this.ResizeEnd += new System.EventHandler(this.ChooseStyle_ResizeEnd);
            this.Shown += new System.EventHandler(this.ChooseStyle_Shown);
            this.groupBoxStyles.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxStyles;
        private System.Windows.Forms.ListView listViewStyles;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderFontName;
        private System.Windows.Forms.ColumnHeader columnHeaderFontSize;
        private System.Windows.Forms.ColumnHeader columnHeaderUseCount;
        private System.Windows.Forms.ColumnHeader columnHeaderPrimaryColor;
        private System.Windows.Forms.ColumnHeader columnHeaderOutline;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
    }
}