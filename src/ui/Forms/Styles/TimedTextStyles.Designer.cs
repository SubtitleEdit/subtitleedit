namespace Nikse.SubtitleEdit.Forms.Styles
{
    partial class TimedTextStyles
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
            this.groupBoxProperties = new System.Windows.Forms.GroupBox();
            this.textBoxStyleName = new System.Windows.Forms.TextBox();
            this.labelStyleName = new System.Windows.Forms.Label();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.textBoxFontSize = new System.Windows.Forms.TextBox();
            this.comboBoxFontWeight = new System.Windows.Forms.ComboBox();
            this.labelFontWeight = new System.Windows.Forms.Label();
            this.comboBoxFontStyle = new System.Windows.Forms.ComboBox();
            this.labelFontStyle = new System.Windows.Forms.Label();
            this.comboBoxFontName = new System.Windows.Forms.ComboBox();
            this.panelFontColor = new System.Windows.Forms.Panel();
            this.labelFontSize = new System.Windows.Forms.Label();
            this.buttonFontColor = new System.Windows.Forms.Button();
            this.labelFontName = new System.Windows.Forms.Label();
            this.groupBoxStyles = new System.Windows.Forms.GroupBox();
            this.buttonRemoveAll = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.listViewStyles = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFontName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFontSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderColor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderUsed = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colorDialogStyle = new System.Windows.Forms.ColorDialog();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxProperties.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.groupBoxFont.SuspendLayout();
            this.groupBoxStyles.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxProperties
            // 
            this.groupBoxProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxProperties.Controls.Add(this.textBoxStyleName);
            this.groupBoxProperties.Controls.Add(this.labelStyleName);
            this.groupBoxProperties.Controls.Add(this.groupBoxPreview);
            this.groupBoxProperties.Controls.Add(this.groupBoxFont);
            this.groupBoxProperties.Location = new System.Drawing.Point(518, 15);
            this.groupBoxProperties.Name = "groupBoxProperties";
            this.groupBoxProperties.Size = new System.Drawing.Size(313, 352);
            this.groupBoxProperties.TabIndex = 2;
            this.groupBoxProperties.TabStop = false;
            this.groupBoxProperties.Text = "Properties";
            // 
            // textBoxStyleName
            // 
            this.textBoxStyleName.Location = new System.Drawing.Point(49, 22);
            this.textBoxStyleName.Name = "textBoxStyleName";
            this.textBoxStyleName.Size = new System.Drawing.Size(246, 20);
            this.textBoxStyleName.TabIndex = 0;
            this.textBoxStyleName.TextChanged += new System.EventHandler(this.textBoxStyleName_TextChanged);
            // 
            // labelStyleName
            // 
            this.labelStyleName.AutoSize = true;
            this.labelStyleName.Location = new System.Drawing.Point(7, 26);
            this.labelStyleName.Name = "labelStyleName";
            this.labelStyleName.Size = new System.Drawing.Size(35, 13);
            this.labelStyleName.TabIndex = 0;
            this.labelStyleName.Text = "Name";
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.pictureBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(10, 226);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(297, 120);
            this.groupBoxPreview.TabIndex = 2;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 16);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(291, 101);
            this.pictureBoxPreview.TabIndex = 0;
            this.pictureBoxPreview.TabStop = false;
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFont.Controls.Add(this.textBoxFontSize);
            this.groupBoxFont.Controls.Add(this.comboBoxFontWeight);
            this.groupBoxFont.Controls.Add(this.labelFontWeight);
            this.groupBoxFont.Controls.Add(this.comboBoxFontStyle);
            this.groupBoxFont.Controls.Add(this.labelFontStyle);
            this.groupBoxFont.Controls.Add(this.comboBoxFontName);
            this.groupBoxFont.Controls.Add(this.panelFontColor);
            this.groupBoxFont.Controls.Add(this.labelFontSize);
            this.groupBoxFont.Controls.Add(this.buttonFontColor);
            this.groupBoxFont.Controls.Add(this.labelFontName);
            this.groupBoxFont.Location = new System.Drawing.Point(7, 51);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(300, 169);
            this.groupBoxFont.TabIndex = 3;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "Font";
            // 
            // textBoxFontSize
            // 
            this.textBoxFontSize.Location = new System.Drawing.Point(100, 43);
            this.textBoxFontSize.Name = "textBoxFontSize";
            this.textBoxFontSize.Size = new System.Drawing.Size(188, 20);
            this.textBoxFontSize.TabIndex = 1;
            this.textBoxFontSize.TextChanged += new System.EventHandler(this.textBoxFontSize_TextChanged);
            // 
            // comboBoxFontWeight
            // 
            this.comboBoxFontWeight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontWeight.FormattingEnabled = true;
            this.comboBoxFontWeight.Items.AddRange(new object[] {
            "normal",
            "bold"});
            this.comboBoxFontWeight.Location = new System.Drawing.Point(100, 97);
            this.comboBoxFontWeight.Name = "comboBoxFontWeight";
            this.comboBoxFontWeight.Size = new System.Drawing.Size(188, 21);
            this.comboBoxFontWeight.TabIndex = 3;
            this.comboBoxFontWeight.TextChanged += new System.EventHandler(this.comboBoxFontWeight_TextChanged);
            // 
            // labelFontWeight
            // 
            this.labelFontWeight.AutoSize = true;
            this.labelFontWeight.Location = new System.Drawing.Point(10, 100);
            this.labelFontWeight.Name = "labelFontWeight";
            this.labelFontWeight.Size = new System.Drawing.Size(62, 13);
            this.labelFontWeight.TabIndex = 9;
            this.labelFontWeight.Text = "Font weight";
            // 
            // comboBoxFontStyle
            // 
            this.comboBoxFontStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontStyle.FormattingEnabled = true;
            this.comboBoxFontStyle.Items.AddRange(new object[] {
            "normal",
            "italic",
            "oblique"});
            this.comboBoxFontStyle.Location = new System.Drawing.Point(100, 70);
            this.comboBoxFontStyle.Name = "comboBoxFontStyle";
            this.comboBoxFontStyle.Size = new System.Drawing.Size(188, 21);
            this.comboBoxFontStyle.TabIndex = 2;
            this.comboBoxFontStyle.TextChanged += new System.EventHandler(this.comboBoxFontStyle_TextChanged);
            // 
            // labelFontStyle
            // 
            this.labelFontStyle.AutoSize = true;
            this.labelFontStyle.Location = new System.Drawing.Point(10, 73);
            this.labelFontStyle.Name = "labelFontStyle";
            this.labelFontStyle.Size = new System.Drawing.Size(52, 13);
            this.labelFontStyle.TabIndex = 7;
            this.labelFontStyle.Text = "Font style";
            // 
            // comboBoxFontName
            // 
            this.comboBoxFontName.FormattingEnabled = true;
            this.comboBoxFontName.Location = new System.Drawing.Point(100, 17);
            this.comboBoxFontName.Name = "comboBoxFontName";
            this.comboBoxFontName.Size = new System.Drawing.Size(188, 21);
            this.comboBoxFontName.TabIndex = 0;
            this.comboBoxFontName.TextChanged += new System.EventHandler(this.comboBoxFontName_TextChanged);
            // 
            // panelFontColor
            // 
            this.panelFontColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFontColor.Location = new System.Drawing.Point(135, 127);
            this.panelFontColor.Name = "panelFontColor";
            this.panelFontColor.Size = new System.Drawing.Size(21, 20);
            this.panelFontColor.TabIndex = 5;
            this.panelFontColor.Click += new System.EventHandler(this.buttonFontColor_Click);
            // 
            // labelFontSize
            // 
            this.labelFontSize.AutoSize = true;
            this.labelFontSize.Location = new System.Drawing.Point(10, 46);
            this.labelFontSize.Name = "labelFontSize";
            this.labelFontSize.Size = new System.Drawing.Size(49, 13);
            this.labelFontSize.TabIndex = 2;
            this.labelFontSize.Text = "Font size";
            // 
            // buttonFontColor
            // 
            this.buttonFontColor.Location = new System.Drawing.Point(13, 127);
            this.buttonFontColor.Name = "buttonFontColor";
            this.buttonFontColor.Size = new System.Drawing.Size(112, 23);
            this.buttonFontColor.TabIndex = 4;
            this.buttonFontColor.Text = "Font color";
            this.buttonFontColor.UseVisualStyleBackColor = true;
            this.buttonFontColor.Click += new System.EventHandler(this.buttonFontColor_Click);
            // 
            // labelFontName
            // 
            this.labelFontName.AutoSize = true;
            this.labelFontName.Location = new System.Drawing.Point(10, 20);
            this.labelFontName.Name = "labelFontName";
            this.labelFontName.Size = new System.Drawing.Size(57, 13);
            this.labelFontName.TabIndex = 0;
            this.labelFontName.Text = "Font family";
            // 
            // groupBoxStyles
            // 
            this.groupBoxStyles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxStyles.Controls.Add(this.buttonRemoveAll);
            this.groupBoxStyles.Controls.Add(this.buttonAdd);
            this.groupBoxStyles.Controls.Add(this.buttonRemove);
            this.groupBoxStyles.Controls.Add(this.listViewStyles);
            this.groupBoxStyles.Location = new System.Drawing.Point(12, 12);
            this.groupBoxStyles.Name = "groupBoxStyles";
            this.groupBoxStyles.Size = new System.Drawing.Size(500, 355);
            this.groupBoxStyles.TabIndex = 1;
            this.groupBoxStyles.TabStop = false;
            this.groupBoxStyles.Text = "Styles";
            // 
            // buttonRemoveAll
            // 
            this.buttonRemoveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemoveAll.Location = new System.Drawing.Point(401, 328);
            this.buttonRemoveAll.Name = "buttonRemoveAll";
            this.buttonRemoveAll.Size = new System.Drawing.Size(92, 23);
            this.buttonRemoveAll.TabIndex = 3;
            this.buttonRemoveAll.Text = "Remove all";
            this.buttonRemoveAll.UseVisualStyleBackColor = true;
            this.buttonRemoveAll.Click += new System.EventHandler(this.buttonRemoveAll_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAdd.Location = new System.Drawing.Point(225, 328);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(82, 23);
            this.buttonAdd.TabIndex = 1;
            this.buttonAdd.Text = "New";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemove.Location = new System.Drawing.Point(313, 328);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(82, 23);
            this.buttonRemove.TabIndex = 2;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // listViewStyles
            // 
            this.listViewStyles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewStyles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderFontName,
            this.columnHeaderFontSize,
            this.columnHeaderColor,
            this.columnHeaderUsed});
            this.listViewStyles.FullRowSelect = true;
            this.listViewStyles.HideSelection = false;
            this.listViewStyles.Location = new System.Drawing.Point(6, 23);
            this.listViewStyles.MultiSelect = false;
            this.listViewStyles.Name = "listViewStyles";
            this.listViewStyles.Size = new System.Drawing.Size(487, 300);
            this.listViewStyles.TabIndex = 0;
            this.listViewStyles.UseCompatibleStateImageBehavior = false;
            this.listViewStyles.View = System.Windows.Forms.View.Details;
            this.listViewStyles.SelectedIndexChanged += new System.EventHandler(this.listViewStyles_SelectedIndexChanged);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 110;
            // 
            // columnHeaderFontName
            // 
            this.columnHeaderFontName.Text = "Font family";
            this.columnHeaderFontName.Width = 118;
            // 
            // columnHeaderFontSize
            // 
            this.columnHeaderFontSize.Text = "Font size";
            this.columnHeaderFontSize.Width = 80;
            // 
            // columnHeaderColor
            // 
            this.columnHeaderColor.Text = "Color";
            this.columnHeaderColor.Width = 110;
            // 
            // columnHeaderUsed
            // 
            this.columnHeaderUsed.Text = "#Used";
            this.columnHeaderUsed.Width = 62;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(756, 373);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(675, 373);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // TimedTextStyles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(843, 406);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxProperties);
            this.Controls.Add(this.groupBoxStyles);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(850, 440);
            this.Name = "TimedTextStyles";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Timed Text - Styles";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TimedTextStyles_KeyDown);
            this.ResizeEnd += new System.EventHandler(this.TimedTextStyles_ResizeEnd);
            this.Shown += new System.EventHandler(this.TimedTextStyles_Shown);
            this.SizeChanged += new System.EventHandler(this.TimedTextStyles_SizeChanged);
            this.groupBoxProperties.ResumeLayout(false);
            this.groupBoxProperties.PerformLayout();
            this.groupBoxPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.groupBoxFont.ResumeLayout(false);
            this.groupBoxFont.PerformLayout();
            this.groupBoxStyles.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxProperties;
        private System.Windows.Forms.TextBox textBoxStyleName;
        private System.Windows.Forms.Label labelStyleName;
        private System.Windows.Forms.Panel panelFontColor;
        private System.Windows.Forms.Button buttonFontColor;
        private System.Windows.Forms.GroupBox groupBoxFont;
        private System.Windows.Forms.ComboBox comboBoxFontName;
        private System.Windows.Forms.Label labelFontSize;
        private System.Windows.Forms.Label labelFontName;
        private System.Windows.Forms.GroupBox groupBoxStyles;
        private System.Windows.Forms.Button buttonRemoveAll;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.ListView listViewStyles;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderFontName;
        private System.Windows.Forms.ColumnHeader columnHeaderFontSize;
        private System.Windows.Forms.ColumnHeader columnHeaderColor;
        private System.Windows.Forms.ColumnHeader columnHeaderUsed;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.ComboBox comboBoxFontWeight;
        private System.Windows.Forms.Label labelFontWeight;
        private System.Windows.Forms.ComboBox comboBoxFontStyle;
        private System.Windows.Forms.Label labelFontStyle;
        private System.Windows.Forms.ColorDialog colorDialogStyle;
        private System.Windows.Forms.TextBox textBoxFontSize;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
    }
}