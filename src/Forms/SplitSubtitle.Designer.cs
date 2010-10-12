namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class SplitSubtitle
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
            this.buttonGetFrameRate = new System.Windows.Forms.Button();
            this.buttonDone = new System.Windows.Forms.Button();
            this.buttonSplit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.labelHoursMinSecsMilliSecs = new System.Windows.Forms.Label();
            this.splitTimeUpDownAdjust = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.SuspendLayout();
            // 
            // buttonGetFrameRate
            // 
            this.buttonGetFrameRate.Location = new System.Drawing.Point(178, 102);
            this.buttonGetFrameRate.Name = "buttonGetFrameRate";
            this.buttonGetFrameRate.Size = new System.Drawing.Size(24, 22);
            this.buttonGetFrameRate.TabIndex = 8;
            this.buttonGetFrameRate.Text = "...";
            this.buttonGetFrameRate.UseVisualStyleBackColor = true;
            this.buttonGetFrameRate.Click += new System.EventHandler(this.buttonGetFrameRate_Click);
            // 
            // buttonDone
            // 
            this.buttonDone.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonDone.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDone.Location = new System.Drawing.Point(198, 156);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(75, 21);
            this.buttonDone.TabIndex = 18;
            this.buttonDone.Text = "&Done";
            this.buttonDone.UseVisualStyleBackColor = true;
            // 
            // buttonSplit
            // 
            this.buttonSplit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSplit.Location = new System.Drawing.Point(117, 156);
            this.buttonSplit.Name = "buttonSplit";
            this.buttonSplit.Size = new System.Drawing.Size(75, 21);
            this.buttonSplit.TabIndex = 17;
            this.buttonSplit.Text = "&Split";
            this.buttonSplit.UseVisualStyleBackColor = true;
            this.buttonSplit.Click += new System.EventHandler(this.ButtonFixClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Enter length of first part of video or browse";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(39, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(146, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "and get length from video file:";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // labelHoursMinSecsMilliSecs
            // 
            this.labelHoursMinSecsMilliSecs.AutoSize = true;
            this.labelHoursMinSecsMilliSecs.Location = new System.Drawing.Point(82, 88);
            this.labelHoursMinSecsMilliSecs.Name = "labelHoursMinSecsMilliSecs";
            this.labelHoursMinSecsMilliSecs.Size = new System.Drawing.Size(107, 13);
            this.labelHoursMinSecsMilliSecs.TabIndex = 22;
            this.labelHoursMinSecsMilliSecs.Text = "Hours:min:sec.msecs";
            // 
            // splitTimeUpDownAdjust
            // 
            this.splitTimeUpDownAdjust.AutoSize = true;
            this.splitTimeUpDownAdjust.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.splitTimeUpDownAdjust.Location = new System.Drawing.Point(83, 102);
            this.splitTimeUpDownAdjust.Name = "splitTimeUpDownAdjust";
            this.splitTimeUpDownAdjust.Size = new System.Drawing.Size(89, 24);
            this.splitTimeUpDownAdjust.TabIndex = 21;
            this.splitTimeUpDownAdjust.TimeCode = null;
            // 
            // SplitSubtitle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 182);
            this.Controls.Add(this.labelHoursMinSecsMilliSecs);
            this.Controls.Add(this.splitTimeUpDownAdjust);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonDone);
            this.Controls.Add(this.buttonSplit);
            this.Controls.Add(this.buttonGetFrameRate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplitSubtitle";
            this.Text = "Split subtitle";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormSplitSubtitle_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonGetFrameRate;
        private System.Windows.Forms.Button buttonDone;
        private System.Windows.Forms.Button buttonSplit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private Nikse.SubtitleEdit.Controls.TimeUpDown splitTimeUpDownAdjust;
        private System.Windows.Forms.Label labelHoursMinSecsMilliSecs;
    }
}