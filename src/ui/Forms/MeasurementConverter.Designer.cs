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
            this.listBoxCategory = new Nikse.SubtitleEdit.Controls.NikseListBox();
            this.labelConvertFrom = new System.Windows.Forms.Label();
            this.comboBoxFrom = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.textBoxInput = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.labelConvertTo = new System.Windows.Forms.Label();
            this.comboBoxTo = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.textBoxResult = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.checkBoxCloseOnInsert = new System.Windows.Forms.CheckBox();
            this.buttonInsert = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBoxCategory
            // 
            this.listBoxCategory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBoxCategory.FormattingEnabled = true;
            this.listBoxCategory.ItemHeight = 13;
            this.listBoxCategory.Location = new System.Drawing.Point(10, 10);
            this.listBoxCategory.Name = "listBoxCategory";
            this.listBoxCategory.SelectedIndex = -1;
            this.listBoxCategory.SelectedItem = null;
            this.listBoxCategory.SelectionMode = System.Windows.Forms.SelectionMode.One;
            this.listBoxCategory.Size = new System.Drawing.Size(106, 175);
            this.listBoxCategory.Sorted = false;
            this.listBoxCategory.TabIndex = 0;
            this.listBoxCategory.TopIndex = 0;
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
            this.comboBoxFrom.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFrom.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFrom.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFrom.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFrom.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFrom.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFrom.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFrom.DropDownHeight = 400;
            this.comboBoxFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFrom.DropDownWidth = 132;
            this.comboBoxFrom.FormattingEnabled = true;
            this.comboBoxFrom.Location = new System.Drawing.Point(127, 54);
            this.comboBoxFrom.MaxLength = 32767;
            this.comboBoxFrom.Name = "comboBoxFrom";
            this.comboBoxFrom.SelectedIndex = -1;
            this.comboBoxFrom.SelectedItem = null;
            this.comboBoxFrom.SelectedText = "";
            this.comboBoxFrom.Size = new System.Drawing.Size(132, 21);
            this.comboBoxFrom.TabIndex = 2;
            this.comboBoxFrom.SelectedIndexChanged += new System.EventHandler(this.comboBoxFrom_SelectedIndexChanged);
            // 
            // textBoxInput
            // 
            this.textBoxInput.BackColor = System.Drawing.Color.DarkGray;
            this.textBoxInput.CurrentLanguage = "";
            this.textBoxInput.CurrentLineIndex = 0;
            this.textBoxInput.HideSelection = true;
            this.textBoxInput.IsDictionaryDownloaded = true;
            this.textBoxInput.IsSpellCheckerInitialized = false;
            this.textBoxInput.IsSpellCheckRequested = false;
            this.textBoxInput.IsWrongWord = false;
            this.textBoxInput.LanguageChanged = false;
            this.textBoxInput.Location = new System.Drawing.Point(127, 86);
            this.textBoxInput.MaxLength = 32767;
            this.textBoxInput.Multiline = true;
            this.textBoxInput.Name = "textBoxInput";
            this.textBoxInput.Padding = new System.Windows.Forms.Padding(1);
            this.textBoxInput.ReadOnly = false;
            this.textBoxInput.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.textBoxInput.SelectedText = "";
            this.textBoxInput.SelectionLength = 0;
            this.textBoxInput.SelectionStart = 0;
            this.textBoxInput.Size = new System.Drawing.Size(132, 20);
            this.textBoxInput.TabIndex = 3;
            this.textBoxInput.TextBoxFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxInput.UseSystemPasswordChar = false;
            this.textBoxInput.TextChanged += new System.EventHandler(this.textBoxInput_TextChanged);
            this.textBoxInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxInput_KeyUp);
            this.textBoxInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxInput_KeyPress);
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
            this.comboBoxTo.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxTo.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxTo.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxTo.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxTo.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxTo.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxTo.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxTo.DropDownHeight = 400;
            this.comboBoxTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTo.DropDownWidth = 132;
            this.comboBoxTo.FormattingEnabled = true;
            this.comboBoxTo.Location = new System.Drawing.Point(267, 54);
            this.comboBoxTo.MaxLength = 32767;
            this.comboBoxTo.Name = "comboBoxTo";
            this.comboBoxTo.SelectedIndex = -1;
            this.comboBoxTo.SelectedItem = null;
            this.comboBoxTo.SelectedText = "";
            this.comboBoxTo.Size = new System.Drawing.Size(132, 21);
            this.comboBoxTo.TabIndex = 5;
            this.comboBoxTo.SelectedIndexChanged += new System.EventHandler(this.comboBoxTo_SelectedIndexChanged);
            // 
            // textBoxResult
            // 
            this.textBoxResult.BackColor = System.Drawing.Color.DarkGray;
            this.textBoxResult.CurrentLanguage = "";
            this.textBoxResult.CurrentLineIndex = 0;
            this.textBoxResult.HideSelection = true;
            this.textBoxResult.IsDictionaryDownloaded = true;
            this.textBoxResult.IsSpellCheckerInitialized = false;
            this.textBoxResult.IsSpellCheckRequested = false;
            this.textBoxResult.IsWrongWord = false;
            this.textBoxResult.LanguageChanged = false;
            this.textBoxResult.Location = new System.Drawing.Point(267, 86);
            this.textBoxResult.MaxLength = 32767;
            this.textBoxResult.Multiline = true;
            this.textBoxResult.Name = "textBoxResult";
            this.textBoxResult.Padding = new System.Windows.Forms.Padding(1);
            this.textBoxResult.ReadOnly = true;
            this.textBoxResult.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.textBoxResult.SelectedText = "";
            this.textBoxResult.SelectionLength = 0;
            this.textBoxResult.SelectionStart = 0;
            this.textBoxResult.Size = new System.Drawing.Size(132, 20);
            this.textBoxResult.TabIndex = 6;
            this.textBoxResult.TextBoxFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxResult.UseSystemPasswordChar = false;
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
            this.checkBoxCloseOnInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxCloseOnInsert.AutoSize = true;
            this.checkBoxCloseOnInsert.Location = new System.Drawing.Point(122, 178);
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
            this.buttonInsert.Location = new System.Drawing.Point(307, 174);
            this.buttonInsert.Name = "buttonInsert";
            this.buttonInsert.Size = new System.Drawing.Size(102, 23);
            this.buttonInsert.TabIndex = 9;
            this.buttonInsert.Text = "&Insert";
            this.buttonInsert.UseVisualStyleBackColor = true;
            this.buttonInsert.Click += new System.EventHandler(this.buttonInsert_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(412, 174);
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
            this.ClientSize = new System.Drawing.Size(499, 209);
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

        private Nikse.SubtitleEdit.Controls.NikseListBox listBoxCategory;
        private System.Windows.Forms.Label labelConvertFrom;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFrom;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxInput;
        private System.Windows.Forms.Label labelConvertTo;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxTo;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxResult;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox checkBoxCloseOnInsert;
        private System.Windows.Forms.Button buttonInsert;
        private System.Windows.Forms.Button buttonOK;
    }
}