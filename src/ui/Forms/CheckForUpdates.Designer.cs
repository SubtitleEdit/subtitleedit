using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    partial class CheckForUpdates
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
            this.labelStatus = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonDownloadAndInstall = new System.Windows.Forms.Button();
            this.timerCheckForUpdates = new System.Windows.Forms.Timer(this.components);
            this.buttonDontCheckUpdates = new System.Windows.Forms.Button();
            this.linkLabelUpdatePlugins = new System.Windows.Forms.LinkLabel();
            this.labelPluginsHaveUpdates = new System.Windows.Forms.Label();
            this.textBoxChangeLog = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(10, 15);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(70, 13);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "labelStatus";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(556, 168);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "C&ancel";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonDownloadAndInstall
            // 
            this.buttonDownloadAndInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownloadAndInstall.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDownloadAndInstall.Location = new System.Drawing.Point(224, 168);
            this.buttonDownloadAndInstall.Name = "buttonDownloadAndInstall";
            this.buttonDownloadAndInstall.Size = new System.Drawing.Size(160, 23);
            this.buttonDownloadAndInstall.TabIndex = 0;
            this.buttonDownloadAndInstall.Text = "&OK";
            this.buttonDownloadAndInstall.UseVisualStyleBackColor = true;
            this.buttonDownloadAndInstall.Click += new System.EventHandler(this.buttonDownloadAndInstall_Click);
            // 
            // timerCheckForUpdates
            // 
            this.timerCheckForUpdates.Tick += new System.EventHandler(this.timerCheckForUpdates_Tick);
            // 
            // buttonDontCheckUpdates
            // 
            this.buttonDontCheckUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDontCheckUpdates.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDontCheckUpdates.Location = new System.Drawing.Point(390, 168);
            this.buttonDontCheckUpdates.Name = "buttonDontCheckUpdates";
            this.buttonDontCheckUpdates.Size = new System.Drawing.Size(160, 23);
            this.buttonDontCheckUpdates.TabIndex = 1;
            this.buttonDontCheckUpdates.Text = "Don\'t check for updates";
            this.buttonDontCheckUpdates.UseVisualStyleBackColor = true;
            this.buttonDontCheckUpdates.Click += new System.EventHandler(this.buttonDontCheckUpdates_Click);
            // 
            // linkLabelUpdatePlugins
            // 
            this.linkLabelUpdatePlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelUpdatePlugins.AutoSize = true;
            this.linkLabelUpdatePlugins.Location = new System.Drawing.Point(182, 150);
            this.linkLabelUpdatePlugins.Name = "linkLabelUpdatePlugins";
            this.linkLabelUpdatePlugins.Size = new System.Drawing.Size(63, 13);
            this.linkLabelUpdatePlugins.TabIndex = 6;
            this.linkLabelUpdatePlugins.TabStop = true;
            this.linkLabelUpdatePlugins.Text = "update now";
            this.linkLabelUpdatePlugins.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUpdatePlugins_LinkClicked);
            // 
            // labelPluginsHaveUpdates
            // 
            this.labelPluginsHaveUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelPluginsHaveUpdates.AutoSize = true;
            this.labelPluginsHaveUpdates.Location = new System.Drawing.Point(13, 150);
            this.labelPluginsHaveUpdates.Name = "labelPluginsHaveUpdates";
            this.labelPluginsHaveUpdates.Size = new System.Drawing.Size(169, 13);
            this.labelPluginsHaveUpdates.TabIndex = 5;
            this.labelPluginsHaveUpdates.Text = "X plugins have updates available -";
            // 
            // textBoxChangeLog
            // 
            this.textBoxChangeLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxChangeLog.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxChangeLog.Location = new System.Drawing.Point(13, 32);
            this.textBoxChangeLog.Multiline = true;
            this.textBoxChangeLog.Name = "textBoxChangeLog";
            this.textBoxChangeLog.ReadOnly = true;
            this.textBoxChangeLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxChangeLog.Size = new System.Drawing.Size(618, 106);
            this.textBoxChangeLog.TabIndex = 4;
            // 
            // CheckForUpdates
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(643, 201);
            this.Controls.Add(this.linkLabelUpdatePlugins);
            this.Controls.Add(this.labelPluginsHaveUpdates);
            this.Controls.Add(this.buttonDontCheckUpdates);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonDownloadAndInstall);
            this.Controls.Add(this.textBoxChangeLog);
            this.Controls.Add(this.labelStatus);
            this.KeyPreview = true;
            this.Name = "CheckForUpdates";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "CheckForUpdates";
            this.Shown += new System.EventHandler(this.CheckForUpdates_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CheckForUpdates_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelStatus;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxChangeLog;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonDownloadAndInstall;
        private System.Windows.Forms.Timer timerCheckForUpdates;
        private System.Windows.Forms.Button buttonDontCheckUpdates;
        private LinkLabel linkLabelUpdatePlugins;
        private Label labelPluginsHaveUpdates;
    }
}