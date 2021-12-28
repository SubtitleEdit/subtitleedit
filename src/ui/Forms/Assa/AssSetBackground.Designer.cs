
namespace Nikse.SubtitleEdit.Forms.Assa
{
    sealed partial class AssSetBackground
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
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelInfo = new System.Windows.Forms.Label();
            this.numericUpDownRotateX = new System.Windows.Forms.NumericUpDown();
            this.labelRotateX = new System.Windows.Forms.Label();
            this.numericUpDownRotateY = new System.Windows.Forms.NumericUpDown();
            this.labelRotateY = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.comboBoxProgressBarEdge = new System.Windows.Forms.ComboBox();
            this.labelEdgeStyle = new System.Windows.Forms.Label();
            this.labelPreviewPleaseWait = new System.Windows.Forms.Label();
            this.buttonPreview = new System.Windows.Forms.Button();
            this.groupBoxPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.pictureBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(12, 157);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(949, 537);
            this.groupBoxPreview.TabIndex = 2;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 16);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(943, 518);
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
            // numericUpDownRotateX
            // 
            this.numericUpDownRotateX.Location = new System.Drawing.Point(88, 17);
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
            this.labelRotateX.Location = new System.Drawing.Point(12, 19);
            this.labelRotateX.Name = "labelRotateX";
            this.labelRotateX.Size = new System.Drawing.Size(65, 13);
            this.labelRotateX.TabIndex = 17;
            this.labelRotateX.Text = "X margin left";
            // 
            // numericUpDownRotateY
            // 
            this.numericUpDownRotateY.Location = new System.Drawing.Point(88, 42);
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
            this.labelRotateY.Location = new System.Drawing.Point(12, 44);
            this.labelRotateY.Name = "labelRotateY";
            this.labelRotateY.Size = new System.Drawing.Size(71, 13);
            this.labelRotateY.TabIndex = 19;
            this.labelRotateY.Text = "X margin right";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Y margin left";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(88, 71);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(52, 20);
            this.numericUpDown1.TabIndex = 22;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 98);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "Y margin right";
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(88, 96);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDown2.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(52, 20);
            this.numericUpDown2.TabIndex = 24;
            // 
            // comboBoxProgressBarEdge
            // 
            this.comboBoxProgressBarEdge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxProgressBarEdge.FormattingEnabled = true;
            this.comboBoxProgressBarEdge.Items.AddRange(new object[] {
            "Square corners",
            "Rounded corners"});
            this.comboBoxProgressBarEdge.Location = new System.Drawing.Point(217, 19);
            this.comboBoxProgressBarEdge.Name = "comboBoxProgressBarEdge";
            this.comboBoxProgressBarEdge.Size = new System.Drawing.Size(188, 21);
            this.comboBoxProgressBarEdge.TabIndex = 26;
            // 
            // labelEdgeStyle
            // 
            this.labelEdgeStyle.AutoSize = true;
            this.labelEdgeStyle.Location = new System.Drawing.Point(181, 22);
            this.labelEdgeStyle.Name = "labelEdgeStyle";
            this.labelEdgeStyle.Size = new System.Drawing.Size(30, 13);
            this.labelEdgeStyle.TabIndex = 25;
            this.labelEdgeStyle.Text = "Style";
            // 
            // labelPreviewPleaseWait
            // 
            this.labelPreviewPleaseWait.AutoSize = true;
            this.labelPreviewPleaseWait.Location = new System.Drawing.Point(857, 35);
            this.labelPreviewPleaseWait.Name = "labelPreviewPleaseWait";
            this.labelPreviewPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPreviewPleaseWait.TabIndex = 28;
            this.labelPreviewPleaseWait.Text = "Please wait...";
            // 
            // buttonPreview
            // 
            this.buttonPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPreview.Location = new System.Drawing.Point(860, 9);
            this.buttonPreview.Name = "buttonPreview";
            this.buttonPreview.Size = new System.Drawing.Size(101, 23);
            this.buttonPreview.TabIndex = 27;
            this.buttonPreview.Text = "Preview";
            this.buttonPreview.UseVisualStyleBackColor = true;
            this.buttonPreview.Click += new System.EventHandler(this.buttonPreview_Click);
            // 
            // AssSetBackground
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(973, 734);
            this.Controls.Add(this.labelPreviewPleaseWait);
            this.Controls.Add(this.buttonPreview);
            this.Controls.Add(this.comboBoxProgressBarEdge);
            this.Controls.Add(this.labelEdgeStyle);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericUpDown2);
            this.Controls.Add(this.labelRotateX);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.numericUpDownRotateX);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelRotateY);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.numericUpDownRotateY);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(790, 545);
            this.Name = "AssSetBackground";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generate background box";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ApplyCustomStyles_FormClosing);
            this.Shown += new System.EventHandler(this.SetPosition_Shown);
            this.ResizeEnd += new System.EventHandler(this.SetPosition_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ApplyCustomStyles_KeyDown);
            this.groupBoxPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRotateY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.NumericUpDown numericUpDownRotateX;
        private System.Windows.Forms.Label labelRotateX;
        private System.Windows.Forms.NumericUpDown numericUpDownRotateY;
        private System.Windows.Forms.Label labelRotateY;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.ComboBox comboBoxProgressBarEdge;
        private System.Windows.Forms.Label labelEdgeStyle;
        private System.Windows.Forms.Label labelPreviewPleaseWait;
        private System.Windows.Forms.Button buttonPreview;
    }
}