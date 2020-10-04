namespace Nikse.SubtitleEdit.Forms
{
    partial class ImportCdg
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
            this.labelFileName = new System.Windows.Forms.Label();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelDuration = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.labelProgress = new System.Windows.Forms.Label();
            this.radioButtonBluRaySup = new System.Windows.Forms.RadioButton();
            this.labelProgress2 = new System.Windows.Forms.Label();
            this.buttonChooseBackgroundImage = new System.Windows.Forms.Button();
            this.labelBackgroundImage = new System.Windows.Forms.Label();
            this.pictureBoxBackgroundImage = new System.Windows.Forms.PictureBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.labelAudioFileName = new System.Windows.Forms.Label();
            this.buttonAudioFileBrowse = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBackgroundImage)).BeginInit();
            this.SuspendLayout();
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(12, 18);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(55, 13);
            this.labelFileName.TabIndex = 6;
            this.labelFileName.Text = "File name:";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(15, 146);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(154, 32);
            this.buttonStart.TabIndex = 4;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(886, 439);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(12, 39);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(50, 13);
            this.labelDuration.TabIndex = 7;
            this.labelDuration.Text = "Duration:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Target format";
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(15, 181);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(70, 13);
            this.labelProgress.TabIndex = 11;
            this.labelProgress.Text = "labelProgress";
            // 
            // radioButtonBluRaySup
            // 
            this.radioButtonBluRaySup.AutoSize = true;
            this.radioButtonBluRaySup.Checked = true;
            this.radioButtonBluRaySup.Location = new System.Drawing.Point(29, 110);
            this.radioButtonBluRaySup.Name = "radioButtonBluRaySup";
            this.radioButtonBluRaySup.Size = new System.Drawing.Size(200, 17);
            this.radioButtonBluRaySup.TabIndex = 13;
            this.radioButtonBluRaySup.TabStop = true;
            this.radioButtonBluRaySup.Text = "Blu-ray  (.sup) - does not require OCR";
            this.radioButtonBluRaySup.UseVisualStyleBackColor = true;
            // 
            // labelProgress2
            // 
            this.labelProgress2.AutoSize = true;
            this.labelProgress2.Location = new System.Drawing.Point(15, 199);
            this.labelProgress2.Name = "labelProgress2";
            this.labelProgress2.Size = new System.Drawing.Size(76, 13);
            this.labelProgress2.TabIndex = 14;
            this.labelProgress2.Text = "labelProgress2";
            // 
            // buttonChooseBackgroundImage
            // 
            this.buttonChooseBackgroundImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChooseBackgroundImage.Location = new System.Drawing.Point(757, 75);
            this.buttonChooseBackgroundImage.Name = "buttonChooseBackgroundImage";
            this.buttonChooseBackgroundImage.Size = new System.Drawing.Size(204, 23);
            this.buttonChooseBackgroundImage.TabIndex = 15;
            this.buttonChooseBackgroundImage.Text = "Background image...";
            this.buttonChooseBackgroundImage.UseVisualStyleBackColor = true;
            this.buttonChooseBackgroundImage.Click += new System.EventHandler(this.buttonChooseBackgroundImage_Click);
            // 
            // labelBackgroundImage
            // 
            this.labelBackgroundImage.AutoSize = true;
            this.labelBackgroundImage.Location = new System.Drawing.Point(305, 80);
            this.labelBackgroundImage.Name = "labelBackgroundImage";
            this.labelBackgroundImage.Size = new System.Drawing.Size(116, 13);
            this.labelBackgroundImage.TabIndex = 16;
            this.labelBackgroundImage.Text = "labelBackgroundImage";
            // 
            // pictureBoxBackgroundImage
            // 
            this.pictureBoxBackgroundImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxBackgroundImage.Location = new System.Drawing.Point(308, 104);
            this.pictureBoxBackgroundImage.Name = "pictureBoxBackgroundImage";
            this.pictureBoxBackgroundImage.Size = new System.Drawing.Size(653, 329);
            this.pictureBoxBackgroundImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxBackgroundImage.TabIndex = 17;
            this.pictureBoxBackgroundImage.TabStop = false;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // labelAudioFileName
            // 
            this.labelAudioFileName.AutoSize = true;
            this.labelAudioFileName.Location = new System.Drawing.Point(305, 15);
            this.labelAudioFileName.Name = "labelAudioFileName";
            this.labelAudioFileName.Size = new System.Drawing.Size(82, 13);
            this.labelAudioFileName.TabIndex = 18;
            this.labelAudioFileName.Text = "Audio file name:";
            // 
            // buttonAudioFileBrowse
            // 
            this.buttonAudioFileBrowse.Location = new System.Drawing.Point(308, 32);
            this.buttonAudioFileBrowse.Name = "buttonAudioFileBrowse";
            this.buttonAudioFileBrowse.Size = new System.Drawing.Size(25, 23);
            this.buttonAudioFileBrowse.TabIndex = 19;
            this.buttonAudioFileBrowse.Text = "...";
            this.buttonAudioFileBrowse.UseVisualStyleBackColor = true;
            this.buttonAudioFileBrowse.Click += new System.EventHandler(this.buttonAudioFileBrowse_Click);
            // 
            // ImportCdg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(973, 474);
            this.Controls.Add(this.buttonAudioFileBrowse);
            this.Controls.Add(this.labelAudioFileName);
            this.Controls.Add(this.pictureBoxBackgroundImage);
            this.Controls.Add(this.labelBackgroundImage);
            this.Controls.Add(this.buttonChooseBackgroundImage);
            this.Controls.Add(this.labelProgress2);
            this.Controls.Add(this.radioButtonBluRaySup);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelDuration);
            this.Controls.Add(this.labelFileName);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.Name = "ImportCdg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import CD+G kareoke";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBackgroundImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.RadioButton radioButtonBluRaySup;
        private System.Windows.Forms.Label labelProgress2;
        private System.Windows.Forms.Button buttonChooseBackgroundImage;
        private System.Windows.Forms.Label labelBackgroundImage;
        private System.Windows.Forms.PictureBox pictureBoxBackgroundImage;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label labelAudioFileName;
        private System.Windows.Forms.Button buttonAudioFileBrowse;
    }
}