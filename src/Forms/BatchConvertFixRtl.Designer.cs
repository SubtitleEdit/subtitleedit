namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class BatchConvertFixRtl
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonReverseStartEnd = new System.Windows.Forms.RadioButton();
            this.radioButtonRemoveUnicode = new System.Windows.Forms.RadioButton();
            this.radioButtonAddUnicode = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(173, 142);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(254, 142);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.radioButtonReverseStartEnd);
            this.groupBox1.Controls.Add(this.radioButtonRemoveUnicode);
            this.groupBox1.Controls.Add(this.radioButtonAddUnicode);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(317, 124);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // radioButtonReverseStartEnd
            // 
            this.radioButtonReverseStartEnd.AutoSize = true;
            this.radioButtonReverseStartEnd.Location = new System.Drawing.Point(19, 77);
            this.radioButtonReverseStartEnd.Name = "radioButtonReverseStartEnd";
            this.radioButtonReverseStartEnd.Size = new System.Drawing.Size(135, 17);
            this.radioButtonReverseStartEnd.TabIndex = 2;
            this.radioButtonReverseStartEnd.TabStop = true;
            this.radioButtonReverseStartEnd.Text = "Reverse RTL start/end";
            this.radioButtonReverseStartEnd.UseVisualStyleBackColor = true;
            // 
            // radioButtonRemoveUnicode
            // 
            this.radioButtonRemoveUnicode.AutoSize = true;
            this.radioButtonRemoveUnicode.Location = new System.Drawing.Point(19, 54);
            this.radioButtonRemoveUnicode.Name = "radioButtonRemoveUnicode";
            this.radioButtonRemoveUnicode.Size = new System.Drawing.Size(153, 17);
            this.radioButtonRemoveUnicode.TabIndex = 1;
            this.radioButtonRemoveUnicode.TabStop = true;
            this.radioButtonRemoveUnicode.Text = "Remove RTL unicode tags";
            this.radioButtonRemoveUnicode.UseVisualStyleBackColor = true;
            // 
            // radioButtonAddUnicode
            // 
            this.radioButtonAddUnicode.AutoSize = true;
            this.radioButtonAddUnicode.Location = new System.Drawing.Point(19, 31);
            this.radioButtonAddUnicode.Name = "radioButtonAddUnicode";
            this.radioButtonAddUnicode.Size = new System.Drawing.Size(145, 17);
            this.radioButtonAddUnicode.TabIndex = 0;
            this.radioButtonAddUnicode.TabStop = true;
            this.radioButtonAddUnicode.Text = "Fix RTL via Unicode tags";
            this.radioButtonAddUnicode.UseVisualStyleBackColor = true;
            // 
            // BatchConvertFixRtl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 175);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BatchConvertFixRtl";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Fix RTL";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BatchConvertFixRtl_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonReverseStartEnd;
        private System.Windows.Forms.RadioButton radioButtonRemoveUnicode;
        private System.Windows.Forms.RadioButton radioButtonAddUnicode;
    }
}