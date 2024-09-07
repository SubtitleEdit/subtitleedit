﻿namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    sealed partial class VoskAudioToTextSelectedLines
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
            this.components = new System.ComponentModel.Container();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelProgress = new System.Windows.Forms.Label();
            this.textBoxLog = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelInfo = new System.Windows.Forms.Label();
            this.groupBoxModels = new System.Windows.Forms.GroupBox();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.linkLabelOpenModelsFolder = new System.Windows.Forms.LinkLabel();
            this.labelModel = new System.Windows.Forms.Label();
            this.comboBoxModels = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.linkLabelVoskWebSite = new System.Windows.Forms.LinkLabel();
            this.labelTime = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.checkBoxUsePostProcessing = new System.Windows.Forms.CheckBox();
            this.groupBoxInputFiles = new System.Windows.Forms.GroupBox();
            this.listViewInputFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxModels.SuspendLayout();
            this.groupBoxInputFiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(622, 427);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGenerate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenerate.Location = new System.Drawing.Point(491, 427);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(125, 23);
            this.buttonGenerate.TabIndex = 5;
            this.buttonGenerate.Text = "&Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.ButtonGenerate_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 427);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(473, 12);
            this.progressBar1.TabIndex = 4;
            this.progressBar1.Visible = false;
            // 
            // labelProgress
            // 
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(12, 409);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(70, 13);
            this.labelProgress.TabIndex = 4;
            this.labelProgress.Text = "labelProgress";
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxLog.Location = new System.Drawing.Point(529, 9);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(168, 258);
            this.textBoxLog.TabIndex = 0;
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(12, 9);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(288, 13);
            this.labelInfo.TabIndex = 1;
            this.labelInfo.Text = "Generate text from audio via Vosk/Kaldi speech recognition";
            // 
            // groupBoxModels
            // 
            this.groupBoxModels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxModels.Controls.Add(this.buttonDownload);
            this.groupBoxModels.Controls.Add(this.linkLabelOpenModelsFolder);
            this.groupBoxModels.Controls.Add(this.labelModel);
            this.groupBoxModels.Controls.Add(this.comboBoxModels);
            this.groupBoxModels.Location = new System.Drawing.Point(15, 66);
            this.groupBoxModels.Name = "groupBoxModels";
            this.groupBoxModels.Size = new System.Drawing.Size(682, 82);
            this.groupBoxModels.TabIndex = 1;
            this.groupBoxModels.TabStop = false;
            this.groupBoxModels.Text = "Models";
            // 
            // buttonDownload
            // 
            this.buttonDownload.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDownload.Location = new System.Drawing.Point(265, 43);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(28, 23);
            this.buttonDownload.TabIndex = 1;
            this.buttonDownload.Text = "...";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // linkLabelOpenModelsFolder
            // 
            this.linkLabelOpenModelsFolder.AutoSize = true;
            this.linkLabelOpenModelsFolder.Location = new System.Drawing.Point(301, 51);
            this.linkLabelOpenModelsFolder.Name = "linkLabelOpenModelsFolder";
            this.linkLabelOpenModelsFolder.Size = new System.Drawing.Size(98, 13);
            this.linkLabelOpenModelsFolder.TabIndex = 2;
            this.linkLabelOpenModelsFolder.TabStop = true;
            this.linkLabelOpenModelsFolder.Text = "Open models folder";
            this.linkLabelOpenModelsFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenModelFolder_LinkClicked);
            // 
            // labelModel
            // 
            this.labelModel.AutoSize = true;
            this.labelModel.Location = new System.Drawing.Point(16, 29);
            this.labelModel.Name = "labelModel";
            this.labelModel.Size = new System.Drawing.Size(167, 13);
            this.labelModel.TabIndex = 0;
            this.labelModel.Text = "Choose speech recognition model";
            // 
            // comboBoxModels
            // 
            this.comboBoxModels.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxModels.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxModels.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxModels.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxModels.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxModels.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxModels.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxModels.DropDownHeight = 400;
            this.comboBoxModels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxModels.DropDownWidth = 240;
            this.comboBoxModels.FormattingEnabled = true;
            this.comboBoxModels.Location = new System.Drawing.Point(19, 45);
            this.comboBoxModels.MaxLength = 32767;
            this.comboBoxModels.Name = "comboBoxModels";
            this.comboBoxModels.SelectedIndex = -1;
            this.comboBoxModels.SelectedItem = null;
            this.comboBoxModels.SelectedText = "";
            this.comboBoxModels.Size = new System.Drawing.Size(240, 23);
            this.comboBoxModels.TabIndex = 0;
            this.comboBoxModels.UsePopupWindow = false;
            this.comboBoxModels.SelectedIndexChanged += new System.EventHandler(this.comboBoxModels_SelectedIndexChanged);
            // 
            // linkLabelVoskWebSite
            // 
            this.linkLabelVoskWebSite.AutoSize = true;
            this.linkLabelVoskWebSite.Location = new System.Drawing.Point(12, 26);
            this.linkLabelVoskWebSite.Name = "linkLabelVoskWebSite";
            this.linkLabelVoskWebSite.Size = new System.Drawing.Size(70, 13);
            this.linkLabelVoskWebSite.TabIndex = 0;
            this.linkLabelVoskWebSite.TabStop = true;
            this.linkLabelVoskWebSite.Text = "Vosk website";
            this.linkLabelVoskWebSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelVoskWebsite_LinkClicked);
            // 
            // labelTime
            // 
            this.labelTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTime.AutoSize = true;
            this.labelTime.Location = new System.Drawing.Point(12, 442);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(88, 13);
            this.labelTime.TabIndex = 6;
            this.labelTime.Text = "Remaining time...";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // checkBoxUsePostProcessing
            // 
            this.checkBoxUsePostProcessing.AutoSize = true;
            this.checkBoxUsePostProcessing.Location = new System.Drawing.Point(15, 162);
            this.checkBoxUsePostProcessing.Name = "checkBoxUsePostProcessing";
            this.checkBoxUsePostProcessing.Size = new System.Drawing.Size(312, 17);
            this.checkBoxUsePostProcessing.TabIndex = 2;
            this.checkBoxUsePostProcessing.Text = "Use post-processing (line merge, fix casing, and punctuation)";
            this.checkBoxUsePostProcessing.UseVisualStyleBackColor = true;
            // 
            // groupBoxInputFiles
            // 
            this.groupBoxInputFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxInputFiles.Controls.Add(this.listViewInputFiles);
            this.groupBoxInputFiles.Location = new System.Drawing.Point(15, 200);
            this.groupBoxInputFiles.Name = "groupBoxInputFiles";
            this.groupBoxInputFiles.Size = new System.Drawing.Size(682, 185);
            this.groupBoxInputFiles.TabIndex = 3;
            this.groupBoxInputFiles.TabStop = false;
            this.groupBoxInputFiles.Text = "Input files";
            // 
            // listViewInputFiles
            // 
            this.listViewInputFiles.AllowDrop = true;
            this.listViewInputFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewInputFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFileName});
            this.listViewInputFiles.FullRowSelect = true;
            this.listViewInputFiles.HideSelection = false;
            this.listViewInputFiles.Location = new System.Drawing.Point(6, 18);
            this.listViewInputFiles.Name = "listViewInputFiles";
            this.listViewInputFiles.Size = new System.Drawing.Size(670, 150);
            this.listViewInputFiles.TabIndex = 0;
            this.listViewInputFiles.UseCompatibleStateImageBehavior = false;
            this.listViewInputFiles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderFileName
            // 
            this.columnHeaderFileName.Text = "File name";
            this.columnHeaderFileName.Width = 455;
            // 
            // VoskAudioToTextSelectedLines
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(709, 464);
            this.Controls.Add(this.groupBoxInputFiles);
            this.Controls.Add(this.checkBoxUsePostProcessing);
            this.Controls.Add(this.labelTime);
            this.Controls.Add(this.linkLabelVoskWebSite);
            this.Controls.Add(this.groupBoxModels);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.textBoxLog);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(720, 450);
            this.Name = "VoskAudioToTextSelectedLines";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Audio to text";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AudioToText_FormClosing);
            this.Load += new System.EventHandler(this.AudioToText_Load);
            this.Shown += new System.EventHandler(this.AudioToTextSelectedLines_Shown);
            this.ResizeEnd += new System.EventHandler(this.AudioToTextSelectedLines_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AudioToText_KeyDown);
            this.groupBoxModels.ResumeLayout(false);
            this.groupBoxModels.PerformLayout();
            this.groupBoxInputFiles.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelProgress;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxLog;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.GroupBox groupBoxModels;
        private System.Windows.Forms.LinkLabel linkLabelVoskWebSite;
        private System.Windows.Forms.Label labelModel;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxModels;
        private System.Windows.Forms.LinkLabel linkLabelOpenModelsFolder;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox checkBoxUsePostProcessing;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.GroupBox groupBoxInputFiles;
        private System.Windows.Forms.ListView listViewInputFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFileName;
    }
}