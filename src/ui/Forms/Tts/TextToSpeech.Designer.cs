namespace Nikse.SubtitleEdit.Forms.Tts
{
    sealed partial class TextToSpeech
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelProgress = new System.Windows.Forms.Label();
            this.buttonGenerateTTS = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelEngine = new System.Windows.Forms.Label();
            this.groupBoxSettings = new System.Windows.Forms.GroupBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.linkLabelCustomAudio = new System.Windows.Forms.LinkLabel();
            this.checkBoxAudioEncoding = new System.Windows.Forms.CheckBox();
            this.labelRegion = new System.Windows.Forms.Label();
            this.labelVoiceCount = new System.Windows.Forms.Label();
            this.checkBoxShowPreview = new System.Windows.Forms.CheckBox();
            this.labelApiKey = new System.Windows.Forms.Label();
            this.buttonTestVoice = new System.Windows.Forms.Button();
            this.checkBoxAddToVideoFile = new System.Windows.Forms.CheckBox();
            this.labelVoice = new System.Windows.Forms.Label();
            this.contextMenuStripVoices = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.refreshVoicesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listViewActors = new System.Windows.Forms.ListView();
            this.columnHeaderActor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVoice = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripActors = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.labelActors = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.nikseUpDownStability = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.nikseUpDownSimilarity = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelSimilarity = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.labelStability = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.nikseComboBoxLanguage = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.nikseComboBoxRegion = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.nikseTextBoxApiKey = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.TextBoxTest = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.nikseComboBoxVoice = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.nikseComboBoxEngine = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.groupBoxSettings.SuspendLayout();
            this.contextMenuStripVoices.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(720, 486);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 100;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelProgress
            // 
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgress.AutoSize = true;
            this.labelProgress.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelProgress.Location = new System.Drawing.Point(12, 463);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(70, 13);
            this.labelProgress.TabIndex = 9;
            this.labelProgress.Text = "labelProgress";
            // 
            // buttonGenerateTTS
            // 
            this.buttonGenerateTTS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGenerateTTS.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenerateTTS.Location = new System.Drawing.Point(648, 453);
            this.buttonGenerateTTS.Name = "buttonGenerateTTS";
            this.buttonGenerateTTS.Size = new System.Drawing.Size(228, 23);
            this.buttonGenerateTTS.TabIndex = 90;
            this.buttonGenerateTTS.Text = "Generate speech from text";
            this.buttonGenerateTTS.UseVisualStyleBackColor = true;
            this.buttonGenerateTTS.Click += new System.EventHandler(this.ButtonGenerateTtsClick);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 486);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(699, 10);
            this.progressBar1.TabIndex = 12;
            // 
            // labelEngine
            // 
            this.labelEngine.AutoSize = true;
            this.labelEngine.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelEngine.Location = new System.Drawing.Point(14, 21);
            this.labelEngine.Name = "labelEngine";
            this.labelEngine.Size = new System.Drawing.Size(40, 13);
            this.labelEngine.TabIndex = 14;
            this.labelEngine.Text = "Engine";
            // 
            // groupBoxSettings
            // 
            this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxSettings.Controls.Add(this.nikseUpDownStability);
            this.groupBoxSettings.Controls.Add(this.nikseUpDownSimilarity);
            this.groupBoxSettings.Controls.Add(this.labelSimilarity);
            this.groupBoxSettings.Controls.Add(this.labelStability);
            this.groupBoxSettings.Controls.Add(this.labelLanguage);
            this.groupBoxSettings.Controls.Add(this.nikseComboBoxLanguage);
            this.groupBoxSettings.Controls.Add(this.linkLabelCustomAudio);
            this.groupBoxSettings.Controls.Add(this.checkBoxAudioEncoding);
            this.groupBoxSettings.Controls.Add(this.labelRegion);
            this.groupBoxSettings.Controls.Add(this.nikseComboBoxRegion);
            this.groupBoxSettings.Controls.Add(this.labelVoiceCount);
            this.groupBoxSettings.Controls.Add(this.checkBoxShowPreview);
            this.groupBoxSettings.Controls.Add(this.labelApiKey);
            this.groupBoxSettings.Controls.Add(this.nikseTextBoxApiKey);
            this.groupBoxSettings.Controls.Add(this.TextBoxTest);
            this.groupBoxSettings.Controls.Add(this.buttonTestVoice);
            this.groupBoxSettings.Controls.Add(this.checkBoxAddToVideoFile);
            this.groupBoxSettings.Controls.Add(this.labelVoice);
            this.groupBoxSettings.Controls.Add(this.nikseComboBoxVoice);
            this.groupBoxSettings.Controls.Add(this.labelEngine);
            this.groupBoxSettings.Controls.Add(this.nikseComboBoxEngine);
            this.groupBoxSettings.Location = new System.Drawing.Point(15, 12);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(391, 435);
            this.groupBoxSettings.TabIndex = 1;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "Settings";
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelLanguage.Location = new System.Drawing.Point(14, 274);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(55, 13);
            this.labelLanguage.TabIndex = 36;
            this.labelLanguage.Text = "Language";
            // 
            // linkLabelCustomAudio
            // 
            this.linkLabelCustomAudio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelCustomAudio.AutoSize = true;
            this.linkLabelCustomAudio.Location = new System.Drawing.Point(168, 400);
            this.linkLabelCustomAudio.Name = "linkLabelCustomAudio";
            this.linkLabelCustomAudio.Size = new System.Drawing.Size(45, 13);
            this.linkLabelCustomAudio.TabIndex = 43;
            this.linkLabelCustomAudio.TabStop = true;
            this.linkLabelCustomAudio.Text = "Settings";
            this.linkLabelCustomAudio.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelCustomAudio_LinkClicked);
            // 
            // checkBoxAudioEncoding
            // 
            this.checkBoxAudioEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxAudioEncoding.AutoSize = true;
            this.checkBoxAudioEncoding.Location = new System.Drawing.Point(32, 401);
            this.checkBoxAudioEncoding.Name = "checkBoxAudioEncoding";
            this.checkBoxAudioEncoding.Size = new System.Drawing.Size(137, 17);
            this.checkBoxAudioEncoding.TabIndex = 42;
            this.checkBoxAudioEncoding.Text = "Custom audio encoding";
            this.checkBoxAudioEncoding.UseVisualStyleBackColor = true;
            // 
            // labelRegion
            // 
            this.labelRegion.AutoSize = true;
            this.labelRegion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelRegion.Location = new System.Drawing.Point(14, 245);
            this.labelRegion.Name = "labelRegion";
            this.labelRegion.Size = new System.Drawing.Size(41, 13);
            this.labelRegion.TabIndex = 32;
            this.labelRegion.Text = "Region";
            // 
            // labelVoiceCount
            // 
            this.labelVoiceCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelVoiceCount.Location = new System.Drawing.Point(172, 83);
            this.labelVoiceCount.Name = "labelVoiceCount";
            this.labelVoiceCount.Size = new System.Drawing.Size(196, 23);
            this.labelVoiceCount.TabIndex = 29;
            this.labelVoiceCount.Text = "255";
            this.labelVoiceCount.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // checkBoxShowPreview
            // 
            this.checkBoxShowPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxShowPreview.AutoSize = true;
            this.checkBoxShowPreview.Checked = true;
            this.checkBoxShowPreview.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowPreview.Location = new System.Drawing.Point(17, 352);
            this.checkBoxShowPreview.Name = "checkBoxShowPreview";
            this.checkBoxShowPreview.Size = new System.Drawing.Size(115, 17);
            this.checkBoxShowPreview.TabIndex = 40;
            this.checkBoxShowPreview.Text = "Review audio clips";
            this.checkBoxShowPreview.UseVisualStyleBackColor = true;
            // 
            // labelApiKey
            // 
            this.labelApiKey.AutoSize = true;
            this.labelApiKey.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelApiKey.Location = new System.Drawing.Point(14, 216);
            this.labelApiKey.Name = "labelApiKey";
            this.labelApiKey.Size = new System.Drawing.Size(44, 13);
            this.labelApiKey.TabIndex = 28;
            this.labelApiKey.Text = "API key";
            // 
            // buttonTestVoice
            // 
            this.buttonTestVoice.Location = new System.Drawing.Point(17, 141);
            this.buttonTestVoice.Name = "buttonTestVoice";
            this.buttonTestVoice.Size = new System.Drawing.Size(150, 23);
            this.buttonTestVoice.TabIndex = 15;
            this.buttonTestVoice.Text = "Test voice";
            this.buttonTestVoice.UseVisualStyleBackColor = true;
            this.buttonTestVoice.Click += new System.EventHandler(this.buttonTestVoice_Click);
            // 
            // checkBoxAddToVideoFile
            // 
            this.checkBoxAddToVideoFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxAddToVideoFile.AutoSize = true;
            this.checkBoxAddToVideoFile.Checked = true;
            this.checkBoxAddToVideoFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddToVideoFile.Location = new System.Drawing.Point(17, 376);
            this.checkBoxAddToVideoFile.Name = "checkBoxAddToVideoFile";
            this.checkBoxAddToVideoFile.Size = new System.Drawing.Size(176, 17);
            this.checkBoxAddToVideoFile.TabIndex = 41;
            this.checkBoxAddToVideoFile.Text = "Add audio to video file (new file)";
            this.checkBoxAddToVideoFile.UseVisualStyleBackColor = true;
            // 
            // labelVoice
            // 
            this.labelVoice.AutoSize = true;
            this.labelVoice.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelVoice.Location = new System.Drawing.Point(14, 93);
            this.labelVoice.Name = "labelVoice";
            this.labelVoice.Size = new System.Drawing.Size(34, 13);
            this.labelVoice.TabIndex = 16;
            this.labelVoice.Text = "Voice";
            // 
            // contextMenuStripVoices
            // 
            this.contextMenuStripVoices.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshVoicesToolStripMenuItem});
            this.contextMenuStripVoices.Name = "contextMenuStripVoices";
            this.contextMenuStripVoices.Size = new System.Drawing.Size(150, 26);
            this.contextMenuStripVoices.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripVoices_Opening);
            // 
            // refreshVoicesToolStripMenuItem
            // 
            this.refreshVoicesToolStripMenuItem.Name = "refreshVoicesToolStripMenuItem";
            this.refreshVoicesToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.refreshVoicesToolStripMenuItem.Text = "Refresh voices";
            this.refreshVoicesToolStripMenuItem.Click += new System.EventHandler(this.refreshVoicesToolStripMenuItem_Click);
            // 
            // listViewActors
            // 
            this.listViewActors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewActors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderActor,
            this.columnHeaderVoice});
            this.listViewActors.ContextMenuStrip = this.contextMenuStripActors;
            this.listViewActors.FullRowSelect = true;
            this.listViewActors.GridLines = true;
            this.listViewActors.HideSelection = false;
            this.listViewActors.Location = new System.Drawing.Point(412, 42);
            this.listViewActors.Name = "listViewActors";
            this.listViewActors.Size = new System.Drawing.Size(464, 405);
            this.listViewActors.TabIndex = 40;
            this.listViewActors.UseCompatibleStateImageBehavior = false;
            this.listViewActors.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderActor
            // 
            this.columnHeaderActor.Text = "Actor";
            this.columnHeaderActor.Width = 200;
            // 
            // columnHeaderVoice
            // 
            this.columnHeaderVoice.Text = "Voice";
            this.columnHeaderVoice.Width = 200;
            // 
            // contextMenuStripActors
            // 
            this.contextMenuStripActors.Name = "contextMenuStripActors";
            this.contextMenuStripActors.Size = new System.Drawing.Size(61, 4);
            // 
            // labelActors
            // 
            this.labelActors.AutoSize = true;
            this.labelActors.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelActors.Location = new System.Drawing.Point(412, 19);
            this.labelActors.Name = "labelActors";
            this.labelActors.Size = new System.Drawing.Size(170, 13);
            this.labelActors.TabIndex = 19;
            this.labelActors.Text = "Right-click to assign actor to voice";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(801, 486);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 101;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // nikseUpDownStability
            // 
            this.nikseUpDownStability.BackColor = System.Drawing.SystemColors.Window;
            this.nikseUpDownStability.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseUpDownStability.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseUpDownStability.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseUpDownStability.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseUpDownStability.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseUpDownStability.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseUpDownStability.DecimalPlaces = 0;
            this.nikseUpDownStability.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nikseUpDownStability.Location = new System.Drawing.Point(102, 298);
            this.nikseUpDownStability.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nikseUpDownStability.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nikseUpDownStability.Name = "nikseUpDownStability";
            this.nikseUpDownStability.Size = new System.Drawing.Size(75, 23);
            this.nikseUpDownStability.TabIndex = 34;
            this.nikseUpDownStability.TabStop = false;
            this.nikseUpDownStability.Text = "nikseUpDownStability";
            this.nikseUpDownStability.ThousandsSeparator = false;
            this.nikseUpDownStability.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // nikseUpDownSimilarity
            // 
            this.nikseUpDownSimilarity.BackColor = System.Drawing.SystemColors.Window;
            this.nikseUpDownSimilarity.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseUpDownSimilarity.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseUpDownSimilarity.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseUpDownSimilarity.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseUpDownSimilarity.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseUpDownSimilarity.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseUpDownSimilarity.DecimalPlaces = 0;
            this.nikseUpDownSimilarity.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nikseUpDownSimilarity.Location = new System.Drawing.Point(251, 298);
            this.nikseUpDownSimilarity.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nikseUpDownSimilarity.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nikseUpDownSimilarity.Name = "nikseUpDownSimilarity";
            this.nikseUpDownSimilarity.Size = new System.Drawing.Size(75, 23);
            this.nikseUpDownSimilarity.TabIndex = 35;
            this.nikseUpDownSimilarity.TabStop = false;
            this.nikseUpDownSimilarity.Text = "nikseUpDownSimilarity";
            this.nikseUpDownSimilarity.ThousandsSeparator = false;
            this.nikseUpDownSimilarity.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // labelSimilarity
            // 
            this.labelSimilarity.AutoSize = true;
            this.labelSimilarity.Location = new System.Drawing.Point(198, 302);
            this.labelSimilarity.Name = "labelSimilarity";
            this.labelSimilarity.Size = new System.Drawing.Size(47, 13);
            this.labelSimilarity.TabIndex = 99;
            this.labelSimilarity.Text = "Similarity";
            // 
            // labelStability
            // 
            this.labelStability.AutoSize = true;
            this.labelStability.Location = new System.Drawing.Point(15, 302);
            this.labelStability.Name = "labelStability";
            this.labelStability.Size = new System.Drawing.Size(43, 13);
            this.labelStability.TabIndex = 97;
            this.labelStability.Text = "Stability";
            // 
            // nikseComboBoxLanguage
            // 
            this.nikseComboBoxLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nikseComboBoxLanguage.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxLanguage.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxLanguage.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxLanguage.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxLanguage.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxLanguage.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxLanguage.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxLanguage.DropDownHeight = 400;
            this.nikseComboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nikseComboBoxLanguage.DropDownWidth = 0;
            this.nikseComboBoxLanguage.FormattingEnabled = false;
            this.nikseComboBoxLanguage.Location = new System.Drawing.Point(102, 269);
            this.nikseComboBoxLanguage.MaxLength = 32767;
            this.nikseComboBoxLanguage.Name = "nikseComboBoxLanguage";
            this.nikseComboBoxLanguage.SelectedIndex = -1;
            this.nikseComboBoxLanguage.SelectedItem = null;
            this.nikseComboBoxLanguage.SelectedText = "";
            this.nikseComboBoxLanguage.Size = new System.Drawing.Size(266, 23);
            this.nikseComboBoxLanguage.TabIndex = 35;
            this.nikseComboBoxLanguage.UsePopupWindow = false;
            this.nikseComboBoxLanguage.SelectedIndexChanged += new System.EventHandler(this.nikseComboBoxLanguage_SelectedIndexChanged);
            // 
            // nikseComboBoxRegion
            // 
            this.nikseComboBoxRegion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nikseComboBoxRegion.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxRegion.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxRegion.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxRegion.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxRegion.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxRegion.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxRegion.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxRegion.DropDownHeight = 400;
            this.nikseComboBoxRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nikseComboBoxRegion.DropDownWidth = 0;
            this.nikseComboBoxRegion.FormattingEnabled = false;
            this.nikseComboBoxRegion.Location = new System.Drawing.Point(102, 240);
            this.nikseComboBoxRegion.MaxLength = 32767;
            this.nikseComboBoxRegion.Name = "nikseComboBoxRegion";
            this.nikseComboBoxRegion.SelectedIndex = -1;
            this.nikseComboBoxRegion.SelectedItem = null;
            this.nikseComboBoxRegion.SelectedText = "";
            this.nikseComboBoxRegion.Size = new System.Drawing.Size(266, 23);
            this.nikseComboBoxRegion.TabIndex = 31;
            this.nikseComboBoxRegion.UsePopupWindow = false;
            this.nikseComboBoxRegion.SelectedIndexChanged += new System.EventHandler(this.nikseComboBoxRegion_SelectedIndexChanged);
            // 
            // nikseTextBoxApiKey
            // 
            this.nikseTextBoxApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nikseTextBoxApiKey.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxApiKey.Location = new System.Drawing.Point(102, 213);
            this.nikseTextBoxApiKey.Name = "nikseTextBoxApiKey";
            this.nikseTextBoxApiKey.Size = new System.Drawing.Size(266, 20);
            this.nikseTextBoxApiKey.TabIndex = 27;
            // 
            // TextBoxTest
            // 
            this.TextBoxTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBoxTest.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.TextBoxTest.Location = new System.Drawing.Point(17, 172);
            this.TextBoxTest.Name = "TextBoxTest";
            this.TextBoxTest.Size = new System.Drawing.Size(351, 20);
            this.TextBoxTest.TabIndex = 20;
            this.TextBoxTest.Text = "Hello, how are you?";
            this.TextBoxTest.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxTest_KeyDown);
            // 
            // nikseComboBoxVoice
            // 
            this.nikseComboBoxVoice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nikseComboBoxVoice.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxVoice.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxVoice.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxVoice.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxVoice.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxVoice.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxVoice.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxVoice.ContextMenuStrip = this.contextMenuStripVoices;
            this.nikseComboBoxVoice.DropDownHeight = 400;
            this.nikseComboBoxVoice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nikseComboBoxVoice.DropDownWidth = 0;
            this.nikseComboBoxVoice.FormattingEnabled = false;
            this.nikseComboBoxVoice.Location = new System.Drawing.Point(17, 110);
            this.nikseComboBoxVoice.MaxLength = 32767;
            this.nikseComboBoxVoice.Name = "nikseComboBoxVoice";
            this.nikseComboBoxVoice.SelectedIndex = -1;
            this.nikseComboBoxVoice.SelectedItem = null;
            this.nikseComboBoxVoice.SelectedText = "";
            this.nikseComboBoxVoice.Size = new System.Drawing.Size(351, 23);
            this.nikseComboBoxVoice.TabIndex = 10;
            this.nikseComboBoxVoice.UsePopupWindow = false;
            // 
            // nikseComboBoxEngine
            // 
            this.nikseComboBoxEngine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nikseComboBoxEngine.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxEngine.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxEngine.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxEngine.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxEngine.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxEngine.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxEngine.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxEngine.DropDownHeight = 400;
            this.nikseComboBoxEngine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.nikseComboBoxEngine.DropDownWidth = 391;
            this.nikseComboBoxEngine.FormattingEnabled = false;
            this.nikseComboBoxEngine.Location = new System.Drawing.Point(17, 40);
            this.nikseComboBoxEngine.MaxLength = 32767;
            this.nikseComboBoxEngine.Name = "nikseComboBoxEngine";
            this.nikseComboBoxEngine.SelectedIndex = -1;
            this.nikseComboBoxEngine.SelectedItem = null;
            this.nikseComboBoxEngine.SelectedText = "";
            this.nikseComboBoxEngine.Size = new System.Drawing.Size(351, 23);
            this.nikseComboBoxEngine.TabIndex = 5;
            this.nikseComboBoxEngine.TabStop = false;
            this.nikseComboBoxEngine.Text = "nikseComboBoxEngine";
            this.nikseComboBoxEngine.UsePopupWindow = false;
            // 
            // TextToSpeech
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(888, 521);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelActors);
            this.Controls.Add(this.listViewActors);
            this.Controls.Add(this.groupBoxSettings);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonGenerateTTS);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(860, 520);
            this.Name = "TextToSpeech";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Text to speech";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TextToSpeech_FormClosing);
            this.Load += new System.EventHandler(this.TextToSpeech_Load);
            this.Shown += new System.EventHandler(this.TextToSpeech_Shown);
            this.ResizeEnd += new System.EventHandler(this.TextToSpeech_ResizeEnd);
            this.SizeChanged += new System.EventHandler(this.TextToSpeech_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextToSpeech_KeyDown);
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            this.contextMenuStripVoices.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.Button buttonGenerateTTS;
        private System.Windows.Forms.ProgressBar progressBar1;
        private Controls.NikseComboBox nikseComboBoxEngine;
        private System.Windows.Forms.Label labelEngine;
        private System.Windows.Forms.GroupBox groupBoxSettings;
        private System.Windows.Forms.Label labelVoice;
        private Controls.NikseComboBox nikseComboBoxVoice;
        private System.Windows.Forms.CheckBox checkBoxAddToVideoFile;
        private System.Windows.Forms.ListView listViewActors;
        private System.Windows.Forms.ColumnHeader columnHeaderActor;
        private System.Windows.Forms.ColumnHeader columnHeaderVoice;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripActors;
        private System.Windows.Forms.Button buttonTestVoice;
        private Controls.NikseTextBox TextBoxTest;
        private System.Windows.Forms.Label labelActors;
        private System.Windows.Forms.Label labelApiKey;
        private Controls.NikseTextBox nikseTextBoxApiKey;
        private System.Windows.Forms.CheckBox checkBoxShowPreview;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelVoiceCount;
        private System.Windows.Forms.Label labelRegion;
        private Controls.NikseComboBox nikseComboBoxRegion;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripVoices;
        private System.Windows.Forms.ToolStripMenuItem refreshVoicesToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxAudioEncoding;
        private System.Windows.Forms.LinkLabel linkLabelCustomAudio;
        private System.Windows.Forms.Label labelLanguage;
        private Controls.NikseComboBox nikseComboBoxLanguage;
        private Controls.NikseUpDown nikseUpDownStability;
        private Controls.NikseUpDown nikseUpDownSimilarity;
        private Controls.NikseLabel labelSimilarity;
        private Controls.NikseLabel labelStability;
    }
}