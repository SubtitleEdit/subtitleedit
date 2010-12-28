namespace Nikse.SubtitleEdit.Forms
{
    partial class NetworkJoin
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
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelWebServiceUrl = new System.Windows.Forms.Label();
            this.comboBoxWebServiceUrl = new System.Windows.Forms.ComboBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.buttonJoin = new System.Windows.Forms.Button();
            this.labelUserName = new System.Windows.Forms.Label();
            this.textBoxSessionKey = new System.Windows.Forms.TextBox();
            this.labelSessionKey = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(35, 197);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 18;
            this.labelStatus.Text = "labelStatus";
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(12, 9);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(212, 26);
            this.labelInfo.TabIndex = 17;
            this.labelInfo.Text = "Join existing session where multiple persons\r\ncan edit in same subtitle file.\r\n";
            // 
            // labelWebServiceUrl
            // 
            this.labelWebServiceUrl.AutoSize = true;
            this.labelWebServiceUrl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelWebServiceUrl.Location = new System.Drawing.Point(9, 138);
            this.labelWebServiceUrl.Name = "labelWebServiceUrl";
            this.labelWebServiceUrl.Size = new System.Drawing.Size(81, 13);
            this.labelWebServiceUrl.TabIndex = 16;
            this.labelWebServiceUrl.Text = "Web service url";
            // 
            // comboBoxWebServiceUrl
            // 
            this.comboBoxWebServiceUrl.FormattingEnabled = true;
            this.comboBoxWebServiceUrl.Items.AddRange(new object[] {
            "http://www.nikse.dk/se/SeService.asmx",
            "http://nikse555.brinkster.net/SeService.asmx"});
            this.comboBoxWebServiceUrl.Location = new System.Drawing.Point(96, 131);
            this.comboBoxWebServiceUrl.Name = "comboBoxWebServiceUrl";
            this.comboBoxWebServiceUrl.Size = new System.Drawing.Size(290, 21);
            this.comboBoxWebServiceUrl.TabIndex = 2;
            // 
            // buttonCancel
            // 
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(311, 173);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Location = new System.Drawing.Point(96, 79);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(290, 20);
            this.textBoxUserName.TabIndex = 0;
            // 
            // buttonJoin
            // 
            this.buttonJoin.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonJoin.Location = new System.Drawing.Point(230, 173);
            this.buttonJoin.Name = "buttonJoin";
            this.buttonJoin.Size = new System.Drawing.Size(75, 21);
            this.buttonJoin.TabIndex = 3;
            this.buttonJoin.Text = "&Join";
            this.buttonJoin.UseVisualStyleBackColor = true;
            this.buttonJoin.Click += new System.EventHandler(this.buttonJoin_Click);
            // 
            // labelUserName
            // 
            this.labelUserName.AutoSize = true;
            this.labelUserName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelUserName.Location = new System.Drawing.Point(35, 82);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(55, 13);
            this.labelUserName.TabIndex = 11;
            this.labelUserName.Text = "Username";
            // 
            // textBoxSessionKey
            // 
            this.textBoxSessionKey.Location = new System.Drawing.Point(96, 105);
            this.textBoxSessionKey.Name = "textBoxSessionKey";
            this.textBoxSessionKey.Size = new System.Drawing.Size(290, 20);
            this.textBoxSessionKey.TabIndex = 1;
            // 
            // labelSessionKey
            // 
            this.labelSessionKey.AutoSize = true;
            this.labelSessionKey.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelSessionKey.Location = new System.Drawing.Point(35, 108);
            this.labelSessionKey.Name = "labelSessionKey";
            this.labelSessionKey.Size = new System.Drawing.Size(25, 13);
            this.labelSessionKey.TabIndex = 19;
            this.labelSessionKey.Text = "Key";
            // 
            // NetworkJoin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 214);
            this.Controls.Add(this.textBoxSessionKey);
            this.Controls.Add(this.labelSessionKey);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelWebServiceUrl);
            this.Controls.Add(this.comboBoxWebServiceUrl);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(this.buttonJoin);
            this.Controls.Add(this.labelUserName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NetworkJoin";
            this.Text = "Join network session";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NetworkJoin_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelWebServiceUrl;
        private System.Windows.Forms.ComboBox comboBoxWebServiceUrl;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Button buttonJoin;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.TextBox textBoxSessionKey;
        private System.Windows.Forms.Label labelSessionKey;
    }
}