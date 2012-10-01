namespace Nikse.SubtitleEdit.Forms
{
    partial class PluginsGet
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
            this.labelChooseLanguageAndClickDownload = new System.Windows.Forms.Label();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.labelDescription1 = new System.Windows.Forms.Label();
            this.linkLabelOpenDictionaryFolder = new System.Windows.Forms.LinkLabel();
            this.buttonOK = new System.Windows.Forms.Button();
            this.listViewGetPlugins = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControlPlugins = new System.Windows.Forms.TabControl();
            this.tabPageInstalledPlugins = new System.Windows.Forms.TabPage();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.listViewInstalledPlugins = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPageGetPlugins = new System.Windows.Forms.TabPage();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.tabControlPlugins.SuspendLayout();
            this.tabPageInstalledPlugins.SuspendLayout();
            this.tabPageGetPlugins.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelChooseLanguageAndClickDownload
            // 
            this.labelChooseLanguageAndClickDownload.AutoSize = true;
            this.labelChooseLanguageAndClickDownload.Location = new System.Drawing.Point(13, 45);
            this.labelChooseLanguageAndClickDownload.Name = "labelChooseLanguageAndClickDownload";
            this.labelChooseLanguageAndClickDownload.Size = new System.Drawing.Size(181, 13);
            this.labelChooseLanguageAndClickDownload.TabIndex = 23;
            this.labelChooseLanguageAndClickDownload.Text = "Choose plugin and click \"Download\"";
            // 
            // buttonDownload
            // 
            this.buttonDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownload.Location = new System.Drawing.Point(516, 205);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(104, 25);
            this.buttonDownload.TabIndex = 22;
            this.buttonDownload.Text = "&Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // labelDescription1
            // 
            this.labelDescription1.AutoSize = true;
            this.labelDescription1.Location = new System.Drawing.Point(13, 16);
            this.labelDescription1.Name = "labelDescription1";
            this.labelDescription1.Size = new System.Drawing.Size(306, 13);
            this.labelDescription1.TabIndex = 21;
            this.labelDescription1.Text = "Subtitle Edit plugins must be downloaded to the \"Plugins\" folder";
            // 
            // linkLabelOpenDictionaryFolder
            // 
            this.linkLabelOpenDictionaryFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelOpenDictionaryFolder.AutoSize = true;
            this.linkLabelOpenDictionaryFolder.Location = new System.Drawing.Point(12, 285);
            this.linkLabelOpenDictionaryFolder.Name = "linkLabelOpenDictionaryFolder";
            this.linkLabelOpenDictionaryFolder.Size = new System.Drawing.Size(106, 13);
            this.linkLabelOpenDictionaryFolder.TabIndex = 25;
            this.linkLabelOpenDictionaryFolder.TabStop = true;
            this.linkLabelOpenDictionaryFolder.Text = "Open \'Plug-ins\' folder";
            this.linkLabelOpenDictionaryFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenDictionaryFolder_LinkClicked);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(532, 280);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(104, 23);
            this.buttonOK.TabIndex = 26;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // listViewGetPlugins
            // 
            this.listViewGetPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewGetPlugins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderDescription,
            this.columnHeaderVersion,
            this.columnHeaderDate});
            this.listViewGetPlugins.FullRowSelect = true;
            this.listViewGetPlugins.HideSelection = false;
            this.listViewGetPlugins.Location = new System.Drawing.Point(16, 61);
            this.listViewGetPlugins.MultiSelect = false;
            this.listViewGetPlugins.Name = "listViewGetPlugins";
            this.listViewGetPlugins.Size = new System.Drawing.Size(604, 138);
            this.listViewGetPlugins.TabIndex = 27;
            this.listViewGetPlugins.UseCompatibleStateImageBehavior = false;
            this.listViewGetPlugins.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 140;
            // 
            // columnHeaderDescription
            // 
            this.columnHeaderDescription.Text = "Description";
            this.columnHeaderDescription.Width = 280;
            // 
            // columnHeaderVersion
            // 
            this.columnHeaderVersion.Text = "Version";
            this.columnHeaderVersion.Width = 80;
            // 
            // columnHeaderDate
            // 
            this.columnHeaderDate.Text = "Date";
            this.columnHeaderDate.Width = 70;
            // 
            // tabControlPlugins
            // 
            this.tabControlPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlPlugins.Controls.Add(this.tabPageInstalledPlugins);
            this.tabControlPlugins.Controls.Add(this.tabPageGetPlugins);
            this.tabControlPlugins.Location = new System.Drawing.Point(12, 12);
            this.tabControlPlugins.Name = "tabControlPlugins";
            this.tabControlPlugins.SelectedIndex = 0;
            this.tabControlPlugins.Size = new System.Drawing.Size(635, 262);
            this.tabControlPlugins.TabIndex = 28;
            // 
            // tabPageInstalledPlugins
            // 
            this.tabPageInstalledPlugins.Controls.Add(this.buttonRemove);
            this.tabPageInstalledPlugins.Controls.Add(this.listViewInstalledPlugins);
            this.tabPageInstalledPlugins.Location = new System.Drawing.Point(4, 22);
            this.tabPageInstalledPlugins.Name = "tabPageInstalledPlugins";
            this.tabPageInstalledPlugins.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInstalledPlugins.Size = new System.Drawing.Size(627, 236);
            this.tabPageInstalledPlugins.TabIndex = 0;
            this.tabPageInstalledPlugins.Text = "Installed plugins";
            this.tabPageInstalledPlugins.UseVisualStyleBackColor = true;
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemove.Location = new System.Drawing.Point(516, 205);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(104, 25);
            this.buttonRemove.TabIndex = 29;
            this.buttonRemove.Text = "&Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // listViewInstalledPlugins
            // 
            this.listViewInstalledPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewInstalledPlugins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listViewInstalledPlugins.FullRowSelect = true;
            this.listViewInstalledPlugins.HideSelection = false;
            this.listViewInstalledPlugins.Location = new System.Drawing.Point(6, 6);
            this.listViewInstalledPlugins.MultiSelect = false;
            this.listViewInstalledPlugins.Name = "listViewInstalledPlugins";
            this.listViewInstalledPlugins.Size = new System.Drawing.Size(614, 193);
            this.listViewInstalledPlugins.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewInstalledPlugins.TabIndex = 28;
            this.listViewInstalledPlugins.UseCompatibleStateImageBehavior = false;
            this.listViewInstalledPlugins.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 140;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Description";
            this.columnHeader2.Width = 280;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Version";
            this.columnHeader3.Width = 80;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Date";
            this.columnHeader4.Width = 80;
            // 
            // tabPageGetPlugins
            // 
            this.tabPageGetPlugins.Controls.Add(this.labelPleaseWait);
            this.tabPageGetPlugins.Controls.Add(this.listViewGetPlugins);
            this.tabPageGetPlugins.Controls.Add(this.labelDescription1);
            this.tabPageGetPlugins.Controls.Add(this.buttonDownload);
            this.tabPageGetPlugins.Controls.Add(this.labelChooseLanguageAndClickDownload);
            this.tabPageGetPlugins.Location = new System.Drawing.Point(4, 22);
            this.tabPageGetPlugins.Name = "tabPageGetPlugins";
            this.tabPageGetPlugins.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGetPlugins.Size = new System.Drawing.Size(627, 236);
            this.tabPageGetPlugins.TabIndex = 1;
            this.tabPageGetPlugins.Text = "Get plugins";
            this.tabPageGetPlugins.UseVisualStyleBackColor = true;
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(13, 458);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPleaseWait.TabIndex = 28;
            this.labelPleaseWait.Text = "Please wait...";
            // 
            // PluginsGet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 312);
            this.Controls.Add(this.tabControlPlugins);
            this.Controls.Add(this.linkLabelOpenDictionaryFolder);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 350);
            this.Name = "PluginsGet";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Plugins";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PluginsGet_KeyDown);
            this.tabControlPlugins.ResumeLayout(false);
            this.tabPageInstalledPlugins.ResumeLayout(false);
            this.tabPageGetPlugins.ResumeLayout(false);
            this.tabPageGetPlugins.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelChooseLanguageAndClickDownload;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.Label labelDescription1;
        private System.Windows.Forms.LinkLabel linkLabelOpenDictionaryFolder;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ListView listViewGetPlugins;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderVersion;
        private System.Windows.Forms.ColumnHeader columnHeaderDescription;
        private System.Windows.Forms.ColumnHeader columnHeaderDate;
        private System.Windows.Forms.TabControl tabControlPlugins;
        private System.Windows.Forms.TabPage tabPageInstalledPlugins;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.ListView listViewInstalledPlugins;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.TabPage tabPageGetPlugins;
        private System.Windows.Forms.Label labelPleaseWait;
    }
}