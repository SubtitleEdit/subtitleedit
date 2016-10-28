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
            this.SuspendLayout();
            // 
            // radioButtonRegEx
            // 
            this.radioButtonRegEx.AutoSize = true;
            this.radioButtonRegEx.Location = new System.Drawing.Point(15, 149);
            this.radioButtonRegEx.Name = "radioButtonRegEx";
            this.radioButtonRegEx.Size = new System.Drawing.Size(56, 17);
            this.radioButtonRegEx.TabIndex = 4;
            this.radioButtonRegEx.Text = "RegEx";
            this.radioButtonRegEx.UseVisualStyleBackColor = true;
            this.radioButtonRegEx.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // radioButtonCaseSensitive
            // 
            this.radioButtonCaseSensitive.AutoSize = true;
            this.radioButtonCaseSensitive.Location = new System.Drawing.Point(15, 126);
            this.radioButtonCaseSensitive.Name = "radioButtonCaseSensitive";
            this.radioButtonCaseSensitive.Size = new System.Drawing.Size(94, 17);
            this.radioButtonCaseSensitive.TabIndex = 3;
            this.radioButtonCaseSensitive.Text = "Case sensitive";
            this.radioButtonCaseSensitive.UseVisualStyleBackColor = true;
            this.radioButtonCaseSensitive.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // radioButtonNormal
            // 
            this.radioButtonNormal.AutoSize = true;
            this.radioButtonNormal.Checked = true;
            this.radioButtonNormal.Location = new System.Drawing.Point(15, 103);
            this.radioButtonNormal.Name = "radioButtonNormal";
            this.radioButtonNormal.Size = new System.Drawing.Size(58, 17);
            this.radioButtonNormal.TabIndex = 2;
            this.radioButtonNormal.TabStop = true;
            this.radioButtonNormal.Text = "Normal";
            this.radioButtonNormal.UseVisualStyleBackColor = true;
            this.radioButtonNormal.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // buttonReplace
            // 
            this.buttonReplace.Location = new System.Drawing.Point(96, 180);
            this.buttonReplace.Name = "buttonReplace";
            this.buttonReplace.Size = new System.Drawing.Size(75, 21);
            this.buttonReplace.TabIndex = 6;
            this.buttonReplace.Text = "Replace";
            this.buttonReplace.UseVisualStyleBackColor = true;
            this.buttonReplace.Click += new System.EventHandler(this.ButtonReplaceClick);
            // 
            // textBoxFind
            // 
            this.textBoxFind.Location = new System.Drawing.Point(15, 25);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(265, 21);
            this.textBoxFind.TabIndex = 0;
            this.textBoxFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxFindKeyDown);
            // 
            // buttonReplaceAll
            // 
            this.buttonReplaceAll.Location = new System.Drawing.Point(177, 180);
            this.buttonReplaceAll.Name = "buttonReplaceAll";
            this.buttonReplaceAll.Size = new System.Drawing.Size(103, 21);
            this.buttonReplaceAll.TabIndex = 7;
            this.buttonReplaceAll.Text = "Replace all";
            this.buttonReplaceAll.UseVisualStyleBackColor = true;
            this.buttonReplaceAll.Click += new System.EventHandler(this.ButtonReplaceAllClick);
            // 
            // textBoxReplace
            // 
            this.textBoxReplace.Location = new System.Drawing.Point(15, 71);
            this.textBoxReplace.Name = "textBoxReplace";
            this.textBoxReplace.Size = new System.Drawing.Size(265, 21);
            this.textBoxReplace.TabIndex = 1;
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
            this.buttonFind.Location = new System.Drawing.Point(15, 180);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(75, 21);
            this.buttonFind.TabIndex = 5;
            this.buttonFind.Text = "Find";
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.ButtonFindClick);
            // 
            // ReplaceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(330, 208);
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
    }
}