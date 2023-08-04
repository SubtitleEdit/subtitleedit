namespace Nikse.SubtitleEdit.Forms.BeautifyTimeCodes
{
    sealed partial class BeautifyTimeCodesProfileSimple
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BeautifyTimeCodesProfileSimple));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelInstructions = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxSnapClosestCue = new System.Windows.Forms.CheckBox();
            this.pictureBoxChainingInfo = new System.Windows.Forms.PictureBox();
            this.labelChainingGapAfterShotChangesPrefix = new System.Windows.Forms.Label();
            this.numericUpDownChainingGapAfterShotChanges = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelChainingGapAfterShotChangesSuffix = new System.Windows.Forms.Label();
            this.checkBoxChainingGapAfterShotChanges = new System.Windows.Forms.CheckBox();
            this.numericUpDownChainingGap = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelChainingGapInstruction = new System.Windows.Forms.Label();
            this.labelChainingGapSuffix = new System.Windows.Forms.Label();
            this.labelChainingGap = new System.Windows.Forms.Label();
            this.labelOffsetInstruction = new System.Windows.Forms.Label();
            this.labelOffsetSuffix = new System.Windows.Forms.Label();
            this.numericUpDownOffset = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelOffset = new System.Windows.Forms.Label();
            this.labelSafeZoneInstruction = new System.Windows.Forms.Label();
            this.labelSafeZoneSuffix = new System.Windows.Forms.Label();
            this.numericUpDownSafeZone = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.comboBoxOutCues = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxInCues = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelOutCues = new System.Windows.Forms.Label();
            this.labelInCues = new System.Windows.Forms.Label();
            this.labelSafeZone = new System.Windows.Forms.Label();
            this.labelGapInstruction = new System.Windows.Forms.Label();
            this.labelGapHint = new System.Windows.Forms.Label();
            this.labelGapSuffix = new System.Windows.Forms.Label();
            this.numericUpDownGap = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelGap = new System.Windows.Forms.Label();
            this.toolTipChaining = new System.Windows.Forms.ToolTip(this.components);
            this.buttonLoadNetflixRules = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChainingInfo)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(463, 527);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 101;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(382, 527);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 100;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelInstructions
            // 
            this.labelInstructions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInstructions.AutoEllipsis = true;
            this.labelInstructions.Location = new System.Drawing.Point(9, 9);
            this.labelInstructions.Name = "labelInstructions";
            this.labelInstructions.Size = new System.Drawing.Size(528, 15);
            this.labelInstructions.TabIndex = 1;
            this.labelInstructions.Text = "Enter these basic rules, and the current profile will be updated accordingly.";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.labelChainingGapAfterShotChangesPrefix);
            this.groupBox1.Controls.Add(this.numericUpDownChainingGapAfterShotChanges);
            this.groupBox1.Controls.Add(this.labelChainingGapAfterShotChangesSuffix);
            this.groupBox1.Controls.Add(this.pictureBoxChainingInfo);
            this.groupBox1.Controls.Add(this.checkBoxSnapClosestCue);
            this.groupBox1.Controls.Add(this.checkBoxChainingGapAfterShotChanges);
            this.groupBox1.Controls.Add(this.numericUpDownChainingGap);
            this.groupBox1.Controls.Add(this.labelChainingGapInstruction);
            this.groupBox1.Controls.Add(this.labelChainingGapSuffix);
            this.groupBox1.Controls.Add(this.labelChainingGap);
            this.groupBox1.Controls.Add(this.labelOffsetInstruction);
            this.groupBox1.Controls.Add(this.labelOffsetSuffix);
            this.groupBox1.Controls.Add(this.numericUpDownOffset);
            this.groupBox1.Controls.Add(this.labelOffset);
            this.groupBox1.Controls.Add(this.labelSafeZoneInstruction);
            this.groupBox1.Controls.Add(this.labelSafeZoneSuffix);
            this.groupBox1.Controls.Add(this.numericUpDownSafeZone);
            this.groupBox1.Controls.Add(this.comboBoxOutCues);
            this.groupBox1.Controls.Add(this.comboBoxInCues);
            this.groupBox1.Controls.Add(this.labelOutCues);
            this.groupBox1.Controls.Add(this.labelInCues);
            this.groupBox1.Controls.Add(this.labelSafeZone);
            this.groupBox1.Controls.Add(this.labelGapInstruction);
            this.groupBox1.Controls.Add(this.labelGapHint);
            this.groupBox1.Controls.Add(this.labelGapSuffix);
            this.groupBox1.Controls.Add(this.numericUpDownGap);
            this.groupBox1.Controls.Add(this.labelGap);
            this.groupBox1.Location = new System.Drawing.Point(12, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(525, 490);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            // 
            // checkBoxSnapClosestCue
            // 
            this.checkBoxSnapClosestCue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxSnapClosestCue.AutoEllipsis = true;
            this.checkBoxSnapClosestCue.Checked = true;
            this.checkBoxSnapClosestCue.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSnapClosestCue.Location = new System.Drawing.Point(134, 150);
            this.checkBoxSnapClosestCue.Name = "checkBoxSnapClosestCue";
            this.checkBoxSnapClosestCue.Size = new System.Drawing.Size(377, 49);
            this.checkBoxSnapClosestCue.TabIndex = 10;
            this.checkBoxSnapClosestCue.Text = "For connected subtitles, snap the in or out cue to the shot change based on which" +
    " one is closer";
            this.checkBoxSnapClosestCue.UseVisualStyleBackColor = true;
            // 
            // pictureBoxChainingInfo
            // 
            this.pictureBoxChainingInfo.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxChainingInfo.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxChainingInfo.Image")));
            this.pictureBoxChainingInfo.Location = new System.Drawing.Point(15, 381);
            this.pictureBoxChainingInfo.Name = "pictureBoxChainingInfo";
            this.pictureBoxChainingInfo.Size = new System.Drawing.Size(16, 16);
            this.pictureBoxChainingInfo.TabIndex = 31;
            this.pictureBoxChainingInfo.TabStop = false;
            // 
            // labelChainingGapAfterShotChangesPrefix
            // 
            this.labelChainingGapAfterShotChangesPrefix.AutoSize = true;
            this.labelChainingGapAfterShotChangesPrefix.BackColor = System.Drawing.Color.Transparent;
            this.labelChainingGapAfterShotChangesPrefix.Location = new System.Drawing.Point(150, 448);
            this.labelChainingGapAfterShotChangesPrefix.Name = "labelChainingGapAfterShotChangesPrefix";
            this.labelChainingGapAfterShotChangesPrefix.Size = new System.Drawing.Size(33, 15);
            this.labelChainingGapAfterShotChangesPrefix.TabIndex = 24;
            this.labelChainingGapAfterShotChangesPrefix.Text = "Max.";
            // 
            // numericUpDownChainingGapAfterShotChanges
            // 
            this.numericUpDownChainingGapAfterShotChanges.Location = new System.Drawing.Point(193, 445);
            this.numericUpDownChainingGapAfterShotChanges.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDownChainingGapAfterShotChanges.Name = "numericUpDownChainingGapAfterShotChanges";
            this.numericUpDownChainingGapAfterShotChanges.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownChainingGapAfterShotChanges.TabIndex = 25;
            this.numericUpDownChainingGapAfterShotChanges.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // labelChainingGapAfterShotChangesSuffix
            // 
            this.labelChainingGapAfterShotChangesSuffix.AutoSize = true;
            this.labelChainingGapAfterShotChangesSuffix.BackColor = System.Drawing.Color.Transparent;
            this.labelChainingGapAfterShotChangesSuffix.Location = new System.Drawing.Point(251, 448);
            this.labelChainingGapAfterShotChangesSuffix.Name = "labelChainingGapAfterShotChangesSuffix";
            this.labelChainingGapAfterShotChangesSuffix.Size = new System.Drawing.Size(23, 15);
            this.labelChainingGapAfterShotChangesSuffix.TabIndex = 26;
            this.labelChainingGapAfterShotChangesSuffix.Text = "ms";
            // 
            // checkBoxChainingGapAfterShotChanges
            // 
            this.checkBoxChainingGapAfterShotChanges.AutoEllipsis = true;
            this.checkBoxChainingGapAfterShotChanges.Checked = true;
            this.checkBoxChainingGapAfterShotChanges.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxChainingGapAfterShotChanges.Location = new System.Drawing.Point(134, 414);
            this.checkBoxChainingGapAfterShotChanges.Name = "checkBoxChainingGapAfterShotChanges";
            this.checkBoxChainingGapAfterShotChanges.Size = new System.Drawing.Size(377, 35);
            this.checkBoxChainingGapAfterShotChanges.TabIndex = 23;
            this.checkBoxChainingGapAfterShotChanges.Text = "After an out cue on a shot change, the gap may be smaller";
            this.checkBoxChainingGapAfterShotChanges.UseVisualStyleBackColor = true;
            this.checkBoxChainingGapAfterShotChanges.CheckedChanged += new System.EventHandler(this.checkBoxChainingGapAfterShotChanges_CheckedChanged);
            // 
            // numericUpDownChainingGap
            // 
            this.numericUpDownChainingGap.Location = new System.Drawing.Point(134, 352);
            this.numericUpDownChainingGap.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDownChainingGap.Name = "numericUpDownChainingGap";
            this.numericUpDownChainingGap.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownChainingGap.TabIndex = 20;
            this.numericUpDownChainingGap.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // labelChainingGapInstruction
            // 
            this.labelChainingGapInstruction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelChainingGapInstruction.AutoEllipsis = true;
            this.labelChainingGapInstruction.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelChainingGapInstruction.Location = new System.Drawing.Point(131, 381);
            this.labelChainingGapInstruction.Name = "labelChainingGapInstruction";
            this.labelChainingGapInstruction.Size = new System.Drawing.Size(380, 30);
            this.labelChainingGapInstruction.TabIndex = 22;
            this.labelChainingGapInstruction.Text = "If the space between two subtitles is smaller than this amount, the subtitles wil" +
    "l be connected.";
            // 
            // labelChainingGapSuffix
            // 
            this.labelChainingGapSuffix.AutoSize = true;
            this.labelChainingGapSuffix.Location = new System.Drawing.Point(192, 355);
            this.labelChainingGapSuffix.Name = "labelChainingGapSuffix";
            this.labelChainingGapSuffix.Size = new System.Drawing.Size(23, 15);
            this.labelChainingGapSuffix.TabIndex = 21;
            this.labelChainingGapSuffix.Text = "ms";
            // 
            // labelChainingGap
            // 
            this.labelChainingGap.AutoEllipsis = true;
            this.labelChainingGap.Location = new System.Drawing.Point(12, 347);
            this.labelChainingGap.Name = "labelChainingGap";
            this.labelChainingGap.Size = new System.Drawing.Size(116, 31);
            this.labelChainingGap.TabIndex = 19;
            this.labelChainingGap.Text = "Max. chaining gap:";
            this.labelChainingGap.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelOffsetInstruction
            // 
            this.labelOffsetInstruction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelOffsetInstruction.AutoEllipsis = true;
            this.labelOffsetInstruction.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelOffsetInstruction.Location = new System.Drawing.Point(131, 237);
            this.labelOffsetInstruction.Name = "labelOffsetInstruction";
            this.labelOffsetInstruction.Size = new System.Drawing.Size(380, 30);
            this.labelOffsetInstruction.TabIndex = 14;
            this.labelOffsetInstruction.Text = "Cues within this distance from shot changes will be considered as being on the sh" +
    "ot change.";
            // 
            // labelOffsetSuffix
            // 
            this.labelOffsetSuffix.AutoSize = true;
            this.labelOffsetSuffix.Location = new System.Drawing.Point(192, 211);
            this.labelOffsetSuffix.Name = "labelOffsetSuffix";
            this.labelOffsetSuffix.Size = new System.Drawing.Size(43, 15);
            this.labelOffsetSuffix.TabIndex = 13;
            this.labelOffsetSuffix.Text = "frames";
            // 
            // numericUpDownOffset
            // 
            this.numericUpDownOffset.Location = new System.Drawing.Point(134, 208);
            this.numericUpDownOffset.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownOffset.Name = "numericUpDownOffset";
            this.numericUpDownOffset.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownOffset.TabIndex = 12;
            this.numericUpDownOffset.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // labelOffset
            // 
            this.labelOffset.AutoEllipsis = true;
            this.labelOffset.Location = new System.Drawing.Point(12, 203);
            this.labelOffset.Name = "labelOffset";
            this.labelOffset.Size = new System.Drawing.Size(116, 31);
            this.labelOffset.TabIndex = 11;
            this.labelOffset.Text = "Max. offset:";
            this.labelOffset.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelSafeZoneInstruction
            // 
            this.labelSafeZoneInstruction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSafeZoneInstruction.AutoEllipsis = true;
            this.labelSafeZoneInstruction.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelSafeZoneInstruction.Location = new System.Drawing.Point(131, 309);
            this.labelSafeZoneInstruction.Name = "labelSafeZoneInstruction";
            this.labelSafeZoneInstruction.Size = new System.Drawing.Size(380, 30);
            this.labelSafeZoneInstruction.TabIndex = 18;
            this.labelSafeZoneInstruction.Text = "The amount of frames around shot changes where no cues are allowed.";
            // 
            // labelSafeZoneSuffix
            // 
            this.labelSafeZoneSuffix.AutoSize = true;
            this.labelSafeZoneSuffix.Location = new System.Drawing.Point(192, 283);
            this.labelSafeZoneSuffix.Name = "labelSafeZoneSuffix";
            this.labelSafeZoneSuffix.Size = new System.Drawing.Size(43, 15);
            this.labelSafeZoneSuffix.TabIndex = 17;
            this.labelSafeZoneSuffix.Text = "frames";
            // 
            // numericUpDownSafeZone
            // 
            this.numericUpDownSafeZone.Location = new System.Drawing.Point(134, 280);
            this.numericUpDownSafeZone.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownSafeZone.Name = "numericUpDownSafeZone";
            this.numericUpDownSafeZone.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownSafeZone.TabIndex = 16;
            this.numericUpDownSafeZone.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            // 
            // comboBoxOutCues
            // 
            this.comboBoxOutCues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxOutCues.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOutCues.FormattingEnabled = true;
            this.comboBoxOutCues.Items.AddRange(new object[] {
            "On the shot change",
            "1 frame before the shot change",
            "2 frames before the shot change",
            "3 frames before the shot change",
            "The same amount of frames as the gap before the shot change"});
            this.comboBoxOutCues.Location = new System.Drawing.Point(134, 118);
            this.comboBoxOutCues.Name = "comboBoxOutCues";
            this.comboBoxOutCues.Size = new System.Drawing.Size(377, 23);
            this.comboBoxOutCues.TabIndex = 9;
            this.comboBoxOutCues.SelectedIndexChanged += new System.EventHandler(this.comboBoxInCues_SelectedIndexChanged);
            // 
            // comboBoxInCues
            // 
            this.comboBoxInCues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxInCues.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxInCues.FormattingEnabled = true;
            this.comboBoxInCues.Items.AddRange(new object[] {
            "On the shot change",
            "1 frame after the shot change",
            "2 frames after the shot change",
            "3 frames after the shot change"});
            this.comboBoxInCues.Location = new System.Drawing.Point(134, 79);
            this.comboBoxInCues.Name = "comboBoxInCues";
            this.comboBoxInCues.Size = new System.Drawing.Size(377, 23);
            this.comboBoxInCues.TabIndex = 7;
            this.comboBoxInCues.SelectedIndexChanged += new System.EventHandler(this.comboBoxInCues_SelectedIndexChanged);
            // 
            // labelOutCues
            // 
            this.labelOutCues.AutoEllipsis = true;
            this.labelOutCues.Location = new System.Drawing.Point(12, 113);
            this.labelOutCues.Name = "labelOutCues";
            this.labelOutCues.Size = new System.Drawing.Size(116, 31);
            this.labelOutCues.TabIndex = 8;
            this.labelOutCues.Text = "Out cues should be:";
            this.labelOutCues.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelInCues
            // 
            this.labelInCues.AutoEllipsis = true;
            this.labelInCues.Location = new System.Drawing.Point(12, 74);
            this.labelInCues.Name = "labelInCues";
            this.labelInCues.Size = new System.Drawing.Size(116, 31);
            this.labelInCues.TabIndex = 6;
            this.labelInCues.Text = "In cues should be:";
            this.labelInCues.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelSafeZone
            // 
            this.labelSafeZone.AutoEllipsis = true;
            this.labelSafeZone.Location = new System.Drawing.Point(12, 275);
            this.labelSafeZone.Name = "labelSafeZone";
            this.labelSafeZone.Size = new System.Drawing.Size(116, 31);
            this.labelSafeZone.TabIndex = 15;
            this.labelSafeZone.Text = "Safe zone:";
            this.labelSafeZone.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelGapInstruction
            // 
            this.labelGapInstruction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelGapInstruction.AutoEllipsis = true;
            this.labelGapInstruction.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelGapInstruction.Location = new System.Drawing.Point(131, 51);
            this.labelGapInstruction.Name = "labelGapInstruction";
            this.labelGapInstruction.Size = new System.Drawing.Size(380, 15);
            this.labelGapInstruction.TabIndex = 5;
            this.labelGapInstruction.Text = "The minimum amount of space between subtitles.";
            // 
            // labelGapHint
            // 
            this.labelGapHint.AutoSize = true;
            this.labelGapHint.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.labelGapHint.Location = new System.Drawing.Point(250, 25);
            this.labelGapHint.Name = "labelGapHint";
            this.labelGapHint.Size = new System.Drawing.Size(95, 15);
            this.labelGapHint.TabIndex = 4;
            this.labelGapHint.Text = "120 ms @ 25 FPS";
            // 
            // labelGapSuffix
            // 
            this.labelGapSuffix.AutoSize = true;
            this.labelGapSuffix.Location = new System.Drawing.Point(192, 25);
            this.labelGapSuffix.Name = "labelGapSuffix";
            this.labelGapSuffix.Size = new System.Drawing.Size(43, 15);
            this.labelGapSuffix.TabIndex = 3;
            this.labelGapSuffix.Text = "frames";
            // 
            // numericUpDownGap
            // 
            this.numericUpDownGap.Location = new System.Drawing.Point(134, 22);
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
            this.labelGap.AutoEllipsis = true;
            this.labelGap.Location = new System.Drawing.Point(12, 17);
            this.labelGap.Name = "labelGap";
            this.labelGap.Size = new System.Drawing.Size(116, 31);
            this.labelGap.TabIndex = 1;
            this.labelGap.Text = "Gap:";
            this.labelGap.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolTipChaining
            // 
            this.toolTipChaining.IsBalloon = true;
            this.toolTipChaining.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // buttonLoadNetflixRules
            // 
            this.buttonLoadNetflixRules.Location = new System.Drawing.Point(11, 527);
            this.buttonLoadNetflixRules.Name = "buttonLoadNetflixRules";
            this.buttonLoadNetflixRules.Size = new System.Drawing.Size(169, 23);
            this.buttonLoadNetflixRules.TabIndex = 102;
            this.buttonLoadNetflixRules.Text = "Load Netflix rules";
            this.buttonLoadNetflixRules.UseVisualStyleBackColor = true;
            this.buttonLoadNetflixRules.Click += new System.EventHandler(this.buttonLoadNetflixRules_Click);
            // 
            // BeautifyTimeCodesProfileSimple
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(549, 562);
            this.Controls.Add(this.buttonLoadNetflixRules);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.labelInstructions);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BeautifyTimeCodesProfileSimple";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BeautifyTimeCodesProfileSimple";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChainingInfo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelInstructions;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelGap;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownGap;
        private System.Windows.Forms.Label labelGapHint;
        private System.Windows.Forms.Label labelGapSuffix;
        private System.Windows.Forms.Label labelGapInstruction;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxOutCues;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxInCues;
        private System.Windows.Forms.Label labelOutCues;
        private System.Windows.Forms.Label labelInCues;
        private System.Windows.Forms.Label labelSafeZone;
        private System.Windows.Forms.Label labelOffsetInstruction;
        private System.Windows.Forms.Label labelOffsetSuffix;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownOffset;
        private System.Windows.Forms.Label labelOffset;
        private System.Windows.Forms.Label labelSafeZoneInstruction;
        private System.Windows.Forms.Label labelSafeZoneSuffix;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownSafeZone;
        private System.Windows.Forms.Label labelChainingGapInstruction;
        private System.Windows.Forms.Label labelChainingGapSuffix;
        private System.Windows.Forms.Label labelChainingGap;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownChainingGap;
        private System.Windows.Forms.CheckBox checkBoxChainingGapAfterShotChanges;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownChainingGapAfterShotChanges;
        private System.Windows.Forms.Label labelChainingGapAfterShotChangesSuffix;
        private System.Windows.Forms.Label labelChainingGapAfterShotChangesPrefix;
        private System.Windows.Forms.PictureBox pictureBoxChainingInfo;
        private System.Windows.Forms.ToolTip toolTipChaining;
        private System.Windows.Forms.CheckBox checkBoxSnapClosestCue;
        private System.Windows.Forms.Button buttonLoadNetflixRules;
    }
}