
namespace Nikse.SubtitleEdit.Forms.BinaryEdit
{
    sealed partial class BinEditResize
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
            this.trackBarResize = new System.Windows.Forms.TrackBar();
            this.pictureBoxResized = new System.Windows.Forms.PictureBox();
            this.labelResize = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxAlignment = new System.Windows.Forms.GroupBox();
            this.comboBoxAlignment = new System.Windows.Forms.ComboBox();
            this.checkBoxFixAlignment = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarResize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResized)).BeginInit();
            this.groupBoxAlignment.SuspendLayout();
            this.SuspendLayout();
            // 
            // trackBarResize
            // 
            this.trackBarResize.Location = new System.Drawing.Point(12, 51);
            this.trackBarResize.Maximum = 500;
            this.trackBarResize.Minimum = 10;
            this.trackBarResize.Name = "trackBarResize";
            this.trackBarResize.Size = new System.Drawing.Size(429, 45);
            this.trackBarResize.TabIndex = 0;
            this.trackBarResize.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarResize.Value = 100;
            this.trackBarResize.Scroll += new System.EventHandler(this.trackBarResize_Scroll);
            // 
            // pictureBoxResized
            // 
            this.pictureBoxResized.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxResized.Location = new System.Drawing.Point(6, 102);
            this.pictureBoxResized.Name = "pictureBoxResized";
            this.pictureBoxResized.Size = new System.Drawing.Size(832, 161);
            this.pictureBoxResized.TabIndex = 1;
            this.pictureBoxResized.TabStop = false;
            // 
            // labelResize
            // 
            this.labelResize.AutoSize = true;
            this.labelResize.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResize.Location = new System.Drawing.Point(12, 30);
            this.labelResize.Name = "labelResize";
            this.labelResize.Size = new System.Drawing.Size(154, 18);
            this.labelResize.TabIndex = 2;
            this.labelResize.Text = "Resize in % - 100%";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(685, 269);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(769, 269);
            this.buttonCancel.MinimumSize = new System.Drawing.Size(75, 23);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBoxAlignment
            // 
            this.groupBoxAlignment.Controls.Add(this.comboBoxAlignment);
            this.groupBoxAlignment.Controls.Add(this.checkBoxFixAlignment);
            this.groupBoxAlignment.Location = new System.Drawing.Point(497, 12);
            this.groupBoxAlignment.Name = "groupBoxAlignment";
            this.groupBoxAlignment.Size = new System.Drawing.Size(347, 84);
            this.groupBoxAlignment.TabIndex = 10;
            this.groupBoxAlignment.TabStop = false;
            this.groupBoxAlignment.Text = "Alignment";
            // 
            // comboBoxAlignment
            // 
            this.comboBoxAlignment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAlignment.FormattingEnabled = true;
            this.comboBoxAlignment.Location = new System.Drawing.Point(6, 44);
            this.comboBoxAlignment.Name = "comboBoxAlignment";
            this.comboBoxAlignment.Size = new System.Drawing.Size(257, 21);
            this.comboBoxAlignment.TabIndex = 11;
            this.comboBoxAlignment.SelectedIndexChanged += new System.EventHandler(this.comboBoxAlignment_SelectedIndexChanged);
            // 
            // checkBoxFixAlignment
            // 
            this.checkBoxFixAlignment.AutoSize = true;
            this.checkBoxFixAlignment.Location = new System.Drawing.Point(6, 21);
            this.checkBoxFixAlignment.Name = "checkBoxFixAlignment";
            this.checkBoxFixAlignment.Size = new System.Drawing.Size(141, 17);
            this.checkBoxFixAlignment.TabIndex = 0;
            this.checkBoxFixAlignment.Text = "Fix alignment after resize";
            this.checkBoxFixAlignment.UseVisualStyleBackColor = true;
            this.checkBoxFixAlignment.CheckedChanged += new System.EventHandler(this.checkBoxFixAlignment_CheckedChanged);
            // 
            // BinEditResize
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(856, 304);
            this.Controls.Add(this.groupBoxAlignment);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelResize);
            this.Controls.Add(this.pictureBoxResized);
            this.Controls.Add(this.trackBarResize);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(500, 343);
            this.Name = "BinEditResize";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BinEditResize";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BinEditResize_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarResize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResized)).EndInit();
            this.groupBoxAlignment.ResumeLayout(false);
            this.groupBoxAlignment.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar trackBarResize;
        private System.Windows.Forms.PictureBox pictureBoxResized;
        private System.Windows.Forms.Label labelResize;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxAlignment;
        private System.Windows.Forms.ComboBox comboBoxAlignment;
        private System.Windows.Forms.CheckBox checkBoxFixAlignment;
    }
}