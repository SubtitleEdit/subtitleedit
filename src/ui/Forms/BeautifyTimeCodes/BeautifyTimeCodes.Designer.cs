﻿
namespace Nikse.SubtitleEdit.Forms.BeautifyTimeCodes
{
    partial class BeautifyTimeCodes
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxTimeCodes = new System.Windows.Forms.GroupBox();
            this.checkBoxAlignTimeCodes = new System.Windows.Forms.CheckBox();
            this.groupBoxShotChanges = new System.Windows.Forms.GroupBox();
            this.checkBoxSnapToShotChanges = new System.Windows.Forms.CheckBox();
            this.panelShotChanges = new System.Windows.Forms.Panel();
            this.buttonImportShotChanges = new System.Windows.Forms.Button();
            this.labelShotChangesStatus = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.buttonEditProfile = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.checkBoxExtractExactTimeCodes = new System.Windows.Forms.CheckBox();
            this.panelTimeCodes = new System.Windows.Forms.Panel();
            this.buttonCancelTimeCodes = new System.Windows.Forms.Button();
            this.labelTimeCodesStatus = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.buttonExtractTimeCodes = new System.Windows.Forms.Button();
            this.progressBarExtractTimeCodes = new System.Windows.Forms.ProgressBar();
            this.labelExtractTimeCodesProgress = new System.Windows.Forms.Label();
            this.panelExtractTimeCodes = new System.Windows.Forms.Panel();
            this.groupBoxTimeCodes.SuspendLayout();
            this.groupBoxShotChanges.SuspendLayout();
            this.panelShotChanges.SuspendLayout();
            this.panelTimeCodes.SuspendLayout();
            this.panelExtractTimeCodes.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(415, 362);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 30;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(496, 362);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 31;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // groupBoxTimeCodes
            // 
            this.groupBoxTimeCodes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTimeCodes.Controls.Add(this.checkBoxAlignTimeCodes);
            this.groupBoxTimeCodes.Controls.Add(this.panelTimeCodes);
            this.groupBoxTimeCodes.Location = new System.Drawing.Point(12, 12);
            this.groupBoxTimeCodes.Name = "groupBoxTimeCodes";
            this.groupBoxTimeCodes.Size = new System.Drawing.Size(558, 157);
            this.groupBoxTimeCodes.TabIndex = 1;
            this.groupBoxTimeCodes.TabStop = false;
            this.groupBoxTimeCodes.Text = "Time codes";
            // 
            // checkBoxAlignTimeCodes
            // 
            this.checkBoxAlignTimeCodes.AutoSize = true;
            this.checkBoxAlignTimeCodes.Checked = true;
            this.checkBoxAlignTimeCodes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAlignTimeCodes.Enabled = false;
            this.checkBoxAlignTimeCodes.Location = new System.Drawing.Point(10, 22);
            this.checkBoxAlignTimeCodes.Name = "checkBoxAlignTimeCodes";
            this.checkBoxAlignTimeCodes.Size = new System.Drawing.Size(224, 19);
            this.checkBoxAlignTimeCodes.TabIndex = 2;
            this.checkBoxAlignTimeCodes.Text = "Align time codes to frame time codes";
            this.checkBoxAlignTimeCodes.UseVisualStyleBackColor = true;
            this.checkBoxAlignTimeCodes.CheckedChanged += new System.EventHandler(this.checkBoxAlignTimeCodes_CheckedChanged);
            // 
            // groupBoxShotChanges
            // 
            this.groupBoxShotChanges.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxShotChanges.Controls.Add(this.checkBoxSnapToShotChanges);
            this.groupBoxShotChanges.Controls.Add(this.panelShotChanges);
            this.groupBoxShotChanges.Location = new System.Drawing.Point(12, 175);
            this.groupBoxShotChanges.Name = "groupBoxShotChanges";
            this.groupBoxShotChanges.Size = new System.Drawing.Size(558, 103);
            this.groupBoxShotChanges.TabIndex = 10;
            this.groupBoxShotChanges.TabStop = false;
            this.groupBoxShotChanges.Text = "Shot changes";
            // 
            // checkBoxSnapToShotChanges
            // 
            this.checkBoxSnapToShotChanges.AutoSize = true;
            this.checkBoxSnapToShotChanges.Checked = true;
            this.checkBoxSnapToShotChanges.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSnapToShotChanges.Location = new System.Drawing.Point(10, 22);
            this.checkBoxSnapToShotChanges.Name = "checkBoxSnapToShotChanges";
            this.checkBoxSnapToShotChanges.Size = new System.Drawing.Size(166, 19);
            this.checkBoxSnapToShotChanges.TabIndex = 11;
            this.checkBoxSnapToShotChanges.Text = "Snap cues to shot changes";
            this.checkBoxSnapToShotChanges.UseVisualStyleBackColor = true;
            this.checkBoxSnapToShotChanges.CheckedChanged += new System.EventHandler(this.checkBoxSnapToShotChanges_CheckedChanged);
            // 
            // panelShotChanges
            // 
            this.panelShotChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelShotChanges.Controls.Add(this.buttonImportShotChanges);
            this.panelShotChanges.Controls.Add(this.labelShotChangesStatus);
            this.panelShotChanges.Location = new System.Drawing.Point(6, 47);
            this.panelShotChanges.Name = "panelShotChanges";
            this.panelShotChanges.Size = new System.Drawing.Size(546, 50);
            this.panelShotChanges.TabIndex = 2;
            // 
            // buttonImportShotChanges
            // 
            this.buttonImportShotChanges.Location = new System.Drawing.Point(23, 20);
            this.buttonImportShotChanges.Name = "buttonImportShotChanges";
            this.buttonImportShotChanges.Size = new System.Drawing.Size(221, 23);
            this.buttonImportShotChanges.TabIndex = 12;
            this.buttonImportShotChanges.Text = "Generate / import shot changes...";
            this.buttonImportShotChanges.UseVisualStyleBackColor = true;
            this.buttonImportShotChanges.Click += new System.EventHandler(this.buttonImportShotChanges_Click);
            // 
            // labelShotChangesStatus
            // 
            this.labelShotChangesStatus.AutoSize = true;
            this.labelShotChangesStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelShotChangesStatus.Location = new System.Drawing.Point(20, 1);
            this.labelShotChangesStatus.Name = "labelShotChangesStatus";
            this.labelShotChangesStatus.Size = new System.Drawing.Size(143, 15);
            this.labelShotChangesStatus.TabIndex = 1;
            this.labelShotChangesStatus.Text = "123 shot changes loaded";
            // 
            // buttonEditProfile
            // 
            this.buttonEditProfile.Location = new System.Drawing.Point(11, 288);
            this.buttonEditProfile.Name = "buttonEditProfile";
            this.buttonEditProfile.Size = new System.Drawing.Size(178, 28);
            this.buttonEditProfile.TabIndex = 20;
            this.buttonEditProfile.Text = "Edit profile...";
            this.buttonEditProfile.UseVisualStyleBackColor = true;
            this.buttonEditProfile.Click += new System.EventHandler(this.buttonEditProfile_Click);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 327);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(558, 23);
            this.progressBar.TabIndex = 21;
            // 
            // checkBoxExtractExactTimeCodes
            // 
            this.checkBoxExtractExactTimeCodes.AutoSize = true;
            this.checkBoxExtractExactTimeCodes.Checked = true;
            this.checkBoxExtractExactTimeCodes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxExtractExactTimeCodes.Location = new System.Drawing.Point(4, 2);
            this.checkBoxExtractExactTimeCodes.Name = "checkBoxExtractExactTimeCodes";
            this.checkBoxExtractExactTimeCodes.Size = new System.Drawing.Size(266, 19);
            this.checkBoxExtractExactTimeCodes.TabIndex = 3;
            this.checkBoxExtractExactTimeCodes.Text = "Use ffprobe to extract exact frame time codes";
            this.checkBoxExtractExactTimeCodes.UseVisualStyleBackColor = true;
            this.checkBoxExtractExactTimeCodes.CheckedChanged += new System.EventHandler(this.checkBoxExtractExactTimeCodes_CheckedChanged);
            // 
            // panelTimeCodes
            // 
            this.panelTimeCodes.Controls.Add(this.checkBoxExtractExactTimeCodes);
            this.panelTimeCodes.Controls.Add(this.panelExtractTimeCodes);
            this.panelTimeCodes.Location = new System.Drawing.Point(6, 41);
            this.panelTimeCodes.Name = "panelTimeCodes";
            this.panelTimeCodes.Size = new System.Drawing.Size(546, 110);
            this.panelTimeCodes.TabIndex = 2;
            // 
            // buttonCancelTimeCodes
            // 
            this.buttonCancelTimeCodes.Location = new System.Drawing.Point(23, 20);
            this.buttonCancelTimeCodes.Name = "buttonCancelTimeCodes";
            this.buttonCancelTimeCodes.Size = new System.Drawing.Size(149, 23);
            this.buttonCancelTimeCodes.TabIndex = 5;
            this.buttonCancelTimeCodes.Text = "Cancel";
            this.buttonCancelTimeCodes.UseVisualStyleBackColor = true;
            this.buttonCancelTimeCodes.Visible = false;
            this.buttonCancelTimeCodes.Click += new System.EventHandler(this.buttonCancelTimeCodes_Click);
            // 
            // labelTimeCodesStatus
            // 
            this.labelTimeCodesStatus.AutoSize = true;
            this.labelTimeCodesStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTimeCodesStatus.Location = new System.Drawing.Point(20, 1);
            this.labelTimeCodesStatus.Name = "labelTimeCodesStatus";
            this.labelTimeCodesStatus.Size = new System.Drawing.Size(132, 15);
            this.labelTimeCodesStatus.TabIndex = 1;
            this.labelTimeCodesStatus.Text = "123 time codes loaded";
            // 
            // buttonExtractTimeCodes
            // 
            this.buttonExtractTimeCodes.Location = new System.Drawing.Point(23, 20);
            this.buttonExtractTimeCodes.Name = "buttonExtractTimeCodes";
            this.buttonExtractTimeCodes.Size = new System.Drawing.Size(149, 23);
            this.buttonExtractTimeCodes.TabIndex = 4;
            this.buttonExtractTimeCodes.Text = "Extract time codes";
            this.buttonExtractTimeCodes.UseVisualStyleBackColor = true;
            this.buttonExtractTimeCodes.Click += new System.EventHandler(this.buttonExtractTimeCodes_Click);
            // 
            // progressBarExtractTimeCodes
            // 
            this.progressBarExtractTimeCodes.Location = new System.Drawing.Point(24, 52);
            this.progressBarExtractTimeCodes.Name = "progressBarExtractTimeCodes";
            this.progressBarExtractTimeCodes.Size = new System.Drawing.Size(516, 23);
            this.progressBarExtractTimeCodes.TabIndex = 4;
            // 
            // labelExtractTimeCodesProgress
            // 
            this.labelExtractTimeCodesProgress.AutoSize = true;
            this.labelExtractTimeCodesProgress.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelExtractTimeCodesProgress.Location = new System.Drawing.Point(178, 24);
            this.labelExtractTimeCodesProgress.Name = "labelExtractTimeCodesProgress";
            this.labelExtractTimeCodesProgress.Size = new System.Drawing.Size(102, 15);
            this.labelExtractTimeCodesProgress.TabIndex = 3;
            this.labelExtractTimeCodesProgress.Text = "00:00:00 / 00:00:00";
            // 
            // panelExtractTimeCodes
            // 
            this.panelExtractTimeCodes.Controls.Add(this.labelExtractTimeCodesProgress);
            this.panelExtractTimeCodes.Controls.Add(this.progressBarExtractTimeCodes);
            this.panelExtractTimeCodes.Controls.Add(this.buttonExtractTimeCodes);
            this.panelExtractTimeCodes.Controls.Add(this.labelTimeCodesStatus);
            this.panelExtractTimeCodes.Controls.Add(this.buttonCancelTimeCodes);
            this.panelExtractTimeCodes.Location = new System.Drawing.Point(0, 27);
            this.panelExtractTimeCodes.Name = "panelExtractTimeCodes";
            this.panelExtractTimeCodes.Size = new System.Drawing.Size(546, 83);
            this.panelExtractTimeCodes.TabIndex = 2;
            // 
            // BeautifyTimeCodes
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(582, 397);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.groupBoxShotChanges);
            this.Controls.Add(this.buttonEditProfile);
            this.Controls.Add(this.groupBoxTimeCodes);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BeautifyTimeCodes";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BeautifyTimeCodes";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BeautifyTimeCodes_KeyDown);
            this.groupBoxTimeCodes.ResumeLayout(false);
            this.groupBoxTimeCodes.PerformLayout();
            this.groupBoxShotChanges.ResumeLayout(false);
            this.groupBoxShotChanges.PerformLayout();
            this.panelShotChanges.ResumeLayout(false);
            this.panelShotChanges.PerformLayout();
            this.panelTimeCodes.ResumeLayout(false);
            this.panelTimeCodes.PerformLayout();
            this.panelExtractTimeCodes.ResumeLayout(false);
            this.panelExtractTimeCodes.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxTimeCodes;
        private System.Windows.Forms.CheckBox checkBoxAlignTimeCodes;
        private System.Windows.Forms.GroupBox groupBoxShotChanges;
        private System.Windows.Forms.CheckBox checkBoxSnapToShotChanges;
        private System.Windows.Forms.Panel panelShotChanges;
        private System.Windows.Forms.Button buttonImportShotChanges;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelShotChangesStatus;
        private System.Windows.Forms.Button buttonEditProfile;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Panel panelTimeCodes;
        private System.Windows.Forms.CheckBox checkBoxExtractExactTimeCodes;
        private System.Windows.Forms.Panel panelExtractTimeCodes;
        private System.Windows.Forms.Label labelExtractTimeCodesProgress;
        private System.Windows.Forms.ProgressBar progressBarExtractTimeCodes;
        private System.Windows.Forms.Button buttonExtractTimeCodes;
        private Controls.NikseLabel labelTimeCodesStatus;
        private System.Windows.Forms.Button buttonCancelTimeCodes;
    }
}