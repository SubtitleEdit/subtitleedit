namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ChangeCasing
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
            this.groupBoxChangeCasing = new System.Windows.Forms.GroupBox();
            this.radioButtonFixOnlyNames = new System.Windows.Forms.RadioButton();
            this.checkBoxFixNames = new System.Windows.Forms.CheckBox();
            this.checkBoxOnlyAllUpper = new System.Windows.Forms.CheckBox();
            this.radioButtonLowercase = new System.Windows.Forms.RadioButton();
            this.radioButtonUppercase = new System.Windows.Forms.RadioButton();
            this.radioButtonNormal = new System.Windows.Forms.RadioButton();
            this.groupBoxChangeCasing.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(417, 195);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 14;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(336, 195);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 12;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxChangeCasing
            // 
            this.groupBoxChangeCasing.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonFixOnlyNames);
            this.groupBoxChangeCasing.Controls.Add(this.checkBoxFixNames);
            this.groupBoxChangeCasing.Controls.Add(this.checkBoxOnlyAllUpper);
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonLowercase);
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonUppercase);
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonNormal);
            this.groupBoxChangeCasing.Location = new System.Drawing.Point(12, 12);
            this.groupBoxChangeCasing.Name = "groupBoxChangeCasing";
            this.groupBoxChangeCasing.Size = new System.Drawing.Size(480, 172);
            this.groupBoxChangeCasing.TabIndex = 12;
            this.groupBoxChangeCasing.TabStop = false;
            this.groupBoxChangeCasing.Text = "Change casing to";
            // 
            // radioButtonFixOnlyNames
            // 
            this.radioButtonFixOnlyNames.AutoSize = true;
            this.radioButtonFixOnlyNames.Location = new System.Drawing.Point(11, 88);
            this.radioButtonFixOnlyNames.Name = "radioButtonFixOnlyNames";
            this.radioButtonFixOnlyNames.Size = new System.Drawing.Size(267, 17);
            this.radioButtonFixOnlyNames.TabIndex = 6;
            this.radioButtonFixOnlyNames.Text = "Fix only names casing (via Dictionaries\\names.xml)";
            this.radioButtonFixOnlyNames.UseVisualStyleBackColor = true;
            this.radioButtonFixOnlyNames.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // checkBoxFixNames
            // 
            this.checkBoxFixNames.AutoSize = true;
            this.checkBoxFixNames.Checked = true;
            this.checkBoxFixNames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFixNames.Location = new System.Drawing.Point(23, 39);
            this.checkBoxFixNames.Name = "checkBoxFixNames";
            this.checkBoxFixNames.Size = new System.Drawing.Size(245, 17);
            this.checkBoxFixNames.TabIndex = 2;
            this.checkBoxFixNames.Text = "Fix names casing (via Dictionaries\\names.xml)";
            this.checkBoxFixNames.UseVisualStyleBackColor = true;
            // 
            // checkBoxOnlyAllUpper
            // 
            this.checkBoxOnlyAllUpper.AutoSize = true;
            this.checkBoxOnlyAllUpper.Location = new System.Drawing.Point(23, 62);
            this.checkBoxOnlyAllUpper.Name = "checkBoxOnlyAllUpper";
            this.checkBoxOnlyAllUpper.Size = new System.Drawing.Size(183, 17);
            this.checkBoxOnlyAllUpper.TabIndex = 4;
            this.checkBoxOnlyAllUpper.Text = "Only change all upper case lines.";
            this.checkBoxOnlyAllUpper.UseVisualStyleBackColor = true;
            // 
            // radioButtonLowercase
            // 
            this.radioButtonLowercase.AutoSize = true;
            this.radioButtonLowercase.Location = new System.Drawing.Point(11, 142);
            this.radioButtonLowercase.Name = "radioButtonLowercase";
            this.radioButtonLowercase.Size = new System.Drawing.Size(86, 17);
            this.radioButtonLowercase.TabIndex = 10;
            this.radioButtonLowercase.Text = "all lowercase";
            this.radioButtonLowercase.UseVisualStyleBackColor = true;
            this.radioButtonLowercase.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // radioButtonUppercase
            // 
            this.radioButtonUppercase.AutoSize = true;
            this.radioButtonUppercase.Location = new System.Drawing.Point(11, 116);
            this.radioButtonUppercase.Name = "radioButtonUppercase";
            this.radioButtonUppercase.Size = new System.Drawing.Size(103, 17);
            this.radioButtonUppercase.TabIndex = 8;
            this.radioButtonUppercase.Text = "ALL UPPERCASE";
            this.radioButtonUppercase.UseVisualStyleBackColor = true;
            this.radioButtonUppercase.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // radioButtonNormal
            // 
            this.radioButtonNormal.AutoSize = true;
            this.radioButtonNormal.Checked = true;
            this.radioButtonNormal.Location = new System.Drawing.Point(11, 18);
            this.radioButtonNormal.Name = "radioButtonNormal";
            this.radioButtonNormal.Size = new System.Drawing.Size(286, 17);
            this.radioButtonNormal.TabIndex = 0;
            this.radioButtonNormal.TabStop = true;
            this.radioButtonNormal.Text = "Normal casing. Sentences begin with uppercase letter.";
            this.radioButtonNormal.UseVisualStyleBackColor = true;
            this.radioButtonNormal.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // ChangeCasing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 228);
            this.Controls.Add(this.groupBoxChangeCasing);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChangeCasing";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Change casing";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormChangeCasing_KeyDown);
            this.groupBoxChangeCasing.ResumeLayout(false);
            this.groupBoxChangeCasing.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxChangeCasing;
        private System.Windows.Forms.RadioButton radioButtonUppercase;
        private System.Windows.Forms.RadioButton radioButtonNormal;
        private System.Windows.Forms.RadioButton radioButtonLowercase;
        private System.Windows.Forms.CheckBox checkBoxOnlyAllUpper;
        private System.Windows.Forms.CheckBox checkBoxFixNames;
        private System.Windows.Forms.RadioButton radioButtonFixOnlyNames;
    }
}