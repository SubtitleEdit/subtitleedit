namespace Nikse.SubtitleEdit.Forms.Networking
{
    partial class NetworkChat
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetworkChat));
            this.listViewChat = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageListUsers = new System.Windows.Forms.ImageList(this.components);
            this.listViewUsers = new System.Windows.Forms.ListView();
            this.columnHeaderNick = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBoxChat = new System.Windows.Forms.TextBox();
            this.buttonSendChat = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listViewChat
            // 
            this.listViewChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewChat.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader4});
            this.listViewChat.Location = new System.Drawing.Point(3, 107);
            this.listViewChat.Name = "listViewChat";
            this.listViewChat.Size = new System.Drawing.Size(274, 239);
            this.listViewChat.SmallImageList = this.imageListUsers;
            this.listViewChat.TabIndex = 3;
            this.listViewChat.UseCompatibleStateImageBehavior = false;
            this.listViewChat.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Username";
            this.columnHeader1.Width = 82;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Text";
            this.columnHeader4.Width = 188;
            // 
            // imageListUsers
            // 
            this.imageListUsers.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListUsers.ImageStream")));
            this.imageListUsers.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListUsers.Images.SetKeyName(0, "person.jpg");
            this.imageListUsers.Images.SetKeyName(1, "person2.jpg");
            this.imageListUsers.Images.SetKeyName(2, "Person3.jpg");
            this.imageListUsers.Images.SetKeyName(3, "Person4.jpg");
            this.imageListUsers.Images.SetKeyName(4, "Person5.jpg");
            this.imageListUsers.Images.SetKeyName(5, "Person6.jpg");
            this.imageListUsers.Images.SetKeyName(6, "Person7.jpg");
            this.imageListUsers.Images.SetKeyName(7, "Person8.jpg");
            // 
            // listViewUsers
            // 
            this.listViewUsers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderNick,
            this.columnHeader2});
            this.listViewUsers.Location = new System.Drawing.Point(2, 2);
            this.listViewUsers.Name = "listViewUsers";
            this.listViewUsers.Size = new System.Drawing.Size(275, 103);
            this.listViewUsers.SmallImageList = this.imageListUsers;
            this.listViewUsers.TabIndex = 2;
            this.listViewUsers.UseCompatibleStateImageBehavior = false;
            this.listViewUsers.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderNick
            // 
            this.columnHeaderNick.Text = "Username";
            this.columnHeaderNick.Width = 178;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "IP";
            this.columnHeader2.Width = 93;
            // 
            // textBoxChat
            // 
            this.textBoxChat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxChat.Location = new System.Drawing.Point(3, 352);
            this.textBoxChat.Multiline = true;
            this.textBoxChat.Name = "textBoxChat";
            this.textBoxChat.Size = new System.Drawing.Size(188, 46);
            this.textBoxChat.TabIndex = 0;
            this.textBoxChat.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxChat_KeyDown);
            // 
            // buttonSendChat
            // 
            this.buttonSendChat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSendChat.Location = new System.Drawing.Point(197, 375);
            this.buttonSendChat.Name = "buttonSendChat";
            this.buttonSendChat.Size = new System.Drawing.Size(75, 23);
            this.buttonSendChat.TabIndex = 1;
            this.buttonSendChat.Text = "Send message";
            this.buttonSendChat.UseVisualStyleBackColor = true;
            this.buttonSendChat.Click += new System.EventHandler(this.buttonSendChat_Click);
            // 
            // NetworkChat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(281, 402);
            this.Controls.Add(this.listViewChat);
            this.Controls.Add(this.listViewUsers);
            this.Controls.Add(this.textBoxChat);
            this.Controls.Add(this.buttonSendChat);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(200, 300);
            this.Name = "NetworkChat";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NetworkChat";
            this.ResizeEnd += new System.EventHandler(this.NetworkChat_ResizeEnd);
            this.Shown += new System.EventHandler(this.NetworkChat_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewChat;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ListView listViewUsers;
        private System.Windows.Forms.ColumnHeader columnHeaderNick;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TextBox textBoxChat;
        private System.Windows.Forms.Button buttonSendChat;
        private System.Windows.Forms.ImageList imageListUsers;
    }
}