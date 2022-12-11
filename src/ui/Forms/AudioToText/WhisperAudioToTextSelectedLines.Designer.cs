namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    sealed partial class WhisperAudioToTextSelectedLines
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
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.labelInfo = new System.Windows.Forms.Label();
            this.groupBoxModels = new System.Windows.Forms.GroupBox();
            this.labelChooseLanguage = new System.Windows.Forms.Label();
            this.comboBoxLanguages = new System.Windows.Forms.ComboBox();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.linkLabelOpenModelsFolder = new System.Windows.Forms.LinkLabel();
            this.labelModel = new System.Windows.Forms.Label();
            this.comboBoxModels = new System.Windows.Forms.ComboBox();
            this.linkLabeWhisperWebSite = new System.Windows.Forms.LinkLabel();
            this.labelTime = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.checkBoxUsePostProcessing = new System.Windows.Forms.CheckBox();
            this.groupBoxInputFiles = new System.Windows.Forms.GroupBox();
            this.listViewInputFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.checkBoxTranslateToEnglish = new System.Windows.Forms.CheckBox();
            this.labelCpp = new System.Windows.Forms.Label();
            this.contextMenuStripWhisperAdvanced = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.whisperPhpOriginalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.whisperCppCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.removeTemporaryFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxModels.SuspendLayout();
            this.groupBoxInputFiles.SuspendLayout();
            this.contextMenuStripWhisperAdvanced.SuspendLayout();
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
            this.textBoxLog.Location = new System.Drawing.Point(469, 9);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(168, 258);
            this.textBoxLog.TabIndex = 0;
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(12, 9);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(275, 13);
            this.labelInfo.TabIndex = 1;
            this.labelInfo.Text = "Generate text from audio via Whisper speech recognition";
            // 
            // groupBoxModels
            // 
            this.groupBoxModels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxModels.Controls.Add(this.labelChooseLanguage);
            this.groupBoxModels.Controls.Add(this.comboBoxLanguages);
            this.groupBoxModels.Controls.Add(this.buttonDownload);
            this.groupBoxModels.Controls.Add(this.linkLabelOpenModelsFolder);
            this.groupBoxModels.Controls.Add(this.labelModel);
            this.groupBoxModels.Controls.Add(this.comboBoxModels);
            this.groupBoxModels.Location = new System.Drawing.Point(15, 60);
            this.groupBoxModels.Name = "groupBoxModels";
            this.groupBoxModels.Size = new System.Drawing.Size(682, 82);
            this.groupBoxModels.TabIndex = 1;
            this.groupBoxModels.TabStop = false;
            this.groupBoxModels.Text = "Models";
            // 
            // labelChooseLanguage
            // 
            this.labelChooseLanguage.AutoSize = true;
            this.labelChooseLanguage.Location = new System.Drawing.Point(3, 37);
            this.labelChooseLanguage.Name = "labelChooseLanguage";
            this.labelChooseLanguage.Size = new System.Drawing.Size(90, 13);
            this.labelChooseLanguage.TabIndex = 6;
            this.labelChooseLanguage.Text = "Choose language";
            // 
            // comboBoxLanguages
            // 
            this.comboBoxLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguages.FormattingEnabled = true;
            this.comboBoxLanguages.Location = new System.Drawing.Point(6, 53);
            this.comboBoxLanguages.Name = "comboBoxLanguages";
            this.comboBoxLanguages.Size = new System.Drawing.Size(194, 21);
            this.comboBoxLanguages.TabIndex = 7;
            this.comboBoxLanguages.SelectedIndexChanged += new System.EventHandler(this.comboBoxLanguages_SelectedIndexChanged);
            // 
            // buttonDownload
            // 
            this.buttonDownload.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDownload.Location = new System.Drawing.Point(503, 51);
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
            this.linkLabelOpenModelsFolder.Location = new System.Drawing.Point(539, 59);
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
            this.labelModel.Location = new System.Drawing.Point(254, 37);
            this.labelModel.Name = "labelModel";
            this.labelModel.Size = new System.Drawing.Size(167, 13);
            this.labelModel.TabIndex = 0;
            this.labelModel.Text = "Choose speech recognition model";
            // 
            // comboBoxModels
            // 
            this.comboBoxModels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxModels.FormattingEnabled = true;
            this.comboBoxModels.Location = new System.Drawing.Point(257, 53);
            this.comboBoxModels.Name = "comboBoxModels";
            this.comboBoxModels.Size = new System.Drawing.Size(240, 21);
            this.comboBoxModels.TabIndex = 0;
            // 
            // linkLabeWhisperWebSite
            // 
            this.linkLabeWhisperWebSite.AutoSize = true;
            this.linkLabeWhisperWebSite.Location = new System.Drawing.Point(12, 26);
            this.linkLabeWhisperWebSite.Name = "linkLabeWhisperWebSite";
            this.linkLabeWhisperWebSite.Size = new System.Drawing.Size(85, 13);
            this.linkLabeWhisperWebSite.TabIndex = 0;
            this.linkLabeWhisperWebSite.TabStop = true;
            this.linkLabeWhisperWebSite.Text = "Whisper website";
            this.linkLabeWhisperWebSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelWhisperWebsite_LinkClicked);
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
            this.checkBoxUsePostProcessing.Location = new System.Drawing.Point(15, 172);
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
            // checkBoxTranslateToEnglish
            // 
            this.checkBoxTranslateToEnglish.AutoSize = true;
            this.checkBoxTranslateToEnglish.Location = new System.Drawing.Point(15, 148);
            this.checkBoxTranslateToEnglish.Name = "checkBoxTranslateToEnglish";
            this.checkBoxTranslateToEnglish.Size = new System.Drawing.Size(119, 17);
            this.checkBoxTranslateToEnglish.TabIndex = 21;
            this.checkBoxTranslateToEnglish.Text = "Translate to English";
            this.checkBoxTranslateToEnglish.UseVisualStyleBackColor = true;
            // 
            // labelCpp
            // 
            this.labelCpp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCpp.AutoSize = true;
            this.labelCpp.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.labelCpp.Location = new System.Drawing.Point(669, 9);
            this.labelCpp.Name = "labelCpp";
            this.labelCpp.Size = new System.Drawing.Size(28, 13);
            this.labelCpp.TabIndex = 22;
            this.labelCpp.Text = "CPP";
            // 
            // contextMenuStripWhisperAdvanced
            // 
            this.contextMenuStripWhisperAdvanced.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.whisperPhpOriginalToolStripMenuItem,
            this.whisperCppCToolStripMenuItem,
            this.toolStripSeparator1,
            this.removeTemporaryFilesToolStripMenuItem});
            this.contextMenuStripWhisperAdvanced.Name = "contextMenuStripWhisperAdvanced";
            this.contextMenuStripWhisperAdvanced.Size = new System.Drawing.Size(200, 98);
            // 
            // whisperPhpOriginalToolStripMenuItem
            // 
            this.whisperPhpOriginalToolStripMenuItem.Name = "whisperPhpOriginalToolStripMenuItem";
            this.whisperPhpOriginalToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.whisperPhpOriginalToolStripMenuItem.Text = "Whisper OpenAI (php)";
            this.whisperPhpOriginalToolStripMenuItem.Click += new System.EventHandler(this.whisperPhpOriginalToolStripMenuItem_Click);
            // 
            // whisperCppCToolStripMenuItem
            // 
            this.whisperCppCToolStripMenuItem.Name = "whisperCppCToolStripMenuItem";
            this.whisperCppCToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.whisperCppCToolStripMenuItem.Text = "Whisper cpp (C++)";
            this.whisperCppCToolStripMenuItem.Click += new System.EventHandler(this.whisperCppCToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(196, 6);
            // 
            // removeTemporaryFilesToolStripMenuItem
            // 
            this.removeTemporaryFilesToolStripMenuItem.Name = "removeTemporaryFilesToolStripMenuItem";
            this.removeTemporaryFilesToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.removeTemporaryFilesToolStripMenuItem.Text = "Remove temporary files";
            this.removeTemporaryFilesToolStripMenuItem.Click += new System.EventHandler(this.removeTemporaryFilesToolStripMenuItem_Click);
            // 
            // WhisperAudioToTextSelectedLines
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(709, 464);
            this.Controls.Add(this.labelCpp);
            this.Controls.Add(this.checkBoxTranslateToEnglish);
            this.Controls.Add(this.groupBoxInputFiles);
            this.Controls.Add(this.checkBoxUsePostProcessing);
            this.Controls.Add(this.labelTime);
            this.Controls.Add(this.linkLabeWhisperWebSite);
            this.Controls.Add(this.groupBoxModels);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.textBoxLog);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(720, 450);
            this.Name = "WhisperAudioToTextSelectedLines";
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
            this.contextMenuStripWhisperAdvanced.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.GroupBox groupBoxModels;
        private System.Windows.Forms.LinkLabel linkLabeWhisperWebSite;
        private System.Windows.Forms.Label labelModel;
        private System.Windows.Forms.ComboBox comboBoxModels;
        private System.Windows.Forms.LinkLabel linkLabelOpenModelsFolder;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox checkBoxUsePostProcessing;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.GroupBox groupBoxInputFiles;
        private System.Windows.Forms.ListView listViewInputFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFileName;
        private System.Windows.Forms.Label labelChooseLanguage;
        private System.Windows.Forms.ComboBox comboBoxLanguages;
        private System.Windows.Forms.CheckBox checkBoxTranslateToEnglish;
        private System.Windows.Forms.Label labelCpp;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripWhisperAdvanced;
        private System.Windows.Forms.ToolStripMenuItem whisperPhpOriginalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem whisperCppCToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem removeTemporaryFilesToolStripMenuItem;
    }
}