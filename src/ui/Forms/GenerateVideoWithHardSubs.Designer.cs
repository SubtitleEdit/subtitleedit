
namespace Nikse.SubtitleEdit.Forms
{
    partial class GenerateVideoWithHardSubs
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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.numericUpDownFontSize = new System.Windows.Forms.NumericUpDown();
            this.labelFontSize = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelProgress = new System.Windows.Forms.Label();
            this.groupBoxSettings = new System.Windows.Forms.GroupBox();
            this.checkBoxBox = new System.Windows.Forms.CheckBox();
            this.labelPreviewPleaseWait = new System.Windows.Forms.Label();
            this.checkBoxAlignRight = new System.Windows.Forms.CheckBox();
            this.checkBoxRightToLeft = new System.Windows.Forms.CheckBox();
            this.comboBoxSubtitleFont = new System.Windows.Forms.ComboBox();
            this.labelSubtitleFont = new System.Windows.Forms.Label();
            this.buttonPreview = new System.Windows.Forms.Button();
            this.groupBoxVideo = new System.Windows.Forms.GroupBox();
            this.labelResolution = new System.Windows.Forms.Label();
            this.numericUpDownWidth = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
            this.labelX = new System.Windows.Forms.Label();
            this.labelPreset = new System.Windows.Forms.Label();
            this.comboBoxTune = new System.Windows.Forms.ComboBox();
            this.comboBoxPreset = new System.Windows.Forms.ComboBox();
            this.labelTune = new System.Windows.Forms.Label();
            this.labelCRF = new System.Windows.Forms.Label();
            this.comboBoxVideoEncoding = new System.Windows.Forms.ComboBox();
            this.comboBoxCrf = new System.Windows.Forms.ComboBox();
            this.labelVideoEncoding = new System.Windows.Forms.Label();
            this.groupBoxAudio = new System.Windows.Forms.GroupBox();
            this.labelAudioEnc = new System.Windows.Forms.Label();
            this.comboBoxAudioBitRate = new System.Windows.Forms.ComboBox();
            this.comboBoxAudioEnc = new System.Windows.Forms.ComboBox();
            this.labelAudioBitRate = new System.Windows.Forms.Label();
            this.checkBoxMakeStereo = new System.Windows.Forms.CheckBox();
            this.labelAudioSampleRate = new System.Windows.Forms.Label();
            this.comboBoxAudioSampleRate = new System.Windows.Forms.ComboBox();
            this.numericUpDownTargetFileSize = new System.Windows.Forms.NumericUpDown();
            this.labelFileSize = new System.Windows.Forms.Label();
            this.checkBoxTargetFileSize = new System.Windows.Forms.CheckBox();
            this.labelFileName = new System.Windows.Forms.Label();
            this.linkLabelHelp = new System.Windows.Forms.LinkLabel();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelPass = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).BeginInit();
            this.groupBoxSettings.SuspendLayout();
            this.groupBoxVideo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
            this.groupBoxAudio.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTargetFileSize)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 570);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(628, 11);
            this.progressBar1.TabIndex = 22;
            this.progressBar1.Visible = false;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(646, 570);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(121, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "Generate";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(773, 570);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(12, 554);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPleaseWait.TabIndex = 25;
            this.labelPleaseWait.Text = "Please wait...";
            // 
            // numericUpDownFontSize
            // 
            this.numericUpDownFontSize.Location = new System.Drawing.Point(120, 33);
            this.numericUpDownFontSize.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownFontSize.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericUpDownFontSize.Name = "numericUpDownFontSize";
            this.numericUpDownFontSize.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownFontSize.TabIndex = 1;
            this.numericUpDownFontSize.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // labelFontSize
            // 
            this.labelFontSize.AutoSize = true;
            this.labelFontSize.Location = new System.Drawing.Point(19, 35);
            this.labelFontSize.Name = "labelFontSize";
            this.labelFontSize.Size = new System.Drawing.Size(49, 13);
            this.labelFontSize.TabIndex = 0;
            this.labelFontSize.Text = "Font size";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // labelProgress
            // 
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(12, 584);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(88, 13);
            this.labelProgress.TabIndex = 29;
            this.labelProgress.Text = "Remaining time...";
            // 
            // groupBoxSettings
            // 
            this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSettings.Controls.Add(this.checkBoxBox);
            this.groupBoxSettings.Controls.Add(this.labelPreviewPleaseWait);
            this.groupBoxSettings.Controls.Add(this.checkBoxAlignRight);
            this.groupBoxSettings.Controls.Add(this.checkBoxRightToLeft);
            this.groupBoxSettings.Controls.Add(this.comboBoxSubtitleFont);
            this.groupBoxSettings.Controls.Add(this.labelSubtitleFont);
            this.groupBoxSettings.Controls.Add(this.buttonPreview);
            this.groupBoxSettings.Controls.Add(this.groupBoxVideo);
            this.groupBoxSettings.Controls.Add(this.groupBoxAudio);
            this.groupBoxSettings.Controls.Add(this.numericUpDownTargetFileSize);
            this.groupBoxSettings.Controls.Add(this.labelFileSize);
            this.groupBoxSettings.Controls.Add(this.checkBoxTargetFileSize);
            this.groupBoxSettings.Controls.Add(this.labelFileName);
            this.groupBoxSettings.Controls.Add(this.numericUpDownFontSize);
            this.groupBoxSettings.Controls.Add(this.labelFontSize);
            this.groupBoxSettings.Location = new System.Drawing.Point(12, 13);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(836, 502);
            this.groupBoxSettings.TabIndex = 0;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "Settings";
            // 
            // checkBoxBox
            // 
            this.checkBoxBox.AutoSize = true;
            this.checkBoxBox.Location = new System.Drawing.Point(120, 136);
            this.checkBoxBox.Name = "checkBoxBox";
            this.checkBoxBox.Size = new System.Drawing.Size(44, 17);
            this.checkBoxBox.TabIndex = 6;
            this.checkBoxBox.Text = "Box";
            this.checkBoxBox.UseVisualStyleBackColor = true;
            // 
            // labelPreviewPleaseWait
            // 
            this.labelPreviewPleaseWait.AutoSize = true;
            this.labelPreviewPleaseWait.Location = new System.Drawing.Point(717, 45);
            this.labelPreviewPleaseWait.Name = "labelPreviewPleaseWait";
            this.labelPreviewPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPreviewPleaseWait.TabIndex = 7;
            this.labelPreviewPleaseWait.Text = "Please wait...";
            // 
            // checkBoxAlignRight
            // 
            this.checkBoxAlignRight.AutoSize = true;
            this.checkBoxAlignRight.Location = new System.Drawing.Point(120, 113);
            this.checkBoxAlignRight.Name = "checkBoxAlignRight";
            this.checkBoxAlignRight.Size = new System.Drawing.Size(72, 17);
            this.checkBoxAlignRight.TabIndex = 5;
            this.checkBoxAlignRight.Text = "Align right";
            this.checkBoxAlignRight.UseVisualStyleBackColor = true;
            // 
            // checkBoxRightToLeft
            // 
            this.checkBoxRightToLeft.AutoSize = true;
            this.checkBoxRightToLeft.Location = new System.Drawing.Point(120, 90);
            this.checkBoxRightToLeft.Name = "checkBoxRightToLeft";
            this.checkBoxRightToLeft.Size = new System.Drawing.Size(80, 17);
            this.checkBoxRightToLeft.TabIndex = 4;
            this.checkBoxRightToLeft.Text = "Right to left";
            this.checkBoxRightToLeft.UseVisualStyleBackColor = true;
            // 
            // comboBoxSubtitleFont
            // 
            this.comboBoxSubtitleFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFont.DropDownWidth = 250;
            this.comboBoxSubtitleFont.FormattingEnabled = true;
            this.comboBoxSubtitleFont.Location = new System.Drawing.Point(120, 63);
            this.comboBoxSubtitleFont.Name = "comboBoxSubtitleFont";
            this.comboBoxSubtitleFont.Size = new System.Drawing.Size(250, 21);
            this.comboBoxSubtitleFont.TabIndex = 3;
            // 
            // labelSubtitleFont
            // 
            this.labelSubtitleFont.AutoSize = true;
            this.labelSubtitleFont.Location = new System.Drawing.Point(19, 66);
            this.labelSubtitleFont.Name = "labelSubtitleFont";
            this.labelSubtitleFont.Size = new System.Drawing.Size(63, 13);
            this.labelSubtitleFont.TabIndex = 2;
            this.labelSubtitleFont.Text = "Subtitle font";
            // 
            // buttonPreview
            // 
            this.buttonPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPreview.Location = new System.Drawing.Point(720, 19);
            this.buttonPreview.Name = "buttonPreview";
            this.buttonPreview.Size = new System.Drawing.Size(101, 23);
            this.buttonPreview.TabIndex = 6;
            this.buttonPreview.Text = "Preview";
            this.buttonPreview.UseVisualStyleBackColor = true;
            this.buttonPreview.Click += new System.EventHandler(this.buttonPreview_Click);
            // 
            // groupBoxVideo
            // 
            this.groupBoxVideo.Controls.Add(this.labelResolution);
            this.groupBoxVideo.Controls.Add(this.numericUpDownWidth);
            this.groupBoxVideo.Controls.Add(this.numericUpDownHeight);
            this.groupBoxVideo.Controls.Add(this.labelX);
            this.groupBoxVideo.Controls.Add(this.labelPreset);
            this.groupBoxVideo.Controls.Add(this.comboBoxTune);
            this.groupBoxVideo.Controls.Add(this.comboBoxPreset);
            this.groupBoxVideo.Controls.Add(this.labelTune);
            this.groupBoxVideo.Controls.Add(this.labelCRF);
            this.groupBoxVideo.Controls.Add(this.comboBoxVideoEncoding);
            this.groupBoxVideo.Controls.Add(this.comboBoxCrf);
            this.groupBoxVideo.Controls.Add(this.labelVideoEncoding);
            this.groupBoxVideo.Location = new System.Drawing.Point(6, 165);
            this.groupBoxVideo.Name = "groupBoxVideo";
            this.groupBoxVideo.Size = new System.Drawing.Size(364, 197);
            this.groupBoxVideo.TabIndex = 8;
            this.groupBoxVideo.TabStop = false;
            this.groupBoxVideo.Text = "Video";
            // 
            // labelResolution
            // 
            this.labelResolution.AutoSize = true;
            this.labelResolution.Location = new System.Drawing.Point(16, 28);
            this.labelResolution.Name = "labelResolution";
            this.labelResolution.Size = new System.Drawing.Size(57, 13);
            this.labelResolution.TabIndex = 0;
            this.labelResolution.Text = "Resolution";
            // 
            // numericUpDownWidth
            // 
            this.numericUpDownWidth.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownWidth.Location = new System.Drawing.Point(102, 26);
            this.numericUpDownWidth.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownWidth.Name = "numericUpDownWidth";
            this.numericUpDownWidth.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownWidth.TabIndex = 1;
            this.numericUpDownWidth.Value = new decimal(new int[] {
            1280,
            0,
            0,
            0});
            this.numericUpDownWidth.ValueChanged += new System.EventHandler(this.numericUpDownWidth_ValueChanged);
            // 
            // numericUpDownHeight
            // 
            this.numericUpDownHeight.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownHeight.Location = new System.Drawing.Point(190, 26);
            this.numericUpDownHeight.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownHeight.Name = "numericUpDownHeight";
            this.numericUpDownHeight.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownHeight.TabIndex = 2;
            this.numericUpDownHeight.Value = new decimal(new int[] {
            720,
            0,
            0,
            0});
            this.numericUpDownHeight.ValueChanged += new System.EventHandler(this.numericUpDownHeight_ValueChanged);
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(172, 28);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(12, 13);
            this.labelX.TabIndex = 31;
            this.labelX.Text = "x";
            // 
            // labelPreset
            // 
            this.labelPreset.AutoSize = true;
            this.labelPreset.Location = new System.Drawing.Point(16, 87);
            this.labelPreset.Name = "labelPreset";
            this.labelPreset.Size = new System.Drawing.Size(37, 13);
            this.labelPreset.TabIndex = 5;
            this.labelPreset.Text = "Preset";
            // 
            // comboBoxTune
            // 
            this.comboBoxTune.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTune.FormattingEnabled = true;
            this.comboBoxTune.Items.AddRange(new object[] {
            "",
            "film",
            "animation",
            "grain"});
            this.comboBoxTune.Location = new System.Drawing.Point(101, 139);
            this.comboBoxTune.Name = "comboBoxTune";
            this.comboBoxTune.Size = new System.Drawing.Size(121, 21);
            this.comboBoxTune.TabIndex = 10;
            // 
            // comboBoxPreset
            // 
            this.comboBoxPreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPreset.FormattingEnabled = true;
            this.comboBoxPreset.Items.AddRange(new object[] {
            "ultrafast",
            "superfast",
            "veryfast",
            "faster",
            "fast",
            "medium",
            "slow",
            "slower",
            "veryslow "});
            this.comboBoxPreset.Location = new System.Drawing.Point(101, 85);
            this.comboBoxPreset.Name = "comboBoxPreset";
            this.comboBoxPreset.Size = new System.Drawing.Size(121, 21);
            this.comboBoxPreset.TabIndex = 6;
            // 
            // labelTune
            // 
            this.labelTune.AutoSize = true;
            this.labelTune.Location = new System.Drawing.Point(16, 141);
            this.labelTune.Name = "labelTune";
            this.labelTune.Size = new System.Drawing.Size(32, 13);
            this.labelTune.TabIndex = 9;
            this.labelTune.Text = "Tune";
            // 
            // labelCRF
            // 
            this.labelCRF.AutoSize = true;
            this.labelCRF.Location = new System.Drawing.Point(16, 114);
            this.labelCRF.Name = "labelCRF";
            this.labelCRF.Size = new System.Drawing.Size(28, 13);
            this.labelCRF.TabIndex = 7;
            this.labelCRF.Text = "CRF";
            // 
            // comboBoxVideoEncoding
            // 
            this.comboBoxVideoEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVideoEncoding.FormattingEnabled = true;
            this.comboBoxVideoEncoding.Items.AddRange(new object[] {
            "libx264",
            "libx264rgb",
            "libx265",
            "libvpx-vp9"});
            this.comboBoxVideoEncoding.Location = new System.Drawing.Point(101, 58);
            this.comboBoxVideoEncoding.Name = "comboBoxVideoEncoding";
            this.comboBoxVideoEncoding.Size = new System.Drawing.Size(121, 21);
            this.comboBoxVideoEncoding.TabIndex = 4;
            this.comboBoxVideoEncoding.SelectedIndexChanged += new System.EventHandler(this.comboBoxVideoEncoding_SelectedIndexChanged);
            // 
            // comboBoxCrf
            // 
            this.comboBoxCrf.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCrf.FormattingEnabled = true;
            this.comboBoxCrf.Items.AddRange(new object[] {
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28"});
            this.comboBoxCrf.Location = new System.Drawing.Point(101, 112);
            this.comboBoxCrf.Name = "comboBoxCrf";
            this.comboBoxCrf.Size = new System.Drawing.Size(121, 21);
            this.comboBoxCrf.TabIndex = 8;
            // 
            // labelVideoEncoding
            // 
            this.labelVideoEncoding.AutoSize = true;
            this.labelVideoEncoding.Location = new System.Drawing.Point(16, 60);
            this.labelVideoEncoding.Name = "labelVideoEncoding";
            this.labelVideoEncoding.Size = new System.Drawing.Size(55, 13);
            this.labelVideoEncoding.TabIndex = 3;
            this.labelVideoEncoding.Text = "Video enc";
            // 
            // groupBoxAudio
            // 
            this.groupBoxAudio.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAudio.Controls.Add(this.labelAudioEnc);
            this.groupBoxAudio.Controls.Add(this.comboBoxAudioBitRate);
            this.groupBoxAudio.Controls.Add(this.comboBoxAudioEnc);
            this.groupBoxAudio.Controls.Add(this.labelAudioBitRate);
            this.groupBoxAudio.Controls.Add(this.checkBoxMakeStereo);
            this.groupBoxAudio.Controls.Add(this.labelAudioSampleRate);
            this.groupBoxAudio.Controls.Add(this.comboBoxAudioSampleRate);
            this.groupBoxAudio.Location = new System.Drawing.Point(376, 165);
            this.groupBoxAudio.Name = "groupBoxAudio";
            this.groupBoxAudio.Size = new System.Drawing.Size(445, 277);
            this.groupBoxAudio.TabIndex = 10;
            this.groupBoxAudio.TabStop = false;
            this.groupBoxAudio.Text = "Audio";
            // 
            // labelAudioEnc
            // 
            this.labelAudioEnc.AutoSize = true;
            this.labelAudioEnc.Location = new System.Drawing.Point(20, 31);
            this.labelAudioEnc.Name = "labelAudioEnc";
            this.labelAudioEnc.Size = new System.Drawing.Size(52, 13);
            this.labelAudioEnc.TabIndex = 0;
            this.labelAudioEnc.Text = "Encoding";
            // 
            // comboBoxAudioBitRate
            // 
            this.comboBoxAudioBitRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAudioBitRate.FormattingEnabled = true;
            this.comboBoxAudioBitRate.Items.AddRange(new object[] {
            "64k",
            "128k",
            "160k",
            "196k",
            "320k"});
            this.comboBoxAudioBitRate.Location = new System.Drawing.Point(97, 110);
            this.comboBoxAudioBitRate.Name = "comboBoxAudioBitRate";
            this.comboBoxAudioBitRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAudioBitRate.TabIndex = 5;
            // 
            // comboBoxAudioEnc
            // 
            this.comboBoxAudioEnc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAudioEnc.FormattingEnabled = true;
            this.comboBoxAudioEnc.Items.AddRange(new object[] {
            "copy",
            "aac"});
            this.comboBoxAudioEnc.Location = new System.Drawing.Point(97, 29);
            this.comboBoxAudioEnc.Name = "comboBoxAudioEnc";
            this.comboBoxAudioEnc.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAudioEnc.TabIndex = 1;
            this.comboBoxAudioEnc.SelectedIndexChanged += new System.EventHandler(this.comboBoxAudioEnc_SelectedIndexChanged);
            // 
            // labelAudioBitRate
            // 
            this.labelAudioBitRate.AutoSize = true;
            this.labelAudioBitRate.Location = new System.Drawing.Point(20, 112);
            this.labelAudioBitRate.Name = "labelAudioBitRate";
            this.labelAudioBitRate.Size = new System.Drawing.Size(40, 13);
            this.labelAudioBitRate.TabIndex = 4;
            this.labelAudioBitRate.Text = "Bit rate";
            // 
            // checkBoxMakeStereo
            // 
            this.checkBoxMakeStereo.AutoSize = true;
            this.checkBoxMakeStereo.Checked = true;
            this.checkBoxMakeStereo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMakeStereo.Location = new System.Drawing.Point(97, 58);
            this.checkBoxMakeStereo.Name = "checkBoxMakeStereo";
            this.checkBoxMakeStereo.Size = new System.Drawing.Size(57, 17);
            this.checkBoxMakeStereo.TabIndex = 2;
            this.checkBoxMakeStereo.Text = "Stereo";
            this.checkBoxMakeStereo.UseVisualStyleBackColor = true;
            // 
            // labelAudioSampleRate
            // 
            this.labelAudioSampleRate.AutoSize = true;
            this.labelAudioSampleRate.Location = new System.Drawing.Point(20, 85);
            this.labelAudioSampleRate.Name = "labelAudioSampleRate";
            this.labelAudioSampleRate.Size = new System.Drawing.Size(63, 13);
            this.labelAudioSampleRate.TabIndex = 44;
            this.labelAudioSampleRate.Text = "Sample rate";
            // 
            // comboBoxAudioSampleRate
            // 
            this.comboBoxAudioSampleRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAudioSampleRate.FormattingEnabled = true;
            this.comboBoxAudioSampleRate.Items.AddRange(new object[] {
            "44100 Hz",
            "48000 Hz",
            "88200 Hz",
            "96000 Hz",
            "192000 Hz"});
            this.comboBoxAudioSampleRate.Location = new System.Drawing.Point(97, 83);
            this.comboBoxAudioSampleRate.Name = "comboBoxAudioSampleRate";
            this.comboBoxAudioSampleRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAudioSampleRate.TabIndex = 3;
            // 
            // numericUpDownTargetFileSize
            // 
            this.numericUpDownTargetFileSize.Location = new System.Drawing.Point(120, 401);
            this.numericUpDownTargetFileSize.Maximum = new decimal(new int[] {
            25000,
            0,
            0,
            0});
            this.numericUpDownTargetFileSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTargetFileSize.Name = "numericUpDownTargetFileSize";
            this.numericUpDownTargetFileSize.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownTargetFileSize.TabIndex = 13;
            this.numericUpDownTargetFileSize.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // labelFileSize
            // 
            this.labelFileSize.AutoSize = true;
            this.labelFileSize.Location = new System.Drawing.Point(35, 403);
            this.labelFileSize.Name = "labelFileSize";
            this.labelFileSize.Size = new System.Drawing.Size(74, 13);
            this.labelFileSize.TabIndex = 12;
            this.labelFileSize.Text = "File size in MB";
            // 
            // checkBoxTargetFileSize
            // 
            this.checkBoxTargetFileSize.AutoSize = true;
            this.checkBoxTargetFileSize.Location = new System.Drawing.Point(22, 378);
            this.checkBoxTargetFileSize.Name = "checkBoxTargetFileSize";
            this.checkBoxTargetFileSize.Size = new System.Drawing.Size(192, 17);
            this.checkBoxTargetFileSize.TabIndex = 11;
            this.checkBoxTargetFileSize.Text = "Target file size (two pass encoding)";
            this.checkBoxTargetFileSize.UseVisualStyleBackColor = true;
            this.checkBoxTargetFileSize.CheckedChanged += new System.EventHandler(this.checkBoxTargetFileSize_CheckedChanged);
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(19, 449);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(52, 13);
            this.labelFileName.TabIndex = 0;
            this.labelFileName.Text = "File name";
            // 
            // linkLabelHelp
            // 
            this.linkLabelHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelHelp.AutoSize = true;
            this.linkLabelHelp.Location = new System.Drawing.Point(819, 518);
            this.linkLabelHelp.Name = "linkLabelHelp";
            this.linkLabelHelp.Size = new System.Drawing.Size(29, 13);
            this.linkLabelHelp.TabIndex = 2;
            this.linkLabelHelp.TabStop = true;
            this.linkLabelHelp.Text = "Help";
            this.linkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelp_LinkClicked);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Location = new System.Drawing.Point(12, 13);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.Size = new System.Drawing.Size(188, 26);
            this.textBoxLog.TabIndex = 31;
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(12, 518);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(25, 13);
            this.labelInfo.TabIndex = 1;
            this.labelInfo.Text = "Info";
            // 
            // labelPass
            // 
            this.labelPass.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPass.AutoSize = true;
            this.labelPass.Location = new System.Drawing.Point(589, 554);
            this.labelPass.Name = "labelPass";
            this.labelPass.Size = new System.Drawing.Size(51, 13);
            this.labelPass.TabIndex = 47;
            this.labelPass.Text = "Pass one";
            // 
            // GenerateVideoWithHardSubs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 605);
            this.Controls.Add(this.labelPass);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.groupBoxSettings);
            this.Controls.Add(this.linkLabelHelp);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.textBoxLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GenerateVideoWithHardSubs";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GenerateVideoWithHardSubs";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GenerateVideoWithHardSubs_FormClosing);
            this.Shown += new System.EventHandler(this.GenerateVideoWithHardSubs_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenerateVideoWithHardSubs_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).EndInit();
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            this.groupBoxVideo.ResumeLayout(false);
            this.groupBoxVideo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).EndInit();
            this.groupBoxAudio.ResumeLayout(false);
            this.groupBoxAudio.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTargetFileSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.NumericUpDown numericUpDownFontSize;
        private System.Windows.Forms.Label labelFontSize;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.GroupBox groupBoxSettings;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.Label labelResolution;
        private System.Windows.Forms.NumericUpDown numericUpDownHeight;
        private System.Windows.Forms.NumericUpDown numericUpDownWidth;
        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.ComboBox comboBoxPreset;
        private System.Windows.Forms.Label labelPreset;
        private System.Windows.Forms.ComboBox comboBoxCrf;
        private System.Windows.Forms.Label labelCRF;
        private System.Windows.Forms.ComboBox comboBoxVideoEncoding;
        private System.Windows.Forms.Label labelVideoEncoding;
        private System.Windows.Forms.ComboBox comboBoxAudioEnc;
        private System.Windows.Forms.Label labelAudioEnc;
        private System.Windows.Forms.ComboBox comboBoxAudioSampleRate;
        private System.Windows.Forms.Label labelAudioSampleRate;
        private System.Windows.Forms.CheckBox checkBoxMakeStereo;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.LinkLabel linkLabelHelp;
        private System.Windows.Forms.ComboBox comboBoxTune;
        private System.Windows.Forms.Label labelTune;
        private System.Windows.Forms.NumericUpDown numericUpDownTargetFileSize;
        private System.Windows.Forms.Label labelFileSize;
        private System.Windows.Forms.CheckBox checkBoxTargetFileSize;
        private System.Windows.Forms.Label labelPass;
        private System.Windows.Forms.ComboBox comboBoxAudioBitRate;
        private System.Windows.Forms.Label labelAudioBitRate;
        private System.Windows.Forms.GroupBox groupBoxAudio;
        private System.Windows.Forms.GroupBox groupBoxVideo;
        private System.Windows.Forms.Button buttonPreview;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFont;
        private System.Windows.Forms.Label labelSubtitleFont;
        private System.Windows.Forms.CheckBox checkBoxRightToLeft;
        private System.Windows.Forms.CheckBox checkBoxAlignRight;
        private System.Windows.Forms.Label labelPreviewPleaseWait;
        private System.Windows.Forms.CheckBox checkBoxBox;
    }
}