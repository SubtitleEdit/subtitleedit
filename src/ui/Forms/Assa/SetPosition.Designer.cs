
namespace Nikse.SubtitleEdit.Forms.Assa
{
    sealed partial class SetPosition
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
            this.groupBoxApplyTo = new System.Windows.Forms.GroupBox();
            this.radioButtonClipboard = new System.Windows.Forms.RadioButton();
            this.radioButtonSelectedLines = new System.Windows.Forms.RadioButton();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelCurrentPosition = new System.Windows.Forms.Label();
            this.labelStyleAlignment = new System.Windows.Forms.Label();
            this.labelVideoResolution = new System.Windows.Forms.Label();
            this.labelCurrentTextPosition = new System.Windows.Forms.Label();
            this.numericUpDownRotateX = new System.Windows.Forms.NumericUpDown();
            this.labelRotateX = new System.Windows.Forms.Label();
            this.numericUpDownRotateY = new System.Windows.Forms.NumericUpDown();
            this.labelRotateY = new System.Windows.Forms.Label();
            this.numericUpDownRotateZ = new System.Windows.Forms.NumericUpDown();
            this.labelRotateZ = new System.Windows.Forms.Label();
            this.panelAdvanced = new System.Windows.Forms.Panel();
            this.labelDistortX = new System.Windows.Forms.Label();
            this.numericUpDownDistortX = new System.Windows.Forms.NumericUpDown();
            this.labelDistortY = new System.Windows.Forms.Label();
            this.numericUpDownDistortY = new System.Windows.Forms.NumericUpDown();
            this.groupBoxApplyTo.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateZ)).BeginInit();
            this.panelAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDistortX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDistortY)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxApplyTo
            // 
            this.groupBoxApplyTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxApplyTo.Controls.Add(this.radioButtonClipboard);
            this.groupBoxApplyTo.Controls.Add(this.radioButtonSelectedLines);
            this.groupBoxApplyTo.Location = new System.Drawing.Point(763, 12);
            this.groupBoxApplyTo.Name = "groupBoxApplyTo";
            this.groupBoxApplyTo.Size = new System.Drawing.Size(195, 75);
            this.groupBoxApplyTo.TabIndex = 1;
            this.groupBoxApplyTo.TabStop = false;
            this.groupBoxApplyTo.Text = "Apply to";
            // 
            // radioButtonClipboard
            // 
            this.radioButtonClipboard.AutoSize = true;
            this.radioButtonClipboard.Location = new System.Drawing.Point(7, 44);
            this.radioButtonClipboard.Name = "radioButtonClipboard";
            this.radioButtonClipboard.Size = new System.Drawing.Size(69, 17);
            this.radioButtonClipboard.TabIndex = 1;
            this.radioButtonClipboard.TabStop = true;
            this.radioButtonClipboard.Text = "Clipboard";
            this.radioButtonClipboard.UseVisualStyleBackColor = true;
            // 
            // radioButtonSelectedLines
            // 
            this.radioButtonSelectedLines.AutoSize = true;
            this.radioButtonSelectedLines.Location = new System.Drawing.Point(7, 20);
            this.radioButtonSelectedLines.Name = "radioButtonSelectedLines";
            this.radioButtonSelectedLines.Size = new System.Drawing.Size(102, 17);
            this.radioButtonSelectedLines.TabIndex = 0;
            this.radioButtonSelectedLines.TabStop = true;
            this.radioButtonSelectedLines.Text = "Selected lines: x";
            this.radioButtonSelectedLines.UseVisualStyleBackColor = true;
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.pictureBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(12, 105);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(949, 589);
            this.groupBoxPreview.TabIndex = 2;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 16);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(943, 570);
            this.pictureBoxPreview.TabIndex = 0;
            this.pictureBoxPreview.TabStop = false;
            this.pictureBoxPreview.Click += new System.EventHandler(this.pictureBoxPreview_Click);
            this.pictureBoxPreview.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxPreview_MouseMove);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(805, 699);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(886, 699);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(12, 704);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(205, 13);
            this.labelInfo.TabIndex = 6;
            this.labelInfo.Text = "Click on video to toggle set/move position";
            // 
            // labelCurrentPosition
            // 
            this.labelCurrentPosition.AutoSize = true;
            this.labelCurrentPosition.Location = new System.Drawing.Point(12, 54);
            this.labelCurrentPosition.Name = "labelCurrentPosition";
            this.labelCurrentPosition.Size = new System.Drawing.Size(117, 13);
            this.labelCurrentPosition.TabIndex = 7;
            this.labelCurrentPosition.Text = "Current mouse position:";
            // 
            // labelStyleAlignment
            // 
            this.labelStyleAlignment.AutoSize = true;
            this.labelStyleAlignment.Location = new System.Drawing.Point(12, 32);
            this.labelStyleAlignment.Name = "labelStyleAlignment";
            this.labelStyleAlignment.Size = new System.Drawing.Size(81, 13);
            this.labelStyleAlignment.TabIndex = 8;
            this.labelStyleAlignment.Text = "Style alignment:";
            // 
            // labelVideoResolution
            // 
            this.labelVideoResolution.AutoSize = true;
            this.labelVideoResolution.Location = new System.Drawing.Point(12, 11);
            this.labelVideoResolution.Name = "labelVideoResolution";
            this.labelVideoResolution.Size = new System.Drawing.Size(85, 13);
            this.labelVideoResolution.TabIndex = 9;
            this.labelVideoResolution.Text = "Video resolution:";
            // 
            // labelCurrentTextPosition
            // 
            this.labelCurrentTextPosition.AutoSize = true;
            this.labelCurrentTextPosition.Location = new System.Drawing.Point(12, 75);
            this.labelCurrentTextPosition.Name = "labelCurrentTextPosition";
            this.labelCurrentTextPosition.Size = new System.Drawing.Size(103, 13);
            this.labelCurrentTextPosition.TabIndex = 10;
            this.labelCurrentTextPosition.Text = "Current text position:";
            // 
            // numericUpDownRotateX
            // 
            this.numericUpDownRotateX.DecimalPlaces = 1;
            this.numericUpDownRotateX.Location = new System.Drawing.Point(91, 9);
            this.numericUpDownRotateX.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDownRotateX.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numericUpDownRotateX.Name = "numericUpDownRotateX";
            this.numericUpDownRotateX.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownRotateX.TabIndex = 18;
            this.numericUpDownRotateX.ValueChanged += new System.EventHandler(this.numericUpDownRotateX_ValueChanged);
            // 
            // labelRotateX
            // 
            this.labelRotateX.AutoSize = true;
            this.labelRotateX.Location = new System.Drawing.Point(15, 11);
            this.labelRotateX.Name = "labelRotateX";
            this.labelRotateX.Size = new System.Drawing.Size(70, 13);
            this.labelRotateX.TabIndex = 17;
            this.labelRotateX.Text = "Rotate X axis";
            // 
            // numericUpDownRotateY
            // 
            this.numericUpDownRotateY.DecimalPlaces = 1;
            this.numericUpDownRotateY.Location = new System.Drawing.Point(91, 34);
            this.numericUpDownRotateY.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDownRotateY.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numericUpDownRotateY.Name = "numericUpDownRotateY";
            this.numericUpDownRotateY.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownRotateY.TabIndex = 20;
            this.numericUpDownRotateY.ValueChanged += new System.EventHandler(this.numericUpDownRotateX_ValueChanged);
            // 
            // labelRotateY
            // 
            this.labelRotateY.AutoSize = true;
            this.labelRotateY.Location = new System.Drawing.Point(15, 36);
            this.labelRotateY.Name = "labelRotateY";
            this.labelRotateY.Size = new System.Drawing.Size(70, 13);
            this.labelRotateY.TabIndex = 19;
            this.labelRotateY.Text = "Rotate Y axis";
            // 
            // numericUpDownRotateZ
            // 
            this.numericUpDownRotateZ.DecimalPlaces = 1;
            this.numericUpDownRotateZ.Location = new System.Drawing.Point(91, 60);
            this.numericUpDownRotateZ.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDownRotateZ.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numericUpDownRotateZ.Name = "numericUpDownRotateZ";
            this.numericUpDownRotateZ.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownRotateZ.TabIndex = 22;
            this.numericUpDownRotateZ.ValueChanged += new System.EventHandler(this.numericUpDownRotateX_ValueChanged);
            // 
            // labelRotateZ
            // 
            this.labelRotateZ.AutoSize = true;
            this.labelRotateZ.Location = new System.Drawing.Point(15, 62);
            this.labelRotateZ.Name = "labelRotateZ";
            this.labelRotateZ.Size = new System.Drawing.Size(70, 13);
            this.labelRotateZ.TabIndex = 21;
            this.labelRotateZ.Text = "Rotate Z axis";
            // 
            // panelAdvanced
            // 
            this.panelAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAdvanced.Controls.Add(this.labelDistortX);
            this.panelAdvanced.Controls.Add(this.numericUpDownDistortX);
            this.panelAdvanced.Controls.Add(this.labelDistortY);
            this.panelAdvanced.Controls.Add(this.numericUpDownDistortY);
            this.panelAdvanced.Controls.Add(this.labelRotateX);
            this.panelAdvanced.Controls.Add(this.numericUpDownRotateZ);
            this.panelAdvanced.Controls.Add(this.numericUpDownRotateX);
            this.panelAdvanced.Controls.Add(this.labelRotateZ);
            this.panelAdvanced.Controls.Add(this.labelRotateY);
            this.panelAdvanced.Controls.Add(this.numericUpDownRotateY);
            this.panelAdvanced.Location = new System.Drawing.Point(458, 11);
            this.panelAdvanced.Name = "panelAdvanced";
            this.panelAdvanced.Size = new System.Drawing.Size(299, 88);
            this.panelAdvanced.TabIndex = 23;
            // 
            // labelDistortX
            // 
            this.labelDistortX.AutoSize = true;
            this.labelDistortX.Location = new System.Drawing.Point(175, 11);
            this.labelDistortX.Name = "labelDistortX";
            this.labelDistortX.Size = new System.Drawing.Size(47, 13);
            this.labelDistortX.TabIndex = 23;
            this.labelDistortX.Text = "Distort X";
            // 
            // numericUpDownDistortX
            // 
            this.numericUpDownDistortX.DecimalPlaces = 2;
            this.numericUpDownDistortX.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numericUpDownDistortX.Location = new System.Drawing.Point(231, 9);
            this.numericUpDownDistortX.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericUpDownDistortX.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            -2147483648});
            this.numericUpDownDistortX.Name = "numericUpDownDistortX";
            this.numericUpDownDistortX.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownDistortX.TabIndex = 24;
            this.numericUpDownDistortX.ValueChanged += new System.EventHandler(this.numericUpDownRotateX_ValueChanged);
            // 
            // labelDistortY
            // 
            this.labelDistortY.AutoSize = true;
            this.labelDistortY.Location = new System.Drawing.Point(175, 36);
            this.labelDistortY.Name = "labelDistortY";
            this.labelDistortY.Size = new System.Drawing.Size(47, 13);
            this.labelDistortY.TabIndex = 25;
            this.labelDistortY.Text = "Distort Y";
            // 
            // numericUpDownDistortY
            // 
            this.numericUpDownDistortY.DecimalPlaces = 2;
            this.numericUpDownDistortY.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDownDistortY.Location = new System.Drawing.Point(231, 34);
            this.numericUpDownDistortY.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownDistortY.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            -2147483648});
            this.numericUpDownDistortY.Name = "numericUpDownDistortY";
            this.numericUpDownDistortY.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownDistortY.TabIndex = 26;
            this.numericUpDownDistortY.ValueChanged += new System.EventHandler(this.numericUpDownRotateX_ValueChanged);
            // 
            // SetPosition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(973, 734);
            this.Controls.Add(this.panelAdvanced);
            this.Controls.Add(this.labelCurrentTextPosition);
            this.Controls.Add(this.labelVideoResolution);
            this.Controls.Add(this.labelStyleAlignment);
            this.Controls.Add(this.labelCurrentPosition);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.groupBoxApplyTo);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(790, 545);
            this.Name = "SetPosition";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set position";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ApplyCustomStyles_FormClosing);
            this.Shown += new System.EventHandler(this.SetPosition_Shown);
            this.ResizeEnd += new System.EventHandler(this.SetPosition_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ApplyCustomStyles_KeyDown);
            this.groupBoxApplyTo.ResumeLayout(false);
            this.groupBoxApplyTo.PerformLayout();
            this.groupBoxPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateZ)).EndInit();
            this.panelAdvanced.ResumeLayout(false);
            this.panelAdvanced.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDistortX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDistortY)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBoxApplyTo;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.RadioButton radioButtonClipboard;
        private System.Windows.Forms.RadioButton radioButtonSelectedLines;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelCurrentPosition;
        private System.Windows.Forms.Label labelStyleAlignment;
        private System.Windows.Forms.Label labelVideoResolution;
        private System.Windows.Forms.Label labelCurrentTextPosition;
        private System.Windows.Forms.NumericUpDown numericUpDownRotateX;
        private System.Windows.Forms.Label labelRotateX;
        private System.Windows.Forms.NumericUpDown numericUpDownRotateY;
        private System.Windows.Forms.Label labelRotateY;
        private System.Windows.Forms.NumericUpDown numericUpDownRotateZ;
        private System.Windows.Forms.Label labelRotateZ;
        private System.Windows.Forms.Panel panelAdvanced;
        private System.Windows.Forms.Label labelDistortX;
        private System.Windows.Forms.NumericUpDown numericUpDownDistortX;
        private System.Windows.Forms.Label labelDistortY;
        private System.Windows.Forms.NumericUpDown numericUpDownDistortY;
    }
}