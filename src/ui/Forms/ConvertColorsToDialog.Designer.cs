namespace Nikse.SubtitleEdit.Forms
{
    partial class ConvertColorsToDialog
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
            this.checkBoxAddNewLines = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveColorTags = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.checkBoxReBreakLines = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBoxAddNewLines
            // 
            this.checkBoxAddNewLines.AutoSize = true;
            this.checkBoxAddNewLines.Location = new System.Drawing.Point(12, 35);
            this.checkBoxAddNewLines.Name = "checkBoxAddNewLines";
            this.checkBoxAddNewLines.Size = new System.Drawing.Size(165, 17);
            this.checkBoxAddNewLines.TabIndex = 2;
            this.checkBoxAddNewLines.Text = "Place every dash on new line";
            this.checkBoxAddNewLines.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveColorTags
            // 
            this.checkBoxRemoveColorTags.AutoSize = true;
            this.checkBoxRemoveColorTags.Location = new System.Drawing.Point(12, 12);
            this.checkBoxRemoveColorTags.Name = "checkBoxRemoveColorTags";
            this.checkBoxRemoveColorTags.Size = new System.Drawing.Size(115, 17);
            this.checkBoxRemoveColorTags.TabIndex = 1;
            this.checkBoxRemoveColorTags.Text = "Remove color tags";
            this.checkBoxRemoveColorTags.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(248, 104);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(167, 104);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // checkBoxReBreakLines
            // 
            this.checkBoxReBreakLines.AutoSize = true;
            this.checkBoxReBreakLines.Location = new System.Drawing.Point(12, 58);
            this.checkBoxReBreakLines.Name = "checkBoxReBreakLines";
            this.checkBoxReBreakLines.Size = new System.Drawing.Size(94, 17);
            this.checkBoxReBreakLines.TabIndex = 3;
            this.checkBoxReBreakLines.Text = "Re-break lines";
            this.checkBoxReBreakLines.UseVisualStyleBackColor = true;
            // 
            // ConvertColorsToDialog
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(335, 139);
            this.Controls.Add(this.checkBoxReBreakLines);
            this.Controls.Add(this.checkBoxAddNewLines);
            this.Controls.Add(this.checkBoxRemoveColorTags);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConvertColorsToDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ConvertColorsToDialog";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConvertColorsToDialog_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxAddNewLines;
        private System.Windows.Forms.CheckBox checkBoxRemoveColorTags;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.CheckBox checkBoxReBreakLines;
    }
}