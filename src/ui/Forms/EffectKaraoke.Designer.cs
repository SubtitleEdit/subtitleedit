namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class EffectKaraoke
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
            this.labelTotalMilliseconds = new System.Windows.Forms.Label();
            this.labelTM = new System.Windows.Forms.Label();
            this.labelColor = new System.Windows.Forms.Label();
            this.labelEndDelay = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonChooseColor = new System.Windows.Forms.Button();
            this.panelColor = new System.Windows.Forms.Panel();
            this.labelChooseColor = new System.Windows.Forms.Label();
            this.buttonPreview = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.richTextBoxPreview = new System.Windows.Forms.RichTextBox();
            this.radioButtonByWordEffect = new System.Windows.Forms.RadioButton();
            this.radioButtonByCharEffect = new System.Windows.Forms.RadioButton();
            this.numericUpDownDelay = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.SuspendLayout();
            // 
            // labelTotalMilliseconds
            // 
            this.labelTotalMilliseconds.AutoSize = true;
            this.labelTotalMilliseconds.Location = new System.Drawing.Point(169, 52);
            this.labelTotalMilliseconds.Name = "labelTotalMilliseconds";
            this.labelTotalMilliseconds.Size = new System.Drawing.Size(108, 13);
            this.labelTotalMilliseconds.TabIndex = 5;
            this.labelTotalMilliseconds.Text = "labelTotalMilliseconds";
            // 
            // labelTM
            // 
            this.labelTM.Location = new System.Drawing.Point(3, 52);
            this.labelTM.Name = "labelTM";
            this.labelTM.Size = new System.Drawing.Size(163, 13);
            this.labelTM.TabIndex = 4;
            this.labelTM.Text = "Total milliseconds:";
            this.labelTM.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelColor
            // 
            this.labelColor.AutoSize = true;
            this.labelColor.Location = new System.Drawing.Point(228, 22);
            this.labelColor.Name = "labelColor";
            this.labelColor.Size = new System.Drawing.Size(32, 13);
            this.labelColor.TabIndex = 3;
            this.labelColor.Text = "Color";
            // 
            // labelEndDelay
            // 
            this.labelEndDelay.Location = new System.Drawing.Point(3, 76);
            this.labelEndDelay.Name = "labelEndDelay";
            this.labelEndDelay.Size = new System.Drawing.Size(163, 17);
            this.labelEndDelay.TabIndex = 6;
            this.labelEndDelay.Text = "End delay in milliseconds:";
            this.labelEndDelay.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.BackColor = System.Drawing.SystemColors.Control;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(421, 252);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(340, 252);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 12;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonChooseColor
            // 
            this.buttonChooseColor.Location = new System.Drawing.Point(169, 17);
            this.buttonChooseColor.Name = "buttonChooseColor";
            this.buttonChooseColor.Size = new System.Drawing.Size(27, 23);
            this.buttonChooseColor.TabIndex = 1;
            this.buttonChooseColor.Text = "...";
            this.buttonChooseColor.UseVisualStyleBackColor = true;
            this.buttonChooseColor.Click += new System.EventHandler(this.ButtonChooseColorClick);
            // 
            // panelColor
            // 
            this.panelColor.Location = new System.Drawing.Point(202, 19);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(20, 20);
            this.panelColor.TabIndex = 2;
            this.panelColor.TabStop = true;
            this.panelColor.Click += new System.EventHandler(this.ButtonChooseColorClick);
            // 
            // labelChooseColor
            // 
            this.labelChooseColor.Location = new System.Drawing.Point(0, 22);
            this.labelChooseColor.Name = "labelChooseColor";
            this.labelChooseColor.Size = new System.Drawing.Size(166, 13);
            this.labelChooseColor.TabIndex = 0;
            this.labelChooseColor.Text = "Choose color:";
            this.labelChooseColor.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // buttonPreview
            // 
            this.buttonPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPreview.Location = new System.Drawing.Point(12, 233);
            this.buttonPreview.Name = "buttonPreview";
            this.buttonPreview.Size = new System.Drawing.Size(100, 23);
            this.buttonPreview.TabIndex = 11;
            this.buttonPreview.Text = "Preview";
            this.buttonPreview.UseVisualStyleBackColor = true;
            this.buttonPreview.Click += new System.EventHandler(this.ButtonPreviewClick);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer1Tick);
            // 
            // richTextBoxPreview
            // 
            this.richTextBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxPreview.BackColor = System.Drawing.Color.Black;
            this.richTextBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxPreview.DetectUrls = false;
            this.richTextBoxPreview.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxPreview.ForeColor = System.Drawing.Color.White;
            this.richTextBoxPreview.Location = new System.Drawing.Point(13, 99);
            this.richTextBoxPreview.Name = "richTextBoxPreview";
            this.richTextBoxPreview.ReadOnly = true;
            this.richTextBoxPreview.Size = new System.Drawing.Size(483, 128);
            this.richTextBoxPreview.TabIndex = 10;
            this.richTextBoxPreview.TabStop = false;
            this.richTextBoxPreview.Text = "";
            // 
            // radioButtonByWordEffect
            // 
            this.radioButtonByWordEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonByWordEffect.AutoSize = true;
            this.radioButtonByWordEffect.Checked = true;
            this.radioButtonByWordEffect.Location = new System.Drawing.Point(375, 52);
            this.radioButtonByWordEffect.Name = "radioButtonByWordEffect";
            this.radioButtonByWordEffect.Size = new System.Drawing.Size(83, 17);
            this.radioButtonByWordEffect.TabIndex = 8;
            this.radioButtonByWordEffect.TabStop = true;
            this.radioButtonByWordEffect.Text = "Word effect";
            this.radioButtonByWordEffect.UseVisualStyleBackColor = true;
            // 
            // radioButtonByCharEffect
            // 
            this.radioButtonByCharEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonByCharEffect.AutoSize = true;
            this.radioButtonByCharEffect.Location = new System.Drawing.Point(375, 72);
            this.radioButtonByCharEffect.Name = "radioButtonByCharEffect";
            this.radioButtonByCharEffect.Size = new System.Drawing.Size(105, 17);
            this.radioButtonByCharEffect.TabIndex = 9;
            this.radioButtonByCharEffect.TabStop = true;
            this.radioButtonByCharEffect.Text = "Character effect";
            this.radioButtonByCharEffect.UseVisualStyleBackColor = true;
            // 
            // numericUpDownDelay
            // 
            this.numericUpDownDelay.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownDelay.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownDelay.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownDelay.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownDelay.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownDelay.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownDelay.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownDelay.DecimalPlaces = 3;
            this.numericUpDownDelay.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownDelay.Location = new System.Drawing.Point(169, 72);
            this.numericUpDownDelay.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numericUpDownDelay.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this.numericUpDownDelay.Name = "numericUpDownDelay";
            this.numericUpDownDelay.Size = new System.Drawing.Size(54, 23);
            this.numericUpDownDelay.TabIndex = 7;
            this.numericUpDownDelay.TabStop = false;
            this.numericUpDownDelay.ThousandsSeparator = false;
            this.numericUpDownDelay.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // EffectKaraoke
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(508, 287);
            this.Controls.Add(this.radioButtonByCharEffect);
            this.Controls.Add(this.radioButtonByWordEffect);
            this.Controls.Add(this.richTextBoxPreview);
            this.Controls.Add(this.labelTotalMilliseconds);
            this.Controls.Add(this.labelTM);
            this.Controls.Add(this.numericUpDownDelay);
            this.Controls.Add(this.labelColor);
            this.Controls.Add(this.labelEndDelay);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonChooseColor);
            this.Controls.Add(this.panelColor);
            this.Controls.Add(this.labelChooseColor);
            this.Controls.Add(this.buttonPreview);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EffectKaraoke";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Karaoke effect";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormEffectKaraoke_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTotalMilliseconds;
        private System.Windows.Forms.Label labelTM;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownDelay;
        private System.Windows.Forms.Label labelColor;
        private System.Windows.Forms.Label labelEndDelay;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonChooseColor;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Label labelChooseColor;
        private System.Windows.Forms.Button buttonPreview;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.RichTextBox richTextBoxPreview;
        private System.Windows.Forms.RadioButton radioButtonByWordEffect;
        private System.Windows.Forms.RadioButton radioButtonByCharEffect;
    }
}