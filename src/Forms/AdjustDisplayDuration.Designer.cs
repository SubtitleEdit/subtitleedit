namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class AdjustDisplayDuration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdjustDisplayDuration));
            this.radioButtonPercent = new System.Windows.Forms.RadioButton();
            this.radioButtonSeconds = new System.Windows.Forms.RadioButton();
            this.groupBoxAdjustVia = new System.Windows.Forms.GroupBox();
            this.comboBoxPercent = new System.Windows.Forms.ComboBox();
            this.comboBoxSeconds = new System.Windows.Forms.ComboBox();
            this.labelNote = new System.Windows.Forms.Label();
            this.labelAddInPercent = new System.Windows.Forms.Label();
            this.labelAddSeconds = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxAdjustVia.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButtonPercent
            // 
            this.radioButtonPercent.AutoSize = true;
            this.radioButtonPercent.Location = new System.Drawing.Point(171, 21);
            this.radioButtonPercent.Name = "radioButtonPercent";
            this.radioButtonPercent.Size = new System.Drawing.Size(62, 17);
            this.radioButtonPercent.TabIndex = 0;
            this.radioButtonPercent.Text = "Percent";
            this.radioButtonPercent.UseVisualStyleBackColor = true;
            this.radioButtonPercent.CheckedChanged += new System.EventHandler(this.RadioButtonPercentCheckedChanged);
            // 
            // radioButtonSeconds
            // 
            this.radioButtonSeconds.AutoSize = true;
            this.radioButtonSeconds.Checked = true;
            this.radioButtonSeconds.Location = new System.Drawing.Point(11, 21);
            this.radioButtonSeconds.Name = "radioButtonSeconds";
            this.radioButtonSeconds.Size = new System.Drawing.Size(67, 17);
            this.radioButtonSeconds.TabIndex = 1;
            this.radioButtonSeconds.TabStop = true;
            this.radioButtonSeconds.Text = "Seconds";
            this.radioButtonSeconds.UseVisualStyleBackColor = true;
            this.radioButtonSeconds.CheckedChanged += new System.EventHandler(this.RadioButtonSecondsCheckedChanged);
            // 
            // groupBoxAdjustVia
            // 
            this.groupBoxAdjustVia.Controls.Add(this.radioButtonPercent);
            this.groupBoxAdjustVia.Controls.Add(this.radioButtonSeconds);
            this.groupBoxAdjustVia.Location = new System.Drawing.Point(13, 13);
            this.groupBoxAdjustVia.Name = "groupBoxAdjustVia";
            this.groupBoxAdjustVia.Size = new System.Drawing.Size(365, 47);
            this.groupBoxAdjustVia.TabIndex = 2;
            this.groupBoxAdjustVia.TabStop = false;
            this.groupBoxAdjustVia.Text = "Adjust via";
            // 
            // comboBoxPercent
            // 
            this.comboBoxPercent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPercent.Enabled = false;
            this.comboBoxPercent.FormattingEnabled = true;
            this.comboBoxPercent.Items.AddRange(new object[] {
            "- Please choose -",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "101",
            "102",
            "103",
            "104",
            "105",
            "106",
            "107",
            "108",
            "109",
            "110",
            "111",
            "112",
            "113",
            "114",
            "115",
            "116",
            "117",
            "118",
            "119",
            "120",
            "121",
            "122",
            "123",
            "124",
            "125"});
            this.comboBoxPercent.Location = new System.Drawing.Point(182, 89);
            this.comboBoxPercent.Name = "comboBoxPercent";
            this.comboBoxPercent.Size = new System.Drawing.Size(141, 21);
            this.comboBoxPercent.TabIndex = 3;
            // 
            // comboBoxSeconds
            // 
            this.comboBoxSeconds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSeconds.FormattingEnabled = true;
            this.comboBoxSeconds.Items.AddRange(new object[] {
            "- Please choose -",
            "-0.5",
            "-0.4",
            "-0.3",
            "-0.2",
            "-0.1",
            "+0.1",
            "+0.2",
            "+0.3",
            "+0.4",
            "+0.5",
            "+0.6",
            "+0.7",
            "+0.8",
            "+0.9",
            "+1.0"});
            this.comboBoxSeconds.Location = new System.Drawing.Point(13, 89);
            this.comboBoxSeconds.Name = "comboBoxSeconds";
            this.comboBoxSeconds.Size = new System.Drawing.Size(141, 21);
            this.comboBoxSeconds.TabIndex = 4;
            // 
            // labelNote
            // 
            this.labelNote.AutoSize = true;
            this.labelNote.Location = new System.Drawing.Point(10, 135);
            this.labelNote.Name = "labelNote";
            this.labelNote.Size = new System.Drawing.Size(279, 13);
            this.labelNote.TabIndex = 5;
            this.labelNote.Text = "Note: Display time will not overlap start time of next text";
            // 
            // labelAddInPercent
            // 
            this.labelAddInPercent.AutoSize = true;
            this.labelAddInPercent.Location = new System.Drawing.Point(179, 70);
            this.labelAddInPercent.Name = "labelAddInPercent";
            this.labelAddInPercent.Size = new System.Drawing.Size(89, 13);
            this.labelAddInPercent.TabIndex = 6;
            this.labelAddInPercent.Text = "Adjust in percent";
            // 
            // labelAddSeconds
            // 
            this.labelAddSeconds.AutoSize = true;
            this.labelAddSeconds.Location = new System.Drawing.Point(10, 70);
            this.labelAddSeconds.Name = "labelAddSeconds";
            this.labelAddSeconds.Size = new System.Drawing.Size(68, 13);
            this.labelAddSeconds.TabIndex = 7;
            this.labelAddSeconds.Text = "Add seconds";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(303, 169);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(222, 169);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // AdjustDisplayDuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 206);
            this.Controls.Add(this.comboBoxSeconds);
            this.Controls.Add(this.comboBoxPercent);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelAddSeconds);
            this.Controls.Add(this.labelAddInPercent);
            this.Controls.Add(this.labelNote);
            this.Controls.Add(this.groupBoxAdjustVia);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdjustDisplayDuration";
            this.ShowInTaskbar = false;
            this.Text = "Adjust display time";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormAdjustDisplayTime_KeyDown);
            this.groupBoxAdjustVia.ResumeLayout(false);
            this.groupBoxAdjustVia.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonPercent;
        private System.Windows.Forms.RadioButton radioButtonSeconds;
        private System.Windows.Forms.GroupBox groupBoxAdjustVia;
        private System.Windows.Forms.ComboBox comboBoxPercent;
        private System.Windows.Forms.ComboBox comboBoxSeconds;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.Label labelAddInPercent;
        private System.Windows.Forms.Label labelAddSeconds;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
    }
}