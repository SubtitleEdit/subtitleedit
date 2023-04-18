namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ReplaceDialog
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
            this.radioButtonRegEx = new System.Windows.Forms.RadioButton();
            this.radioButtonCaseSensitive = new System.Windows.Forms.RadioButton();
            this.radioButtonNormal = new System.Windows.Forms.RadioButton();
            this.buttonReplace = new System.Windows.Forms.Button();
            this.textBoxFind = new System.Windows.Forms.TextBox();
            this.buttonReplaceAll = new System.Windows.Forms.Button();
            this.textBoxReplace = new System.Windows.Forms.TextBox();
            this.labelReplaceWith = new System.Windows.Forms.Label();
            this.labelFindWhat = new System.Windows.Forms.Label();
            this.buttonFind = new System.Windows.Forms.Button();
            this.checkBoxWholeWord = new System.Windows.Forms.CheckBox();
            this.comboBoxFindReplaceIn = new System.Windows.Forms.ComboBox();
            this.labelFindReplaceIn = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // radioButtonRegEx
            // 
            this.radioButtonRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButtonRegEx.AutoSize = true;
            this.radioButtonRegEx.Location = new System.Drawing.Point(15, 235);
            this.radioButtonRegEx.Name = "radioButtonRegEx";
            this.radioButtonRegEx.Size = new System.Drawing.Size(56, 17);
            this.radioButtonRegEx.TabIndex = 70;
            this.radioButtonRegEx.Text = "RegEx";
            this.radioButtonRegEx.UseVisualStyleBackColor = true;
            this.radioButtonRegEx.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // radioButtonCaseSensitive
            // 
            this.radioButtonCaseSensitive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButtonCaseSensitive.AutoSize = true;
            this.radioButtonCaseSensitive.Location = new System.Drawing.Point(15, 212);
            this.radioButtonCaseSensitive.Name = "radioButtonCaseSensitive";
            this.radioButtonCaseSensitive.Size = new System.Drawing.Size(94, 17);
            this.radioButtonCaseSensitive.TabIndex = 60;
            this.radioButtonCaseSensitive.Text = "Case sensitive";
            this.radioButtonCaseSensitive.UseVisualStyleBackColor = true;
            this.radioButtonCaseSensitive.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // radioButtonNormal
            // 
            this.radioButtonNormal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButtonNormal.AutoSize = true;
            this.radioButtonNormal.Checked = true;
            this.radioButtonNormal.Location = new System.Drawing.Point(15, 189);
            this.radioButtonNormal.Name = "radioButtonNormal";
            this.radioButtonNormal.Size = new System.Drawing.Size(58, 17);
            this.radioButtonNormal.TabIndex = 50;
            this.radioButtonNormal.TabStop = true;
            this.radioButtonNormal.Text = "Normal";
            this.radioButtonNormal.UseVisualStyleBackColor = true;
            this.radioButtonNormal.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // buttonReplace
            // 
            this.buttonReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReplace.Location = new System.Drawing.Point(266, 54);
            this.buttonReplace.Name = "buttonReplace";
            this.buttonReplace.Size = new System.Drawing.Size(119, 23);
            this.buttonReplace.TabIndex = 88;
            this.buttonReplace.Text = "Replace";
            this.buttonReplace.UseVisualStyleBackColor = true;
            this.buttonReplace.Click += new System.EventHandler(this.ButtonReplaceClick);
            // 
            // textBoxFind
            // 
            this.textBoxFind.Location = new System.Drawing.Point(15, 25);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(232, 21);
            this.textBoxFind.TabIndex = 0;
            this.textBoxFind.TextChanged += new System.EventHandler(this.textBoxFind_TextChanged);
            this.textBoxFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxFindKeyDown);
            // 
            // buttonReplaceAll
            // 
            this.buttonReplaceAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReplaceAll.Location = new System.Drawing.Point(266, 83);
            this.buttonReplaceAll.Name = "buttonReplaceAll";
            this.buttonReplaceAll.Size = new System.Drawing.Size(119, 23);
            this.buttonReplaceAll.TabIndex = 92;
            this.buttonReplaceAll.Text = "Replace all";
            this.buttonReplaceAll.UseVisualStyleBackColor = true;
            this.buttonReplaceAll.Click += new System.EventHandler(this.ButtonReplaceAllClick);
            // 
            // textBoxReplace
            // 
            this.textBoxReplace.Location = new System.Drawing.Point(15, 71);
            this.textBoxReplace.Name = "textBoxReplace";
            this.textBoxReplace.Size = new System.Drawing.Size(232, 21);
            this.textBoxReplace.TabIndex = 10;
            this.textBoxReplace.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxFindKeyDown);
            // 
            // labelReplaceWith
            // 
            this.labelReplaceWith.AutoSize = true;
            this.labelReplaceWith.Location = new System.Drawing.Point(12, 54);
            this.labelReplaceWith.Name = "labelReplaceWith";
            this.labelReplaceWith.Size = new System.Drawing.Size(72, 13);
            this.labelReplaceWith.TabIndex = 14;
            this.labelReplaceWith.Text = "Replace with:";
            // 
            // labelFindWhat
            // 
            this.labelFindWhat.AutoSize = true;
            this.labelFindWhat.Location = new System.Drawing.Point(12, 8);
            this.labelFindWhat.Name = "labelFindWhat";
            this.labelFindWhat.Size = new System.Drawing.Size(58, 13);
            this.labelFindWhat.TabIndex = 9;
            this.labelFindWhat.Text = "Find what:";
            // 
            // buttonFind
            // 
            this.buttonFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFind.Location = new System.Drawing.Point(266, 25);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(119, 23);
            this.buttonFind.TabIndex = 80;
            this.buttonFind.Text = "Find";
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.ButtonFindClick);
            // 
            // checkBoxWholeWord
            // 
            this.checkBoxWholeWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxWholeWord.AutoSize = true;
            this.checkBoxWholeWord.Location = new System.Drawing.Point(15, 159);
            this.checkBoxWholeWord.Name = "checkBoxWholeWord";
            this.checkBoxWholeWord.Size = new System.Drawing.Size(83, 17);
            this.checkBoxWholeWord.TabIndex = 30;
            this.checkBoxWholeWord.Text = "Whole word";
            this.checkBoxWholeWord.UseVisualStyleBackColor = true;
            // 
            // comboBoxFindReplaceIn
            // 
            this.comboBoxFindReplaceIn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFindReplaceIn.FormattingEnabled = true;
            this.comboBoxFindReplaceIn.Location = new System.Drawing.Point(15, 120);
            this.comboBoxFindReplaceIn.Name = "comboBoxFindReplaceIn";
            this.comboBoxFindReplaceIn.Size = new System.Drawing.Size(232, 21);
            this.comboBoxFindReplaceIn.TabIndex = 20;
            // 
            // labelFindReplaceIn
            // 
            this.labelFindReplaceIn.AutoSize = true;
            this.labelFindReplaceIn.Location = new System.Drawing.Point(12, 104);
            this.labelFindReplaceIn.Name = "labelFindReplaceIn";
            this.labelFindReplaceIn.Size = new System.Drawing.Size(96, 13);
            this.labelFindReplaceIn.TabIndex = 16;
            this.labelFindReplaceIn.Text = "Replace/search in:";
            // 
            // ReplaceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 266);
            this.Controls.Add(this.labelFindReplaceIn);
            this.Controls.Add(this.comboBoxFindReplaceIn);
            this.Controls.Add(this.checkBoxWholeWord);
            this.Controls.Add(this.buttonFind);
            this.Controls.Add(this.labelFindWhat);
            this.Controls.Add(this.labelReplaceWith);
            this.Controls.Add(this.textBoxReplace);
            this.Controls.Add(this.buttonReplaceAll);
            this.Controls.Add(this.radioButtonRegEx);
            this.Controls.Add(this.radioButtonCaseSensitive);
            this.Controls.Add(this.radioButtonNormal);
            this.Controls.Add(this.buttonReplace);
            this.Controls.Add(this.textBoxFind);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReplaceDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Replace";
            this.Activated += new System.EventHandler(this.ReplaceDialog_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReplaceDialog_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormReplaceDialog_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonRegEx;
        private System.Windows.Forms.RadioButton radioButtonCaseSensitive;
        private System.Windows.Forms.RadioButton radioButtonNormal;
        private System.Windows.Forms.Button buttonReplace;
        private System.Windows.Forms.TextBox textBoxFind;
        private System.Windows.Forms.Button buttonReplaceAll;
        private System.Windows.Forms.TextBox textBoxReplace;
        private System.Windows.Forms.Label labelReplaceWith;
        private System.Windows.Forms.Label labelFindWhat;
        private System.Windows.Forms.Button buttonFind;
        private System.Windows.Forms.CheckBox checkBoxWholeWord;
        private System.Windows.Forms.ComboBox comboBoxFindReplaceIn;
        private System.Windows.Forms.Label labelFindReplaceIn;
    }
}