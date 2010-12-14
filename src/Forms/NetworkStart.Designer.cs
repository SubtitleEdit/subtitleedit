namespace Nikse.SubtitleEdit.Forms
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
            this.buttonConnect = new System.Windows.Forms.Button();
            this.labelGoToLine = new System.Windows.Forms.Label();
            this.comboBoxWebServiceUrl = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
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
            this.textBoxUserName.Location = new System.Drawing.Point(97, 83);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(290, 20);
            this.textBoxUserName.TabIndex = 1;
            // 
            // buttonConnect
            // 
            this.buttonConnect.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonConnect.Location = new System.Drawing.Point(231, 147);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 21);
            this.buttonConnect.TabIndex = 3;
            this.buttonConnect.Text = "&Start";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelGoToLine
            // 
            this.labelGoToLine.AutoSize = true;
            this.labelGoToLine.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelGoToLine.Location = new System.Drawing.Point(36, 86);
            this.labelGoToLine.Name = "labelGoToLine";
            this.labelGoToLine.Size = new System.Drawing.Size(55, 13);
            this.labelGoToLine.TabIndex = 3;
            this.labelGoToLine.Text = "Username";
            // 
            // comboBoxWebServiceUrl
            // 
            this.comboBoxWebServiceUrl.FormattingEnabled = true;
            this.comboBoxWebServiceUrl.Items.AddRange(new object[] {
            "http://www.nikse.dk/se/SeService.asmx",
            "http://nikse555.brinkster.net/SeService.asmx"});
            this.comboBoxWebServiceUrl.Location = new System.Drawing.Point(97, 109);
            this.comboBoxWebServiceUrl.Name = "comboBoxWebServiceUrl";
            this.comboBoxWebServiceUrl.Size = new System.Drawing.Size(290, 21);
            this.comboBoxWebServiceUrl.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(10, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Web service url";
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(13, 13);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(200, 26);
            this.labelInfo.TabIndex = 9;
            this.labelInfo.Text = "Start new session where multiple persons\r\ncan edit in same subtitle file.\r\n";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(36, 179);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 10;
            this.labelStatus.Text = "labelStatus";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(26, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Session Key";
            // 
            // textBoxSessionKey
            // 
            this.textBoxSessionKey.Location = new System.Drawing.Point(97, 57);
            this.textBoxSessionKey.Name = "textBoxSessionKey";
            this.textBoxSessionKey.Size = new System.Drawing.Size(290, 20);
            this.textBoxSessionKey.TabIndex = 0;
            // 
            // NetworkStart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 201);
            this.Controls.Add(this.textBoxSessionKey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxWebServiceUrl);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.labelGoToLine);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NetworkStart";
            this.Text = "Start network session";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NetworkNew_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label labelGoToLine;
        private System.Windows.Forms.ComboBox comboBoxWebServiceUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxSessionKey;
    }
}