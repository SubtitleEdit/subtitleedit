
namespace Nikse.SubtitleEdit.Forms.BeautifyTimeCodes
{
    sealed partial class BeautifyTimeCodesProfile
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
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItemLoadPreset = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxGeneral = new System.Windows.Forms.GroupBox();
            this.labelGapSuffix = new System.Windows.Forms.Label();
            this.numericUpDownGap = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelGap = new System.Windows.Forms.Label();
            this.groupBoxInCues = new System.Windows.Forms.GroupBox();
            this.labelInCuesZones = new System.Windows.Forms.Label();
            this.numericUpDownInCuesLeftGreenZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownInCuesLeftRedZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownInCuesRightGreenZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownInCuesRightRedZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownInCuesGap = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelInCuesGap = new System.Windows.Forms.Label();
            this.cuesPreviewViewInCues = new Nikse.SubtitleEdit.Controls.CuesPreviewView();
            this.groupBoxOutCues = new System.Windows.Forms.GroupBox();
            this.labelOutCuesZones = new System.Windows.Forms.Label();
            this.numericUpDownOutCuesLeftGreenZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownOutCuesLeftRedZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownOutCuesGap = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownOutCuesRightGreenZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownOutCuesRightRedZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelOutCuesGap = new System.Windows.Forms.Label();
            this.cuesPreviewViewOutCues = new Nikse.SubtitleEdit.Controls.CuesPreviewView();
            this.groupBoxConnectedSubtitles = new System.Windows.Forms.GroupBox();
            this.cuesPreviewViewConnectedSubtitlesInCueClosest = new Nikse.SubtitleEdit.Controls.CuesPreviewView();
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest = new Nikse.SubtitleEdit.Controls.CuesPreviewView();
            this.labelConnectedSubtitlesTreatConnectedSuffix = new System.Windows.Forms.Label();
            this.numericUpDownConnectedSubtitlesTreatConnected = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelConnectedSubtitlesTreatConnected = new System.Windows.Forms.Label();
            this.labelConnectedSubtitlesZones = new System.Windows.Forms.Label();
            this.numericUpDownConnectedSubtitlesLeftGreenZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownConnectedSubtitlesLeftRedZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownConnectedSubtitlesRightGreenZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownConnectedSubtitlesRightRedZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.tabControlConnectedSubtitles = new System.Windows.Forms.TabControl();
            this.tabPageConnectedSubtitlesInCueClosest = new System.Windows.Forms.TabPage();
            this.numericUpDownConnectedSubtitlesInCueClosestLeftGap = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownConnectedSubtitlesInCueClosestRightGap = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelConnectedSubtitlesInCueClosestGaps = new System.Windows.Forms.Label();
            this.tabPageConnectedSubtitlesOutCueClosest = new System.Windows.Forms.TabPage();
            this.numericUpDownConnectedSubtitlesOutCueClosestLeftGap = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownConnectedSubtitlesOutCueClosestRightGap = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelConnectedSubtitlesOutCueClosestGaps = new System.Windows.Forms.Label();
            this.groupBoxChaining = new System.Windows.Forms.GroupBox();
            this.cuesPreviewViewChainingGeneral = new Nikse.SubtitleEdit.Controls.CuesPreviewView();
            this.cuesPreviewViewChainingOutCueOnShot = new Nikse.SubtitleEdit.Controls.CuesPreviewView();
            this.cuesPreviewViewChainingInCueOnShot = new Nikse.SubtitleEdit.Controls.CuesPreviewView();
            this.tabControlChaining = new System.Windows.Forms.TabControl();
            this.tabPageChainingGeneral = new System.Windows.Forms.TabPage();
            this.labelChainingGeneralShotChangeBehavior = new System.Windows.Forms.Label();
            this.comboBoxChainingGeneralShotChangeBehavior = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelChainingGeneralMaxGapSuffix = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.radioButtonChainingGeneralZones = new System.Windows.Forms.RadioButton();
            this.radioButtonChainingGeneralMaxGap = new System.Windows.Forms.RadioButton();
            this.numericUpDownChainingGeneralLeftGreenZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownChainingGeneralLeftRedZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownChainingGeneralMaxGap = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.tabPageChainingInCueOnShot = new System.Windows.Forms.TabPage();
            this.labelChainingInCueOnShotMaxGapSuffix = new System.Windows.Forms.Label();
            this.radioButtonChainingInCueOnShotZones = new System.Windows.Forms.RadioButton();
            this.radioButtonChainingInCueOnShotMaxGap = new System.Windows.Forms.RadioButton();
            this.numericUpDownChainingInCueOnShotLeftGreenZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownChainingInCueOnShotLeftRedZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownChainingInCueOnShotMaxGap = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.tabPageChainingOutCueOnShot = new System.Windows.Forms.TabPage();
            this.numericUpDownChainingOutCueOnShotRightGreenZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownChainingOutCueOnShotRightRedZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelChainingOutCueOnShotMaxGapSuffix = new System.Windows.Forms.Label();
            this.radioButtonChainingOutCueOnShotZones = new System.Windows.Forms.RadioButton();
            this.radioButtonChainingOutCueOnShotMaxGap = new System.Windows.Forms.RadioButton();
            this.numericUpDownChainingOutCueOnShotMaxGap = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.buttonCreateSimple = new System.Windows.Forms.Button();
            this.menuStrip.SuspendLayout();
            this.groupBoxGeneral.SuspendLayout();
            this.groupBoxInCues.SuspendLayout();
            this.groupBoxOutCues.SuspendLayout();
            this.groupBoxConnectedSubtitles.SuspendLayout();
            this.tabControlConnectedSubtitles.SuspendLayout();
            this.tabPageConnectedSubtitlesInCueClosest.SuspendLayout();
            this.tabPageConnectedSubtitlesOutCueClosest.SuspendLayout();
            this.groupBoxChaining.SuspendLayout();
            this.tabControlChaining.SuspendLayout();
            this.tabPageChainingGeneral.SuspendLayout();
            this.tabPageChainingInCueOnShot.SuspendLayout();
            this.tabPageChainingOutCueOnShot.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(783, 505);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 101;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(702, 505);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 100;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemLoadPreset});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(870, 24);
            this.menuStrip.TabIndex = 103;
            this.menuStrip.Text = "menuStrip1";
            // 
            // toolStripMenuItemLoadPreset
            // 
            this.toolStripMenuItemLoadPreset.Name = "toolStripMenuItemLoadPreset";
            this.toolStripMenuItemLoadPreset.Size = new System.Drawing.Size(89, 20);
            this.toolStripMenuItemLoadPreset.Text = "Load preset...";
            // 
            // groupBoxGeneral
            // 
            this.groupBoxGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxGeneral.Controls.Add(this.labelGapSuffix);
            this.groupBoxGeneral.Controls.Add(this.numericUpDownGap);
            this.groupBoxGeneral.Controls.Add(this.labelGap);
            this.groupBoxGeneral.Location = new System.Drawing.Point(12, 27);
            this.groupBoxGeneral.Name = "groupBoxGeneral";
            this.groupBoxGeneral.Size = new System.Drawing.Size(846, 59);
            this.groupBoxGeneral.TabIndex = 10;
            this.groupBoxGeneral.TabStop = false;
            this.groupBoxGeneral.Text = "General";
            // 
            // labelGapSuffix
            // 
            this.labelGapSuffix.AutoSize = true;
            this.labelGapSuffix.Location = new System.Drawing.Point(108, 25);
            this.labelGapSuffix.Name = "labelGapSuffix";
            this.labelGapSuffix.Size = new System.Drawing.Size(211, 15);
            this.labelGapSuffix.TabIndex = 3;
            this.labelGapSuffix.Text = "frames (will overwrite custom settings)";
            // 
            // numericUpDownGap
            // 
            this.numericUpDownGap.Location = new System.Drawing.Point(50, 22);
            this.numericUpDownGap.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownGap.Name = "numericUpDownGap";
            this.numericUpDownGap.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownGap.TabIndex = 2;
            this.numericUpDownGap.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericUpDownGap.ValueChanged += new System.EventHandler(this.numericUpDownGap_ValueChanged);
            // 
            // labelGap
            // 
            this.labelGap.AutoSize = true;
            this.labelGap.Location = new System.Drawing.Point(7, 25);
            this.labelGap.Name = "labelGap";
            this.labelGap.Size = new System.Drawing.Size(31, 15);
            this.labelGap.TabIndex = 1;
            this.labelGap.Text = "Gap:";
            // 
            // groupBoxInCues
            // 
            this.groupBoxInCues.Controls.Add(this.labelInCuesZones);
            this.groupBoxInCues.Controls.Add(this.numericUpDownInCuesLeftGreenZone);
            this.groupBoxInCues.Controls.Add(this.numericUpDownInCuesLeftRedZone);
            this.groupBoxInCues.Controls.Add(this.numericUpDownInCuesRightGreenZone);
            this.groupBoxInCues.Controls.Add(this.numericUpDownInCuesRightRedZone);
            this.groupBoxInCues.Controls.Add(this.numericUpDownInCuesGap);
            this.groupBoxInCues.Controls.Add(this.labelInCuesGap);
            this.groupBoxInCues.Controls.Add(this.cuesPreviewViewInCues);
            this.groupBoxInCues.Location = new System.Drawing.Point(12, 92);
            this.groupBoxInCues.Name = "groupBoxInCues";
            this.groupBoxInCues.Size = new System.Drawing.Size(420, 161);
            this.groupBoxInCues.TabIndex = 20;
            this.groupBoxInCues.TabStop = false;
            this.groupBoxInCues.Text = "In cues";
            // 
            // labelInCuesZones
            // 
            this.labelInCuesZones.AutoSize = true;
            this.labelInCuesZones.Location = new System.Drawing.Point(7, 130);
            this.labelInCuesZones.Name = "labelInCuesZones";
            this.labelInCuesZones.Size = new System.Drawing.Size(42, 15);
            this.labelInCuesZones.TabIndex = 4;
            this.labelInCuesZones.Text = "Zones:";
            // 
            // numericUpDownInCuesLeftGreenZone
            // 
            this.numericUpDownInCuesLeftGreenZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.numericUpDownInCuesLeftGreenZone.Location = new System.Drawing.Point(93, 127);
            this.numericUpDownInCuesLeftGreenZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownInCuesLeftGreenZone.Name = "numericUpDownInCuesLeftGreenZone";
            this.numericUpDownInCuesLeftGreenZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownInCuesLeftGreenZone.TabIndex = 5;
            this.numericUpDownInCuesLeftGreenZone.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.numericUpDownInCuesLeftGreenZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownInCuesLeftRedZone
            // 
            this.numericUpDownInCuesLeftRedZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.numericUpDownInCuesLeftRedZone.Location = new System.Drawing.Point(151, 127);
            this.numericUpDownInCuesLeftRedZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownInCuesLeftRedZone.Name = "numericUpDownInCuesLeftRedZone";
            this.numericUpDownInCuesLeftRedZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownInCuesLeftRedZone.TabIndex = 6;
            this.numericUpDownInCuesLeftRedZone.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            this.numericUpDownInCuesLeftRedZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownInCuesRightGreenZone
            // 
            this.numericUpDownInCuesRightGreenZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.numericUpDownInCuesRightGreenZone.Location = new System.Drawing.Point(276, 127);
            this.numericUpDownInCuesRightGreenZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownInCuesRightGreenZone.Name = "numericUpDownInCuesRightGreenZone";
            this.numericUpDownInCuesRightGreenZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownInCuesRightGreenZone.TabIndex = 8;
            this.numericUpDownInCuesRightGreenZone.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.numericUpDownInCuesRightGreenZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownInCuesRightRedZone
            // 
            this.numericUpDownInCuesRightRedZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.numericUpDownInCuesRightRedZone.Location = new System.Drawing.Point(218, 127);
            this.numericUpDownInCuesRightRedZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownInCuesRightRedZone.Name = "numericUpDownInCuesRightRedZone";
            this.numericUpDownInCuesRightRedZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownInCuesRightRedZone.TabIndex = 7;
            this.numericUpDownInCuesRightRedZone.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            this.numericUpDownInCuesRightRedZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownInCuesGap
            // 
            this.numericUpDownInCuesGap.Location = new System.Drawing.Point(218, 98);
            this.numericUpDownInCuesGap.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownInCuesGap.Name = "numericUpDownInCuesGap";
            this.numericUpDownInCuesGap.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownInCuesGap.TabIndex = 3;
            this.numericUpDownInCuesGap.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // labelInCuesGap
            // 
            this.labelInCuesGap.AutoSize = true;
            this.labelInCuesGap.Location = new System.Drawing.Point(7, 101);
            this.labelInCuesGap.Name = "labelInCuesGap";
            this.labelInCuesGap.Size = new System.Drawing.Size(31, 15);
            this.labelInCuesGap.TabIndex = 2;
            this.labelInCuesGap.Text = "Gap:";
            // 
            // cuesPreviewViewInCues
            // 
            this.cuesPreviewViewInCues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cuesPreviewViewInCues.FrameRate = 25F;
            this.cuesPreviewViewInCues.LeftGap = 999;
            this.cuesPreviewViewInCues.LeftGreenZone = 12;
            this.cuesPreviewViewInCues.LeftRedZone = 7;
            this.cuesPreviewViewInCues.Location = new System.Drawing.Point(10, 22);
            this.cuesPreviewViewInCues.Name = "cuesPreviewViewInCues";
            this.cuesPreviewViewInCues.PreviewText = "Subtitle text.";
            this.cuesPreviewViewInCues.RightGap = 0;
            this.cuesPreviewViewInCues.RightGreenZone = 12;
            this.cuesPreviewViewInCues.RightRedZone = 7;
            this.cuesPreviewViewInCues.ShowShotChange = true;
            this.cuesPreviewViewInCues.Size = new System.Drawing.Size(400, 70);
            this.cuesPreviewViewInCues.TabIndex = 1;
            // 
            // groupBoxOutCues
            // 
            this.groupBoxOutCues.Controls.Add(this.labelOutCuesZones);
            this.groupBoxOutCues.Controls.Add(this.numericUpDownOutCuesLeftGreenZone);
            this.groupBoxOutCues.Controls.Add(this.numericUpDownOutCuesLeftRedZone);
            this.groupBoxOutCues.Controls.Add(this.numericUpDownOutCuesGap);
            this.groupBoxOutCues.Controls.Add(this.numericUpDownOutCuesRightGreenZone);
            this.groupBoxOutCues.Controls.Add(this.numericUpDownOutCuesRightRedZone);
            this.groupBoxOutCues.Controls.Add(this.labelOutCuesGap);
            this.groupBoxOutCues.Controls.Add(this.cuesPreviewViewOutCues);
            this.groupBoxOutCues.Location = new System.Drawing.Point(438, 92);
            this.groupBoxOutCues.Name = "groupBoxOutCues";
            this.groupBoxOutCues.Size = new System.Drawing.Size(420, 161);
            this.groupBoxOutCues.TabIndex = 30;
            this.groupBoxOutCues.TabStop = false;
            this.groupBoxOutCues.Text = "Out cues";
            // 
            // labelOutCuesZones
            // 
            this.labelOutCuesZones.AutoSize = true;
            this.labelOutCuesZones.Location = new System.Drawing.Point(7, 130);
            this.labelOutCuesZones.Name = "labelOutCuesZones";
            this.labelOutCuesZones.Size = new System.Drawing.Size(42, 15);
            this.labelOutCuesZones.TabIndex = 4;
            this.labelOutCuesZones.Text = "Zones:";
            // 
            // numericUpDownOutCuesLeftGreenZone
            // 
            this.numericUpDownOutCuesLeftGreenZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.numericUpDownOutCuesLeftGreenZone.Location = new System.Drawing.Point(93, 127);
            this.numericUpDownOutCuesLeftGreenZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownOutCuesLeftGreenZone.Name = "numericUpDownOutCuesLeftGreenZone";
            this.numericUpDownOutCuesLeftGreenZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownOutCuesLeftGreenZone.TabIndex = 5;
            this.numericUpDownOutCuesLeftGreenZone.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.numericUpDownOutCuesLeftGreenZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownOutCuesLeftRedZone
            // 
            this.numericUpDownOutCuesLeftRedZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.numericUpDownOutCuesLeftRedZone.Location = new System.Drawing.Point(151, 127);
            this.numericUpDownOutCuesLeftRedZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownOutCuesLeftRedZone.Name = "numericUpDownOutCuesLeftRedZone";
            this.numericUpDownOutCuesLeftRedZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownOutCuesLeftRedZone.TabIndex = 6;
            this.numericUpDownOutCuesLeftRedZone.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            this.numericUpDownOutCuesLeftRedZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownOutCuesGap
            // 
            this.numericUpDownOutCuesGap.Location = new System.Drawing.Point(151, 98);
            this.numericUpDownOutCuesGap.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownOutCuesGap.Name = "numericUpDownOutCuesGap";
            this.numericUpDownOutCuesGap.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownOutCuesGap.TabIndex = 3;
            this.numericUpDownOutCuesGap.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownOutCuesRightGreenZone
            // 
            this.numericUpDownOutCuesRightGreenZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.numericUpDownOutCuesRightGreenZone.Location = new System.Drawing.Point(276, 127);
            this.numericUpDownOutCuesRightGreenZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownOutCuesRightGreenZone.Name = "numericUpDownOutCuesRightGreenZone";
            this.numericUpDownOutCuesRightGreenZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownOutCuesRightGreenZone.TabIndex = 8;
            this.numericUpDownOutCuesRightGreenZone.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.numericUpDownOutCuesRightGreenZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownOutCuesRightRedZone
            // 
            this.numericUpDownOutCuesRightRedZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.numericUpDownOutCuesRightRedZone.Location = new System.Drawing.Point(218, 127);
            this.numericUpDownOutCuesRightRedZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownOutCuesRightRedZone.Name = "numericUpDownOutCuesRightRedZone";
            this.numericUpDownOutCuesRightRedZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownOutCuesRightRedZone.TabIndex = 7;
            this.numericUpDownOutCuesRightRedZone.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            this.numericUpDownOutCuesRightRedZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // labelOutCuesGap
            // 
            this.labelOutCuesGap.AutoSize = true;
            this.labelOutCuesGap.Location = new System.Drawing.Point(7, 101);
            this.labelOutCuesGap.Name = "labelOutCuesGap";
            this.labelOutCuesGap.Size = new System.Drawing.Size(31, 15);
            this.labelOutCuesGap.TabIndex = 2;
            this.labelOutCuesGap.Text = "Gap:";
            // 
            // cuesPreviewViewOutCues
            // 
            this.cuesPreviewViewOutCues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cuesPreviewViewOutCues.FrameRate = 25F;
            this.cuesPreviewViewOutCues.LeftGap = 0;
            this.cuesPreviewViewOutCues.LeftGreenZone = 12;
            this.cuesPreviewViewOutCues.LeftRedZone = 7;
            this.cuesPreviewViewOutCues.Location = new System.Drawing.Point(10, 22);
            this.cuesPreviewViewOutCues.Name = "cuesPreviewViewOutCues";
            this.cuesPreviewViewOutCues.PreviewText = "Subtitle text.";
            this.cuesPreviewViewOutCues.RightGap = 999;
            this.cuesPreviewViewOutCues.RightGreenZone = 12;
            this.cuesPreviewViewOutCues.RightRedZone = 7;
            this.cuesPreviewViewOutCues.ShowShotChange = true;
            this.cuesPreviewViewOutCues.Size = new System.Drawing.Size(400, 70);
            this.cuesPreviewViewOutCues.TabIndex = 1;
            // 
            // groupBoxConnectedSubtitles
            // 
            this.groupBoxConnectedSubtitles.Controls.Add(this.cuesPreviewViewConnectedSubtitlesInCueClosest);
            this.groupBoxConnectedSubtitles.Controls.Add(this.cuesPreviewViewConnectedSubtitlesOutCueClosest);
            this.groupBoxConnectedSubtitles.Controls.Add(this.labelConnectedSubtitlesTreatConnectedSuffix);
            this.groupBoxConnectedSubtitles.Controls.Add(this.numericUpDownConnectedSubtitlesTreatConnected);
            this.groupBoxConnectedSubtitles.Controls.Add(this.labelConnectedSubtitlesTreatConnected);
            this.groupBoxConnectedSubtitles.Controls.Add(this.labelConnectedSubtitlesZones);
            this.groupBoxConnectedSubtitles.Controls.Add(this.numericUpDownConnectedSubtitlesLeftGreenZone);
            this.groupBoxConnectedSubtitles.Controls.Add(this.numericUpDownConnectedSubtitlesLeftRedZone);
            this.groupBoxConnectedSubtitles.Controls.Add(this.numericUpDownConnectedSubtitlesRightGreenZone);
            this.groupBoxConnectedSubtitles.Controls.Add(this.numericUpDownConnectedSubtitlesRightRedZone);
            this.groupBoxConnectedSubtitles.Controls.Add(this.tabControlConnectedSubtitles);
            this.groupBoxConnectedSubtitles.Location = new System.Drawing.Point(12, 259);
            this.groupBoxConnectedSubtitles.Name = "groupBoxConnectedSubtitles";
            this.groupBoxConnectedSubtitles.Size = new System.Drawing.Size(420, 235);
            this.groupBoxConnectedSubtitles.TabIndex = 40;
            this.groupBoxConnectedSubtitles.TabStop = false;
            this.groupBoxConnectedSubtitles.Text = "Connected subtitles";
            // 
            // cuesPreviewViewConnectedSubtitlesInCueClosest
            // 
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.FrameRate = 25F;
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.LeftGap = 3;
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.LeftGreenZone = 12;
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.LeftRedZone = 7;
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.Location = new System.Drawing.Point(10, 22);
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.Name = "cuesPreviewViewConnectedSubtitlesInCueClosest";
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.PreviewText = "Subtitle text.";
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.RightGap = 0;
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.RightGreenZone = 12;
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.RightRedZone = 7;
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.ShowShotChange = true;
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.Size = new System.Drawing.Size(400, 70);
            this.cuesPreviewViewConnectedSubtitlesInCueClosest.TabIndex = 1;
            // 
            // cuesPreviewViewConnectedSubtitlesOutCueClosest
            // 
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.FrameRate = 25F;
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.LeftGap = 3;
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.LeftGreenZone = 12;
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.LeftRedZone = 7;
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.Location = new System.Drawing.Point(10, 22);
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.Name = "cuesPreviewViewConnectedSubtitlesOutCueClosest";
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.PreviewText = "Subtitle text.";
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.RightGap = 0;
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.RightGreenZone = 12;
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.RightRedZone = 7;
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.ShowShotChange = true;
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.Size = new System.Drawing.Size(400, 70);
            this.cuesPreviewViewConnectedSubtitlesOutCueClosest.TabIndex = 2;
            // 
            // labelConnectedSubtitlesTreatConnectedSuffix
            // 
            this.labelConnectedSubtitlesTreatConnectedSuffix.AutoSize = true;
            this.labelConnectedSubtitlesTreatConnectedSuffix.Location = new System.Drawing.Point(334, 204);
            this.labelConnectedSubtitlesTreatConnectedSuffix.Name = "labelConnectedSubtitlesTreatConnectedSuffix";
            this.labelConnectedSubtitlesTreatConnectedSuffix.Size = new System.Drawing.Size(23, 15);
            this.labelConnectedSubtitlesTreatConnectedSuffix.TabIndex = 27;
            this.labelConnectedSubtitlesTreatConnectedSuffix.Text = "ms";
            // 
            // numericUpDownConnectedSubtitlesTreatConnected
            // 
            this.numericUpDownConnectedSubtitlesTreatConnected.Location = new System.Drawing.Point(276, 201);
            this.numericUpDownConnectedSubtitlesTreatConnected.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesTreatConnected.Name = "numericUpDownConnectedSubtitlesTreatConnected";
            this.numericUpDownConnectedSubtitlesTreatConnected.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownConnectedSubtitlesTreatConnected.TabIndex = 26;
            this.numericUpDownConnectedSubtitlesTreatConnected.Value = new decimal(new int[] {
            180,
            0,
            0,
            0});
            // 
            // labelConnectedSubtitlesTreatConnected
            // 
            this.labelConnectedSubtitlesTreatConnected.AutoSize = true;
            this.labelConnectedSubtitlesTreatConnected.Location = new System.Drawing.Point(7, 204);
            this.labelConnectedSubtitlesTreatConnected.Name = "labelConnectedSubtitlesTreatConnected";
            this.labelConnectedSubtitlesTreatConnected.Size = new System.Drawing.Size(220, 15);
            this.labelConnectedSubtitlesTreatConnected.TabIndex = 25;
            this.labelConnectedSubtitlesTreatConnected.Text = "Treat as connected if gap is smaller than:";
            // 
            // labelConnectedSubtitlesZones
            // 
            this.labelConnectedSubtitlesZones.AutoSize = true;
            this.labelConnectedSubtitlesZones.Location = new System.Drawing.Point(7, 159);
            this.labelConnectedSubtitlesZones.Name = "labelConnectedSubtitlesZones";
            this.labelConnectedSubtitlesZones.Size = new System.Drawing.Size(42, 15);
            this.labelConnectedSubtitlesZones.TabIndex = 20;
            this.labelConnectedSubtitlesZones.Text = "Zones:";
            // 
            // numericUpDownConnectedSubtitlesLeftGreenZone
            // 
            this.numericUpDownConnectedSubtitlesLeftGreenZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.numericUpDownConnectedSubtitlesLeftGreenZone.Location = new System.Drawing.Point(93, 156);
            this.numericUpDownConnectedSubtitlesLeftGreenZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesLeftGreenZone.Name = "numericUpDownConnectedSubtitlesLeftGreenZone";
            this.numericUpDownConnectedSubtitlesLeftGreenZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownConnectedSubtitlesLeftGreenZone.TabIndex = 21;
            this.numericUpDownConnectedSubtitlesLeftGreenZone.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesLeftGreenZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownConnectedSubtitlesLeftRedZone
            // 
            this.numericUpDownConnectedSubtitlesLeftRedZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.numericUpDownConnectedSubtitlesLeftRedZone.Location = new System.Drawing.Point(151, 156);
            this.numericUpDownConnectedSubtitlesLeftRedZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesLeftRedZone.Name = "numericUpDownConnectedSubtitlesLeftRedZone";
            this.numericUpDownConnectedSubtitlesLeftRedZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownConnectedSubtitlesLeftRedZone.TabIndex = 22;
            this.numericUpDownConnectedSubtitlesLeftRedZone.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesLeftRedZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownConnectedSubtitlesRightGreenZone
            // 
            this.numericUpDownConnectedSubtitlesRightGreenZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.numericUpDownConnectedSubtitlesRightGreenZone.Location = new System.Drawing.Point(276, 156);
            this.numericUpDownConnectedSubtitlesRightGreenZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesRightGreenZone.Name = "numericUpDownConnectedSubtitlesRightGreenZone";
            this.numericUpDownConnectedSubtitlesRightGreenZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownConnectedSubtitlesRightGreenZone.TabIndex = 24;
            this.numericUpDownConnectedSubtitlesRightGreenZone.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesRightGreenZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownConnectedSubtitlesRightRedZone
            // 
            this.numericUpDownConnectedSubtitlesRightRedZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.numericUpDownConnectedSubtitlesRightRedZone.Location = new System.Drawing.Point(218, 156);
            this.numericUpDownConnectedSubtitlesRightRedZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesRightRedZone.Name = "numericUpDownConnectedSubtitlesRightRedZone";
            this.numericUpDownConnectedSubtitlesRightRedZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownConnectedSubtitlesRightRedZone.TabIndex = 23;
            this.numericUpDownConnectedSubtitlesRightRedZone.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesRightRedZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // tabControlConnectedSubtitles
            // 
            this.tabControlConnectedSubtitles.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControlConnectedSubtitles.Controls.Add(this.tabPageConnectedSubtitlesInCueClosest);
            this.tabControlConnectedSubtitles.Controls.Add(this.tabPageConnectedSubtitlesOutCueClosest);
            this.tabControlConnectedSubtitles.Location = new System.Drawing.Point(6, 98);
            this.tabControlConnectedSubtitles.Name = "tabControlConnectedSubtitles";
            this.tabControlConnectedSubtitles.SelectedIndex = 0;
            this.tabControlConnectedSubtitles.Size = new System.Drawing.Size(408, 58);
            this.tabControlConnectedSubtitles.TabIndex = 2;
            this.tabControlConnectedSubtitles.SelectedIndexChanged += new System.EventHandler(this.tabControlConnectedSubtitles_SelectedIndexChanged);
            // 
            // tabPageConnectedSubtitlesInCueClosest
            // 
            this.tabPageConnectedSubtitlesInCueClosest.Controls.Add(this.numericUpDownConnectedSubtitlesInCueClosestLeftGap);
            this.tabPageConnectedSubtitlesInCueClosest.Controls.Add(this.numericUpDownConnectedSubtitlesInCueClosestRightGap);
            this.tabPageConnectedSubtitlesInCueClosest.Controls.Add(this.labelConnectedSubtitlesInCueClosestGaps);
            this.tabPageConnectedSubtitlesInCueClosest.Location = new System.Drawing.Point(4, 27);
            this.tabPageConnectedSubtitlesInCueClosest.Name = "tabPageConnectedSubtitlesInCueClosest";
            this.tabPageConnectedSubtitlesInCueClosest.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConnectedSubtitlesInCueClosest.Size = new System.Drawing.Size(400, 27);
            this.tabPageConnectedSubtitlesInCueClosest.TabIndex = 0;
            this.tabPageConnectedSubtitlesInCueClosest.Text = "In cue is closest";
            this.tabPageConnectedSubtitlesInCueClosest.UseVisualStyleBackColor = true;
            // 
            // numericUpDownConnectedSubtitlesInCueClosestLeftGap
            // 
            this.numericUpDownConnectedSubtitlesInCueClosestLeftGap.Location = new System.Drawing.Point(141, 2);
            this.numericUpDownConnectedSubtitlesInCueClosestLeftGap.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesInCueClosestLeftGap.Name = "numericUpDownConnectedSubtitlesInCueClosestLeftGap";
            this.numericUpDownConnectedSubtitlesInCueClosestLeftGap.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownConnectedSubtitlesInCueClosestLeftGap.TabIndex = 2;
            this.numericUpDownConnectedSubtitlesInCueClosestLeftGap.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownConnectedSubtitlesInCueClosestRightGap
            // 
            this.numericUpDownConnectedSubtitlesInCueClosestRightGap.Location = new System.Drawing.Point(208, 2);
            this.numericUpDownConnectedSubtitlesInCueClosestRightGap.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesInCueClosestRightGap.Name = "numericUpDownConnectedSubtitlesInCueClosestRightGap";
            this.numericUpDownConnectedSubtitlesInCueClosestRightGap.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownConnectedSubtitlesInCueClosestRightGap.TabIndex = 3;
            this.numericUpDownConnectedSubtitlesInCueClosestRightGap.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // labelConnectedSubtitlesInCueClosestGaps
            // 
            this.labelConnectedSubtitlesInCueClosestGaps.AutoSize = true;
            this.labelConnectedSubtitlesInCueClosestGaps.Location = new System.Drawing.Point(-3, 5);
            this.labelConnectedSubtitlesInCueClosestGaps.Name = "labelConnectedSubtitlesInCueClosestGaps";
            this.labelConnectedSubtitlesInCueClosestGaps.Size = new System.Drawing.Size(31, 15);
            this.labelConnectedSubtitlesInCueClosestGaps.TabIndex = 1;
            this.labelConnectedSubtitlesInCueClosestGaps.Text = "Gap:";
            // 
            // tabPageConnectedSubtitlesOutCueClosest
            // 
            this.tabPageConnectedSubtitlesOutCueClosest.Controls.Add(this.numericUpDownConnectedSubtitlesOutCueClosestLeftGap);
            this.tabPageConnectedSubtitlesOutCueClosest.Controls.Add(this.numericUpDownConnectedSubtitlesOutCueClosestRightGap);
            this.tabPageConnectedSubtitlesOutCueClosest.Controls.Add(this.labelConnectedSubtitlesOutCueClosestGaps);
            this.tabPageConnectedSubtitlesOutCueClosest.Location = new System.Drawing.Point(4, 27);
            this.tabPageConnectedSubtitlesOutCueClosest.Name = "tabPageConnectedSubtitlesOutCueClosest";
            this.tabPageConnectedSubtitlesOutCueClosest.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConnectedSubtitlesOutCueClosest.Size = new System.Drawing.Size(400, 27);
            this.tabPageConnectedSubtitlesOutCueClosest.TabIndex = 1;
            this.tabPageConnectedSubtitlesOutCueClosest.Text = "Out cue is closest";
            this.tabPageConnectedSubtitlesOutCueClosest.UseVisualStyleBackColor = true;
            // 
            // numericUpDownConnectedSubtitlesOutCueClosestLeftGap
            // 
            this.numericUpDownConnectedSubtitlesOutCueClosestLeftGap.Location = new System.Drawing.Point(141, 2);
            this.numericUpDownConnectedSubtitlesOutCueClosestLeftGap.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesOutCueClosestLeftGap.Name = "numericUpDownConnectedSubtitlesOutCueClosestLeftGap";
            this.numericUpDownConnectedSubtitlesOutCueClosestLeftGap.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownConnectedSubtitlesOutCueClosestLeftGap.TabIndex = 5;
            this.numericUpDownConnectedSubtitlesOutCueClosestLeftGap.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownConnectedSubtitlesOutCueClosestRightGap
            // 
            this.numericUpDownConnectedSubtitlesOutCueClosestRightGap.Location = new System.Drawing.Point(208, 2);
            this.numericUpDownConnectedSubtitlesOutCueClosestRightGap.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownConnectedSubtitlesOutCueClosestRightGap.Name = "numericUpDownConnectedSubtitlesOutCueClosestRightGap";
            this.numericUpDownConnectedSubtitlesOutCueClosestRightGap.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownConnectedSubtitlesOutCueClosestRightGap.TabIndex = 6;
            this.numericUpDownConnectedSubtitlesOutCueClosestRightGap.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // labelConnectedSubtitlesOutCueClosestGaps
            // 
            this.labelConnectedSubtitlesOutCueClosestGaps.AutoSize = true;
            this.labelConnectedSubtitlesOutCueClosestGaps.Location = new System.Drawing.Point(-3, 5);
            this.labelConnectedSubtitlesOutCueClosestGaps.Name = "labelConnectedSubtitlesOutCueClosestGaps";
            this.labelConnectedSubtitlesOutCueClosestGaps.Size = new System.Drawing.Size(31, 15);
            this.labelConnectedSubtitlesOutCueClosestGaps.TabIndex = 4;
            this.labelConnectedSubtitlesOutCueClosestGaps.Text = "Gap:";
            // 
            // groupBoxChaining
            // 
            this.groupBoxChaining.Controls.Add(this.cuesPreviewViewChainingGeneral);
            this.groupBoxChaining.Controls.Add(this.cuesPreviewViewChainingOutCueOnShot);
            this.groupBoxChaining.Controls.Add(this.cuesPreviewViewChainingInCueOnShot);
            this.groupBoxChaining.Controls.Add(this.tabControlChaining);
            this.groupBoxChaining.Location = new System.Drawing.Point(438, 259);
            this.groupBoxChaining.Name = "groupBoxChaining";
            this.groupBoxChaining.Size = new System.Drawing.Size(420, 235);
            this.groupBoxChaining.TabIndex = 60;
            this.groupBoxChaining.TabStop = false;
            this.groupBoxChaining.Text = "Chaining";
            // 
            // cuesPreviewViewChainingGeneral
            // 
            this.cuesPreviewViewChainingGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cuesPreviewViewChainingGeneral.FrameRate = 25F;
            this.cuesPreviewViewChainingGeneral.LeftGap = 25;
            this.cuesPreviewViewChainingGeneral.LeftGreenZone = 12;
            this.cuesPreviewViewChainingGeneral.LeftRedZone = 7;
            this.cuesPreviewViewChainingGeneral.Location = new System.Drawing.Point(10, 22);
            this.cuesPreviewViewChainingGeneral.Name = "cuesPreviewViewChainingGeneral";
            this.cuesPreviewViewChainingGeneral.PreviewText = "Subtitle text.";
            this.cuesPreviewViewChainingGeneral.RightGap = 0;
            this.cuesPreviewViewChainingGeneral.RightGreenZone = 0;
            this.cuesPreviewViewChainingGeneral.RightRedZone = 0;
            this.cuesPreviewViewChainingGeneral.ShowShotChange = false;
            this.cuesPreviewViewChainingGeneral.Size = new System.Drawing.Size(400, 70);
            this.cuesPreviewViewChainingGeneral.TabIndex = 1;
            // 
            // cuesPreviewViewChainingOutCueOnShot
            // 
            this.cuesPreviewViewChainingOutCueOnShot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cuesPreviewViewChainingOutCueOnShot.FrameRate = 25F;
            this.cuesPreviewViewChainingOutCueOnShot.LeftGap = 0;
            this.cuesPreviewViewChainingOutCueOnShot.LeftGreenZone = 0;
            this.cuesPreviewViewChainingOutCueOnShot.LeftRedZone = 0;
            this.cuesPreviewViewChainingOutCueOnShot.Location = new System.Drawing.Point(10, 22);
            this.cuesPreviewViewChainingOutCueOnShot.Name = "cuesPreviewViewChainingOutCueOnShot";
            this.cuesPreviewViewChainingOutCueOnShot.PreviewText = "Subtitle text.";
            this.cuesPreviewViewChainingOutCueOnShot.RightGap = 25;
            this.cuesPreviewViewChainingOutCueOnShot.RightGreenZone = 12;
            this.cuesPreviewViewChainingOutCueOnShot.RightRedZone = 7;
            this.cuesPreviewViewChainingOutCueOnShot.ShowShotChange = true;
            this.cuesPreviewViewChainingOutCueOnShot.Size = new System.Drawing.Size(400, 70);
            this.cuesPreviewViewChainingOutCueOnShot.TabIndex = 3;
            this.cuesPreviewViewChainingOutCueOnShot.Visible = false;
            // 
            // cuesPreviewViewChainingInCueOnShot
            // 
            this.cuesPreviewViewChainingInCueOnShot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cuesPreviewViewChainingInCueOnShot.FrameRate = 25F;
            this.cuesPreviewViewChainingInCueOnShot.LeftGap = 25;
            this.cuesPreviewViewChainingInCueOnShot.LeftGreenZone = 12;
            this.cuesPreviewViewChainingInCueOnShot.LeftRedZone = 7;
            this.cuesPreviewViewChainingInCueOnShot.Location = new System.Drawing.Point(10, 22);
            this.cuesPreviewViewChainingInCueOnShot.Name = "cuesPreviewViewChainingInCueOnShot";
            this.cuesPreviewViewChainingInCueOnShot.PreviewText = "Subtitle text.";
            this.cuesPreviewViewChainingInCueOnShot.RightGap = 0;
            this.cuesPreviewViewChainingInCueOnShot.RightGreenZone = 0;
            this.cuesPreviewViewChainingInCueOnShot.RightRedZone = 0;
            this.cuesPreviewViewChainingInCueOnShot.ShowShotChange = true;
            this.cuesPreviewViewChainingInCueOnShot.Size = new System.Drawing.Size(400, 70);
            this.cuesPreviewViewChainingInCueOnShot.TabIndex = 2;
            this.cuesPreviewViewChainingInCueOnShot.Visible = false;
            // 
            // tabControlChaining
            // 
            this.tabControlChaining.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlChaining.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControlChaining.Controls.Add(this.tabPageChainingGeneral);
            this.tabControlChaining.Controls.Add(this.tabPageChainingInCueOnShot);
            this.tabControlChaining.Controls.Add(this.tabPageChainingOutCueOnShot);
            this.tabControlChaining.Location = new System.Drawing.Point(6, 98);
            this.tabControlChaining.Name = "tabControlChaining";
            this.tabControlChaining.SelectedIndex = 0;
            this.tabControlChaining.Size = new System.Drawing.Size(408, 131);
            this.tabControlChaining.TabIndex = 4;
            this.tabControlChaining.SelectedIndexChanged += new System.EventHandler(this.tabControlChaining_SelectedIndexChanged);
            // 
            // tabPageChainingGeneral
            // 
            this.tabPageChainingGeneral.Controls.Add(this.labelChainingGeneralShotChangeBehavior);
            this.tabPageChainingGeneral.Controls.Add(this.comboBoxChainingGeneralShotChangeBehavior);
            this.tabPageChainingGeneral.Controls.Add(this.labelChainingGeneralMaxGapSuffix);
            this.tabPageChainingGeneral.Controls.Add(this.radioButtonChainingGeneralZones);
            this.tabPageChainingGeneral.Controls.Add(this.radioButtonChainingGeneralMaxGap);
            this.tabPageChainingGeneral.Controls.Add(this.numericUpDownChainingGeneralLeftGreenZone);
            this.tabPageChainingGeneral.Controls.Add(this.numericUpDownChainingGeneralLeftRedZone);
            this.tabPageChainingGeneral.Controls.Add(this.numericUpDownChainingGeneralMaxGap);
            this.tabPageChainingGeneral.Location = new System.Drawing.Point(4, 27);
            this.tabPageChainingGeneral.Name = "tabPageChainingGeneral";
            this.tabPageChainingGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageChainingGeneral.Size = new System.Drawing.Size(400, 100);
            this.tabPageChainingGeneral.TabIndex = 0;
            this.tabPageChainingGeneral.Text = "General";
            this.tabPageChainingGeneral.UseVisualStyleBackColor = true;
            // 
            // labelChainingGeneralShotChangeBehavior
            // 
            this.labelChainingGeneralShotChangeBehavior.AutoSize = true;
            this.labelChainingGeneralShotChangeBehavior.Location = new System.Drawing.Point(0, 79);
            this.labelChainingGeneralShotChangeBehavior.Name = "labelChainingGeneralShotChangeBehavior";
            this.labelChainingGeneralShotChangeBehavior.Size = new System.Drawing.Size(193, 15);
            this.labelChainingGeneralShotChangeBehavior.TabIndex = 7;
            this.labelChainingGeneralShotChangeBehavior.Text = "If there\'s a shot change in between:";
            // 
            // comboBoxChainingGeneralShotChangeBehavior
            // 
            this.comboBoxChainingGeneralShotChangeBehavior.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxChainingGeneralShotChangeBehavior.FormattingEnabled = true;
            this.comboBoxChainingGeneralShotChangeBehavior.Items.AddRange(new object[] {
            "Don\'t chain",
            "Extend, crossing shot change",
            "Extend until shot change"});
            this.comboBoxChainingGeneralShotChangeBehavior.Location = new System.Drawing.Point(208, 76);
            this.comboBoxChainingGeneralShotChangeBehavior.Name = "comboBoxChainingGeneralShotChangeBehavior";
            this.comboBoxChainingGeneralShotChangeBehavior.Size = new System.Drawing.Size(192, 23);
            this.comboBoxChainingGeneralShotChangeBehavior.TabIndex = 8;
            // 
            // labelChainingGeneralMaxGapSuffix
            // 
            this.labelChainingGeneralMaxGapSuffix.AutoSize = true;
            this.labelChainingGeneralMaxGapSuffix.Location = new System.Drawing.Point(199, 6);
            this.labelChainingGeneralMaxGapSuffix.Name = "labelChainingGeneralMaxGapSuffix";
            this.labelChainingGeneralMaxGapSuffix.Size = new System.Drawing.Size(23, 15);
            this.labelChainingGeneralMaxGapSuffix.TabIndex = 3;
            this.labelChainingGeneralMaxGapSuffix.Text = "ms";
            // 
            // radioButtonChainingGeneralZones
            // 
            this.radioButtonChainingGeneralZones.AutoSize = true;
            this.radioButtonChainingGeneralZones.Location = new System.Drawing.Point(0, 32);
            this.radioButtonChainingGeneralZones.Name = "radioButtonChainingGeneralZones";
            this.radioButtonChainingGeneralZones.Size = new System.Drawing.Size(60, 19);
            this.radioButtonChainingGeneralZones.TabIndex = 4;
            this.radioButtonChainingGeneralZones.Text = "Zones:";
            this.radioButtonChainingGeneralZones.UseVisualStyleBackColor = true;
            this.radioButtonChainingGeneralZones.CheckedChanged += new System.EventHandler(this.radioButtonChaining_CheckedChanged);
            // 
            // radioButtonChainingGeneralMaxGap
            // 
            this.radioButtonChainingGeneralMaxGap.AutoSize = true;
            this.radioButtonChainingGeneralMaxGap.Checked = true;
            this.radioButtonChainingGeneralMaxGap.Location = new System.Drawing.Point(0, 3);
            this.radioButtonChainingGeneralMaxGap.Name = "radioButtonChainingGeneralMaxGap";
            this.radioButtonChainingGeneralMaxGap.Size = new System.Drawing.Size(77, 19);
            this.radioButtonChainingGeneralMaxGap.TabIndex = 1;
            this.radioButtonChainingGeneralMaxGap.TabStop = true;
            this.radioButtonChainingGeneralMaxGap.Text = "Max. gap:";
            this.radioButtonChainingGeneralMaxGap.UseVisualStyleBackColor = true;
            this.radioButtonChainingGeneralMaxGap.CheckedChanged += new System.EventHandler(this.radioButtonChaining_CheckedChanged);
            // 
            // numericUpDownChainingGeneralLeftGreenZone
            // 
            this.numericUpDownChainingGeneralLeftGreenZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.numericUpDownChainingGeneralLeftGreenZone.Enabled = false;
            this.numericUpDownChainingGeneralLeftGreenZone.Location = new System.Drawing.Point(83, 31);
            this.numericUpDownChainingGeneralLeftGreenZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownChainingGeneralLeftGreenZone.Name = "numericUpDownChainingGeneralLeftGreenZone";
            this.numericUpDownChainingGeneralLeftGreenZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownChainingGeneralLeftGreenZone.TabIndex = 5;
            this.numericUpDownChainingGeneralLeftGreenZone.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDownChainingGeneralLeftGreenZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownChainingGeneralLeftRedZone
            // 
            this.numericUpDownChainingGeneralLeftRedZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.numericUpDownChainingGeneralLeftRedZone.Enabled = false;
            this.numericUpDownChainingGeneralLeftRedZone.Location = new System.Drawing.Point(141, 31);
            this.numericUpDownChainingGeneralLeftRedZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownChainingGeneralLeftRedZone.Name = "numericUpDownChainingGeneralLeftRedZone";
            this.numericUpDownChainingGeneralLeftRedZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownChainingGeneralLeftRedZone.TabIndex = 6;
            this.numericUpDownChainingGeneralLeftRedZone.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numericUpDownChainingGeneralLeftRedZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownChainingGeneralMaxGap
            // 
            this.numericUpDownChainingGeneralMaxGap.Location = new System.Drawing.Point(141, 2);
            this.numericUpDownChainingGeneralMaxGap.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDownChainingGeneralMaxGap.Name = "numericUpDownChainingGeneralMaxGap";
            this.numericUpDownChainingGeneralMaxGap.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownChainingGeneralMaxGap.TabIndex = 2;
            this.numericUpDownChainingGeneralMaxGap.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // tabPageChainingInCueOnShot
            // 
            this.tabPageChainingInCueOnShot.Controls.Add(this.labelChainingInCueOnShotMaxGapSuffix);
            this.tabPageChainingInCueOnShot.Controls.Add(this.radioButtonChainingInCueOnShotZones);
            this.tabPageChainingInCueOnShot.Controls.Add(this.radioButtonChainingInCueOnShotMaxGap);
            this.tabPageChainingInCueOnShot.Controls.Add(this.numericUpDownChainingInCueOnShotLeftGreenZone);
            this.tabPageChainingInCueOnShot.Controls.Add(this.numericUpDownChainingInCueOnShotLeftRedZone);
            this.tabPageChainingInCueOnShot.Controls.Add(this.numericUpDownChainingInCueOnShotMaxGap);
            this.tabPageChainingInCueOnShot.Location = new System.Drawing.Point(4, 27);
            this.tabPageChainingInCueOnShot.Name = "tabPageChainingInCueOnShot";
            this.tabPageChainingInCueOnShot.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageChainingInCueOnShot.Size = new System.Drawing.Size(400, 100);
            this.tabPageChainingInCueOnShot.TabIndex = 1;
            this.tabPageChainingInCueOnShot.Text = "In cue on shot change";
            this.tabPageChainingInCueOnShot.UseVisualStyleBackColor = true;
            // 
            // labelChainingInCueOnShotMaxGapSuffix
            // 
            this.labelChainingInCueOnShotMaxGapSuffix.AutoSize = true;
            this.labelChainingInCueOnShotMaxGapSuffix.Location = new System.Drawing.Point(199, 6);
            this.labelChainingInCueOnShotMaxGapSuffix.Name = "labelChainingInCueOnShotMaxGapSuffix";
            this.labelChainingInCueOnShotMaxGapSuffix.Size = new System.Drawing.Size(23, 15);
            this.labelChainingInCueOnShotMaxGapSuffix.TabIndex = 12;
            this.labelChainingInCueOnShotMaxGapSuffix.Text = "ms";
            // 
            // radioButtonChainingInCueOnShotZones
            // 
            this.radioButtonChainingInCueOnShotZones.AutoSize = true;
            this.radioButtonChainingInCueOnShotZones.Location = new System.Drawing.Point(0, 32);
            this.radioButtonChainingInCueOnShotZones.Name = "radioButtonChainingInCueOnShotZones";
            this.radioButtonChainingInCueOnShotZones.Size = new System.Drawing.Size(60, 19);
            this.radioButtonChainingInCueOnShotZones.TabIndex = 13;
            this.radioButtonChainingInCueOnShotZones.Text = "Zones:";
            this.radioButtonChainingInCueOnShotZones.UseVisualStyleBackColor = true;
            this.radioButtonChainingInCueOnShotZones.CheckedChanged += new System.EventHandler(this.radioButtonChaining_CheckedChanged);
            // 
            // radioButtonChainingInCueOnShotMaxGap
            // 
            this.radioButtonChainingInCueOnShotMaxGap.AutoSize = true;
            this.radioButtonChainingInCueOnShotMaxGap.Checked = true;
            this.radioButtonChainingInCueOnShotMaxGap.Location = new System.Drawing.Point(0, 3);
            this.radioButtonChainingInCueOnShotMaxGap.Name = "radioButtonChainingInCueOnShotMaxGap";
            this.radioButtonChainingInCueOnShotMaxGap.Size = new System.Drawing.Size(77, 19);
            this.radioButtonChainingInCueOnShotMaxGap.TabIndex = 10;
            this.radioButtonChainingInCueOnShotMaxGap.TabStop = true;
            this.radioButtonChainingInCueOnShotMaxGap.Text = "Max. gap:";
            this.radioButtonChainingInCueOnShotMaxGap.UseVisualStyleBackColor = true;
            this.radioButtonChainingInCueOnShotMaxGap.CheckedChanged += new System.EventHandler(this.radioButtonChaining_CheckedChanged);
            // 
            // numericUpDownChainingInCueOnShotLeftGreenZone
            // 
            this.numericUpDownChainingInCueOnShotLeftGreenZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.numericUpDownChainingInCueOnShotLeftGreenZone.Enabled = false;
            this.numericUpDownChainingInCueOnShotLeftGreenZone.Location = new System.Drawing.Point(83, 31);
            this.numericUpDownChainingInCueOnShotLeftGreenZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownChainingInCueOnShotLeftGreenZone.Name = "numericUpDownChainingInCueOnShotLeftGreenZone";
            this.numericUpDownChainingInCueOnShotLeftGreenZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownChainingInCueOnShotLeftGreenZone.TabIndex = 14;
            this.numericUpDownChainingInCueOnShotLeftGreenZone.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDownChainingInCueOnShotLeftGreenZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownChainingInCueOnShotLeftRedZone
            // 
            this.numericUpDownChainingInCueOnShotLeftRedZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.numericUpDownChainingInCueOnShotLeftRedZone.Enabled = false;
            this.numericUpDownChainingInCueOnShotLeftRedZone.Location = new System.Drawing.Point(141, 31);
            this.numericUpDownChainingInCueOnShotLeftRedZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownChainingInCueOnShotLeftRedZone.Name = "numericUpDownChainingInCueOnShotLeftRedZone";
            this.numericUpDownChainingInCueOnShotLeftRedZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownChainingInCueOnShotLeftRedZone.TabIndex = 15;
            this.numericUpDownChainingInCueOnShotLeftRedZone.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numericUpDownChainingInCueOnShotLeftRedZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownChainingInCueOnShotMaxGap
            // 
            this.numericUpDownChainingInCueOnShotMaxGap.Location = new System.Drawing.Point(141, 2);
            this.numericUpDownChainingInCueOnShotMaxGap.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDownChainingInCueOnShotMaxGap.Name = "numericUpDownChainingInCueOnShotMaxGap";
            this.numericUpDownChainingInCueOnShotMaxGap.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownChainingInCueOnShotMaxGap.TabIndex = 11;
            this.numericUpDownChainingInCueOnShotMaxGap.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // tabPageChainingOutCueOnShot
            // 
            this.tabPageChainingOutCueOnShot.Controls.Add(this.numericUpDownChainingOutCueOnShotRightGreenZone);
            this.tabPageChainingOutCueOnShot.Controls.Add(this.numericUpDownChainingOutCueOnShotRightRedZone);
            this.tabPageChainingOutCueOnShot.Controls.Add(this.labelChainingOutCueOnShotMaxGapSuffix);
            this.tabPageChainingOutCueOnShot.Controls.Add(this.radioButtonChainingOutCueOnShotZones);
            this.tabPageChainingOutCueOnShot.Controls.Add(this.radioButtonChainingOutCueOnShotMaxGap);
            this.tabPageChainingOutCueOnShot.Controls.Add(this.numericUpDownChainingOutCueOnShotMaxGap);
            this.tabPageChainingOutCueOnShot.Location = new System.Drawing.Point(4, 27);
            this.tabPageChainingOutCueOnShot.Name = "tabPageChainingOutCueOnShot";
            this.tabPageChainingOutCueOnShot.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageChainingOutCueOnShot.Size = new System.Drawing.Size(400, 100);
            this.tabPageChainingOutCueOnShot.TabIndex = 2;
            this.tabPageChainingOutCueOnShot.Text = "Out cue on shot change";
            this.tabPageChainingOutCueOnShot.UseVisualStyleBackColor = true;
            // 
            // numericUpDownChainingOutCueOnShotRightGreenZone
            // 
            this.numericUpDownChainingOutCueOnShotRightGreenZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.numericUpDownChainingOutCueOnShotRightGreenZone.Location = new System.Drawing.Point(266, 31);
            this.numericUpDownChainingOutCueOnShotRightGreenZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownChainingOutCueOnShotRightGreenZone.Name = "numericUpDownChainingOutCueOnShotRightGreenZone";
            this.numericUpDownChainingOutCueOnShotRightGreenZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownChainingOutCueOnShotRightGreenZone.TabIndex = 25;
            this.numericUpDownChainingOutCueOnShotRightGreenZone.Value = new decimal(new int[] {
            13,
            0,
            0,
            0});
            this.numericUpDownChainingOutCueOnShotRightGreenZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDownChainingOutCueOnShotRightRedZone
            // 
            this.numericUpDownChainingOutCueOnShotRightRedZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.numericUpDownChainingOutCueOnShotRightRedZone.Location = new System.Drawing.Point(208, 31);
            this.numericUpDownChainingOutCueOnShotRightRedZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownChainingOutCueOnShotRightRedZone.Name = "numericUpDownChainingOutCueOnShotRightRedZone";
            this.numericUpDownChainingOutCueOnShotRightRedZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownChainingOutCueOnShotRightRedZone.TabIndex = 24;
            this.numericUpDownChainingOutCueOnShotRightRedZone.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.numericUpDownChainingOutCueOnShotRightRedZone.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // labelChainingOutCueOnShotMaxGapSuffix
            // 
            this.labelChainingOutCueOnShotMaxGapSuffix.AutoSize = true;
            this.labelChainingOutCueOnShotMaxGapSuffix.Location = new System.Drawing.Point(266, 6);
            this.labelChainingOutCueOnShotMaxGapSuffix.Name = "labelChainingOutCueOnShotMaxGapSuffix";
            this.labelChainingOutCueOnShotMaxGapSuffix.Size = new System.Drawing.Size(23, 15);
            this.labelChainingOutCueOnShotMaxGapSuffix.TabIndex = 22;
            this.labelChainingOutCueOnShotMaxGapSuffix.Text = "ms";
            // 
            // radioButtonChainingOutCueOnShotZones
            // 
            this.radioButtonChainingOutCueOnShotZones.AutoSize = true;
            this.radioButtonChainingOutCueOnShotZones.Location = new System.Drawing.Point(0, 32);
            this.radioButtonChainingOutCueOnShotZones.Name = "radioButtonChainingOutCueOnShotZones";
            this.radioButtonChainingOutCueOnShotZones.Size = new System.Drawing.Size(60, 19);
            this.radioButtonChainingOutCueOnShotZones.TabIndex = 23;
            this.radioButtonChainingOutCueOnShotZones.Text = "Zones:";
            this.radioButtonChainingOutCueOnShotZones.UseVisualStyleBackColor = true;
            this.radioButtonChainingOutCueOnShotZones.CheckedChanged += new System.EventHandler(this.radioButtonChaining_CheckedChanged);
            // 
            // radioButtonChainingOutCueOnShotMaxGap
            // 
            this.radioButtonChainingOutCueOnShotMaxGap.AutoSize = true;
            this.radioButtonChainingOutCueOnShotMaxGap.Checked = true;
            this.radioButtonChainingOutCueOnShotMaxGap.Location = new System.Drawing.Point(0, 3);
            this.radioButtonChainingOutCueOnShotMaxGap.Name = "radioButtonChainingOutCueOnShotMaxGap";
            this.radioButtonChainingOutCueOnShotMaxGap.Size = new System.Drawing.Size(77, 19);
            this.radioButtonChainingOutCueOnShotMaxGap.TabIndex = 20;
            this.radioButtonChainingOutCueOnShotMaxGap.TabStop = true;
            this.radioButtonChainingOutCueOnShotMaxGap.Text = "Max. gap:";
            this.radioButtonChainingOutCueOnShotMaxGap.UseVisualStyleBackColor = true;
            this.radioButtonChainingOutCueOnShotMaxGap.CheckedChanged += new System.EventHandler(this.radioButtonChaining_CheckedChanged);
            // 
            // numericUpDownChainingOutCueOnShotMaxGap
            // 
            this.numericUpDownChainingOutCueOnShotMaxGap.Location = new System.Drawing.Point(208, 2);
            this.numericUpDownChainingOutCueOnShotMaxGap.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDownChainingOutCueOnShotMaxGap.Name = "numericUpDownChainingOutCueOnShotMaxGap";
            this.numericUpDownChainingOutCueOnShotMaxGap.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownChainingOutCueOnShotMaxGap.TabIndex = 21;
            this.numericUpDownChainingOutCueOnShotMaxGap.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // buttonCreateSimple
            // 
            this.buttonCreateSimple.Location = new System.Drawing.Point(11, 505);
            this.buttonCreateSimple.Name = "buttonCreateSimple";
            this.buttonCreateSimple.Size = new System.Drawing.Size(178, 23);
            this.buttonCreateSimple.TabIndex = 102;
            this.buttonCreateSimple.Text = "Simple mode...";
            this.buttonCreateSimple.UseVisualStyleBackColor = true;
            this.buttonCreateSimple.Click += new System.EventHandler(this.buttonCreateSimple_Click);
            // 
            // BeautifyTimeCodesProfile
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(870, 540);
            this.Controls.Add(this.buttonCreateSimple);
            this.Controls.Add(this.groupBoxChaining);
            this.Controls.Add(this.groupBoxConnectedSubtitles);
            this.Controls.Add(this.groupBoxOutCues);
            this.Controls.Add(this.groupBoxInCues);
            this.Controls.Add(this.groupBoxGeneral);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.menuStrip);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BeautifyTimeCodesProfile";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BeautifyTimeCodesProfile";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.groupBoxGeneral.ResumeLayout(false);
            this.groupBoxGeneral.PerformLayout();
            this.groupBoxInCues.ResumeLayout(false);
            this.groupBoxInCues.PerformLayout();
            this.groupBoxOutCues.ResumeLayout(false);
            this.groupBoxOutCues.PerformLayout();
            this.groupBoxConnectedSubtitles.ResumeLayout(false);
            this.groupBoxConnectedSubtitles.PerformLayout();
            this.tabControlConnectedSubtitles.ResumeLayout(false);
            this.tabPageConnectedSubtitlesInCueClosest.ResumeLayout(false);
            this.tabPageConnectedSubtitlesInCueClosest.PerformLayout();
            this.tabPageConnectedSubtitlesOutCueClosest.ResumeLayout(false);
            this.tabPageConnectedSubtitlesOutCueClosest.PerformLayout();
            this.groupBoxChaining.ResumeLayout(false);
            this.tabControlChaining.ResumeLayout(false);
            this.tabPageChainingGeneral.ResumeLayout(false);
            this.tabPageChainingGeneral.PerformLayout();
            this.tabPageChainingInCueOnShot.ResumeLayout(false);
            this.tabPageChainingInCueOnShot.PerformLayout();
            this.tabPageChainingOutCueOnShot.ResumeLayout(false);
            this.tabPageChainingOutCueOnShot.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLoadPreset;
        private System.Windows.Forms.GroupBox groupBoxGeneral;
        private System.Windows.Forms.GroupBox groupBoxInCues;
        private Controls.CuesPreviewView cuesPreviewViewInCues;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownInCuesGap;
        private System.Windows.Forms.Label labelInCuesGap;
        private System.Windows.Forms.Label labelGapSuffix;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownGap;
        private System.Windows.Forms.Label labelGap;
        private System.Windows.Forms.Label labelInCuesZones;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownInCuesLeftGreenZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownInCuesLeftRedZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownInCuesRightGreenZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownInCuesRightRedZone;
        private System.Windows.Forms.GroupBox groupBoxOutCues;
        private System.Windows.Forms.Label labelOutCuesZones;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownOutCuesLeftGreenZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownOutCuesLeftRedZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownOutCuesGap;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownOutCuesRightGreenZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownOutCuesRightRedZone;
        private System.Windows.Forms.Label labelOutCuesGap;
        private Controls.CuesPreviewView cuesPreviewViewOutCues;
        private System.Windows.Forms.GroupBox groupBoxConnectedSubtitles;
        private System.Windows.Forms.Label labelConnectedSubtitlesTreatConnectedSuffix;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownConnectedSubtitlesTreatConnected;
        private System.Windows.Forms.Label labelConnectedSubtitlesTreatConnected;
        private System.Windows.Forms.Label labelConnectedSubtitlesZones;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownConnectedSubtitlesLeftGreenZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownConnectedSubtitlesLeftRedZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownConnectedSubtitlesRightGreenZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownConnectedSubtitlesRightRedZone;
        private Controls.CuesPreviewView cuesPreviewViewConnectedSubtitlesInCueClosest;
        private System.Windows.Forms.GroupBox groupBoxChaining;
        private System.Windows.Forms.TabControl tabControlChaining;
        private System.Windows.Forms.TabPage tabPageChainingGeneral;
        private System.Windows.Forms.TabPage tabPageChainingInCueOnShot;
        private Controls.CuesPreviewView cuesPreviewViewChainingGeneral;
        private System.Windows.Forms.RadioButton radioButtonChainingGeneralZones;
        private System.Windows.Forms.RadioButton radioButtonChainingGeneralMaxGap;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownChainingGeneralLeftGreenZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownChainingGeneralLeftRedZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownChainingGeneralMaxGap;
        private System.Windows.Forms.TabPage tabPageChainingOutCueOnShot;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelChainingGeneralMaxGapSuffix;
        private System.Windows.Forms.Label labelChainingGeneralShotChangeBehavior;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxChainingGeneralShotChangeBehavior;
        private Controls.CuesPreviewView cuesPreviewViewChainingOutCueOnShot;
        private Controls.CuesPreviewView cuesPreviewViewChainingInCueOnShot;
        private System.Windows.Forms.Label labelChainingInCueOnShotMaxGapSuffix;
        private System.Windows.Forms.RadioButton radioButtonChainingInCueOnShotZones;
        private System.Windows.Forms.RadioButton radioButtonChainingInCueOnShotMaxGap;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownChainingInCueOnShotLeftGreenZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownChainingInCueOnShotLeftRedZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownChainingInCueOnShotMaxGap;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownChainingOutCueOnShotRightGreenZone;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownChainingOutCueOnShotRightRedZone;
        private System.Windows.Forms.Label labelChainingOutCueOnShotMaxGapSuffix;
        private System.Windows.Forms.RadioButton radioButtonChainingOutCueOnShotZones;
        private System.Windows.Forms.RadioButton radioButtonChainingOutCueOnShotMaxGap;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownChainingOutCueOnShotMaxGap;
        private System.Windows.Forms.TabControl tabControlConnectedSubtitles;
        private System.Windows.Forms.TabPage tabPageConnectedSubtitlesInCueClosest;
        private System.Windows.Forms.TabPage tabPageConnectedSubtitlesOutCueClosest;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownConnectedSubtitlesInCueClosestLeftGap;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownConnectedSubtitlesInCueClosestRightGap;
        private System.Windows.Forms.Label labelConnectedSubtitlesInCueClosestGaps;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownConnectedSubtitlesOutCueClosestLeftGap;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownConnectedSubtitlesOutCueClosestRightGap;
        private System.Windows.Forms.Label labelConnectedSubtitlesOutCueClosestGaps;
        private Controls.CuesPreviewView cuesPreviewViewConnectedSubtitlesOutCueClosest;
        private System.Windows.Forms.Button buttonCreateSimple;
    }
}