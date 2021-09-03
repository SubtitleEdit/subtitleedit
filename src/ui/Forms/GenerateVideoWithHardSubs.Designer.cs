
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
            this.comboBoxTune = new System.Windows.Forms.ComboBox();
            this.labelTune = new System.Windows.Forms.Label();
            this.linkLabelHelp = new System.Windows.Forms.LinkLabel();
            this.comboBoxAudioSampleRate = new System.Windows.Forms.ComboBox();
            this.labelAudioSampleRate = new System.Windows.Forms.Label();
            this.checkBoxMakeStereo = new System.Windows.Forms.CheckBox();
            this.comboBoxAudioEnc = new System.Windows.Forms.ComboBox();
            this.labelAudioEnc = new System.Windows.Forms.Label();
            this.comboBoxVideoEncoding = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxCrf = new System.Windows.Forms.ComboBox();
            this.labelCRF = new System.Windows.Forms.Label();
            this.comboBoxPreset = new System.Windows.Forms.ComboBox();
            this.labelPreset = new System.Windows.Forms.Label();
            this.labelFileName = new System.Windows.Forms.Label();
            this.labelX = new System.Windows.Forms.Label();
            this.labelResolution = new System.Windows.Forms.Label();
            this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownWidth = new System.Windows.Forms.NumericUpDown();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.labelInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).BeginInit();
            this.groupBoxSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 341);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(556, 11);
            this.progressBar1.TabIndex = 22;
            this.progressBar1.Visible = false;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(574, 341);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(121, 23);
            this.buttonOK.TabIndex = 23;
            this.buttonOK.Text = "Generate";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(701, 341);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 24;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(12, 325);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPleaseWait.TabIndex = 25;
            this.labelPleaseWait.Text = "Please wait...";
            // 
            // numericUpDownFontSize
            // 
            this.numericUpDownFontSize.Location = new System.Drawing.Point(88, 33);
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
            this.numericUpDownFontSize.TabIndex = 27;
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
            this.labelFontSize.TabIndex = 28;
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
            this.labelProgress.Location = new System.Drawing.Point(12, 355);
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
            this.groupBoxSettings.Controls.Add(this.comboBoxTune);
            this.groupBoxSettings.Controls.Add(this.labelTune);
            this.groupBoxSettings.Controls.Add(this.comboBoxAudioSampleRate);
            this.groupBoxSettings.Controls.Add(this.labelAudioSampleRate);
            this.groupBoxSettings.Controls.Add(this.checkBoxMakeStereo);
            this.groupBoxSettings.Controls.Add(this.comboBoxAudioEnc);
            this.groupBoxSettings.Controls.Add(this.labelAudioEnc);
            this.groupBoxSettings.Controls.Add(this.comboBoxVideoEncoding);
            this.groupBoxSettings.Controls.Add(this.label1);
            this.groupBoxSettings.Controls.Add(this.comboBoxCrf);
            this.groupBoxSettings.Controls.Add(this.labelCRF);
            this.groupBoxSettings.Controls.Add(this.comboBoxPreset);
            this.groupBoxSettings.Controls.Add(this.labelPreset);
            this.groupBoxSettings.Controls.Add(this.labelFileName);
            this.groupBoxSettings.Controls.Add(this.labelX);
            this.groupBoxSettings.Controls.Add(this.labelResolution);
            this.groupBoxSettings.Controls.Add(this.numericUpDownHeight);
            this.groupBoxSettings.Controls.Add(this.numericUpDownWidth);
            this.groupBoxSettings.Controls.Add(this.numericUpDownFontSize);
            this.groupBoxSettings.Controls.Add(this.labelFontSize);
            this.groupBoxSettings.Location = new System.Drawing.Point(12, 13);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(764, 273);
            this.groupBoxSettings.TabIndex = 30;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "Settings";
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
            this.comboBoxTune.Location = new System.Drawing.Point(88, 187);
            this.comboBoxTune.Name = "comboBoxTune";
            this.comboBoxTune.Size = new System.Drawing.Size(121, 21);
            this.comboBoxTune.TabIndex = 48;
            // 
            // labelTune
            // 
            this.labelTune.AutoSize = true;
            this.labelTune.Location = new System.Drawing.Point(19, 189);
            this.labelTune.Name = "labelTune";
            this.labelTune.Size = new System.Drawing.Size(32, 13);
            this.labelTune.TabIndex = 47;
            this.labelTune.Text = "Tune";
            // 
            // linkLabelHelp
            // 
            this.linkLabelHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelHelp.AutoSize = true;
            this.linkLabelHelp.Location = new System.Drawing.Point(747, 289);
            this.linkLabelHelp.Name = "linkLabelHelp";
            this.linkLabelHelp.Size = new System.Drawing.Size(29, 13);
            this.linkLabelHelp.TabIndex = 46;
            this.linkLabelHelp.TabStop = true;
            this.linkLabelHelp.Text = "Help";
            this.linkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelp_LinkClicked);
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
            this.comboBoxAudioSampleRate.Location = new System.Drawing.Point(334, 160);
            this.comboBoxAudioSampleRate.Name = "comboBoxAudioSampleRate";
            this.comboBoxAudioSampleRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAudioSampleRate.TabIndex = 45;
            // 
            // labelAudioSampleRate
            // 
            this.labelAudioSampleRate.AutoSize = true;
            this.labelAudioSampleRate.Location = new System.Drawing.Point(265, 162);
            this.labelAudioSampleRate.Name = "labelAudioSampleRate";
            this.labelAudioSampleRate.Size = new System.Drawing.Size(63, 13);
            this.labelAudioSampleRate.TabIndex = 44;
            this.labelAudioSampleRate.Text = "Sample rate";
            // 
            // checkBoxMakeStereo
            // 
            this.checkBoxMakeStereo.AutoSize = true;
            this.checkBoxMakeStereo.Checked = true;
            this.checkBoxMakeStereo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMakeStereo.Location = new System.Drawing.Point(334, 135);
            this.checkBoxMakeStereo.Name = "checkBoxMakeStereo";
            this.checkBoxMakeStereo.Size = new System.Drawing.Size(57, 17);
            this.checkBoxMakeStereo.TabIndex = 43;
            this.checkBoxMakeStereo.Text = "Stereo";
            this.checkBoxMakeStereo.UseVisualStyleBackColor = true;
            // 
            // comboBoxAudioEnc
            // 
            this.comboBoxAudioEnc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAudioEnc.FormattingEnabled = true;
            this.comboBoxAudioEnc.Items.AddRange(new object[] {
            "copy",
            "aac"});
            this.comboBoxAudioEnc.Location = new System.Drawing.Point(334, 106);
            this.comboBoxAudioEnc.Name = "comboBoxAudioEnc";
            this.comboBoxAudioEnc.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAudioEnc.TabIndex = 42;
            this.comboBoxAudioEnc.SelectedIndexChanged += new System.EventHandler(this.comboBoxAudioEnc_SelectedIndexChanged);
            // 
            // labelAudioEnc
            // 
            this.labelAudioEnc.AutoSize = true;
            this.labelAudioEnc.Location = new System.Drawing.Point(265, 108);
            this.labelAudioEnc.Name = "labelAudioEnc";
            this.labelAudioEnc.Size = new System.Drawing.Size(55, 13);
            this.labelAudioEnc.TabIndex = 41;
            this.labelAudioEnc.Text = "Audio enc";
            // 
            // comboBoxVideoEncoding
            // 
            this.comboBoxVideoEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVideoEncoding.FormattingEnabled = true;
            this.comboBoxVideoEncoding.Items.AddRange(new object[] {
            "libx264",
            "libx264rgb",
            "libx265",
            "mpeg4 "});
            this.comboBoxVideoEncoding.Location = new System.Drawing.Point(88, 106);
            this.comboBoxVideoEncoding.Name = "comboBoxVideoEncoding";
            this.comboBoxVideoEncoding.Size = new System.Drawing.Size(121, 21);
            this.comboBoxVideoEncoding.TabIndex = 40;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 108);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 39;
            this.label1.Text = "Video enc";
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
            this.comboBoxCrf.Location = new System.Drawing.Point(88, 160);
            this.comboBoxCrf.Name = "comboBoxCrf";
            this.comboBoxCrf.Size = new System.Drawing.Size(121, 21);
            this.comboBoxCrf.TabIndex = 38;
            // 
            // labelCRF
            // 
            this.labelCRF.AutoSize = true;
            this.labelCRF.Location = new System.Drawing.Point(19, 162);
            this.labelCRF.Name = "labelCRF";
            this.labelCRF.Size = new System.Drawing.Size(28, 13);
            this.labelCRF.TabIndex = 37;
            this.labelCRF.Text = "CRF";
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
            this.comboBoxPreset.Location = new System.Drawing.Point(88, 133);
            this.comboBoxPreset.Name = "comboBoxPreset";
            this.comboBoxPreset.Size = new System.Drawing.Size(121, 21);
            this.comboBoxPreset.TabIndex = 36;
            // 
            // labelPreset
            // 
            this.labelPreset.AutoSize = true;
            this.labelPreset.Location = new System.Drawing.Point(19, 135);
            this.labelPreset.Name = "labelPreset";
            this.labelPreset.Size = new System.Drawing.Size(37, 13);
            this.labelPreset.TabIndex = 35;
            this.labelPreset.Text = "Preset";
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(19, 233);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(52, 13);
            this.labelFileName.TabIndex = 33;
            this.labelFileName.Text = "File name";
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(158, 65);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(12, 13);
            this.labelX.TabIndex = 31;
            this.labelX.Text = "x";
            // 
            // labelResolution
            // 
            this.labelResolution.AutoSize = true;
            this.labelResolution.Location = new System.Drawing.Point(19, 65);
            this.labelResolution.Name = "labelResolution";
            this.labelResolution.Size = new System.Drawing.Size(57, 13);
            this.labelResolution.TabIndex = 29;
            this.labelResolution.Text = "Resolution";
            // 
            // numericUpDownHeight
            // 
            this.numericUpDownHeight.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownHeight.Location = new System.Drawing.Point(176, 63);
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
            this.numericUpDownHeight.TabIndex = 32;
            this.numericUpDownHeight.Value = new decimal(new int[] {
            720,
            0,
            0,
            0});
            this.numericUpDownHeight.ValueChanged += new System.EventHandler(this.numericUpDownHeight_ValueChanged);
            // 
            // numericUpDownWidth
            // 
            this.numericUpDownWidth.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownWidth.Location = new System.Drawing.Point(88, 63);
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
            this.numericUpDownWidth.TabIndex = 30;
            this.numericUpDownWidth.Value = new decimal(new int[] {
            1280,
            0,
            0,
            0});
            this.numericUpDownWidth.ValueChanged += new System.EventHandler(this.numericUpDownWidth_ValueChanged);
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
            this.labelInfo.Location = new System.Drawing.Point(12, 289);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(25, 13);
            this.labelInfo.TabIndex = 46;
            this.labelInfo.Text = "Info";
            // 
            // GenerateVideoWithHardSubs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 376);
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
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenerateVideoWithHardSubs_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).EndInit();
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).EndInit();
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
        private System.Windows.Forms.Label label1;
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
    }
}