﻿namespace Nikse.SubtitleEdit.Forms
{
    partial class NetworkStart
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.labelUserName = new System.Windows.Forms.Label();
            this.comboBoxWebServiceUrl = new System.Windows.Forms.ComboBox();
            this.labelWebServiceUrl = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelSessionKey = new System.Windows.Forms.Label();
            this.textBoxSessionKey = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(312, 147);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Location = new System.Drawing.Point(125, 83);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(262, 20);
            this.textBoxUserName.TabIndex = 1;
            // 
            // buttonStart
            // 
            this.buttonStart.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonStart.Location = new System.Drawing.Point(231, 147);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 21);
            this.buttonStart.TabIndex = 3;
            this.buttonStart.Text = "&Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // labelUserName
            // 
            this.labelUserName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelUserName.Location = new System.Drawing.Point(5, 86);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(114, 16);
            this.labelUserName.TabIndex = 3;
            this.labelUserName.Text = "Username";
            this.labelUserName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // comboBoxWebServiceUrl
            // 
            this.comboBoxWebServiceUrl.FormattingEnabled = true;
            this.comboBoxWebServiceUrl.Items.AddRange(new object[] {
            "http://www.nikse.dk/se/SeService.asmx",
            "http://se-net.azurewebsites.net/seService.asmx"});
            this.comboBoxWebServiceUrl.Location = new System.Drawing.Point(125, 109);
            this.comboBoxWebServiceUrl.Name = "comboBoxWebServiceUrl";
            this.comboBoxWebServiceUrl.Size = new System.Drawing.Size(262, 21);
            this.comboBoxWebServiceUrl.TabIndex = 2;
            // 
            // labelWebServiceUrl
            // 
            this.labelWebServiceUrl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelWebServiceUrl.Location = new System.Drawing.Point(5, 112);
            this.labelWebServiceUrl.Name = "labelWebServiceUrl";
            this.labelWebServiceUrl.Size = new System.Drawing.Size(114, 16);
            this.labelWebServiceUrl.TabIndex = 8;
            this.labelWebServiceUrl.Text = "Web service URL";
            this.labelWebServiceUrl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(16, 13);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(200, 26);
            this.labelInfo.TabIndex = 9;
            this.labelInfo.Text = "Start new session where multiple persons\r\ncan edit in same subtitle file.\r\n";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(16, 167);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 10;
            this.labelStatus.Text = "labelStatus";
            // 
            // labelSessionKey
            // 
            this.labelSessionKey.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelSessionKey.Location = new System.Drawing.Point(5, 60);
            this.labelSessionKey.Name = "labelSessionKey";
            this.labelSessionKey.Size = new System.Drawing.Size(114, 16);
            this.labelSessionKey.TabIndex = 11;
            this.labelSessionKey.Text = "Session Key";
            this.labelSessionKey.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // textBoxSessionKey
            // 
            this.textBoxSessionKey.Location = new System.Drawing.Point(125, 57);
            this.textBoxSessionKey.Name = "textBoxSessionKey";
            this.textBoxSessionKey.Size = new System.Drawing.Size(262, 20);
            this.textBoxSessionKey.TabIndex = 0;
            // 
            // NetworkStart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 185);
            this.Controls.Add(this.textBoxSessionKey);
            this.Controls.Add(this.labelSessionKey);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelWebServiceUrl);
            this.Controls.Add(this.comboBoxWebServiceUrl);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.labelUserName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NetworkStart";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Start network session";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NetworkNew_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.ComboBox comboBoxWebServiceUrl;
        private System.Windows.Forms.Label labelWebServiceUrl;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelSessionKey;
        private System.Windows.Forms.TextBox textBoxSessionKey;
    }
}