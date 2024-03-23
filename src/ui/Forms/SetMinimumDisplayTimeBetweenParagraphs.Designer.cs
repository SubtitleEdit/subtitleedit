namespace Nikse.SubtitleEdit.Forms
{
    partial class SetMinimumDisplayTimeBetweenParagraphs
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
            this.groupBoxLinesFound = new System.Windows.Forms.GroupBox();
            this.SubtitleListview1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.numericUpDownMinMillisecondsBetweenLines = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMaxMillisecondsBetweenLines = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkBoxShowOnlyChangedLines = new System.Windows.Forms.CheckBox();
            this.comboBoxFrameRate = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelFrames = new System.Windows.Forms.Label();
            this.numericUpDownFrames = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelXFrameIsXMS = new System.Windows.Forms.Label();
            this.groupBoxFrameInfo = new System.Windows.Forms.GroupBox();
            this.groupBoxLinesFound.SuspendLayout();
            this.groupBoxFrameInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxLinesFound
            // 
            this.groupBoxLinesFound.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLinesFound.Controls.Add(this.SubtitleListview1);
            this.groupBoxLinesFound.Location = new System.Drawing.Point(18, 94);
            this.groupBoxLinesFound.Name = "groupBoxLinesFound";
            this.groupBoxLinesFound.Size = new System.Drawing.Size(726, 347);
            this.groupBoxLinesFound.TabIndex = 3;
            this.groupBoxLinesFound.TabStop = false;
            this.groupBoxLinesFound.Text = "Preview";
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
            this.SubtitleListview1.Location = new System.Drawing.Point(6, 19);
            this.SubtitleListview1.Name = "SubtitleListview1";
            this.SubtitleListview1.OwnerDraw = true;
            this.SubtitleListview1.Size = new System.Drawing.Size(714, 322);
            this.SubtitleListview1.SubtitleFontBold = false;
            this.SubtitleListview1.SubtitleFontName = "Tahoma";
            this.SubtitleListview1.SubtitleFontSize = 8;
            this.SubtitleListview1.TabIndex = 0;
            this.SubtitleListview1.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview1.UseSyntaxColoring = true;
            this.SubtitleListview1.View = System.Windows.Forms.View.Details;
            // 
            // numericUpDownMinMillisecondsBetweenLines
            // 
            this.numericUpDownMinMillisecondsBetweenLines.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMinMillisecondsBetweenLines.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMinMillisecondsBetweenLines.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMinMillisecondsBetweenLines.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMinMillisecondsBetweenLines.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMinMillisecondsBetweenLines.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMinMillisecondsBetweenLines.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMinMillisecondsBetweenLines.DecimalPlaces = 0;
            this.numericUpDownMinMillisecondsBetweenLines.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMinMillisecondsBetweenLines.Location = new System.Drawing.Point(18, 34);
            this.numericUpDownMinMillisecondsBetweenLines.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMinMillisecondsBetweenLines.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownMinMillisecondsBetweenLines.Name = "numericUpDownMinMillisecondsBetweenLines";
            this.numericUpDownMinMillisecondsBetweenLines.Size = new System.Drawing.Size(64, 23);
            this.numericUpDownMinMillisecondsBetweenLines.TabIndex = 1;
            this.numericUpDownMinMillisecondsBetweenLines.TabStop = false;
            this.numericUpDownMinMillisecondsBetweenLines.ThousandsSeparator = false;
            this.numericUpDownMinMillisecondsBetweenLines.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownMinMillisecondsBetweenLines.ValueChanged += new System.EventHandler(this.numericUpDownMinMillisecondsBetweenLines_ValueChanged);
            this.numericUpDownMinMillisecondsBetweenLines.KeyUp += new System.Windows.Forms.KeyEventHandler(this.numericUpDownMinMillisecondsBetweenLines_KeyUp);
            // 
            // labelMaxMillisecondsBetweenLines
            // 
            this.labelMaxMillisecondsBetweenLines.AutoSize = true;
            this.labelMaxMillisecondsBetweenLines.Location = new System.Drawing.Point(15, 18);
            this.labelMaxMillisecondsBetweenLines.Name = "labelMaxMillisecondsBetweenLines";
            this.labelMaxMillisecondsBetweenLines.Size = new System.Drawing.Size(174, 13);
            this.labelMaxMillisecondsBetweenLines.TabIndex = 2;
            this.labelMaxMillisecondsBetweenLines.Text = "Minimum milliseconds between lines";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(595, 447);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(676, 447);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // checkBoxShowOnlyChangedLines
            // 
            this.checkBoxShowOnlyChangedLines.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxShowOnlyChangedLines.AutoSize = true;
            this.checkBoxShowOnlyChangedLines.Location = new System.Drawing.Point(18, 448);
            this.checkBoxShowOnlyChangedLines.Name = "checkBoxShowOnlyChangedLines";
            this.checkBoxShowOnlyChangedLines.Size = new System.Drawing.Size(142, 17);
            this.checkBoxShowOnlyChangedLines.TabIndex = 4;
            this.checkBoxShowOnlyChangedLines.Text = "Show only modified lines";
            this.checkBoxShowOnlyChangedLines.UseVisualStyleBackColor = true;
            this.checkBoxShowOnlyChangedLines.CheckedChanged += new System.EventHandler(this.checkBoxShowOnlyChangedLines_CheckedChanged);
            // 
            // comboBoxFrameRate
            // 
            this.comboBoxFrameRate.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFrameRate.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFrameRate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFrameRate.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFrameRate.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFrameRate.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFrameRate.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFrameRate.DropDownHeight = 400;
            this.comboBoxFrameRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxFrameRate.DropDownWidth = 160;
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Location = new System.Drawing.Point(7, 25);
            this.comboBoxFrameRate.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxFrameRate.MaxLength = 10;
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.SelectedIndex = -1;
            this.comboBoxFrameRate.SelectedItem = null;
            this.comboBoxFrameRate.SelectedText = "";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(160, 23);
            this.comboBoxFrameRate.TabIndex = 0;
            this.comboBoxFrameRate.TabStop = false;
            this.comboBoxFrameRate.UsePopupWindow = false;
            this.comboBoxFrameRate.SelectedIndexChanged += new System.EventHandler(this.comboBoxFrameRate_SelectedIndexChanged);
            this.comboBoxFrameRate.KeyUp += new System.Windows.Forms.KeyEventHandler(this.comboBoxFrameRate_KeyUp);
            // 
            // labelFrames
            // 
            this.labelFrames.AutoSize = true;
            this.labelFrames.Location = new System.Drawing.Point(174, 29);
            this.labelFrames.Name = "labelFrames";
            this.labelFrames.Size = new System.Drawing.Size(46, 13);
            this.labelFrames.TabIndex = 1;
            this.labelFrames.Text = "Frames:";
            // 
            // numericUpDownFrames
            // 
            this.numericUpDownFrames.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownFrames.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownFrames.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownFrames.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownFrames.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownFrames.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownFrames.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownFrames.DecimalPlaces = 0;
            this.numericUpDownFrames.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownFrames.Location = new System.Drawing.Point(220, 25);
            this.numericUpDownFrames.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDownFrames.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownFrames.Name = "numericUpDownFrames";
            this.numericUpDownFrames.Size = new System.Drawing.Size(56, 23);
            this.numericUpDownFrames.TabIndex = 52;
            this.numericUpDownFrames.TabStop = false;
            this.numericUpDownFrames.ThousandsSeparator = false;
            this.numericUpDownFrames.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownFrames.ValueChanged += new System.EventHandler(this.numericUpDownFrames_ValueChanged);
            // 
            // labelXFrameIsXMS
            // 
            this.labelXFrameIsXMS.AutoSize = true;
            this.labelXFrameIsXMS.Location = new System.Drawing.Point(6, 57);
            this.labelXFrameIsXMS.Name = "labelXFrameIsXMS";
            this.labelXFrameIsXMS.Size = new System.Drawing.Size(121, 13);
            this.labelXFrameIsXMS.TabIndex = 3;
            this.labelXFrameIsXMS.Text = "x frame is y milliseconds";
            // 
            // groupBoxFrameInfo
            // 
            this.groupBoxFrameInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFrameInfo.Controls.Add(this.comboBoxFrameRate);
            this.groupBoxFrameInfo.Controls.Add(this.labelFrames);
            this.groupBoxFrameInfo.Controls.Add(this.numericUpDownFrames);
            this.groupBoxFrameInfo.Controls.Add(this.labelXFrameIsXMS);
            this.groupBoxFrameInfo.Location = new System.Drawing.Point(355, 12);
            this.groupBoxFrameInfo.Name = "groupBoxFrameInfo";
            this.groupBoxFrameInfo.Size = new System.Drawing.Size(383, 76);
            this.groupBoxFrameInfo.TabIndex = 2;
            this.groupBoxFrameInfo.TabStop = false;
            this.groupBoxFrameInfo.Text = "Frame rate info";
            // 
            // SetMinimumDisplayTimeBetweenParagraphs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 480);
            this.Controls.Add(this.groupBoxFrameInfo);
            this.Controls.Add(this.checkBoxShowOnlyChangedLines);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.numericUpDownMinMillisecondsBetweenLines);
            this.Controls.Add(this.labelMaxMillisecondsBetweenLines);
            this.Controls.Add(this.groupBoxLinesFound);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(750, 500);
            this.Name = "SetMinimumDisplayTimeBetweenParagraphs";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set minimum display time between paragraphs";
            this.Shown += new System.EventHandler(this.SetMinimumDisplayTimeBetweenParagraphs_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SetMinimalDisplayTimeDifference_KeyDown);
            this.groupBoxLinesFound.ResumeLayout(false);
            this.groupBoxFrameInfo.ResumeLayout(false);
            this.groupBoxFrameInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxLinesFound;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMinMillisecondsBetweenLines;
        private System.Windows.Forms.Label labelMaxMillisecondsBetweenLines;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private Controls.SubtitleListView SubtitleListview1;
        private System.Windows.Forms.CheckBox checkBoxShowOnlyChangedLines;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFrameRate;
        private System.Windows.Forms.Label labelXFrameIsXMS;
        private System.Windows.Forms.Label labelFrames;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownFrames;
        private System.Windows.Forms.GroupBox groupBoxFrameInfo;
    }
}