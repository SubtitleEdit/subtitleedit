namespace Nikse.SubtitleEdit.Forms.Options
{
    sealed partial class DoNotBreakAfterListEdit
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
            this.groupBoxNamesIgonoreLists = new System.Windows.Forms.GroupBox();
            this.radioButtonRegEx = new System.Windows.Forms.RadioButton();
            this.radioButtonText = new System.Windows.Forms.RadioButton();
            this.buttonRemoveNoBreakAfter = new System.Windows.Forms.Button();
            this.listBoxNoBreakAfter = new System.Windows.Forms.ListBox();
            this.buttonAddNoBreakAfter = new System.Windows.Forms.Button();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonNew = new System.Windows.Forms.Button();
            this.comboBoxDictionaries = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.textBoxNoBreakAfter = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.groupBoxNamesIgonoreLists.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxNamesIgonoreLists
            // 
            this.groupBoxNamesIgonoreLists.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNamesIgonoreLists.Controls.Add(this.radioButtonRegEx);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.radioButtonText);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.buttonRemoveNoBreakAfter);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.listBoxNoBreakAfter);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.textBoxNoBreakAfter);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.buttonAddNoBreakAfter);
            this.groupBoxNamesIgonoreLists.Location = new System.Drawing.Point(12, 54);
            this.groupBoxNamesIgonoreLists.Name = "groupBoxNamesIgonoreLists";
            this.groupBoxNamesIgonoreLists.Size = new System.Drawing.Size(313, 304);
            this.groupBoxNamesIgonoreLists.TabIndex = 3;
            this.groupBoxNamesIgonoreLists.TabStop = false;
            // 
            // radioButtonRegEx
            // 
            this.radioButtonRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButtonRegEx.AutoSize = true;
            this.radioButtonRegEx.Location = new System.Drawing.Point(83, 279);
            this.radioButtonRegEx.Name = "radioButtonRegEx";
            this.radioButtonRegEx.Size = new System.Drawing.Size(115, 17);
            this.radioButtonRegEx.TabIndex = 21;
            this.radioButtonRegEx.Text = "Regular expression";
            this.radioButtonRegEx.UseVisualStyleBackColor = true;
            this.radioButtonRegEx.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // radioButtonText
            // 
            this.radioButtonText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButtonText.AutoSize = true;
            this.radioButtonText.Checked = true;
            this.radioButtonText.Location = new System.Drawing.Point(6, 279);
            this.radioButtonText.Name = "radioButtonText";
            this.radioButtonText.Size = new System.Drawing.Size(46, 17);
            this.radioButtonText.TabIndex = 20;
            this.radioButtonText.TabStop = true;
            this.radioButtonText.Text = "Text";
            this.radioButtonText.UseVisualStyleBackColor = true;
            this.radioButtonText.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // buttonRemoveNoBreakAfter
            // 
            this.buttonRemoveNoBreakAfter.Location = new System.Drawing.Point(230, 19);
            this.buttonRemoveNoBreakAfter.Name = "buttonRemoveNoBreakAfter";
            this.buttonRemoveNoBreakAfter.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveNoBreakAfter.TabIndex = 1;
            this.buttonRemoveNoBreakAfter.Text = "Remove";
            this.buttonRemoveNoBreakAfter.UseVisualStyleBackColor = true;
            this.buttonRemoveNoBreakAfter.Click += new System.EventHandler(this.buttonRemoveNameEtc_Click);
            // 
            // listBoxNoBreakAfter
            // 
            this.listBoxNoBreakAfter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxNoBreakAfter.FormattingEnabled = true;
            this.listBoxNoBreakAfter.Location = new System.Drawing.Point(6, 19);
            this.listBoxNoBreakAfter.Name = "listBoxNoBreakAfter";
            this.listBoxNoBreakAfter.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxNoBreakAfter.Size = new System.Drawing.Size(218, 225);
            this.listBoxNoBreakAfter.TabIndex = 0;
            this.listBoxNoBreakAfter.SelectedIndexChanged += new System.EventHandler(this.listBoxNames_SelectedIndexChanged);
            // 
            // buttonAddNoBreakAfter
            // 
            this.buttonAddNoBreakAfter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddNoBreakAfter.Location = new System.Drawing.Point(230, 248);
            this.buttonAddNoBreakAfter.Name = "buttonAddNoBreakAfter";
            this.buttonAddNoBreakAfter.Size = new System.Drawing.Size(75, 23);
            this.buttonAddNoBreakAfter.TabIndex = 11;
            this.buttonAddNoBreakAfter.Text = "Add";
            this.buttonAddNoBreakAfter.UseVisualStyleBackColor = true;
            this.buttonAddNoBreakAfter.Click += new System.EventHandler(this.buttonAddNames_Click);
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(12, 11);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(55, 13);
            this.labelLanguage.TabIndex = 0;
            this.labelLanguage.Text = "Language";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(166, 369);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 99;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(250, 369);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 100;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonNew
            // 
            this.buttonNew.Location = new System.Drawing.Point(242, 27);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(75, 23);
            this.buttonNew.TabIndex = 2;
            this.buttonNew.Text = "New...";
            this.buttonNew.UseVisualStyleBackColor = true;
            this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
            // 
            // comboBoxDictionaries
            // 
            this.comboBoxDictionaries.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxDictionaries.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxDictionaries.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxDictionaries.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxDictionaries.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxDictionaries.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxDictionaries.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxDictionaries.DropDownHeight = 400;
            this.comboBoxDictionaries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDictionaries.DropDownWidth = 224;
            this.comboBoxDictionaries.FormattingEnabled = true;
            this.comboBoxDictionaries.Location = new System.Drawing.Point(12, 27);
            this.comboBoxDictionaries.MaxLength = 32767;
            this.comboBoxDictionaries.Name = "comboBoxDictionaries";
            this.comboBoxDictionaries.SelectedIndex = -1;
            this.comboBoxDictionaries.SelectedItem = null;
            this.comboBoxDictionaries.SelectedText = "";
            this.comboBoxDictionaries.Size = new System.Drawing.Size(224, 21);
            this.comboBoxDictionaries.TabIndex = 1;
            this.comboBoxDictionaries.UsePopupWindow = false;
            this.comboBoxDictionaries.SelectedIndexChanged += new System.EventHandler(this.comboBoxDictionaries_SelectedIndexChanged);
            // 
            // textBoxNoBreakAfter
            // 
            this.textBoxNoBreakAfter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxNoBreakAfter.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxNoBreakAfter.Location = new System.Drawing.Point(6, 250);
            this.textBoxNoBreakAfter.Name = "textBoxNoBreakAfter";
            this.textBoxNoBreakAfter.Size = new System.Drawing.Size(218, 20);
            this.textBoxNoBreakAfter.TabIndex = 10;
            this.textBoxNoBreakAfter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxNoBreakAfter_KeyDown);
            // 
            // DoNotBreakAfterListEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 402);
            this.Controls.Add(this.buttonNew);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.comboBoxDictionaries);
            this.Controls.Add(this.labelLanguage);
            this.Controls.Add(this.groupBoxNamesIgonoreLists);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(350, 340);
            this.Name = "DoNotBreakAfterListEdit";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Do not break after list - edit";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoNotBreakAfterListEdit_KeyDown);
            this.groupBoxNamesIgonoreLists.ResumeLayout(false);
            this.groupBoxNamesIgonoreLists.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxNamesIgonoreLists;
        private System.Windows.Forms.Button buttonRemoveNoBreakAfter;
        private System.Windows.Forms.ListBox listBoxNoBreakAfter;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxNoBreakAfter;
        private System.Windows.Forms.Button buttonAddNoBreakAfter;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxDictionaries;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.RadioButton radioButtonRegEx;
        private System.Windows.Forms.RadioButton radioButtonText;
        private System.Windows.Forms.Button buttonNew;
    }
}