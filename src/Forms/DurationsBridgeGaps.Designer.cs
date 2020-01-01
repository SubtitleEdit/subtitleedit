namespace Nikse.SubtitleEdit.Forms
{
    partial class DurationsBridgeGaps
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
            this.labelMilliseconds = new System.Windows.Forms.Label();
            this.groupBoxLinesFound = new System.Windows.Forms.GroupBox();
            this.SubtitleListview1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.numericUpDownMaxMs = new System.Windows.Forms.NumericUpDown();
            this.labelBridgePart1 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.radioButtonProlongEndTime = new System.Windows.Forms.RadioButton();
            this.radioButtonDivideEven = new System.Windows.Forms.RadioButton();
            this.labelMinMsBetweenLines = new System.Windows.Forms.Label();
            this.numericUpDownMinMsBetweenLines = new System.Windows.Forms.NumericUpDown();
            this.groupBoxLinesFound.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinMsBetweenLines)).BeginInit();
            this.SuspendLayout();
            // 
            // labelMilliseconds
            // 
            this.labelMilliseconds.AutoSize = true;
            this.labelMilliseconds.Location = new System.Drawing.Point(196, 12);
            this.labelMilliseconds.Name = "labelMilliseconds";
            this.labelMilliseconds.Size = new System.Drawing.Size(63, 13);
            this.labelMilliseconds.TabIndex = 2;
            this.labelMilliseconds.Text = "milliseconds";
            // 
            // groupBoxLinesFound
            // 
            this.groupBoxLinesFound.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLinesFound.Controls.Add(this.SubtitleListview1);
            this.groupBoxLinesFound.Location = new System.Drawing.Point(15, 65);
            this.groupBoxLinesFound.Name = "groupBoxLinesFound";
            this.groupBoxLinesFound.Size = new System.Drawing.Size(871, 484);
            this.groupBoxLinesFound.TabIndex = 7;
            this.groupBoxLinesFound.TabStop = false;
            this.groupBoxLinesFound.Text = "Lines that will be bridged";
            // 
            // SubtitleListview1
            // 
            this.SubtitleListview1.AllowColumnReorder = true;
            this.SubtitleListview1.AllowDrop = true;
            this.SubtitleListview1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SubtitleListview1.FirstVisibleIndex = -1;
            this.SubtitleListview1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubtitleListview1.FullRowSelect = true;
            this.SubtitleListview1.GridLines = true;
            this.SubtitleListview1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SubtitleListview1.HideSelection = false;
            this.SubtitleListview1.Location = new System.Drawing.Point(3, 16);
            this.SubtitleListview1.Name = "SubtitleListview1";
            this.SubtitleListview1.OwnerDraw = true;
            this.SubtitleListview1.Size = new System.Drawing.Size(865, 465);
            this.SubtitleListview1.SubtitleFontBold = false;
            this.SubtitleListview1.SubtitleFontName = "Tahoma";
            this.SubtitleListview1.SubtitleFontSize = 8;
            this.SubtitleListview1.TabIndex = 0;
            this.SubtitleListview1.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview1.UseSyntaxColoring = true;
            this.SubtitleListview1.View = System.Windows.Forms.View.Details;
            // 
            // numericUpDownMaxMs
            // 
            this.numericUpDownMaxMs.Increment = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDownMaxMs.Location = new System.Drawing.Point(140, 10);
            this.numericUpDownMaxMs.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDownMaxMs.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxMs.Name = "numericUpDownMaxMs";
            this.numericUpDownMaxMs.Size = new System.Drawing.Size(50, 20);
            this.numericUpDownMaxMs.TabIndex = 1;
            this.numericUpDownMaxMs.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMaxMs.ValueChanged += new System.EventHandler(this.numericUpDownMaxMs_ValueChanged);
            this.numericUpDownMaxMs.KeyUp += new System.Windows.Forms.KeyEventHandler(this.numericUpDownMaxMs_KeyUp);
            this.numericUpDownMaxMs.Validated += new System.EventHandler(this.numericUpDownMaxMs_Validated);
            // 
            // labelBridgePart1
            // 
            this.labelBridgePart1.AutoSize = true;
            this.labelBridgePart1.Location = new System.Drawing.Point(12, 12);
            this.labelBridgePart1.Name = "labelBridgePart1";
            this.labelBridgePart1.Size = new System.Drawing.Size(122, 13);
            this.labelBridgePart1.TabIndex = 0;
            this.labelBridgePart1.Text = "Bridge gaps smaller than";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(730, 555);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(811, 555);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // radioButtonProlongEndTime
            // 
            this.radioButtonProlongEndTime.AutoSize = true;
            this.radioButtonProlongEndTime.Checked = true;
            this.radioButtonProlongEndTime.Location = new System.Drawing.Point(586, 17);
            this.radioButtonProlongEndTime.Name = "radioButtonProlongEndTime";
            this.radioButtonProlongEndTime.Size = new System.Drawing.Size(171, 17);
            this.radioButtonProlongEndTime.TabIndex = 5;
            this.radioButtonProlongEndTime.TabStop = true;
            this.radioButtonProlongEndTime.Text = "Previous text takes all gap time";
            this.radioButtonProlongEndTime.UseVisualStyleBackColor = true;
            this.radioButtonProlongEndTime.CheckedChanged += new System.EventHandler(this.numericUpDownMaxMs_ValueChanged);
            // 
            // radioButtonDivideEven
            // 
            this.radioButtonDivideEven.AutoSize = true;
            this.radioButtonDivideEven.Location = new System.Drawing.Point(586, 40);
            this.radioButtonDivideEven.Name = "radioButtonDivideEven";
            this.radioButtonDivideEven.Size = new System.Drawing.Size(125, 17);
            this.radioButtonDivideEven.TabIndex = 6;
            this.radioButtonDivideEven.Text = "Texts divide gap time";
            this.radioButtonDivideEven.UseVisualStyleBackColor = true;
            this.radioButtonDivideEven.CheckedChanged += new System.EventHandler(this.numericUpDownMaxMs_ValueChanged);
            // 
            // labelMinMsBetweenLines
            // 
            this.labelMinMsBetweenLines.AutoSize = true;
            this.labelMinMsBetweenLines.Location = new System.Drawing.Point(15, 40);
            this.labelMinMsBetweenLines.Name = "labelMinMsBetweenLines";
            this.labelMinMsBetweenLines.Size = new System.Drawing.Size(154, 13);
            this.labelMinMsBetweenLines.TabIndex = 3;
            this.labelMinMsBetweenLines.Text = "Min. milliseconds between lines";
            // 
            // numericUpDownMinMsBetweenLines
            // 
            this.numericUpDownMinMsBetweenLines.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMinMsBetweenLines.Location = new System.Drawing.Point(175, 38);
            this.numericUpDownMinMsBetweenLines.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMinMsBetweenLines.Name = "numericUpDownMinMsBetweenLines";
            this.numericUpDownMinMsBetweenLines.Size = new System.Drawing.Size(50, 20);
            this.numericUpDownMinMsBetweenLines.TabIndex = 4;
            this.numericUpDownMinMsBetweenLines.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.numericUpDownMinMsBetweenLines.ValueChanged += new System.EventHandler(this.numericUpDownMinMsBetweenLines_ValueChanged);
            this.numericUpDownMinMsBetweenLines.KeyUp += new System.Windows.Forms.KeyEventHandler(this.numericUpDownMinMsBetweenLines_KeyUp);
            this.numericUpDownMinMsBetweenLines.Validated += new System.EventHandler(this.numericUpDownMinMsBetweenLines_Validated);
            // 
            // DurationsBridgeGaps
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(898, 588);
            this.Controls.Add(this.numericUpDownMinMsBetweenLines);
            this.Controls.Add(this.labelMinMsBetweenLines);
            this.Controls.Add(this.radioButtonDivideEven);
            this.Controls.Add(this.radioButtonProlongEndTime);
            this.Controls.Add(this.labelMilliseconds);
            this.Controls.Add(this.groupBoxLinesFound);
            this.Controls.Add(this.numericUpDownMaxMs);
            this.Controls.Add(this.labelBridgePart1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(850, 400);
            this.Name = "DurationsBridgeGaps";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DurationsBridgeGaps";
            this.Shown += new System.EventHandler(this.DurationsBridgeGaps_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DurationsBridgeGaps_KeyDown);
            this.groupBoxLinesFound.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinMsBetweenLines)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelMilliseconds;
        private System.Windows.Forms.GroupBox groupBoxLinesFound;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxMs;
        private System.Windows.Forms.Label labelBridgePart1;
        private Controls.SubtitleListView SubtitleListview1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.RadioButton radioButtonProlongEndTime;
        private System.Windows.Forms.RadioButton radioButtonDivideEven;
        private System.Windows.Forms.Label labelMinMsBetweenLines;
        private System.Windows.Forms.NumericUpDown numericUpDownMinMsBetweenLines;
    }
}