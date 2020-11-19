using Nikse.SubtitleEdit.Core.Common;

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
            TimeCode timeCode1 = new TimeCode();
            this.buttonGetFrameRate = new System.Windows.Forms.Button();
            this.buttonDone = new System.Windows.Forms.Button();
            this.buttonSplit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.labelHourMinSecMilliSecond = new System.Windows.Forms.Label();
            this.splitTimeUpDownAdjust = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.buttonAdvanced = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonGetFrameRate
            // 
            this.buttonGetFrameRate.Location = new System.Drawing.Point(137, 69);
            this.buttonGetFrameRate.Name = "buttonGetFrameRate";
            this.buttonGetFrameRate.Size = new System.Drawing.Size(25, 23);
            this.buttonGetFrameRate.TabIndex = 8;
            this.buttonGetFrameRate.Text = "...";
            this.buttonGetFrameRate.UseVisualStyleBackColor = true;
            this.buttonGetFrameRate.Click += new System.EventHandler(this.buttonGetFrameRate_Click);
            // 
            // buttonDone
            // 
            this.buttonDone.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonDone.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDone.Location = new System.Drawing.Point(239, 109);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(75, 23);
            this.buttonDone.TabIndex = 18;
            this.buttonDone.Text = "&Done";
            this.buttonDone.UseVisualStyleBackColor = true;
            // 
            // buttonSplit
            // 
            this.buttonSplit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSplit.Location = new System.Drawing.Point(158, 109);
            this.buttonSplit.Name = "buttonSplit";
            this.buttonSplit.Size = new System.Drawing.Size(75, 23);
            this.buttonSplit.TabIndex = 17;
            this.buttonSplit.Text = "&Split";
            this.buttonSplit.UseVisualStyleBackColor = true;
            this.buttonSplit.Click += new System.EventHandler(this.ButtonSplitClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(217, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Enter length of first part of video or browse";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(152, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "and get length from video file:";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // labelHourMinSecMilliSecond
            // 
            this.labelHourMinSecMilliSecond.AutoSize = true;
            this.labelHourMinSecMilliSecond.Location = new System.Drawing.Point(26, 52);
            this.labelHourMinSecMilliSecond.Name = "labelHourMinSecMilliSecond";
            this.labelHourMinSecMilliSecond.Size = new System.Drawing.Size(108, 13);
            this.labelHourMinSecMilliSecond.TabIndex = 22;
            this.labelHourMinSecMilliSecond.Text = "Hours:min:sec.msecs";
            // 
            // splitTimeUpDownAdjust
            // 
            this.splitTimeUpDownAdjust.AutoSize = true;
            this.splitTimeUpDownAdjust.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.splitTimeUpDownAdjust.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.splitTimeUpDownAdjust.Location = new System.Drawing.Point(29, 69);
            this.splitTimeUpDownAdjust.Margin = new System.Windows.Forms.Padding(4);
            this.splitTimeUpDownAdjust.Name = "splitTimeUpDownAdjust";
            this.splitTimeUpDownAdjust.Size = new System.Drawing.Size(96, 27);
            this.splitTimeUpDownAdjust.TabIndex = 21;
            timeCode1.Hours = 0;
            timeCode1.Milliseconds = 0;
            timeCode1.Minutes = 0;
            timeCode1.Seconds = 0;
            timeCode1.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode1.TotalMilliseconds = 0D;
            timeCode1.TotalSeconds = 0D;
            this.splitTimeUpDownAdjust.TimeCode = timeCode1;
            this.splitTimeUpDownAdjust.UseVideoOffset = false;
            // 
            // buttonAdvanced
            // 
            this.buttonAdvanced.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAdvanced.Location = new System.Drawing.Point(54, 109);
            this.buttonAdvanced.Name = "buttonAdvanced";
            this.buttonAdvanced.Size = new System.Drawing.Size(98, 23);
            this.buttonAdvanced.TabIndex = 23;
            this.buttonAdvanced.Text = "&Advanced";
            this.buttonAdvanced.UseVisualStyleBackColor = true;
            this.buttonAdvanced.Click += new System.EventHandler(this.buttonAdvanced_Click);
            // 
            // SplitSubtitle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 140);
            this.Controls.Add(this.buttonAdvanced);
            this.Controls.Add(this.buttonGetFrameRate);
            this.Controls.Add(this.splitTimeUpDownAdjust);
            this.Controls.Add(this.labelHourMinSecMilliSecond);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonDone);
            this.Controls.Add(this.buttonSplit);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplitSubtitle";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
        private System.Windows.Forms.Label labelHourMinSecMilliSecond;
        private System.Windows.Forms.Button buttonAdvanced;
    }
}