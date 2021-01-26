namespace Nikse.SubtitleEdit.Forms.Options
{
    partial class SettingsMpvPreview
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
            this.panelSsaStyle = new System.Windows.Forms.Panel();
            this.groupBoxSsaStyle = new System.Windows.Forms.GroupBox();
            this.groupBoxSsaBorder = new System.Windows.Forms.GroupBox();
            this.numericUpDownSsaOutline = new System.Windows.Forms.NumericUpDown();
            this.labelSsaShadow = new System.Windows.Forms.Label();
            this.numericUpDownSsaShadow = new System.Windows.Forms.NumericUpDown();
            this.checkBoxSsaOpaqueBox = new System.Windows.Forms.CheckBox();
            this.labelSsaOutline = new System.Windows.Forms.Label();
            this.groupBoxSsaColors = new System.Windows.Forms.GroupBox();
            this.panelBackColor = new System.Windows.Forms.Panel();
            this.buttonBackColor = new System.Windows.Forms.Button();
            this.panelOutlineColor = new System.Windows.Forms.Panel();
            this.buttonOutlineColor = new System.Windows.Forms.Button();
            this.panelPrimaryColor = new System.Windows.Forms.Panel();
            this.buttonPrimaryColor = new System.Windows.Forms.Button();
            this.groupBoxMargins = new System.Windows.Forms.GroupBox();
            this.numericUpDownSsaMarginVertical = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownSsaMarginRight = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownSsaMarginLeft = new System.Windows.Forms.NumericUpDown();
            this.labelMarginVertical = new System.Windows.Forms.Label();
            this.labelMarginRight = new System.Windows.Forms.Label();
            this.labelMarginLeft = new System.Windows.Forms.Label();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.colorDialogSSAStyle = new System.Windows.Forms.ColorDialog();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panelSsaStyle.SuspendLayout();
            this.groupBoxSsaStyle.SuspendLayout();
            this.groupBoxSsaBorder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaOutline)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaShadow)).BeginInit();
            this.groupBoxSsaColors.SuspendLayout();
            this.groupBoxMargins.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaMarginVertical)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaMarginRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaMarginLeft)).BeginInit();
            this.groupBoxPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // panelSsaStyle
            // 
            this.panelSsaStyle.Controls.Add(this.groupBoxSsaStyle);
            this.panelSsaStyle.Location = new System.Drawing.Point(8, 7);
            this.panelSsaStyle.Name = "panelSsaStyle";
            this.panelSsaStyle.Padding = new System.Windows.Forms.Padding(3);
            this.panelSsaStyle.Size = new System.Drawing.Size(864, 533);
            this.panelSsaStyle.TabIndex = 0;
            this.panelSsaStyle.Text = "SSA style";
            // 
            // groupBoxSsaStyle
            // 
            this.groupBoxSsaStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSsaStyle.Controls.Add(this.groupBoxSsaBorder);
            this.groupBoxSsaStyle.Controls.Add(this.groupBoxSsaColors);
            this.groupBoxSsaStyle.Controls.Add(this.groupBoxMargins);
            this.groupBoxSsaStyle.Controls.Add(this.groupBoxPreview);
            this.groupBoxSsaStyle.Location = new System.Drawing.Point(0, 0);
            this.groupBoxSsaStyle.Name = "groupBoxSsaStyle";
            this.groupBoxSsaStyle.Size = new System.Drawing.Size(851, 533);
            this.groupBoxSsaStyle.TabIndex = 0;
            this.groupBoxSsaStyle.Text = "Sub Station Alpha style";
            // 
            // groupBoxSsaBorder
            // 
            this.groupBoxSsaBorder.Controls.Add(this.numericUpDownSsaOutline);
            this.groupBoxSsaBorder.Controls.Add(this.labelSsaShadow);
            this.groupBoxSsaBorder.Controls.Add(this.numericUpDownSsaShadow);
            this.groupBoxSsaBorder.Controls.Add(this.checkBoxSsaOpaqueBox);
            this.groupBoxSsaBorder.Controls.Add(this.labelSsaOutline);
            this.groupBoxSsaBorder.Location = new System.Drawing.Point(343, 20);
            this.groupBoxSsaBorder.Name = "groupBoxSsaBorder";
            this.groupBoxSsaBorder.Size = new System.Drawing.Size(210, 108);
            this.groupBoxSsaBorder.TabIndex = 1;
            this.groupBoxSsaBorder.TabStop = false;
            this.groupBoxSsaBorder.Text = "Border";
            // 
            // numericUpDownSsaOutline
            // 
            this.numericUpDownSsaOutline.Location = new System.Drawing.Point(64, 16);
            this.numericUpDownSsaOutline.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numericUpDownSsaOutline.Name = "numericUpDownSsaOutline";
            this.numericUpDownSsaOutline.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownSsaOutline.TabIndex = 5;
            this.numericUpDownSsaOutline.ValueChanged += new System.EventHandler(this.numericUpDownSsaOutline_ValueChanged);
            // 
            // labelSsaShadow
            // 
            this.labelSsaShadow.AutoSize = true;
            this.labelSsaShadow.Location = new System.Drawing.Point(10, 45);
            this.labelSsaShadow.Name = "labelSsaShadow";
            this.labelSsaShadow.Size = new System.Drawing.Size(45, 13);
            this.labelSsaShadow.TabIndex = 6;
            this.labelSsaShadow.Text = "Shadow";
            // 
            // numericUpDownSsaShadow
            // 
            this.numericUpDownSsaShadow.Location = new System.Drawing.Point(64, 43);
            this.numericUpDownSsaShadow.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numericUpDownSsaShadow.Name = "numericUpDownSsaShadow";
            this.numericUpDownSsaShadow.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownSsaShadow.TabIndex = 7;
            this.numericUpDownSsaShadow.ValueChanged += new System.EventHandler(this.numericUpDownSsaShadow_ValueChanged);
            // 
            // checkBoxSsaOpaqueBox
            // 
            this.checkBoxSsaOpaqueBox.AutoSize = true;
            this.checkBoxSsaOpaqueBox.Location = new System.Drawing.Point(13, 71);
            this.checkBoxSsaOpaqueBox.Name = "checkBoxSsaOpaqueBox";
            this.checkBoxSsaOpaqueBox.Size = new System.Drawing.Size(85, 17);
            this.checkBoxSsaOpaqueBox.TabIndex = 8;
            this.checkBoxSsaOpaqueBox.Text = "Opaque box";
            this.checkBoxSsaOpaqueBox.UseVisualStyleBackColor = true;
            this.checkBoxSsaOpaqueBox.CheckedChanged += new System.EventHandler(this.checkBoxSsaOpaqueBox_CheckedChanged);
            // 
            // labelSsaOutline
            // 
            this.labelSsaOutline.AutoSize = true;
            this.labelSsaOutline.Location = new System.Drawing.Point(10, 20);
            this.labelSsaOutline.Name = "labelSsaOutline";
            this.labelSsaOutline.Size = new System.Drawing.Size(41, 13);
            this.labelSsaOutline.TabIndex = 4;
            this.labelSsaOutline.Text = "Outline";
            // 
            // groupBoxSsaColors
            // 
            this.groupBoxSsaColors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSsaColors.Controls.Add(this.panelBackColor);
            this.groupBoxSsaColors.Controls.Add(this.buttonBackColor);
            this.groupBoxSsaColors.Controls.Add(this.panelOutlineColor);
            this.groupBoxSsaColors.Controls.Add(this.buttonOutlineColor);
            this.groupBoxSsaColors.Controls.Add(this.panelPrimaryColor);
            this.groupBoxSsaColors.Controls.Add(this.buttonPrimaryColor);
            this.groupBoxSsaColors.Location = new System.Drawing.Point(6, 20);
            this.groupBoxSsaColors.Name = "groupBoxSsaColors";
            this.groupBoxSsaColors.Size = new System.Drawing.Size(330, 108);
            this.groupBoxSsaColors.TabIndex = 0;
            this.groupBoxSsaColors.TabStop = false;
            this.groupBoxSsaColors.Text = "Colors";
            // 
            // panelBackColor
            // 
            this.panelBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBackColor.Location = new System.Drawing.Point(121, 79);
            this.panelBackColor.Name = "panelBackColor";
            this.panelBackColor.Size = new System.Drawing.Size(21, 20);
            this.panelBackColor.TabIndex = 5;
            this.panelBackColor.Click += new System.EventHandler(this.buttonShadowColor_Click);
            // 
            // buttonBackColor
            // 
            this.buttonBackColor.Location = new System.Drawing.Point(6, 77);
            this.buttonBackColor.Name = "buttonBackColor";
            this.buttonBackColor.Size = new System.Drawing.Size(109, 23);
            this.buttonBackColor.TabIndex = 4;
            this.buttonBackColor.Text = "Shadow";
            this.buttonBackColor.UseVisualStyleBackColor = true;
            this.buttonBackColor.Click += new System.EventHandler(this.buttonShadowColor_Click);
            // 
            // panelOutlineColor
            // 
            this.panelOutlineColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelOutlineColor.Location = new System.Drawing.Point(121, 50);
            this.panelOutlineColor.Name = "panelOutlineColor";
            this.panelOutlineColor.Size = new System.Drawing.Size(21, 20);
            this.panelOutlineColor.TabIndex = 3;
            this.panelOutlineColor.Click += new System.EventHandler(this.buttonOutlineColor_Click);
            // 
            // buttonOutlineColor
            // 
            this.buttonOutlineColor.Location = new System.Drawing.Point(6, 48);
            this.buttonOutlineColor.Name = "buttonOutlineColor";
            this.buttonOutlineColor.Size = new System.Drawing.Size(109, 23);
            this.buttonOutlineColor.TabIndex = 2;
            this.buttonOutlineColor.Text = "Outline";
            this.buttonOutlineColor.UseVisualStyleBackColor = true;
            this.buttonOutlineColor.Click += new System.EventHandler(this.buttonOutlineColor_Click);
            // 
            // panelPrimaryColor
            // 
            this.panelPrimaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPrimaryColor.Location = new System.Drawing.Point(121, 21);
            this.panelPrimaryColor.Name = "panelPrimaryColor";
            this.panelPrimaryColor.Size = new System.Drawing.Size(21, 20);
            this.panelPrimaryColor.TabIndex = 1;
            this.panelPrimaryColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.buttonPrimaryColor_Click);
            // 
            // buttonPrimaryColor
            // 
            this.buttonPrimaryColor.Location = new System.Drawing.Point(6, 19);
            this.buttonPrimaryColor.Name = "buttonPrimaryColor";
            this.buttonPrimaryColor.Size = new System.Drawing.Size(109, 23);
            this.buttonPrimaryColor.TabIndex = 0;
            this.buttonPrimaryColor.Text = "Primary";
            this.buttonPrimaryColor.UseVisualStyleBackColor = true;
            this.buttonPrimaryColor.Click += new System.EventHandler(this.buttonPrimaryColor_Click);
            // 
            // groupBoxMargins
            // 
            this.groupBoxMargins.Controls.Add(this.numericUpDownSsaMarginVertical);
            this.groupBoxMargins.Controls.Add(this.numericUpDownSsaMarginRight);
            this.groupBoxMargins.Controls.Add(this.numericUpDownSsaMarginLeft);
            this.groupBoxMargins.Controls.Add(this.labelMarginVertical);
            this.groupBoxMargins.Controls.Add(this.labelMarginRight);
            this.groupBoxMargins.Controls.Add(this.labelMarginLeft);
            this.groupBoxMargins.Location = new System.Drawing.Point(560, 20);
            this.groupBoxMargins.Name = "groupBoxMargins";
            this.groupBoxMargins.Size = new System.Drawing.Size(281, 108);
            this.groupBoxMargins.TabIndex = 3;
            this.groupBoxMargins.TabStop = false;
            this.groupBoxMargins.Text = "Margins";
            // 
            // numericUpDownSsaMarginVertical
            // 
            this.numericUpDownSsaMarginVertical.Location = new System.Drawing.Point(168, 33);
            this.numericUpDownSsaMarginVertical.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownSsaMarginVertical.Name = "numericUpDownSsaMarginVertical";
            this.numericUpDownSsaMarginVertical.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownSsaMarginVertical.TabIndex = 5;
            this.numericUpDownSsaMarginVertical.ValueChanged += new System.EventHandler(this.numericUpDownSsaMarginVertical_ValueChanged);
            // 
            // numericUpDownSsaMarginRight
            // 
            this.numericUpDownSsaMarginRight.Location = new System.Drawing.Point(93, 33);
            this.numericUpDownSsaMarginRight.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownSsaMarginRight.Name = "numericUpDownSsaMarginRight";
            this.numericUpDownSsaMarginRight.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownSsaMarginRight.TabIndex = 3;
            // 
            // numericUpDownSsaMarginLeft
            // 
            this.numericUpDownSsaMarginLeft.Location = new System.Drawing.Point(16, 33);
            this.numericUpDownSsaMarginLeft.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownSsaMarginLeft.Name = "numericUpDownSsaMarginLeft";
            this.numericUpDownSsaMarginLeft.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownSsaMarginLeft.TabIndex = 1;
            // 
            // labelMarginVertical
            // 
            this.labelMarginVertical.AutoSize = true;
            this.labelMarginVertical.Location = new System.Drawing.Point(165, 17);
            this.labelMarginVertical.Name = "labelMarginVertical";
            this.labelMarginVertical.Size = new System.Drawing.Size(42, 13);
            this.labelMarginVertical.TabIndex = 4;
            this.labelMarginVertical.Text = "Vertical";
            // 
            // labelMarginRight
            // 
            this.labelMarginRight.AutoSize = true;
            this.labelMarginRight.Location = new System.Drawing.Point(90, 16);
            this.labelMarginRight.Name = "labelMarginRight";
            this.labelMarginRight.Size = new System.Drawing.Size(32, 13);
            this.labelMarginRight.TabIndex = 2;
            this.labelMarginRight.Text = "Right";
            // 
            // labelMarginLeft
            // 
            this.labelMarginLeft.AutoSize = true;
            this.labelMarginLeft.Location = new System.Drawing.Point(13, 16);
            this.labelMarginLeft.Name = "labelMarginLeft";
            this.labelMarginLeft.Size = new System.Drawing.Size(26, 13);
            this.labelMarginLeft.TabIndex = 0;
            this.labelMarginLeft.Text = "Left";
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.pictureBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(6, 130);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(839, 397);
            this.groupBoxPreview.TabIndex = 4;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 17);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(833, 377);
            this.pictureBoxPreview.TabIndex = 0;
            this.pictureBoxPreview.TabStop = false;
            this.pictureBoxPreview.Click += new System.EventHandler(this.pictureBoxPreview_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(702, 545);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(783, 545);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // SettingsMpvPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(864, 574);
            this.Controls.Add(this.panelSsaStyle);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsMpvPreview";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings for mpv preview";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SettingsMpvPreview_KeyDown);
            this.panelSsaStyle.ResumeLayout(false);
            this.groupBoxSsaStyle.ResumeLayout(false);
            this.groupBoxSsaBorder.ResumeLayout(false);
            this.groupBoxSsaBorder.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaOutline)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaShadow)).EndInit();
            this.groupBoxSsaColors.ResumeLayout(false);
            this.groupBoxMargins.ResumeLayout(false);
            this.groupBoxMargins.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaMarginVertical)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaMarginRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaMarginLeft)).EndInit();
            this.groupBoxPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelSsaStyle;
        private System.Windows.Forms.GroupBox groupBoxSsaStyle;
        private System.Windows.Forms.GroupBox groupBoxSsaBorder;
        private System.Windows.Forms.GroupBox groupBoxSsaColors;
        private System.Windows.Forms.GroupBox groupBoxMargins;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.NumericUpDown numericUpDownSsaOutline;
        private System.Windows.Forms.Label labelSsaShadow;
        private System.Windows.Forms.NumericUpDown numericUpDownSsaShadow;
        private System.Windows.Forms.CheckBox checkBoxSsaOpaqueBox;
        private System.Windows.Forms.Label labelSsaOutline;
        private System.Windows.Forms.Button buttonPrimaryColor;
        private System.Windows.Forms.Panel panelPrimaryColor;
        private System.Windows.Forms.Button buttonBackColor;
        private System.Windows.Forms.Panel panelBackColor;
        private System.Windows.Forms.Button buttonOutlineColor;
        private System.Windows.Forms.Panel panelOutlineColor;
        private System.Windows.Forms.NumericUpDown numericUpDownSsaMarginVertical;
        private System.Windows.Forms.NumericUpDown numericUpDownSsaMarginRight;
        private System.Windows.Forms.NumericUpDown numericUpDownSsaMarginLeft;
        private System.Windows.Forms.Label labelMarginVertical;
        private System.Windows.Forms.Label labelMarginRight;
        private System.Windows.Forms.Label labelMarginLeft;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.ColorDialog colorDialogSSAStyle;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}