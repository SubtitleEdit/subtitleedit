namespace Nikse.SubtitleEdit.Forms
{
    partial class NetworkLogAndInfo
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
            this.textBoxSessionKey = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelLog = new System.Windows.Forms.Label();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.labelGoToLine = new System.Windows.Forms.Label();
            this.textBoxWebServiceUrl = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxSessionKey
            // 
            this.textBoxSessionKey.Location = new System.Drawing.Point(157, 16);
            this.textBoxSessionKey.Name = "textBoxSessionKey";
            this.textBoxSessionKey.ReadOnly = true;
            this.textBoxSessionKey.Size = new System.Drawing.Size(290, 20);
            this.textBoxSessionKey.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(87, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "Session key";
            // 
            // labelLog
            // 
            this.labelLog.AutoSize = true;
            this.labelLog.Location = new System.Drawing.Point(9, 109);
            this.labelLog.Name = "labelLog";
            this.labelLog.Size = new System.Drawing.Size(28, 13);
            this.labelLog.TabIndex = 24;
            this.labelLog.Text = "Log:";
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(12, 125);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.Size = new System.Drawing.Size(543, 179);
            this.textBoxLog.TabIndex = 4;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConnect.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonConnect.Location = new System.Drawing.Point(480, 310);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 21);
            this.buttonConnect.TabIndex = 0;
            this.buttonConnect.Text = "&OK";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(70, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 29;
            this.label1.Text = "Web service url";
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Location = new System.Drawing.Point(157, 42);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.ReadOnly = true;
            this.textBoxUserName.Size = new System.Drawing.Size(290, 20);
            this.textBoxUserName.TabIndex = 2;
            // 
            // labelGoToLine
            // 
            this.labelGoToLine.AutoSize = true;
            this.labelGoToLine.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelGoToLine.Location = new System.Drawing.Point(96, 45);
            this.labelGoToLine.Name = "labelGoToLine";
            this.labelGoToLine.Size = new System.Drawing.Size(55, 13);
            this.labelGoToLine.TabIndex = 28;
            this.labelGoToLine.Text = "Username";
            // 
            // textBoxWebServiceUrl
            // 
            this.textBoxWebServiceUrl.Location = new System.Drawing.Point(157, 68);
            this.textBoxWebServiceUrl.Name = "textBoxWebServiceUrl";
            this.textBoxWebServiceUrl.ReadOnly = true;
            this.textBoxWebServiceUrl.Size = new System.Drawing.Size(290, 20);
            this.textBoxWebServiceUrl.TabIndex = 3;
            // 
            // NetworkLogAndInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 337);
            this.Controls.Add(this.textBoxWebServiceUrl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(this.labelGoToLine);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.labelLog);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.textBoxSessionKey);
            this.Controls.Add(this.label2);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 350);
            this.Name = "NetworkLogAndInfo";
            this.ShowIcon = false;
            this.Text = "Network session info and log";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NetworkLogAndInfo_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxSessionKey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelLog;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Label labelGoToLine;
        private System.Windows.Forms.TextBox textBoxWebServiceUrl;
    }
}