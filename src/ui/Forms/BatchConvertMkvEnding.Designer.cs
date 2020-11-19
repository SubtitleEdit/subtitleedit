namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class BatchConvertMkvEnding
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
            this.labelFileNameEnding = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelFileNameExample = new System.Windows.Forms.Label();
            this.radioButton3Letter = new System.Windows.Forms.RadioButton();
            this.radioButton2Letter = new System.Windows.Forms.RadioButton();
            this.radioButtonNone = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // labelFileNameEnding
            // 
            this.labelFileNameEnding.AutoSize = true;
            this.labelFileNameEnding.Location = new System.Drawing.Point(12, 17);
            this.labelFileNameEnding.Name = "labelFileNameEnding";
            this.labelFileNameEnding.Size = new System.Drawing.Size(154, 13);
            this.labelFileNameEnding.TabIndex = 108;
            this.labelFileNameEnding.Text = "\"Language\" in output file name";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(287, 123);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 106;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(368, 123);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 107;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelFileNameExample
            // 
            this.labelFileNameExample.AutoSize = true;
            this.labelFileNameExample.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelFileNameExample.Location = new System.Drawing.Point(12, 109);
            this.labelFileNameExample.Name = "labelFileNameExample";
            this.labelFileNameExample.Size = new System.Drawing.Size(50, 13);
            this.labelFileNameExample.TabIndex = 109;
            this.labelFileNameExample.Text = "Example:";
            // 
            // radioButton3Letter
            // 
            this.radioButton3Letter.AutoSize = true;
            this.radioButton3Letter.Location = new System.Drawing.Point(35, 33);
            this.radioButton3Letter.Name = "radioButton3Letter";
            this.radioButton3Letter.Size = new System.Drawing.Size(153, 17);
            this.radioButton3Letter.TabIndex = 110;
            this.radioButton3Letter.TabStop = true;
            this.radioButton3Letter.Text = "Three letter language code";
            this.radioButton3Letter.UseVisualStyleBackColor = true;
            this.radioButton3Letter.CheckedChanged += new System.EventHandler(this.LanguageCodeChanged);
            // 
            // radioButton2Letter
            // 
            this.radioButton2Letter.AutoSize = true;
            this.radioButton2Letter.Location = new System.Drawing.Point(35, 56);
            this.radioButton2Letter.Name = "radioButton2Letter";
            this.radioButton2Letter.Size = new System.Drawing.Size(146, 17);
            this.radioButton2Letter.TabIndex = 111;
            this.radioButton2Letter.TabStop = true;
            this.radioButton2Letter.Text = "Two letter language code";
            this.radioButton2Letter.UseVisualStyleBackColor = true;
            this.radioButton2Letter.CheckedChanged += new System.EventHandler(this.LanguageCodeChanged);
            // 
            // radioButtonNone
            // 
            this.radioButtonNone.AutoSize = true;
            this.radioButtonNone.Location = new System.Drawing.Point(35, 79);
            this.radioButtonNone.Name = "radioButtonNone";
            this.radioButtonNone.Size = new System.Drawing.Size(51, 17);
            this.radioButtonNone.TabIndex = 112;
            this.radioButtonNone.TabStop = true;
            this.radioButtonNone.Text = "None";
            this.radioButtonNone.UseVisualStyleBackColor = true;
            this.radioButtonNone.CheckedChanged += new System.EventHandler(this.LanguageCodeChanged);
            // 
            // BatchConvertMkvEnding
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 158);
            this.Controls.Add(this.radioButtonNone);
            this.Controls.Add(this.radioButton2Letter);
            this.Controls.Add(this.radioButton3Letter);
            this.Controls.Add(this.labelFileNameExample);
            this.Controls.Add(this.labelFileNameEnding);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BatchConvertMkvEnding";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Batch convert mkv settings";
            this.Load += new System.EventHandler(this.BatchConvertMkvEnding_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BatchConvertMkvEnding_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelFileNameEnding;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelFileNameExample;
        private System.Windows.Forms.RadioButton radioButton3Letter;
        private System.Windows.Forms.RadioButton radioButton2Letter;
        private System.Windows.Forms.RadioButton radioButtonNone;
    }
}