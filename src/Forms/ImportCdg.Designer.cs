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
            this.radioButtonBluRaySup = new System.Windows.Forms.RadioButton();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.radioButtonVideo = new System.Windows.Forms.RadioButton();
            this.groupBoxMkvMerge = new System.Windows.Forms.GroupBox();
            this.buttonDownloadMkvToolNix = new System.Windows.Forms.Button();
            this.buttonBrowseToFFmpeg = new System.Windows.Forms.Button();
            this.textBoxFFmpegPath = new System.Windows.Forms.TextBox();
            this.labelFFmpegPath = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBoxVideoExportSettings = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxLeftRightMargin = new System.Windows.Forms.ComboBox();
            this.comboBoxBottomMargin = new System.Windows.Forms.ComboBox();
            this.labelBottomMargin = new System.Windows.Forms.Label();
            this.labelLeftRightMargin = new System.Windows.Forms.Label();
            this.comboBoxRes = new System.Windows.Forms.ComboBox();
            this.buttonAudioFileBrowse = new System.Windows.Forms.Button();
            this.labelAudioFileName = new System.Windows.Forms.Label();
            this.pictureBoxBackgroundImage = new System.Windows.Forms.PictureBox();
            this.labelBackgroundImage = new System.Windows.Forms.Label();
            this.buttonChooseBackgroundImage = new System.Windows.Forms.Button();
            this.buttonDownloadFfmpeg = new System.Windows.Forms.Button();
            this.groupBoxMkvMerge.SuspendLayout();
            this.groupBoxVideoExportSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBackgroundImage)).BeginInit();
            this.SuspendLayout();
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(12, 18);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(55, 13);
            this.labelFileName.TabIndex = 0;
            this.labelFileName.Text = "File name:";
            // 
            // buttonStart
            // 
            this.buttonStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStart.Location = new System.Drawing.Point(15, 178);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(154, 32);
            this.buttonStart.TabIndex = 5;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(844, 441);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
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
            this.labelDuration.TabIndex = 1;
            this.labelDuration.Text = "Duration:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Target format";
            // 
            // radioButtonBluRaySup
            // 
            this.radioButtonBluRaySup.AutoSize = true;
            this.radioButtonBluRaySup.Checked = true;
            this.radioButtonBluRaySup.Location = new System.Drawing.Point(29, 110);
            this.radioButtonBluRaySup.Name = "radioButtonBluRaySup";
            this.radioButtonBluRaySup.Size = new System.Drawing.Size(122, 17);
            this.radioButtonBluRaySup.TabIndex = 3;
            this.radioButtonBluRaySup.TabStop = true;
            this.radioButtonBluRaySup.Text = "Blu-ray subtitle (.sup)";
            this.radioButtonBluRaySup.UseVisualStyleBackColor = true;
            this.radioButtonBluRaySup.CheckedChanged += new System.EventHandler(this.radioButtonBluRaySup_CheckedChanged);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // radioButtonVideo
            // 
            this.radioButtonVideo.AutoSize = true;
            this.radioButtonVideo.Location = new System.Drawing.Point(29, 133);
            this.radioButtonVideo.Name = "radioButtonVideo";
            this.radioButtonVideo.Size = new System.Drawing.Size(241, 17);
            this.radioButtonVideo.TabIndex = 4;
            this.radioButtonVideo.Text = "Matroska video with Blu-ray subtitle softcoded";
            this.radioButtonVideo.UseVisualStyleBackColor = true;
            // 
            // groupBoxMkvMerge
            // 
            this.groupBoxMkvMerge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxMkvMerge.Controls.Add(this.buttonDownloadMkvToolNix);
            this.groupBoxMkvMerge.Controls.Add(this.buttonBrowseToFFmpeg);
            this.groupBoxMkvMerge.Controls.Add(this.textBoxFFmpegPath);
            this.groupBoxMkvMerge.Controls.Add(this.labelFFmpegPath);
            this.groupBoxMkvMerge.Location = new System.Drawing.Point(15, 323);
            this.groupBoxMkvMerge.Name = "groupBoxMkvMerge";
            this.groupBoxMkvMerge.Size = new System.Drawing.Size(426, 112);
            this.groupBoxMkvMerge.TabIndex = 7;
            this.groupBoxMkvMerge.TabStop = false;
            this.groupBoxMkvMerge.Text = "MKVToolNix mkvmerge";
            // 
            // buttonDownloadMkvToolNix
            // 
            this.buttonDownloadMkvToolNix.Location = new System.Drawing.Point(274, 17);
            this.buttonDownloadMkvToolNix.Name = "buttonDownloadMkvToolNix";
            this.buttonDownloadMkvToolNix.Size = new System.Drawing.Size(136, 23);
            this.buttonDownloadMkvToolNix.TabIndex = 0;
            this.buttonDownloadMkvToolNix.Text = "Download MKVToolNix";
            this.buttonDownloadMkvToolNix.UseVisualStyleBackColor = true;
            this.buttonDownloadMkvToolNix.Click += new System.EventHandler(this.buttonDownloadMkvToolNix_Click);
            // 
            // buttonBrowseToFFmpeg
            // 
            this.buttonBrowseToFFmpeg.Location = new System.Drawing.Point(381, 65);
            this.buttonBrowseToFFmpeg.Name = "buttonBrowseToFFmpeg";
            this.buttonBrowseToFFmpeg.Size = new System.Drawing.Size(29, 21);
            this.buttonBrowseToFFmpeg.TabIndex = 3;
            this.buttonBrowseToFFmpeg.Text = "...";
            this.buttonBrowseToFFmpeg.UseVisualStyleBackColor = true;
            this.buttonBrowseToFFmpeg.Click += new System.EventHandler(this.buttonBrowseToFFmpeg_Click);
            // 
            // textBoxFFmpegPath
            // 
            this.textBoxFFmpegPath.Location = new System.Drawing.Point(9, 65);
            this.textBoxFFmpegPath.MaxLength = 1000;
            this.textBoxFFmpegPath.Name = "textBoxFFmpegPath";
            this.textBoxFFmpegPath.Size = new System.Drawing.Size(366, 20);
            this.textBoxFFmpegPath.TabIndex = 2;
            // 
            // labelFFmpegPath
            // 
            this.labelFFmpegPath.AutoSize = true;
            this.labelFFmpegPath.Location = new System.Drawing.Point(6, 49);
            this.labelFFmpegPath.Name = "labelFFmpegPath";
            this.labelFFmpegPath.Size = new System.Drawing.Size(80, 13);
            this.labelFFmpegPath.TabIndex = 1;
            this.labelFFmpegPath.Text = "mkvmerge path";
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(9, 446);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 69;
            this.labelStatus.Text = "labelStatus";
            // 
            // groupBoxVideoExportSettings
            // 
            this.groupBoxVideoExportSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxVideoExportSettings.Controls.Add(this.label3);
            this.groupBoxVideoExportSettings.Controls.Add(this.label2);
            this.groupBoxVideoExportSettings.Controls.Add(this.comboBoxLeftRightMargin);
            this.groupBoxVideoExportSettings.Controls.Add(this.comboBoxBottomMargin);
            this.groupBoxVideoExportSettings.Controls.Add(this.labelBottomMargin);
            this.groupBoxVideoExportSettings.Controls.Add(this.labelLeftRightMargin);
            this.groupBoxVideoExportSettings.Controls.Add(this.comboBoxRes);
            this.groupBoxVideoExportSettings.Controls.Add(this.buttonAudioFileBrowse);
            this.groupBoxVideoExportSettings.Controls.Add(this.labelAudioFileName);
            this.groupBoxVideoExportSettings.Controls.Add(this.pictureBoxBackgroundImage);
            this.groupBoxVideoExportSettings.Controls.Add(this.labelBackgroundImage);
            this.groupBoxVideoExportSettings.Controls.Add(this.buttonChooseBackgroundImage);
            this.groupBoxVideoExportSettings.Location = new System.Drawing.Point(455, 12);
            this.groupBoxVideoExportSettings.Name = "groupBoxVideoExportSettings";
            this.groupBoxVideoExportSettings.Size = new System.Drawing.Size(464, 423);
            this.groupBoxVideoExportSettings.TabIndex = 8;
            this.groupBoxVideoExportSettings.TabStop = false;
            this.groupBoxVideoExportSettings.Text = "Video output settings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Audio file";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Resolution";
            // 
            // comboBoxLeftRightMargin
            // 
            this.comboBoxLeftRightMargin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLeftRightMargin.FormattingEnabled = true;
            this.comboBoxLeftRightMargin.Location = new System.Drawing.Point(123, 108);
            this.comboBoxLeftRightMargin.Name = "comboBoxLeftRightMargin";
            this.comboBoxLeftRightMargin.Size = new System.Drawing.Size(63, 21);
            this.comboBoxLeftRightMargin.TabIndex = 8;
            this.comboBoxLeftRightMargin.SelectedIndexChanged += new System.EventHandler(this.comboBoxLeftRightMargin_SelectedIndexChanged);
            // 
            // comboBoxBottomMargin
            // 
            this.comboBoxBottomMargin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBottomMargin.FormattingEnabled = true;
            this.comboBoxBottomMargin.Location = new System.Drawing.Point(123, 81);
            this.comboBoxBottomMargin.Name = "comboBoxBottomMargin";
            this.comboBoxBottomMargin.Size = new System.Drawing.Size(63, 21);
            this.comboBoxBottomMargin.TabIndex = 6;
            this.comboBoxBottomMargin.SelectedIndexChanged += new System.EventHandler(this.comboBoxBottomMargin_SelectedIndexChanged);
            // 
            // labelBottomMargin
            // 
            this.labelBottomMargin.AutoSize = true;
            this.labelBottomMargin.Location = new System.Drawing.Point(17, 84);
            this.labelBottomMargin.Name = "labelBottomMargin";
            this.labelBottomMargin.Size = new System.Drawing.Size(74, 13);
            this.labelBottomMargin.TabIndex = 5;
            this.labelBottomMargin.Text = "Bottom margin";
            // 
            // labelLeftRightMargin
            // 
            this.labelLeftRightMargin.AutoSize = true;
            this.labelLeftRightMargin.Location = new System.Drawing.Point(17, 110);
            this.labelLeftRightMargin.Name = "labelLeftRightMargin";
            this.labelLeftRightMargin.Size = new System.Drawing.Size(84, 13);
            this.labelLeftRightMargin.TabIndex = 7;
            this.labelLeftRightMargin.Text = "Left/right margin";
            // 
            // comboBoxRes
            // 
            this.comboBoxRes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRes.FormattingEnabled = true;
            this.comboBoxRes.Items.AddRange(new object[] {
            "620x350"});
            this.comboBoxRes.Location = new System.Drawing.Point(123, 54);
            this.comboBoxRes.Name = "comboBoxRes";
            this.comboBoxRes.Size = new System.Drawing.Size(121, 21);
            this.comboBoxRes.TabIndex = 4;
            // 
            // buttonAudioFileBrowse
            // 
            this.buttonAudioFileBrowse.Location = new System.Drawing.Point(123, 25);
            this.buttonAudioFileBrowse.Name = "buttonAudioFileBrowse";
            this.buttonAudioFileBrowse.Size = new System.Drawing.Size(25, 23);
            this.buttonAudioFileBrowse.TabIndex = 1;
            this.buttonAudioFileBrowse.Text = "...";
            this.buttonAudioFileBrowse.UseVisualStyleBackColor = true;
            this.buttonAudioFileBrowse.Click += new System.EventHandler(this.buttonAudioFileBrowse_Click);
            // 
            // labelAudioFileName
            // 
            this.labelAudioFileName.AutoSize = true;
            this.labelAudioFileName.Location = new System.Drawing.Point(154, 30);
            this.labelAudioFileName.Name = "labelAudioFileName";
            this.labelAudioFileName.Size = new System.Drawing.Size(79, 13);
            this.labelAudioFileName.TabIndex = 2;
            this.labelAudioFileName.Text = "Audio file name";
            // 
            // pictureBoxBackgroundImage
            // 
            this.pictureBoxBackgroundImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxBackgroundImage.Location = new System.Drawing.Point(20, 148);
            this.pictureBoxBackgroundImage.Name = "pictureBoxBackgroundImage";
            this.pictureBoxBackgroundImage.Size = new System.Drawing.Size(435, 235);
            this.pictureBoxBackgroundImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxBackgroundImage.TabIndex = 70;
            this.pictureBoxBackgroundImage.TabStop = false;
            // 
            // labelBackgroundImage
            // 
            this.labelBackgroundImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelBackgroundImage.AutoSize = true;
            this.labelBackgroundImage.Location = new System.Drawing.Point(17, 392);
            this.labelBackgroundImage.Name = "labelBackgroundImage";
            this.labelBackgroundImage.Size = new System.Drawing.Size(116, 13);
            this.labelBackgroundImage.TabIndex = 10;
            this.labelBackgroundImage.Text = "labelBackgroundImage";
            // 
            // buttonChooseBackgroundImage
            // 
            this.buttonChooseBackgroundImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChooseBackgroundImage.Location = new System.Drawing.Point(309, 119);
            this.buttonChooseBackgroundImage.Name = "buttonChooseBackgroundImage";
            this.buttonChooseBackgroundImage.Size = new System.Drawing.Size(146, 23);
            this.buttonChooseBackgroundImage.TabIndex = 9;
            this.buttonChooseBackgroundImage.Text = "Background image...";
            this.buttonChooseBackgroundImage.UseVisualStyleBackColor = true;
            this.buttonChooseBackgroundImage.Click += new System.EventHandler(this.buttonChooseBackgroundImage_Click);
            // 
            // buttonDownloadFfmpeg
            // 
            this.buttonDownloadFfmpeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownloadFfmpeg.Location = new System.Drawing.Point(257, 294);
            this.buttonDownloadFfmpeg.Name = "buttonDownloadFfmpeg";
            this.buttonDownloadFfmpeg.Size = new System.Drawing.Size(184, 23);
            this.buttonDownloadFfmpeg.TabIndex = 6;
            this.buttonDownloadFfmpeg.Text = "Download ffmpeg";
            this.buttonDownloadFfmpeg.UseVisualStyleBackColor = true;
            this.buttonDownloadFfmpeg.Click += new System.EventHandler(this.buttonDownloadFfmpeg_Click);
            // 
            // ImportCdg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(931, 476);
            this.Controls.Add(this.buttonDownloadFfmpeg);
            this.Controls.Add(this.groupBoxVideoExportSettings);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.groupBoxMkvMerge);
            this.Controls.Add(this.radioButtonVideo);
            this.Controls.Add(this.radioButtonBluRaySup);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelDuration);
            this.Controls.Add(this.labelFileName);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "ImportCdg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import CD+G kareoke";
            this.Shown += new System.EventHandler(this.ImportCdg_Shown);
            this.groupBoxMkvMerge.ResumeLayout(false);
            this.groupBoxMkvMerge.PerformLayout();
            this.groupBoxVideoExportSettings.ResumeLayout(false);
            this.groupBoxVideoExportSettings.PerformLayout();
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
        private System.Windows.Forms.RadioButton radioButtonBluRaySup;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.RadioButton radioButtonVideo;
        private System.Windows.Forms.GroupBox groupBoxMkvMerge;
        private System.Windows.Forms.Button buttonDownloadMkvToolNix;
        private System.Windows.Forms.Button buttonBrowseToFFmpeg;
        private System.Windows.Forms.TextBox textBoxFFmpegPath;
        private System.Windows.Forms.Label labelFFmpegPath;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.GroupBox groupBoxVideoExportSettings;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxLeftRightMargin;
        private System.Windows.Forms.ComboBox comboBoxBottomMargin;
        private System.Windows.Forms.Label labelBottomMargin;
        private System.Windows.Forms.Label labelLeftRightMargin;
        private System.Windows.Forms.ComboBox comboBoxRes;
        private System.Windows.Forms.Button buttonAudioFileBrowse;
        private System.Windows.Forms.Label labelAudioFileName;
        private System.Windows.Forms.PictureBox pictureBoxBackgroundImage;
        private System.Windows.Forms.Label labelBackgroundImage;
        private System.Windows.Forms.Button buttonChooseBackgroundImage;
        private System.Windows.Forms.Button buttonDownloadFfmpeg;
    }
}