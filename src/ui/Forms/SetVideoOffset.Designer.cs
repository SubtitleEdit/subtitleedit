﻿using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class SetVideoOffset
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
            Nikse.SubtitleEdit.Core.Common.TimeCode timeCode1 = new Nikse.SubtitleEdit.Core.Common.TimeCode();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelDescription = new System.Windows.Forms.Label();
            this.checkBoxFromCurrentPosition = new System.Windows.Forms.CheckBox();
            this.timeUpDownVideoPosition = new Nikse.SubtitleEdit.Controls.NikseTimeUpDown();
            this.buttonReset = new System.Windows.Forms.Button();
            this.checkBoxKeepTimeCodes = new System.Windows.Forms.CheckBox();
            this.buttonPickOffset = new System.Windows.Forms.Button();
            this.contextMenuStripDefaultOffsets = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(386, 129);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 22;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(194, 129);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 20;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelDescription.Location = new System.Drawing.Point(12, 22);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(81, 13);
            this.labelDescription.TabIndex = 0;
            this.labelDescription.Text = "Set video offset";
            // 
            // checkBoxFromCurrentPosition
            // 
            this.checkBoxFromCurrentPosition.AutoSize = true;
            this.checkBoxFromCurrentPosition.Location = new System.Drawing.Point(15, 71);
            this.checkBoxFromCurrentPosition.Name = "checkBoxFromCurrentPosition";
            this.checkBoxFromCurrentPosition.Size = new System.Drawing.Size(153, 17);
            this.checkBoxFromCurrentPosition.TabIndex = 2;
            this.checkBoxFromCurrentPosition.Text = "From current video position";
            this.checkBoxFromCurrentPosition.UseVisualStyleBackColor = true;
            // 
            // timeUpDownVideoPosition
            // 
            this.timeUpDownVideoPosition.BackColor = System.Drawing.SystemColors.Window;
            this.timeUpDownVideoPosition.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.timeUpDownVideoPosition.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.timeUpDownVideoPosition.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.timeUpDownVideoPosition.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.timeUpDownVideoPosition.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.timeUpDownVideoPosition.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.timeUpDownVideoPosition.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.timeUpDownVideoPosition.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.timeUpDownVideoPosition.Location = new System.Drawing.Point(13, 39);
            this.timeUpDownVideoPosition.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownVideoPosition.Name = "timeUpDownVideoPosition";
            this.timeUpDownVideoPosition.Size = new System.Drawing.Size(111, 23);
            this.timeUpDownVideoPosition.TabIndex = 0;
            this.timeUpDownVideoPosition.TabStop = false;
            timeCode1.Hours = 0;
            timeCode1.Milliseconds = 0;
            timeCode1.Minutes = 0;
            timeCode1.Seconds = 0;
            timeCode1.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode1.TotalMilliseconds = 0D;
            timeCode1.TotalSeconds = 0D;
            this.timeUpDownVideoPosition.TimeCode = timeCode1;
            this.timeUpDownVideoPosition.UseVideoOffset = false;
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonReset.Location = new System.Drawing.Point(275, 129);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(105, 23);
            this.buttonReset.TabIndex = 21;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // checkBoxKeepTimeCodes
            // 
            this.checkBoxKeepTimeCodes.AutoSize = true;
            this.checkBoxKeepTimeCodes.Location = new System.Drawing.Point(15, 94);
            this.checkBoxKeepTimeCodes.Name = "checkBoxKeepTimeCodes";
            this.checkBoxKeepTimeCodes.Size = new System.Drawing.Size(261, 17);
            this.checkBoxKeepTimeCodes.TabIndex = 3;
            this.checkBoxKeepTimeCodes.Text = "Keep existing time codes (do not add video offset)";
            this.checkBoxKeepTimeCodes.UseVisualStyleBackColor = true;
            // 
            // buttonPickOffset
            // 
            this.buttonPickOffset.Location = new System.Drawing.Point(131, 40);
            this.buttonPickOffset.Name = "buttonPickOffset";
            this.buttonPickOffset.Size = new System.Drawing.Size(25, 23);
            this.buttonPickOffset.TabIndex = 1;
            this.buttonPickOffset.Text = "...";
            this.buttonPickOffset.UseVisualStyleBackColor = true;
            this.buttonPickOffset.Click += new System.EventHandler(this.buttonPickOffset_Click);
            // 
            // contextMenuStripDefaultOffsets
            // 
            this.contextMenuStripDefaultOffsets.Name = "contextMenuStripRes";
            this.contextMenuStripDefaultOffsets.Size = new System.Drawing.Size(61, 4);
            // 
            // SetVideoOffset
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(473, 162);
            this.Controls.Add(this.buttonPickOffset);
            this.Controls.Add(this.checkBoxKeepTimeCodes);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.checkBoxFromCurrentPosition);
            this.Controls.Add(this.timeUpDownVideoPosition);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelDescription);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetVideoOffset";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SetVideoOffset";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SetVideoOffset_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelDescription;
        private Controls.NikseTimeUpDown timeUpDownVideoPosition;
        private System.Windows.Forms.CheckBox checkBoxFromCurrentPosition;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.CheckBox checkBoxKeepTimeCodes;
        private System.Windows.Forms.Button buttonPickOffset;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripDefaultOffsets;
    }
}