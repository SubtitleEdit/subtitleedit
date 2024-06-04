namespace Nikse.SubtitleEdit.Forms.Translate
{
    sealed partial class AutoTranslate
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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.labelTarget = new System.Windows.Forms.Label();
            this.labelSource = new System.Windows.Forms.Label();
            this.labelUrl = new System.Windows.Forms.Label();
            this.linkLabelPoweredBy = new System.Windows.Forms.LinkLabel();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemStartLibre = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStartNLLBServe = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStartNLLBApi = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.translateCurrentLineToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.startLibreTranslateServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startNLLBServeServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startNLLBAPIServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.translateCurrentLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelApiKey = new System.Windows.Forms.Label();
            this.buttonStrategy = new System.Windows.Forms.Button();
            this.labelFormality = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxFormality = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.nikseTextBoxApiKey = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.nikseComboBoxEngine = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.nikseComboBoxUrl = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxSource = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxTarget = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.subtitleListViewTarget = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.subtitleListViewSource = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.contextMenuStrip2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(699, 29);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(192, 16);
            this.progressBar1.TabIndex = 100;
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(697, 13);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(171, 13);
            this.labelPleaseWait.TabIndex = 99;
            this.labelPleaseWait.Text = "Please wait... this may take a while";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(971, 528);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 202;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(890, 528);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 201;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.Location = new System.Drawing.Point(618, 24);
            this.buttonTranslate.Name = "buttonTranslate";
            this.buttonTranslate.Size = new System.Drawing.Size(75, 23);
            this.buttonTranslate.TabIndex = 96;
            this.buttonTranslate.Text = "Translate";
            this.buttonTranslate.UseVisualStyleBackColor = true;
            this.buttonTranslate.Click += new System.EventHandler(this.buttonTranslate_Click);
            // 
            // labelTarget
            // 
            this.labelTarget.AutoSize = true;
            this.labelTarget.Location = new System.Drawing.Point(462, 29);
            this.labelTarget.Name = "labelTarget";
            this.labelTarget.Size = new System.Drawing.Size(23, 13);
            this.labelTarget.TabIndex = 104;
            this.labelTarget.Text = "To:";
            // 
            // labelSource
            // 
            this.labelSource.AutoSize = true;
            this.labelSource.Location = new System.Drawing.Point(279, 29);
            this.labelSource.Name = "labelSource";
            this.labelSource.Size = new System.Drawing.Size(33, 13);
            this.labelSource.TabIndex = 103;
            this.labelSource.Text = "From:";
            // 
            // labelUrl
            // 
            this.labelUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelUrl.AutoSize = true;
            this.labelUrl.Location = new System.Drawing.Point(14, 500);
            this.labelUrl.Name = "labelUrl";
            this.labelUrl.Size = new System.Drawing.Size(23, 13);
            this.labelUrl.TabIndex = 106;
            this.labelUrl.Text = "Url:";
            // 
            // linkLabelPoweredBy
            // 
            this.linkLabelPoweredBy.AutoSize = true;
            this.linkLabelPoweredBy.Location = new System.Drawing.Point(14, 4);
            this.linkLabelPoweredBy.Name = "linkLabelPoweredBy";
            this.linkLabelPoweredBy.Size = new System.Drawing.Size(73, 13);
            this.linkLabelPoweredBy.TabIndex = 109;
            this.linkLabelPoweredBy.TabStop = true;
            this.linkLabelPoweredBy.Text = "Powered by X";
            this.linkLabelPoweredBy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelPoweredBy_LinkClicked);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemStartLibre,
            this.toolStripMenuItemStartNLLBServe,
            this.toolStripMenuItemStartNLLBApi,
            this.toolStripSeparator1,
            this.translateCurrentLineToolStripMenuItem1});
            this.contextMenuStrip2.Name = "contextMenuStrip1";
            this.contextMenuStrip2.Size = new System.Drawing.Size(197, 98);
            this.contextMenuStrip2.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip2_Opening);
            // 
            // toolStripMenuItemStartLibre
            // 
            this.toolStripMenuItemStartLibre.Name = "toolStripMenuItemStartLibre";
            this.toolStripMenuItemStartLibre.Size = new System.Drawing.Size(196, 22);
            this.toolStripMenuItemStartLibre.Text = "Start Libratransle server";
            this.toolStripMenuItemStartLibre.Click += new System.EventHandler(this.StartLibreTranslateServerToolStripMenuItem_Click);
            // 
            // toolStripMenuItemStartNLLBServe
            // 
            this.toolStripMenuItemStartNLLBServe.Name = "toolStripMenuItemStartNLLBServe";
            this.toolStripMenuItemStartNLLBServe.Size = new System.Drawing.Size(196, 22);
            this.toolStripMenuItemStartNLLBServe.Text = "Start NLLB Serve server";
            this.toolStripMenuItemStartNLLBServe.Click += new System.EventHandler(this.StartNllbServeServerToolStripMenuItem_Click);
            // 
            // toolStripMenuItemStartNLLBApi
            // 
            this.toolStripMenuItemStartNLLBApi.Name = "toolStripMenuItemStartNLLBApi";
            this.toolStripMenuItemStartNLLBApi.Size = new System.Drawing.Size(196, 22);
            this.toolStripMenuItemStartNLLBApi.Text = "Start NLLB API server";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(193, 6);
            // 
            // translateCurrentLineToolStripMenuItem1
            // 
            this.translateCurrentLineToolStripMenuItem1.Name = "translateCurrentLineToolStripMenuItem1";
            this.translateCurrentLineToolStripMenuItem1.Size = new System.Drawing.Size(196, 22);
            this.translateCurrentLineToolStripMenuItem1.Text = "Translate current line";
            this.translateCurrentLineToolStripMenuItem1.Click += new System.EventHandler(this.translateCurrentLineToolStripMenuItem1_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startLibreTranslateServerToolStripMenuItem,
            this.startNLLBServeServerToolStripMenuItem,
            this.startNLLBAPIServerToolStripMenuItem,
            this.toolStripSeparator2,
            this.translateCurrentLineToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(197, 98);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // startLibreTranslateServerToolStripMenuItem
            // 
            this.startLibreTranslateServerToolStripMenuItem.Name = "startLibreTranslateServerToolStripMenuItem";
            this.startLibreTranslateServerToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.startLibreTranslateServerToolStripMenuItem.Text = "Start Libratransle server";
            this.startLibreTranslateServerToolStripMenuItem.Click += new System.EventHandler(this.StartLibreTranslateServerToolStripMenuItem_Click);
            // 
            // startNLLBServeServerToolStripMenuItem
            // 
            this.startNLLBServeServerToolStripMenuItem.Name = "startNLLBServeServerToolStripMenuItem";
            this.startNLLBServeServerToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.startNLLBServeServerToolStripMenuItem.Text = "Start NLLB Serve server";
            this.startNLLBServeServerToolStripMenuItem.Click += new System.EventHandler(this.StartNllbServeServerToolStripMenuItem_Click);
            // 
            // startNLLBAPIServerToolStripMenuItem
            // 
            this.startNLLBAPIServerToolStripMenuItem.Name = "startNLLBAPIServerToolStripMenuItem";
            this.startNLLBAPIServerToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.startNLLBAPIServerToolStripMenuItem.Text = "Start NLLB API server";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(193, 6);
            // 
            // translateCurrentLineToolStripMenuItem
            // 
            this.translateCurrentLineToolStripMenuItem.Name = "translateCurrentLineToolStripMenuItem";
            this.translateCurrentLineToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.translateCurrentLineToolStripMenuItem.Text = "Translate current line";
            this.translateCurrentLineToolStripMenuItem.Click += new System.EventHandler(this.translateCurrentLineToolStripMenuItem_Click);
            // 
            // labelApiKey
            // 
            this.labelApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelApiKey.AutoSize = true;
            this.labelApiKey.Location = new System.Drawing.Point(360, 500);
            this.labelApiKey.Name = "labelApiKey";
            this.labelApiKey.Size = new System.Drawing.Size(47, 13);
            this.labelApiKey.TabIndex = 111;
            this.labelApiKey.Text = "API key:";
            // 
            // buttonStrategy
            // 
            this.buttonStrategy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStrategy.Location = new System.Drawing.Point(755, 528);
            this.buttonStrategy.Name = "buttonStrategy";
            this.buttonStrategy.Size = new System.Drawing.Size(129, 23);
            this.buttonStrategy.TabIndex = 200;
            this.buttonStrategy.Text = "Advanced";
            this.buttonStrategy.UseVisualStyleBackColor = true;
            this.buttonStrategy.Click += new System.EventHandler(this.buttonStrategy_Click);
            // 
            // labelFormality
            // 
            this.labelFormality.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFormality.AutoSize = true;
            this.labelFormality.Location = new System.Drawing.Point(12, 528);
            this.labelFormality.Name = "labelFormality";
            this.labelFormality.Size = new System.Drawing.Size(51, 13);
            this.labelFormality.TabIndex = 113;
            this.labelFormality.Text = "Formality:";
            // 
            // comboBoxFormality
            // 
            this.comboBoxFormality.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxFormality.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFormality.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFormality.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFormality.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFormality.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFormality.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFormality.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFormality.DropDownHeight = 400;
            this.comboBoxFormality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFormality.DropDownWidth = 280;
            this.comboBoxFormality.FormattingEnabled = true;
            this.comboBoxFormality.Items.AddRange(new string[] {
            "default",
            "more",
            "less",
            "prefer_more",
            "prefer_less"});
            this.comboBoxFormality.Location = new System.Drawing.Point(71, 524);
            this.comboBoxFormality.MaxLength = 32767;
            this.comboBoxFormality.Name = "comboBoxFormality";
            this.comboBoxFormality.SelectedIndex = -1;
            this.comboBoxFormality.SelectedItem = null;
            this.comboBoxFormality.SelectedText = "";
            this.comboBoxFormality.Size = new System.Drawing.Size(280, 21);
            this.comboBoxFormality.TabIndex = 112;
            this.comboBoxFormality.UsePopupWindow = false;
            // 
            // nikseTextBoxApiKey
            // 
            this.nikseTextBoxApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nikseTextBoxApiKey.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxApiKey.Location = new System.Drawing.Point(409, 499);
            this.nikseTextBoxApiKey.Name = "nikseTextBoxApiKey";
            this.nikseTextBoxApiKey.Size = new System.Drawing.Size(360, 20);
            this.nikseTextBoxApiKey.TabIndex = 110;
            // 
            // nikseComboBoxEngine
            // 
            this.nikseComboBoxEngine.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxEngine.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxEngine.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxEngine.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxEngine.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxEngine.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxEngine.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxEngine.DropDownHeight = 400;
            this.nikseComboBoxEngine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nikseComboBoxEngine.DropDownWidth = 221;
            this.nikseComboBoxEngine.FormattingEnabled = true;
            this.nikseComboBoxEngine.Location = new System.Drawing.Point(12, 24);
            this.nikseComboBoxEngine.MaxLength = 32767;
            this.nikseComboBoxEngine.Name = "nikseComboBoxEngine";
            this.nikseComboBoxEngine.SelectedIndex = -1;
            this.nikseComboBoxEngine.SelectedItem = null;
            this.nikseComboBoxEngine.SelectedText = "";
            this.nikseComboBoxEngine.Size = new System.Drawing.Size(221, 23);
            this.nikseComboBoxEngine.TabIndex = 107;
            this.nikseComboBoxEngine.UsePopupWindow = false;
            this.nikseComboBoxEngine.SelectedIndexChanged += new System.EventHandler(this.nikseComboBoxEngine_SelectedIndexChanged);
            // 
            // nikseComboBoxUrl
            // 
            this.nikseComboBoxUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nikseComboBoxUrl.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxUrl.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxUrl.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxUrl.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxUrl.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxUrl.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxUrl.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxUrl.DropDownHeight = 400;
            this.nikseComboBoxUrl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.nikseComboBoxUrl.DropDownWidth = 280;
            this.nikseComboBoxUrl.FormattingEnabled = true;
            this.nikseComboBoxUrl.Location = new System.Drawing.Point(56, 495);
            this.nikseComboBoxUrl.MaxLength = 32767;
            this.nikseComboBoxUrl.Name = "nikseComboBoxUrl";
            this.nikseComboBoxUrl.SelectedIndex = -1;
            this.nikseComboBoxUrl.SelectedItem = null;
            this.nikseComboBoxUrl.SelectedText = "";
            this.nikseComboBoxUrl.Size = new System.Drawing.Size(280, 23);
            this.nikseComboBoxUrl.TabIndex = 99;
            this.nikseComboBoxUrl.TabStop = false;
            this.nikseComboBoxUrl.UsePopupWindow = false;
            this.nikseComboBoxUrl.SelectedIndexChanged += new System.EventHandler(this.nikseComboBoxUrl_SelectedIndexChanged);
            // 
            // comboBoxSource
            // 
            this.comboBoxSource.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSource.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSource.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSource.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSource.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSource.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSource.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSource.DropDownHeight = 400;
            this.comboBoxSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSource.DropDownWidth = 140;
            this.comboBoxSource.FormattingEnabled = true;
            this.comboBoxSource.Location = new System.Drawing.Point(318, 25);
            this.comboBoxSource.MaxLength = 32767;
            this.comboBoxSource.Name = "comboBoxSource";
            this.comboBoxSource.SelectedIndex = -1;
            this.comboBoxSource.SelectedItem = null;
            this.comboBoxSource.SelectedText = "";
            this.comboBoxSource.Size = new System.Drawing.Size(125, 23);
            this.comboBoxSource.TabIndex = 94;
            this.comboBoxSource.UsePopupWindow = false;
            this.comboBoxSource.SelectedIndexChanged += new System.EventHandler(this.comboBoxSource_SelectedIndexChanged);
            // 
            // comboBoxTarget
            // 
            this.comboBoxTarget.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxTarget.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxTarget.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxTarget.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxTarget.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxTarget.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxTarget.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxTarget.DropDownHeight = 400;
            this.comboBoxTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTarget.DropDownWidth = 140;
            this.comboBoxTarget.FormattingEnabled = true;
            this.comboBoxTarget.Location = new System.Drawing.Point(491, 25);
            this.comboBoxTarget.MaxLength = 32767;
            this.comboBoxTarget.Name = "comboBoxTarget";
            this.comboBoxTarget.SelectedIndex = -1;
            this.comboBoxTarget.SelectedItem = null;
            this.comboBoxTarget.SelectedText = "";
            this.comboBoxTarget.Size = new System.Drawing.Size(125, 23);
            this.comboBoxTarget.TabIndex = 95;
            this.comboBoxTarget.UsePopupWindow = false;
            this.comboBoxTarget.SelectedIndexChanged += new System.EventHandler(this.comboBoxTarget_SelectedIndexChanged);
            // 
            // subtitleListViewTarget
            // 
            this.subtitleListViewTarget.AllowColumnReorder = true;
            this.subtitleListViewTarget.ContextMenuStrip = this.contextMenuStrip2;
            this.subtitleListViewTarget.FirstVisibleIndex = -1;
            this.subtitleListViewTarget.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListViewTarget.FullRowSelect = true;
            this.subtitleListViewTarget.GridLines = true;
            this.subtitleListViewTarget.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListViewTarget.HideSelection = false;
            this.subtitleListViewTarget.Location = new System.Drawing.Point(465, 53);
            this.subtitleListViewTarget.Name = "subtitleListViewTarget";
            this.subtitleListViewTarget.OwnerDraw = true;
            this.subtitleListViewTarget.Size = new System.Drawing.Size(428, 431);
            this.subtitleListViewTarget.SubtitleFontBold = false;
            this.subtitleListViewTarget.SubtitleFontName = "Tahoma";
            this.subtitleListViewTarget.SubtitleFontSize = 8;
            this.subtitleListViewTarget.TabIndex = 98;
            this.subtitleListViewTarget.UseCompatibleStateImageBehavior = false;
            this.subtitleListViewTarget.UseSyntaxColoring = true;
            this.subtitleListViewTarget.View = System.Windows.Forms.View.Details;
            this.subtitleListViewTarget.Click += new System.EventHandler(this.subtitleListViewTarget_Click);
            this.subtitleListViewTarget.DoubleClick += new System.EventHandler(this.subtitleListViewTarget_DoubleClick);
            // 
            // subtitleListViewSource
            // 
            this.subtitleListViewSource.AllowColumnReorder = true;
            this.subtitleListViewSource.FirstVisibleIndex = -1;
            this.subtitleListViewSource.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListViewSource.FullRowSelect = true;
            this.subtitleListViewSource.GridLines = true;
            this.subtitleListViewSource.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListViewSource.HideSelection = false;
            this.subtitleListViewSource.Location = new System.Drawing.Point(12, 53);
            this.subtitleListViewSource.Name = "subtitleListViewSource";
            this.subtitleListViewSource.OwnerDraw = true;
            this.subtitleListViewSource.Size = new System.Drawing.Size(430, 431);
            this.subtitleListViewSource.SubtitleFontBold = false;
            this.subtitleListViewSource.SubtitleFontName = "Tahoma";
            this.subtitleListViewSource.SubtitleFontSize = 8;
            this.subtitleListViewSource.TabIndex = 97;
            this.subtitleListViewSource.UseCompatibleStateImageBehavior = false;
            this.subtitleListViewSource.UseSyntaxColoring = true;
            this.subtitleListViewSource.View = System.Windows.Forms.View.Details;
            this.subtitleListViewSource.Click += new System.EventHandler(this.subtitleListViewSource_Click);
            this.subtitleListViewSource.DoubleClick += new System.EventHandler(this.subtitleListViewSource_DoubleClick);
            // 
            // AutoTranslate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1058, 563);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.buttonStrategy);
            this.Controls.Add(this.labelFormality);
            this.Controls.Add(this.comboBoxFormality);
            this.Controls.Add(this.labelApiKey);
            this.Controls.Add(this.nikseTextBoxApiKey);
            this.Controls.Add(this.linkLabelPoweredBy);
            this.Controls.Add(this.nikseComboBoxEngine);
            this.Controls.Add(this.labelUrl);
            this.Controls.Add(this.nikseComboBoxUrl);
            this.Controls.Add(this.labelTarget);
            this.Controls.Add(this.labelSource);
            this.Controls.Add(this.comboBoxSource);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonTranslate);
            this.Controls.Add(this.comboBoxTarget);
            this.Controls.Add(this.subtitleListViewTarget);
            this.Controls.Add(this.subtitleListViewSource);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(900, 480);
            this.Name = "AutoTranslate";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AutoTranslate";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AutoTranslate_FormClosing);
            this.ResizeEnd += new System.EventHandler(this.AutoTranslate_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AutoTranslate_KeyDown);
            this.Resize += new System.EventHandler(this.AutoTranslate_Resize);
            this.contextMenuStrip2.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.NikseComboBox comboBoxSource;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonTranslate;
        private Controls.NikseComboBox comboBoxTarget;
        private Controls.SubtitleListView subtitleListViewTarget;
        private Controls.SubtitleListView subtitleListViewSource;
        private System.Windows.Forms.Label labelTarget;
        private System.Windows.Forms.Label labelSource;
        private System.Windows.Forms.Label labelUrl;
        private Controls.NikseComboBox nikseComboBoxUrl;
        private Controls.NikseComboBox nikseComboBoxEngine;
        private System.Windows.Forms.LinkLabel linkLabelPoweredBy;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem startLibreTranslateServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startNLLBServeServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startNLLBAPIServerToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStartLibre;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStartNLLBServe;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStartNLLBApi;
        private Controls.NikseTextBox nikseTextBoxApiKey;
        private System.Windows.Forms.Label labelApiKey;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFormality;
        private Nikse.SubtitleEdit.Controls.NikseLabel labelFormality;
        private System.Windows.Forms.Button buttonStrategy;
        private System.Windows.Forms.ToolStripMenuItem translateCurrentLineToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem translateCurrentLineToolStripMenuItem;
    }
}