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
            this.SuspendLayout();
            //
            // buttonCancel
            //
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(195, 115);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            //
            // buttonOK
            //
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(114, 115);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            //
            // labelInfo
            //
            this.labelInfo.AutoSize = true;
            this.labelInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelInfo.Location = new System.Drawing.Point(37, 9);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(229, 21);
            this.labelInfo.TabIndex = 3;
            this.labelInfo.Text = "Convert frame rate of subtitle";
            //
            // comboBoxFrameRateFrom
            //
            this.comboBoxFrameRateFrom.FormattingEnabled = true;
            this.comboBoxFrameRateFrom.Location = new System.Drawing.Point(119, 45);
            this.comboBoxFrameRateFrom.Name = "comboBoxFrameRateFrom";
            this.comboBoxFrameRateFrom.Size = new System.Drawing.Size(121, 29);
            this.comboBoxFrameRateFrom.TabIndex = 7;
            //
            // labelFromFrameRate
            //
            this.labelFromFrameRate.Location = new System.Drawing.Point(4, 48);
            this.labelFromFrameRate.Name = "labelFromFrameRate";
            this.labelFromFrameRate.Size = new System.Drawing.Size(112, 19);
            this.labelFromFrameRate.TabIndex = 6;
            this.labelFromFrameRate.Text = "From frame rate";
            this.labelFromFrameRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // comboBoxFrameRateTo
            //
            this.comboBoxFrameRateTo.FormattingEnabled = true;
            this.comboBoxFrameRateTo.Location = new System.Drawing.Point(119, 74);
            this.comboBoxFrameRateTo.Name = "comboBoxFrameRateTo";
            this.comboBoxFrameRateTo.Size = new System.Drawing.Size(121, 29);
            this.comboBoxFrameRateTo.TabIndex = 9;
            //
            // labelToFrameRate
            //
            this.labelToFrameRate.Location = new System.Drawing.Point(4, 77);
            this.labelToFrameRate.Name = "labelToFrameRate";
            this.labelToFrameRate.Size = new System.Drawing.Size(109, 22);
            this.labelToFrameRate.TabIndex = 8;
            this.labelToFrameRate.Text = "To frame rate";
            this.labelToFrameRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // buttonGetFrameRateFrom
            //
            this.buttonGetFrameRateFrom.Location = new System.Drawing.Point(246, 45);
            this.buttonGetFrameRateFrom.Name = "buttonGetFrameRateFrom";
            this.buttonGetFrameRateFrom.Size = new System.Drawing.Size(24, 22);
            this.buttonGetFrameRateFrom.TabIndex = 10;
            this.buttonGetFrameRateFrom.Text = "...";
            this.buttonGetFrameRateFrom.UseVisualStyleBackColor = true;
            this.buttonGetFrameRateFrom.Click += new System.EventHandler(this.ButtonGetFrameRateFromClick);
            //
            // buttonGetFrameRateTo
            //
            this.buttonGetFrameRateTo.Location = new System.Drawing.Point(246, 72);
            this.buttonGetFrameRateTo.Name = "buttonGetFrameRateTo";
            this.buttonGetFrameRateTo.Size = new System.Drawing.Size(24, 22);
            this.buttonGetFrameRateTo.TabIndex = 11;
            this.buttonGetFrameRateTo.Text = "...";
            this.buttonGetFrameRateTo.UseVisualStyleBackColor = true;
            this.buttonGetFrameRateTo.Click += new System.EventHandler(this.ButtonGetFrameRateToClick);
            //
            // openFileDialog1
            //
            this.openFileDialog1.FileName = "openFileDialog1";
            //
            // ChangeFrameRate
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 148);
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
            this.Text = "Change frame rate";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormChangeFrameRate_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}