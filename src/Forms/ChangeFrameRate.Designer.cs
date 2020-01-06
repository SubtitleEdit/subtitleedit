namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ChangeFrameRate
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
            this.labelInfo = new System.Windows.Forms.Label();
            this.comboBoxFrameRateFrom = new System.Windows.Forms.ComboBox();
            this.labelFromFrameRate = new System.Windows.Forms.Label();
            this.comboBoxFrameRateTo = new System.Windows.Forms.ComboBox();
            this.labelToFrameRate = new System.Windows.Forms.Label();
            this.buttonGetFrameRateFrom = new System.Windows.Forms.Button();
            this.buttonGetFrameRateTo = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonSwap = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(277, 113);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(194, 113);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // labelInfo
            // 
            this.labelInfo.AllowDrop = true;
            this.labelInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelInfo.Location = new System.Drawing.Point(12, 9);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(284, 26);
            this.labelInfo.TabIndex = 3;
            this.labelInfo.Text = "Convert frame rate of subtitle";
            // 
            // comboBoxFrameRateFrom
            // 
            this.comboBoxFrameRateFrom.FormattingEnabled = true;
            this.comboBoxFrameRateFrom.Location = new System.Drawing.Point(148, 45);
            this.comboBoxFrameRateFrom.Name = "comboBoxFrameRateFrom";
            this.comboBoxFrameRateFrom.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFrameRateFrom.TabIndex = 4;
            // 
            // labelFromFrameRate
            // 
            this.labelFromFrameRate.Location = new System.Drawing.Point(3, 45);
            this.labelFromFrameRate.Name = "labelFromFrameRate";
            this.labelFromFrameRate.Size = new System.Drawing.Size(145, 21);
            this.labelFromFrameRate.TabIndex = 6;
            this.labelFromFrameRate.Text = "From frame rate";
            this.labelFromFrameRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxFrameRateTo
            // 
            this.comboBoxFrameRateTo.FormattingEnabled = true;
            this.comboBoxFrameRateTo.Location = new System.Drawing.Point(148, 74);
            this.comboBoxFrameRateTo.Name = "comboBoxFrameRateTo";
            this.comboBoxFrameRateTo.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFrameRateTo.TabIndex = 6;
            // 
            // labelToFrameRate
            // 
            this.labelToFrameRate.Location = new System.Drawing.Point(6, 74);
            this.labelToFrameRate.Name = "labelToFrameRate";
            this.labelToFrameRate.Size = new System.Drawing.Size(142, 17);
            this.labelToFrameRate.TabIndex = 8;
            this.labelToFrameRate.Text = "To frame rate";
            this.labelToFrameRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // buttonGetFrameRateFrom
            // 
            this.buttonGetFrameRateFrom.Location = new System.Drawing.Point(275, 45);
            this.buttonGetFrameRateFrom.Name = "buttonGetFrameRateFrom";
            this.buttonGetFrameRateFrom.Size = new System.Drawing.Size(28, 23);
            this.buttonGetFrameRateFrom.TabIndex = 5;
            this.buttonGetFrameRateFrom.Text = "...";
            this.buttonGetFrameRateFrom.UseVisualStyleBackColor = true;
            this.buttonGetFrameRateFrom.Click += new System.EventHandler(this.ButtonGetFrameRateFromClick);
            // 
            // buttonGetFrameRateTo
            // 
            this.buttonGetFrameRateTo.Location = new System.Drawing.Point(275, 73);
            this.buttonGetFrameRateTo.Name = "buttonGetFrameRateTo";
            this.buttonGetFrameRateTo.Size = new System.Drawing.Size(28, 23);
            this.buttonGetFrameRateTo.TabIndex = 7;
            this.buttonGetFrameRateTo.Text = "...";
            this.buttonGetFrameRateTo.UseVisualStyleBackColor = true;
            this.buttonGetFrameRateTo.Click += new System.EventHandler(this.ButtonGetFrameRateToClick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonSwap
            // 
            this.buttonSwap.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSwap.Location = new System.Drawing.Point(312, 57);
            this.buttonSwap.Name = "buttonSwap";
            this.buttonSwap.Size = new System.Drawing.Size(25, 28);
            this.buttonSwap.TabIndex = 8;
            this.buttonSwap.Text = "<->";
            this.buttonSwap.UseVisualStyleBackColor = true;
            this.buttonSwap.Click += new System.EventHandler(this.buttonSwap_Click);
            // 
            // ChangeFrameRate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 148);
            this.Controls.Add(this.buttonSwap);
            this.Controls.Add(this.buttonGetFrameRateTo);
            this.Controls.Add(this.buttonGetFrameRateFrom);
            this.Controls.Add(this.comboBoxFrameRateTo);
            this.Controls.Add(this.labelToFrameRate);
            this.Controls.Add(this.comboBoxFrameRateFrom);
            this.Controls.Add(this.labelFromFrameRate);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelInfo);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChangeFrameRate";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Change frame rate";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormChangeFrameRate_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.ComboBox comboBoxFrameRateFrom;
        private System.Windows.Forms.Label labelFromFrameRate;
        private System.Windows.Forms.ComboBox comboBoxFrameRateTo;
        private System.Windows.Forms.Label labelToFrameRate;
        private System.Windows.Forms.Button buttonGetFrameRateFrom;
        private System.Windows.Forms.Button buttonGetFrameRateTo;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonSwap;
    }
}