namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class PluginsGet
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
            this.labelClickDownload = new System.Windows.Forms.Label();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.labelDescription1 = new System.Windows.Forms.Label();
            this.linkLabelOpenPluginFolder = new System.Windows.Forms.LinkLabel();
            this.buttonOK = new System.Windows.Forms.Button();
            this.listViewGetPlugins = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControlPlugins = new System.Windows.Forms.TabControl();
            this.tabPageInstalledPlugins = new System.Windows.Forms.TabPage();
            this.buttonUpdateAll = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.listViewInstalledPlugins = new System.Windows.Forms.ListView();
            this.columnHeaderInsName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderInsDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderInsVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderInsType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPageGetPlugins = new System.Windows.Forms.TabPage();
            this.buttonSearchClear = new System.Windows.Forms.Button();
            this.labelShortcutsSearch = new System.Windows.Forms.Label();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.tabControlPlugins.SuspendLayout();
            this.tabPageInstalledPlugins.SuspendLayout();
            this.tabPageGetPlugins.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelClickDownload
            // 
            this.labelClickDownload.AutoSize = true;
            this.labelClickDownload.Location = new System.Drawing.Point(13, 48);
            this.labelClickDownload.Name = "labelClickDownload";
            this.labelClickDownload.Size = new System.Drawing.Size(181, 13);
            this.labelClickDownload.TabIndex = 23;
            this.labelClickDownload.Text = "Choose plugin and click \"Download\"";
            // 
            // buttonDownload
            // 
            this.buttonDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownload.Location = new System.Drawing.Point(680, 435);
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
            // linkLabelOpenPluginFolder
            // 
            this.linkLabelOpenPluginFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelOpenPluginFolder.AutoSize = true;
            this.linkLabelOpenPluginFolder.Location = new System.Drawing.Point(12, 515);
            this.linkLabelOpenPluginFolder.Name = "linkLabelOpenPluginFolder";
            this.linkLabelOpenPluginFolder.Size = new System.Drawing.Size(106, 13);
            this.linkLabelOpenPluginFolder.TabIndex = 98;
            this.linkLabelOpenPluginFolder.TabStop = true;
            this.linkLabelOpenPluginFolder.Text = "Open \'Plug-ins\' folder";
            this.linkLabelOpenPluginFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenDictionaryFolder_LinkClicked);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(695, 510);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(104, 23);
            this.buttonOK.TabIndex = 99;
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
            this.listViewGetPlugins.Location = new System.Drawing.Point(16, 67);
            this.listViewGetPlugins.MultiSelect = false;
            this.listViewGetPlugins.Name = "listViewGetPlugins";
            this.listViewGetPlugins.Size = new System.Drawing.Size(767, 362);
            this.listViewGetPlugins.TabIndex = 15;
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
            this.columnHeaderDescription.Width = 420;
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
            this.tabControlPlugins.Size = new System.Drawing.Size(798, 492);
            this.tabControlPlugins.TabIndex = 28;
            // 
            // tabPageInstalledPlugins
            // 
            this.tabPageInstalledPlugins.Controls.Add(this.buttonUpdateAll);
            this.tabPageInstalledPlugins.Controls.Add(this.buttonRemove);
            this.tabPageInstalledPlugins.Controls.Add(this.listViewInstalledPlugins);
            this.tabPageInstalledPlugins.Location = new System.Drawing.Point(4, 22);
            this.tabPageInstalledPlugins.Name = "tabPageInstalledPlugins";
            this.tabPageInstalledPlugins.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInstalledPlugins.Size = new System.Drawing.Size(790, 466);
            this.tabPageInstalledPlugins.TabIndex = 0;
            this.tabPageInstalledPlugins.Text = "Installed plugins";
            this.tabPageInstalledPlugins.UseVisualStyleBackColor = true;
            // 
            // buttonUpdateAll
            // 
            this.buttonUpdateAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpdateAll.Location = new System.Drawing.Point(539, 435);
            this.buttonUpdateAll.Name = "buttonUpdateAll";
            this.buttonUpdateAll.Size = new System.Drawing.Size(134, 25);
            this.buttonUpdateAll.TabIndex = 30;
            this.buttonUpdateAll.Text = "Updata all";
            this.buttonUpdateAll.UseVisualStyleBackColor = true;
            this.buttonUpdateAll.Click += new System.EventHandler(this.buttonUpdateAll_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemove.Location = new System.Drawing.Point(679, 435);
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
            this.columnHeaderInsName,
            this.columnHeaderInsDescription,
            this.columnHeaderInsVersion,
            this.columnHeaderInsType});
            this.listViewInstalledPlugins.FullRowSelect = true;
            this.listViewInstalledPlugins.HideSelection = false;
            this.listViewInstalledPlugins.Location = new System.Drawing.Point(6, 6);
            this.listViewInstalledPlugins.MultiSelect = false;
            this.listViewInstalledPlugins.Name = "listViewInstalledPlugins";
            this.listViewInstalledPlugins.Size = new System.Drawing.Size(777, 423);
            this.listViewInstalledPlugins.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewInstalledPlugins.TabIndex = 28;
            this.listViewInstalledPlugins.UseCompatibleStateImageBehavior = false;
            this.listViewInstalledPlugins.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderInsName
            // 
            this.columnHeaderInsName.Text = "Name";
            this.columnHeaderInsName.Width = 140;
            // 
            // columnHeaderInsDescription
            // 
            this.columnHeaderInsDescription.Text = "Description";
            this.columnHeaderInsDescription.Width = 420;
            // 
            // columnHeaderInsVersion
            // 
            this.columnHeaderInsVersion.Text = "Version";
            this.columnHeaderInsVersion.Width = 80;
            // 
            // columnHeaderInsType
            // 
            this.columnHeaderInsType.Text = "Date";
            this.columnHeaderInsType.Width = 80;
            // 
            // tabPageGetPlugins
            // 
            this.tabPageGetPlugins.Controls.Add(this.buttonSearchClear);
            this.tabPageGetPlugins.Controls.Add(this.labelShortcutsSearch);
            this.tabPageGetPlugins.Controls.Add(this.textBoxSearch);
            this.tabPageGetPlugins.Controls.Add(this.labelPleaseWait);
            this.tabPageGetPlugins.Controls.Add(this.listViewGetPlugins);
            this.tabPageGetPlugins.Controls.Add(this.labelDescription1);
            this.tabPageGetPlugins.Controls.Add(this.buttonDownload);
            this.tabPageGetPlugins.Controls.Add(this.labelClickDownload);
            this.tabPageGetPlugins.Location = new System.Drawing.Point(4, 22);
            this.tabPageGetPlugins.Name = "tabPageGetPlugins";
            this.tabPageGetPlugins.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGetPlugins.Size = new System.Drawing.Size(790, 466);
            this.tabPageGetPlugins.TabIndex = 1;
            this.tabPageGetPlugins.Text = "Get plugins";
            this.tabPageGetPlugins.UseVisualStyleBackColor = true;
            // 
            // buttonSearchClear
            // 
            this.buttonSearchClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSearchClear.Enabled = false;
            this.buttonSearchClear.Location = new System.Drawing.Point(672, 38);
            this.buttonSearchClear.Name = "buttonSearchClear";
            this.buttonSearchClear.Size = new System.Drawing.Size(111, 23);
            this.buttonSearchClear.TabIndex = 8;
            this.buttonSearchClear.Text = "Clear";
            this.buttonSearchClear.UseVisualStyleBackColor = true;
            this.buttonSearchClear.Click += new System.EventHandler(this.buttonSearchClear_Click);
            // 
            // labelShortcutsSearch
            // 
            this.labelShortcutsSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelShortcutsSearch.AutoSize = true;
            this.labelShortcutsSearch.Location = new System.Drawing.Point(469, 43);
            this.labelShortcutsSearch.Name = "labelShortcutsSearch";
            this.labelShortcutsSearch.Size = new System.Drawing.Size(41, 13);
            this.labelShortcutsSearch.TabIndex = 43;
            this.labelShortcutsSearch.Text = "Search";
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSearch.Location = new System.Drawing.Point(515, 40);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(151, 20);
            this.textBoxSearch.TabIndex = 5;
            this.textBoxSearch.TextChanged += new System.EventHandler(this.textBoxSearch_TextChanged);
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(13, 447);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPleaseWait.TabIndex = 28;
            this.labelPleaseWait.Text = "Please wait...";
            // 
            // PluginsGet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(811, 542);
            this.Controls.Add(this.tabControlPlugins);
            this.Controls.Add(this.linkLabelOpenPluginFolder);
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

        private System.Windows.Forms.Label labelClickDownload;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.Label labelDescription1;
        private System.Windows.Forms.LinkLabel linkLabelOpenPluginFolder;
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
        private System.Windows.Forms.ColumnHeader columnHeaderInsName;
        private System.Windows.Forms.ColumnHeader columnHeaderInsDescription;
        private System.Windows.Forms.ColumnHeader columnHeaderInsVersion;
        private System.Windows.Forms.ColumnHeader columnHeaderInsType;
        private System.Windows.Forms.TabPage tabPageGetPlugins;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.Button buttonUpdateAll;
        private System.Windows.Forms.Button buttonSearchClear;
        private System.Windows.Forms.Label labelShortcutsSearch;
        private System.Windows.Forms.TextBox textBoxSearch;
    }
}