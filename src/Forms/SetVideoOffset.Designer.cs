namespace Nikse.SubtitleEdit.Forms
{
    partial class SetVideoOffset
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
            this.labelDescription = new System.Windows.Forms.Label();
            this.checkBoxFromCurrentPosition = new System.Windows.Forms.CheckBox();
            this.timeUpDownVideoPosition = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(352, 109);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(271, 109);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelDescription.Location = new System.Drawing.Point(12, 22);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(81, 13);
            this.labelDescription.TabIndex = 3;
            this.labelDescription.Text = "Set video offset";
            // 
            // checkBoxFromCurrentPosition
            // 
            this.checkBoxFromCurrentPosition.AutoSize = true;
            this.checkBoxFromCurrentPosition.Location = new System.Drawing.Point(15, 71);
            this.checkBoxFromCurrentPosition.Name = "checkBoxFromCurrentPosition";
            this.checkBoxFromCurrentPosition.Size = new System.Drawing.Size(153, 17);
            this.checkBoxFromCurrentPosition.TabIndex = 14;
            this.checkBoxFromCurrentPosition.Text = "From current video position";
            this.checkBoxFromCurrentPosition.UseVisualStyleBackColor = true;
            // 
            // timeUpDownVideoPosition
            // 
            this.timeUpDownVideoPosition.AutoSize = true;
            this.timeUpDownVideoPosition.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownVideoPosition.Location = new System.Drawing.Point(13, 39);
            this.timeUpDownVideoPosition.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownVideoPosition.Name = "timeUpDownVideoPosition";
            this.timeUpDownVideoPosition.Size = new System.Drawing.Size(96, 24);
            this.timeUpDownVideoPosition.TabIndex = 13;
            // 
            // SetVideoOffset
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 142);
            this.Controls.Add(this.checkBoxFromCurrentPosition);
            this.Controls.Add(this.timeUpDownVideoPosition);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelDescription);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetVideoOffset";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SetVideoOffset";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelDescription;
        private Controls.TimeUpDown timeUpDownVideoPosition;
        private System.Windows.Forms.CheckBox checkBoxFromCurrentPosition;
    }
}