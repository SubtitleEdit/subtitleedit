namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class MeasurementConverter
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
            this.listBoxCategory = new System.Windows.Forms.ListBox();
            this.labelConvertFrom = new System.Windows.Forms.Label();
            this.comboBoxFrom = new System.Windows.Forms.ComboBox();
            this.textBoxInput = new System.Windows.Forms.TextBox();
            this.labelConvertTo = new System.Windows.Forms.Label();
            this.comboBoxTo = new System.Windows.Forms.ComboBox();
            this.textBoxResult = new System.Windows.Forms.TextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.checkBoxCloseOnInsert = new System.Windows.Forms.CheckBox();
            this.buttonInsert = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBoxCategory
            // 
            this.listBoxCategory.FormattingEnabled = true;
            this.listBoxCategory.Location = new System.Drawing.Point(10, 10);
            this.listBoxCategory.Name = "listBoxCategory";
            this.listBoxCategory.Size = new System.Drawing.Size(106, 160);
            this.listBoxCategory.TabIndex = 0;
            this.listBoxCategory.SelectedIndexChanged += new System.EventHandler(this.listBoxCategory_SelectedIndexChanged);
            this.listBoxCategory.LostFocus += new System.EventHandler(this.listBoxCategory_LostFocus);
            // 
            // labelConvertFrom
            // 
            this.labelConvertFrom.AutoSize = true;
            this.labelConvertFrom.Location = new System.Drawing.Point(124, 32);
            this.labelConvertFrom.Name = "labelConvertFrom";
            this.labelConvertFrom.Size = new System.Drawing.Size(70, 13);
            this.labelConvertFrom.TabIndex = 1;
            this.labelConvertFrom.Text = "Convert from:";
            // 
            // comboBoxFrom
            // 
            this.comboBoxFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFrom.FormattingEnabled = true;
            this.comboBoxFrom.Location = new System.Drawing.Point(127, 54);
            this.comboBoxFrom.Name = "comboBoxFrom";
            this.comboBoxFrom.Size = new System.Drawing.Size(132, 21);
            this.comboBoxFrom.TabIndex = 2;
            this.comboBoxFrom.SelectedIndexChanged += new System.EventHandler(this.comboBoxFrom_SelectedIndexChanged);
            // 
            // textBoxInput
            // 
            this.textBoxInput.Location = new System.Drawing.Point(127, 86);
            this.textBoxInput.Name = "textBoxInput";
            this.textBoxInput.Size = new System.Drawing.Size(132, 20);
            this.textBoxInput.TabIndex = 3;
            this.textBoxInput.TextChanged += new System.EventHandler(this.textBoxInput_TextChanged);
            this.textBoxInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxInput_KeyPress);
            this.textBoxInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxInput_KeyUp);
            // 
            // labelConvertTo
            // 
            this.labelConvertTo.AutoSize = true;
            this.labelConvertTo.Location = new System.Drawing.Point(264, 32);
            this.labelConvertTo.Name = "labelConvertTo";
            this.labelConvertTo.Size = new System.Drawing.Size(59, 13);
            this.labelConvertTo.TabIndex = 4;
            this.labelConvertTo.Text = "Convert to:";
            // 
            // comboBoxTo
            // 
            this.comboBoxTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTo.FormattingEnabled = true;
            this.comboBoxTo.Location = new System.Drawing.Point(267, 54);
            this.comboBoxTo.Name = "comboBoxTo";
            this.comboBoxTo.Size = new System.Drawing.Size(132, 21);
            this.comboBoxTo.TabIndex = 5;
            this.comboBoxTo.SelectedIndexChanged += new System.EventHandler(this.comboBoxTo_SelectedIndexChanged);
            // 
            // textBoxResult
            // 
            this.textBoxResult.Location = new System.Drawing.Point(267, 86);
            this.textBoxResult.Name = "textBoxResult";
            this.textBoxResult.ReadOnly = true;
            this.textBoxResult.Size = new System.Drawing.Size(132, 20);
            this.textBoxResult.TabIndex = 6;
            // 
            // linkLabel1
            // 
            this.linkLabel1.Location = new System.Drawing.Point(218, 112);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(181, 23);
            this.linkLabel1.TabIndex = 7;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Copy to clipboard";
            this.linkLabel1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // checkBoxCloseOnInsert
            // 
            this.checkBoxCloseOnInsert.AutoSize = true;
            this.checkBoxCloseOnInsert.Location = new System.Drawing.Point(127, 158);
            this.checkBoxCloseOnInsert.Name = "checkBoxCloseOnInsert";
            this.checkBoxCloseOnInsert.Size = new System.Drawing.Size(95, 17);
            this.checkBoxCloseOnInsert.TabIndex = 8;
            this.checkBoxCloseOnInsert.Text = "Close on insert";
            this.checkBoxCloseOnInsert.UseVisualStyleBackColor = true;
            // 
            // buttonInsert
            // 
            this.buttonInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInsert.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonInsert.Location = new System.Drawing.Point(254, 155);
            this.buttonInsert.Name = "buttonInsert";
            this.buttonInsert.Size = new System.Drawing.Size(75, 23);
            this.buttonInsert.TabIndex = 9;
            this.buttonInsert.Text = "&Insert";
            this.buttonInsert.UseVisualStyleBackColor = true;
            this.buttonInsert.Click += new System.EventHandler(this.buttonInsert_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(332, 155);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // MeasurementConverter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(419, 190);
            this.Controls.Add(this.listBoxCategory);
            this.Controls.Add(this.labelConvertFrom);
            this.Controls.Add(this.comboBoxFrom);
            this.Controls.Add(this.textBoxInput);
            this.Controls.Add(this.labelConvertTo);
            this.Controls.Add(this.comboBoxTo);
            this.Controls.Add(this.textBoxResult);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.checkBoxCloseOnInsert);
            this.Controls.Add(this.buttonInsert);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MeasurementConverter";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Measurement converter";
            this.Activated += new System.EventHandler(this.MeasurementConverter_Activated);
            this.Deactivate += new System.EventHandler(this.MeasurementConverter_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MeasurementConverter_FormClosed);
            this.Load += new System.EventHandler(this.MeasurementConverter_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MeasurementConverter_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxCategory;
        private System.Windows.Forms.Label labelConvertFrom;
        private System.Windows.Forms.ComboBox comboBoxFrom;
        private System.Windows.Forms.TextBox textBoxInput;
        private System.Windows.Forms.Label labelConvertTo;
        private System.Windows.Forms.ComboBox comboBoxTo;
        private System.Windows.Forms.TextBox textBoxResult;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox checkBoxCloseOnInsert;
        private System.Windows.Forms.Button buttonInsert;
        private System.Windows.Forms.Button buttonOK;
    }
}