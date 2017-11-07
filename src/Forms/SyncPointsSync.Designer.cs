﻿namespace Nikse.SubtitleEdit.Forms
{
    partial class SyncPointsSync
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
            this.groupBoxImportResult = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownMatchPercentage = new System.Windows.Forms.NumericUpDown();
            this.labelAutoSyncing = new System.Windows.Forms.Label();
            this.progressAutoSync = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxOtherSubLanguage = new System.Windows.Forms.ComboBox();
            this.comboBoxSubToSyncLanguage = new System.Windows.Forms.ComboBox();
            this.buttonAutoSync = new System.Windows.Forms.Button();
            this.labelAdjustFactor = new System.Windows.Forms.Label();
            this.buttonFindTextOther = new System.Windows.Forms.Button();
            this.buttonFindText = new System.Windows.Forms.Button();
            this.labelOtherSubtitleFileName = new System.Windows.Forms.Label();
            this.labelSubtitleFileName = new System.Windows.Forms.Label();
            this.subtitleListView2 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.listBoxSyncPoints = new System.Windows.Forms.ListBox();
            this.labelNoOfSyncPoints = new System.Windows.Forms.Label();
            this.buttonRemoveSyncPoint = new System.Windows.Forms.Button();
            this.buttonSetSyncPoint = new System.Windows.Forms.Button();
            this.SubtitleListview1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.labelSyncInfo = new System.Windows.Forms.Label();
            this.buttonApplySync = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxImportResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMatchPercentage)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxImportResult
            // 
            this.groupBoxImportResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportResult.Controls.Add(this.label3);
            this.groupBoxImportResult.Controls.Add(this.numericUpDownMatchPercentage);
            this.groupBoxImportResult.Controls.Add(this.labelAutoSyncing);
            this.groupBoxImportResult.Controls.Add(this.progressAutoSync);
            this.groupBoxImportResult.Controls.Add(this.label2);
            this.groupBoxImportResult.Controls.Add(this.label1);
            this.groupBoxImportResult.Controls.Add(this.comboBoxOtherSubLanguage);
            this.groupBoxImportResult.Controls.Add(this.comboBoxSubToSyncLanguage);
            this.groupBoxImportResult.Controls.Add(this.buttonAutoSync);
            this.groupBoxImportResult.Controls.Add(this.labelAdjustFactor);
            this.groupBoxImportResult.Controls.Add(this.buttonFindTextOther);
            this.groupBoxImportResult.Controls.Add(this.buttonFindText);
            this.groupBoxImportResult.Controls.Add(this.labelOtherSubtitleFileName);
            this.groupBoxImportResult.Controls.Add(this.labelSubtitleFileName);
            this.groupBoxImportResult.Controls.Add(this.subtitleListView2);
            this.groupBoxImportResult.Controls.Add(this.listBoxSyncPoints);
            this.groupBoxImportResult.Controls.Add(this.labelNoOfSyncPoints);
            this.groupBoxImportResult.Controls.Add(this.buttonRemoveSyncPoint);
            this.groupBoxImportResult.Controls.Add(this.buttonSetSyncPoint);
            this.groupBoxImportResult.Controls.Add(this.SubtitleListview1);
            this.groupBoxImportResult.Location = new System.Drawing.Point(12, 12);
            this.groupBoxImportResult.Name = "groupBoxImportResult";
            this.groupBoxImportResult.Size = new System.Drawing.Size(1096, 434);
            this.groupBoxImportResult.TabIndex = 16;
            this.groupBoxImportResult.TabStop = false;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Gray;
            this.label3.Location = new System.Drawing.Point(481, 412);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 14);
            this.label3.TabIndex = 43;
            this.label3.Text = "% Match";
            // 
            // numericUpDownMatchPercentage
            // 
            this.numericUpDownMatchPercentage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.numericUpDownMatchPercentage.Location = new System.Drawing.Point(544, 406);
            this.numericUpDownMatchPercentage.Name = "numericUpDownMatchPercentage";
            this.numericUpDownMatchPercentage.Size = new System.Drawing.Size(77, 24);
            this.numericUpDownMatchPercentage.TabIndex = 42;
            this.numericUpDownMatchPercentage.Value = new decimal(new int[] {
            70,
            0,
            0,
            0});
            // 
            // labelAutoSyncing
            // 
            this.labelAutoSyncing.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelAutoSyncing.AutoSize = true;
            this.labelAutoSyncing.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAutoSyncing.ForeColor = System.Drawing.Color.Gray;
            this.labelAutoSyncing.Location = new System.Drawing.Point(481, 33);
            this.labelAutoSyncing.Name = "labelAutoSyncing";
            this.labelAutoSyncing.Size = new System.Drawing.Size(78, 14);
            this.labelAutoSyncing.TabIndex = 41;
            this.labelAutoSyncing.Text = "Auto syncing";
            this.labelAutoSyncing.Visible = false;
            // 
            // progressAutoSync
            // 
            this.progressAutoSync.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.progressAutoSync.Location = new System.Drawing.Point(484, 50);
            this.progressAutoSync.Name = "progressAutoSync";
            this.progressAutoSync.Size = new System.Drawing.Size(135, 23);
            this.progressAutoSync.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressAutoSync.TabIndex = 40;
            this.progressAutoSync.Visible = false;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(481, 358);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 14);
            this.label2.TabIndex = 39;
            this.label2.Text = "Language other subtitle";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(481, 313);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 14);
            this.label1.TabIndex = 38;
            this.label1.Text = "Language subtile to sync";
            // 
            // comboBoxOtherSubLanguage
            // 
            this.comboBoxOtherSubLanguage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.comboBoxOtherSubLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOtherSubLanguage.FormattingEnabled = true;
            this.comboBoxOtherSubLanguage.Location = new System.Drawing.Point(484, 376);
            this.comboBoxOtherSubLanguage.Name = "comboBoxOtherSubLanguage";
            this.comboBoxOtherSubLanguage.Size = new System.Drawing.Size(138, 25);
            this.comboBoxOtherSubLanguage.TabIndex = 37;
            // 
            // comboBoxSubToSyncLanguage
            // 
            this.comboBoxSubToSyncLanguage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.comboBoxSubToSyncLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubToSyncLanguage.FormattingEnabled = true;
            this.comboBoxSubToSyncLanguage.Location = new System.Drawing.Point(484, 330);
            this.comboBoxSubToSyncLanguage.Name = "comboBoxSubToSyncLanguage";
            this.comboBoxSubToSyncLanguage.Size = new System.Drawing.Size(137, 25);
            this.comboBoxSubToSyncLanguage.TabIndex = 20;
            // 
            // buttonAutoSync
            // 
            this.buttonAutoSync.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonAutoSync.Location = new System.Drawing.Point(484, 289);
            this.buttonAutoSync.Name = "buttonAutoSync";
            this.buttonAutoSync.Size = new System.Drawing.Size(138, 21);
            this.buttonAutoSync.TabIndex = 36;
            this.buttonAutoSync.Text = "Auto Sync (Béta)";
            this.buttonAutoSync.UseVisualStyleBackColor = true;
            this.buttonAutoSync.Click += new System.EventHandler(this.buttonAutoSync_Click);
            // 
            // labelAdjustFactor
            // 
            this.labelAdjustFactor.AutoSize = true;
            this.labelAdjustFactor.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAdjustFactor.ForeColor = System.Drawing.Color.Gray;
            this.labelAdjustFactor.Location = new System.Drawing.Point(481, 262);
            this.labelAdjustFactor.Name = "labelAdjustFactor";
            this.labelAdjustFactor.Size = new System.Drawing.Size(76, 14);
            this.labelAdjustFactor.TabIndex = 35;
            this.labelAdjustFactor.Text = "AdjustFactor";
            // 
            // buttonFindTextOther
            // 
            this.buttonFindTextOther.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFindTextOther.Location = new System.Drawing.Point(976, 407);
            this.buttonFindTextOther.Name = "buttonFindTextOther";
            this.buttonFindTextOther.Size = new System.Drawing.Size(106, 21);
            this.buttonFindTextOther.TabIndex = 34;
            this.buttonFindTextOther.Text = "Find text...";
            this.buttonFindTextOther.UseVisualStyleBackColor = true;
            this.buttonFindTextOther.Click += new System.EventHandler(this.ButtonFindTextOtherClick);
            // 
            // buttonFindText
            // 
            this.buttonFindText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFindText.Location = new System.Drawing.Point(369, 407);
            this.buttonFindText.Name = "buttonFindText";
            this.buttonFindText.Size = new System.Drawing.Size(106, 21);
            this.buttonFindText.TabIndex = 33;
            this.buttonFindText.Text = "Find text...";
            this.buttonFindText.UseVisualStyleBackColor = true;
            this.buttonFindText.Click += new System.EventHandler(this.ButtonFindTextClick);
            // 
            // labelOtherSubtitleFileName
            // 
            this.labelOtherSubtitleFileName.AutoSize = true;
            this.labelOtherSubtitleFileName.Location = new System.Drawing.Point(626, 17);
            this.labelOtherSubtitleFileName.Name = "labelOtherSubtitleFileName";
            this.labelOtherSubtitleFileName.Size = new System.Drawing.Size(167, 17);
            this.labelOtherSubtitleFileName.TabIndex = 21;
            this.labelOtherSubtitleFileName.Text = "labelOtherSubtitleFileName";
            // 
            // labelSubtitleFileName
            // 
            this.labelSubtitleFileName.AutoSize = true;
            this.labelSubtitleFileName.Location = new System.Drawing.Point(6, 17);
            this.labelSubtitleFileName.Name = "labelSubtitleFileName";
            this.labelSubtitleFileName.Size = new System.Drawing.Size(132, 17);
            this.labelSubtitleFileName.TabIndex = 20;
            this.labelSubtitleFileName.Text = "labelSubtitleFileName";
            // 
            // subtitleListView2
            // 
            this.subtitleListView2.AllowColumnReorder = true;
            this.subtitleListView2.AllowDrop = true;
            this.subtitleListView2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.subtitleListView2.FirstVisibleIndex = -1;
            this.subtitleListView2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListView2.FullRowSelect = true;
            this.subtitleListView2.GridLines = true;
            this.subtitleListView2.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListView2.HideSelection = false;
            this.subtitleListView2.Location = new System.Drawing.Point(629, 33);
            this.subtitleListView2.MultiSelect = false;
            this.subtitleListView2.Name = "subtitleListView2";
            this.subtitleListView2.OwnerDraw = true;
            this.subtitleListView2.Size = new System.Drawing.Size(453, 368);
            this.subtitleListView2.SubtitleFontBold = false;
            this.subtitleListView2.SubtitleFontName = "Tahoma";
            this.subtitleListView2.SubtitleFontSize = 8;
            this.subtitleListView2.TabIndex = 19;
            this.subtitleListView2.UseCompatibleStateImageBehavior = false;
            this.subtitleListView2.UseSyntaxColoring = true;
            this.subtitleListView2.View = System.Windows.Forms.View.Details;
            // 
            // listBoxSyncPoints
            // 
            this.listBoxSyncPoints.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.listBoxSyncPoints.FormattingEnabled = true;
            this.listBoxSyncPoints.ItemHeight = 17;
            this.listBoxSyncPoints.Location = new System.Drawing.Point(482, 187);
            this.listBoxSyncPoints.Name = "listBoxSyncPoints";
            this.listBoxSyncPoints.Size = new System.Drawing.Size(140, 72);
            this.listBoxSyncPoints.TabIndex = 18;
            this.listBoxSyncPoints.SelectedIndexChanged += new System.EventHandler(this.listBoxSyncPoints_SelectedIndexChanged);
            // 
            // labelNoOfSyncPoints
            // 
            this.labelNoOfSyncPoints.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelNoOfSyncPoints.AutoSize = true;
            this.labelNoOfSyncPoints.Location = new System.Drawing.Point(481, 169);
            this.labelNoOfSyncPoints.Name = "labelNoOfSyncPoints";
            this.labelNoOfSyncPoints.Size = new System.Drawing.Size(97, 17);
            this.labelNoOfSyncPoints.TabIndex = 16;
            this.labelNoOfSyncPoints.Text = "Sync points: 0";
            // 
            // buttonRemoveSyncPoint
            // 
            this.buttonRemoveSyncPoint.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonRemoveSyncPoint.Location = new System.Drawing.Point(484, 143);
            this.buttonRemoveSyncPoint.Name = "buttonRemoveSyncPoint";
            this.buttonRemoveSyncPoint.Size = new System.Drawing.Size(138, 21);
            this.buttonRemoveSyncPoint.TabIndex = 14;
            this.buttonRemoveSyncPoint.Text = "Remove sync point";
            this.buttonRemoveSyncPoint.UseVisualStyleBackColor = true;
            this.buttonRemoveSyncPoint.Click += new System.EventHandler(this.buttonRemoveSyncPoint_Click);
            // 
            // buttonSetSyncPoint
            // 
            this.buttonSetSyncPoint.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonSetSyncPoint.Location = new System.Drawing.Point(484, 116);
            this.buttonSetSyncPoint.Name = "buttonSetSyncPoint";
            this.buttonSetSyncPoint.Size = new System.Drawing.Size(138, 21);
            this.buttonSetSyncPoint.TabIndex = 13;
            this.buttonSetSyncPoint.Text = "Set sync point";
            this.buttonSetSyncPoint.UseVisualStyleBackColor = true;
            this.buttonSetSyncPoint.Click += new System.EventHandler(this.buttonSetSyncPoint_Click);
            // 
            // SubtitleListview1
            // 
            this.SubtitleListview1.AllowColumnReorder = true;
            this.SubtitleListview1.AllowDrop = true;
            this.SubtitleListview1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SubtitleListview1.FirstVisibleIndex = -1;
            this.SubtitleListview1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubtitleListview1.FullRowSelect = true;
            this.SubtitleListview1.GridLines = true;
            this.SubtitleListview1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SubtitleListview1.HideSelection = false;
            this.SubtitleListview1.Location = new System.Drawing.Point(9, 33);
            this.SubtitleListview1.MultiSelect = false;
            this.SubtitleListview1.Name = "SubtitleListview1";
            this.SubtitleListview1.OwnerDraw = true;
            this.SubtitleListview1.Size = new System.Drawing.Size(466, 368);
            this.SubtitleListview1.SubtitleFontBold = false;
            this.SubtitleListview1.SubtitleFontName = "Tahoma";
            this.SubtitleListview1.SubtitleFontSize = 8;
            this.SubtitleListview1.TabIndex = 12;
            this.SubtitleListview1.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview1.UseSyntaxColoring = true;
            this.SubtitleListview1.View = System.Windows.Forms.View.Details;
            this.SubtitleListview1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.SubtitleListview1_MouseDoubleClick);
            // 
            // labelSyncInfo
            // 
            this.labelSyncInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSyncInfo.AutoSize = true;
            this.labelSyncInfo.Location = new System.Drawing.Point(18, 456);
            this.labelSyncInfo.Name = "labelSyncInfo";
            this.labelSyncInfo.Size = new System.Drawing.Size(371, 17);
            this.labelSyncInfo.TabIndex = 17;
            this.labelSyncInfo.Text = "Set at least two sync points to make rough synchronization";
            // 
            // buttonApplySync
            // 
            this.buttonApplySync.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApplySync.Enabled = false;
            this.buttonApplySync.Location = new System.Drawing.Point(1029, 452);
            this.buttonApplySync.Name = "buttonApplySync";
            this.buttonApplySync.Size = new System.Drawing.Size(80, 21);
            this.buttonApplySync.TabIndex = 15;
            this.buttonApplySync.Text = "Apply";
            this.buttonApplySync.UseVisualStyleBackColor = true;
            this.buttonApplySync.Click += new System.EventHandler(this.buttonSync_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(948, 452);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 19;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(867, 452);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 18;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // SyncPointsSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1120, 485);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonApplySync);
            this.Controls.Add(this.groupBoxImportResult);
            this.Controls.Add(this.labelSyncInfo);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "SyncPointsSync";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SyncPointsSync";
            this.Shown += new System.EventHandler(this.SyncPointsSyncShown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SyncPointsSync_KeyDown);
            this.Resize += new System.EventHandler(this.SyncPointsSyncResize);
            this.groupBoxImportResult.ResumeLayout(false);
            this.groupBoxImportResult.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMatchPercentage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxImportResult;
        private System.Windows.Forms.Label labelSyncInfo;
        private System.Windows.Forms.Label labelNoOfSyncPoints;
        private System.Windows.Forms.Button buttonSetSyncPoint;
        private System.Windows.Forms.Button buttonApplySync;
        private System.Windows.Forms.Button buttonRemoveSyncPoint;
        private Controls.SubtitleListView SubtitleListview1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ListBox listBoxSyncPoints;
        private System.Windows.Forms.Label labelOtherSubtitleFileName;
        private System.Windows.Forms.Label labelSubtitleFileName;
        private Controls.SubtitleListView subtitleListView2;
        private System.Windows.Forms.Button buttonFindText;
        private System.Windows.Forms.Button buttonFindTextOther;
        private System.Windows.Forms.Label labelAdjustFactor;
        private System.Windows.Forms.Button buttonAutoSync;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxOtherSubLanguage;
        private System.Windows.Forms.ComboBox comboBoxSubToSyncLanguage;
        private System.Windows.Forms.Label labelAutoSyncing;
        private System.Windows.Forms.ProgressBar progressAutoSync;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownMatchPercentage;
    }
}