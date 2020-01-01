using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ShowEarlierLater
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
            Nikse.SubtitleEdit.Core.TimeCode timeCode1 = new Nikse.SubtitleEdit.Core.TimeCode();
            this.labelHourMinSecMilliSecond = new System.Windows.Forms.Label();
            this.buttonShowLater = new System.Windows.Forms.Button();
            this.buttonShowEarlier = new System.Windows.Forms.Button();
            this.labelTotalAdjustment = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.radioButtonAllLines = new System.Windows.Forms.RadioButton();
            this.radioButtonSelectedLinesOnly = new System.Windows.Forms.RadioButton();
            this.timeUpDownAdjust = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.radioButtonSelectedLineAndForward = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // labelHourMinSecMilliSecond
            // 
            this.labelHourMinSecMilliSecond.AutoSize = true;
            this.labelHourMinSecMilliSecond.Location = new System.Drawing.Point(11, 6);
            this.labelHourMinSecMilliSecond.Name = "labelHourMinSecMilliSecond";
            this.labelHourMinSecMilliSecond.Size = new System.Drawing.Size(108, 13);
            this.labelHourMinSecMilliSecond.TabIndex = 18;
            this.labelHourMinSecMilliSecond.Text = "Hours:min:sec.msecs";
            // 
            // buttonShowLater
            // 
            this.buttonShowLater.Location = new System.Drawing.Point(145, 55);
            this.buttonShowLater.Name = "buttonShowLater";
            this.buttonShowLater.Size = new System.Drawing.Size(119, 23);
            this.buttonShowLater.TabIndex = 20;
            this.buttonShowLater.Text = "Show later";
            this.buttonShowLater.UseVisualStyleBackColor = true;
            this.buttonShowLater.Click += new System.EventHandler(this.ButtonShowLaterClick);
            // 
            // buttonShowEarlier
            // 
            this.buttonShowEarlier.Location = new System.Drawing.Point(145, 26);
            this.buttonShowEarlier.Name = "buttonShowEarlier";
            this.buttonShowEarlier.Size = new System.Drawing.Size(120, 23);
            this.buttonShowEarlier.TabIndex = 19;
            this.buttonShowEarlier.Text = "Show earlier";
            this.buttonShowEarlier.UseVisualStyleBackColor = true;
            this.buttonShowEarlier.Click += new System.EventHandler(this.ButtonShowEarlierClick);
            // 
            // labelTotalAdjustment
            // 
            this.labelTotalAdjustment.AutoSize = true;
            this.labelTotalAdjustment.Location = new System.Drawing.Point(9, 148);
            this.labelTotalAdjustment.Name = "labelTotalAdjustment";
            this.labelTotalAdjustment.Size = new System.Drawing.Size(108, 13);
            this.labelTotalAdjustment.TabIndex = 38;
            this.labelTotalAdjustment.Text = "labelTotalAdjustment";
            // 
            // timer1
            // 
            this.timer1.Interval = 250;
            // 
            // radioButtonAllLines
            // 
            this.radioButtonAllLines.AutoSize = true;
            this.radioButtonAllLines.Location = new System.Drawing.Point(14, 70);
            this.radioButtonAllLines.Name = "radioButtonAllLines";
            this.radioButtonAllLines.Size = new System.Drawing.Size(60, 17);
            this.radioButtonAllLines.TabIndex = 39;
            this.radioButtonAllLines.TabStop = true;
            this.radioButtonAllLines.Text = "All lines";
            this.radioButtonAllLines.UseVisualStyleBackColor = true;
            this.radioButtonAllLines.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // radioButtonSelectedLinesOnly
            // 
            this.radioButtonSelectedLinesOnly.AutoSize = true;
            this.radioButtonSelectedLinesOnly.Location = new System.Drawing.Point(14, 93);
            this.radioButtonSelectedLinesOnly.Name = "radioButtonSelectedLinesOnly";
            this.radioButtonSelectedLinesOnly.Size = new System.Drawing.Size(113, 17);
            this.radioButtonSelectedLinesOnly.TabIndex = 40;
            this.radioButtonSelectedLinesOnly.TabStop = true;
            this.radioButtonSelectedLinesOnly.Text = "Selected lines only";
            this.radioButtonSelectedLinesOnly.UseVisualStyleBackColor = true;
            this.radioButtonSelectedLinesOnly.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // timeUpDownAdjust
            // 
            this.timeUpDownAdjust.AutoSize = true;
            this.timeUpDownAdjust.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownAdjust.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.timeUpDownAdjust.Location = new System.Drawing.Point(12, 23);
            this.timeUpDownAdjust.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownAdjust.Name = "timeUpDownAdjust";
            this.timeUpDownAdjust.Size = new System.Drawing.Size(96, 27);
            this.timeUpDownAdjust.TabIndex = 21;
            timeCode1.Hours = 0;
            timeCode1.Milliseconds = 0;
            timeCode1.Minutes = 0;
            timeCode1.Seconds = 0;
            timeCode1.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode1.TotalMilliseconds = 0D;
            timeCode1.TotalSeconds = 0D;
            this.timeUpDownAdjust.TimeCode = timeCode1;
            this.timeUpDownAdjust.UseVideoOffset = false;
            // 
            // radioButtonSelectedLineAndForward
            // 
            this.radioButtonSelectedLineAndForward.AutoSize = true;
            this.radioButtonSelectedLineAndForward.Location = new System.Drawing.Point(14, 116);
            this.radioButtonSelectedLineAndForward.Name = "radioButtonSelectedLineAndForward";
            this.radioButtonSelectedLineAndForward.Size = new System.Drawing.Size(160, 17);
            this.radioButtonSelectedLineAndForward.TabIndex = 41;
            this.radioButtonSelectedLineAndForward.TabStop = true;
            this.radioButtonSelectedLineAndForward.Text = "Selected line(s) and forward";
            this.radioButtonSelectedLineAndForward.UseVisualStyleBackColor = true;
            this.radioButtonSelectedLineAndForward.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // ShowEarlierLater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(301, 170);
            this.Controls.Add(this.radioButtonSelectedLineAndForward);
            this.Controls.Add(this.radioButtonSelectedLinesOnly);
            this.Controls.Add(this.radioButtonAllLines);
            this.Controls.Add(this.buttonShowLater);
            this.Controls.Add(this.buttonShowEarlier);
            this.Controls.Add(this.labelTotalAdjustment);
            this.Controls.Add(this.timeUpDownAdjust);
            this.Controls.Add(this.labelHourMinSecMilliSecond);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShowEarlierLater";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Show selected lines earlier/later";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShowEarlierLater_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ShowEarlierLater_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Nikse.SubtitleEdit.Controls.TimeUpDown timeUpDownAdjust;
        private System.Windows.Forms.Label labelHourMinSecMilliSecond;
        private System.Windows.Forms.Button buttonShowLater;
        private System.Windows.Forms.Button buttonShowEarlier;
        private System.Windows.Forms.Label labelTotalAdjustment;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.RadioButton radioButtonAllLines;
        private System.Windows.Forms.RadioButton radioButtonSelectedLinesOnly;
        private System.Windows.Forms.RadioButton radioButtonSelectedLineAndForward;
    }
}