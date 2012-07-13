namespace Nikse.SubtitleEdit.Forms
{
    partial class TimedTextProperties
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
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.textBoxDescription = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxTitle = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxDefaultRegion = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxDropMode = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxFrameRate = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxTimeBase = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.labelCollision = new System.Windows.Forms.Label();
            this.comboBoxDefaultStyle = new System.Windows.Forms.ComboBox();
            this.labelWrapStyle = new System.Windows.Forms.Label();
            this.comboBoxFrameRateMultiplier = new System.Windows.Forms.ComboBox();
            this.groupBoxOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(335, 328);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(416, 328);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOptions.Controls.Add(this.comboBoxFrameRateMultiplier);
            this.groupBoxOptions.Controls.Add(this.textBoxDescription);
            this.groupBoxOptions.Controls.Add(this.label7);
            this.groupBoxOptions.Controls.Add(this.textBoxTitle);
            this.groupBoxOptions.Controls.Add(this.label6);
            this.groupBoxOptions.Controls.Add(this.comboBoxDefaultRegion);
            this.groupBoxOptions.Controls.Add(this.label5);
            this.groupBoxOptions.Controls.Add(this.comboBoxDropMode);
            this.groupBoxOptions.Controls.Add(this.label4);
            this.groupBoxOptions.Controls.Add(this.label3);
            this.groupBoxOptions.Controls.Add(this.comboBoxFrameRate);
            this.groupBoxOptions.Controls.Add(this.label2);
            this.groupBoxOptions.Controls.Add(this.comboBoxTimeBase);
            this.groupBoxOptions.Controls.Add(this.label1);
            this.groupBoxOptions.Controls.Add(this.comboBoxLanguage);
            this.groupBoxOptions.Controls.Add(this.labelCollision);
            this.groupBoxOptions.Controls.Add(this.comboBoxDefaultStyle);
            this.groupBoxOptions.Controls.Add(this.labelWrapStyle);
            this.groupBoxOptions.Location = new System.Drawing.Point(12, 12);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(479, 310);
            this.groupBoxOptions.TabIndex = 0;
            this.groupBoxOptions.TabStop = false;
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Location = new System.Drawing.Point(191, 56);
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(263, 20);
            this.textBoxDescription.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 58);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Description";
            // 
            // textBoxTitle
            // 
            this.textBoxTitle.Location = new System.Drawing.Point(191, 30);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new System.Drawing.Size(263, 20);
            this.textBoxTitle.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 32);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(27, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Title";
            // 
            // comboBoxDefaultRegion
            // 
            this.comboBoxDefaultRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDefaultRegion.FormattingEnabled = true;
            this.comboBoxDefaultRegion.Location = new System.Drawing.Point(191, 270);
            this.comboBoxDefaultRegion.Name = "comboBoxDefaultRegion";
            this.comboBoxDefaultRegion.Size = new System.Drawing.Size(263, 21);
            this.comboBoxDefaultRegion.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 273);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(73, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Default region";
            // 
            // comboBoxDropMode
            // 
            this.comboBoxDropMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDropMode.FormattingEnabled = true;
            this.comboBoxDropMode.Items.AddRange(new object[] {
            "[N/A]",
            "dropNTSC",
            "dropPAL",
            "nonDrop"});
            this.comboBoxDropMode.Location = new System.Drawing.Point(191, 203);
            this.comboBoxDropMode.Name = "comboBoxDropMode";
            this.comboBoxDropMode.Size = new System.Drawing.Size(263, 21);
            this.comboBoxDropMode.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 206);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Drop mode";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 179);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Frame rate multiplier";
            // 
            // comboBoxFrameRate
            // 
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Location = new System.Drawing.Point(191, 149);
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(263, 21);
            this.comboBoxFrameRate.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 152);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Frame rate";
            // 
            // comboBoxTimeBase
            // 
            this.comboBoxTimeBase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTimeBase.FormattingEnabled = true;
            this.comboBoxTimeBase.Items.AddRange(new object[] {
            "[N/A]",
            "media",
            "smpte",
            "clock"});
            this.comboBoxTimeBase.Location = new System.Drawing.Point(191, 122);
            this.comboBoxTimeBase.Name = "comboBoxTimeBase";
            this.comboBoxTimeBase.Size = new System.Drawing.Size(263, 21);
            this.comboBoxTimeBase.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 125);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Time base";
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(191, 95);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(263, 21);
            this.comboBoxLanguage.TabIndex = 2;
            // 
            // labelCollision
            // 
            this.labelCollision.AutoSize = true;
            this.labelCollision.Location = new System.Drawing.Point(6, 98);
            this.labelCollision.Name = "labelCollision";
            this.labelCollision.Size = new System.Drawing.Size(55, 13);
            this.labelCollision.TabIndex = 3;
            this.labelCollision.Text = "Language";
            // 
            // comboBoxDefaultStyle
            // 
            this.comboBoxDefaultStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDefaultStyle.FormattingEnabled = true;
            this.comboBoxDefaultStyle.Location = new System.Drawing.Point(191, 243);
            this.comboBoxDefaultStyle.Name = "comboBoxDefaultStyle";
            this.comboBoxDefaultStyle.Size = new System.Drawing.Size(263, 21);
            this.comboBoxDefaultStyle.TabIndex = 7;
            // 
            // labelWrapStyle
            // 
            this.labelWrapStyle.AutoSize = true;
            this.labelWrapStyle.Location = new System.Drawing.Point(6, 246);
            this.labelWrapStyle.Name = "labelWrapStyle";
            this.labelWrapStyle.Size = new System.Drawing.Size(65, 13);
            this.labelWrapStyle.TabIndex = 1;
            this.labelWrapStyle.Text = "Default style";
            // 
            // comboBoxFrameRateMultiplier
            // 
            this.comboBoxFrameRateMultiplier.FormattingEnabled = true;
            this.comboBoxFrameRateMultiplier.Items.AddRange(new object[] {
            "1 1",
            "999 1000"});
            this.comboBoxFrameRateMultiplier.Location = new System.Drawing.Point(191, 176);
            this.comboBoxFrameRateMultiplier.Name = "comboBoxFrameRateMultiplier";
            this.comboBoxFrameRateMultiplier.Size = new System.Drawing.Size(263, 21);
            this.comboBoxFrameRateMultiplier.TabIndex = 18;
            // 
            // TimedTextProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(503, 361);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxOptions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TimedTextProperties";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Timed Text properties";
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.ComboBox comboBoxDropMode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxFrameRate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxTimeBase;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelCollision;
        private System.Windows.Forms.ComboBox comboBoxDefaultStyle;
        private System.Windows.Forms.Label labelWrapStyle;
        private System.Windows.Forms.ComboBox comboBoxDefaultRegion;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxTitle;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxFrameRateMultiplier;
    }
}