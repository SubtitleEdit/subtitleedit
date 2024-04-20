
namespace Nikse.SubtitleEdit.Forms
{
    partial class GenerateVideoWithSoftSubs
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
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.contextMenuStripGenerate = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.promptParameterBeforeGenerateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelInputVideoFile = new System.Windows.Forms.Label();
            this.groupBoxSettings = new System.Windows.Forms.GroupBox();
            this.contextMenuStripMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setSuffixToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelNotSupported = new System.Windows.Forms.Label();
            this.buttonSetLanguage = new System.Windows.Forms.Button();
            this.buttonSetDefault = new System.Windows.Forms.Button();
            this.buttonToggleForced = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.ButtonRemoveSubtitles = new System.Windows.Forms.Button();
            this.ButtonMoveSubDown = new System.Windows.Forms.Button();
            this.ButtonMoveSubUp = new System.Windows.Forms.Button();
            this.buttonAddSubtitles = new System.Windows.Forms.Button();
            this.labelSubtitles = new System.Windows.Forms.Label();
            this.listViewSubtitles = new System.Windows.Forms.ListView();
            this.columnHeader1Type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2Language = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3Default = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4Forced = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5FileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuSubtitles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemStorageRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStorageRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemStorageMoveUp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStorageMoveDown = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleForcedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonOpenVideoFile = new System.Windows.Forms.Button();
            this.textBoxInputFileName = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.contextMenuStripRes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.x2160ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uHD3840x2160ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.k2048x1080ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dCI2KScope2048x858ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dCI2KFlat1998x1080ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.p1920x1080ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x1080ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.p1280x720ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x720ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.p848x480ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pAL720x576ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nTSC720x480ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x352ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x272ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.checkBoxDeleteInputVideoAfterGeneration = new System.Windows.Forms.CheckBox();
            this.textBoxLog = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.contextMenuStripMain2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSuffix2 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripGenerate.SuspendLayout();
            this.groupBoxSettings.SuspendLayout();
            this.contextMenuStripMain.SuspendLayout();
            this.contextMenuSubtitles.SuspendLayout();
            this.contextMenuStripRes.SuspendLayout();
            this.contextMenuStripMain2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGenerate.ContextMenuStrip = this.contextMenuStripGenerate;
            this.buttonGenerate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenerate.Location = new System.Drawing.Point(746, 441);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(121, 23);
            this.buttonGenerate.TabIndex = 4;
            this.buttonGenerate.Text = "Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
            // 
            // contextMenuStripGenerate
            // 
            this.contextMenuStripGenerate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.promptParameterBeforeGenerateToolStripMenuItem});
            this.contextMenuStripGenerate.Name = "contextMenuStripGenerate";
            this.contextMenuStripGenerate.Size = new System.Drawing.Size(290, 26);
            // 
            // promptParameterBeforeGenerateToolStripMenuItem
            // 
            this.promptParameterBeforeGenerateToolStripMenuItem.Name = "promptParameterBeforeGenerateToolStripMenuItem";
            this.promptParameterBeforeGenerateToolStripMenuItem.Size = new System.Drawing.Size(289, 22);
            this.promptParameterBeforeGenerateToolStripMenuItem.Text = "Prompt FFmpeg parameter and generate";
            this.promptParameterBeforeGenerateToolStripMenuItem.Click += new System.EventHandler(this.promptParameterBeforeGenerateToolStripMenuItem_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(873, 441);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelInputVideoFile
            // 
            this.labelInputVideoFile.AutoSize = true;
            this.labelInputVideoFile.Location = new System.Drawing.Point(19, 28);
            this.labelInputVideoFile.Name = "labelInputVideoFile";
            this.labelInputVideoFile.Size = new System.Drawing.Size(76, 13);
            this.labelInputVideoFile.TabIndex = 0;
            this.labelInputVideoFile.Text = "Input video file";
            // 
            // groupBoxSettings
            // 
            this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSettings.ContextMenuStrip = this.contextMenuStripMain;
            this.groupBoxSettings.Controls.Add(this.labelNotSupported);
            this.groupBoxSettings.Controls.Add(this.buttonSetLanguage);
            this.groupBoxSettings.Controls.Add(this.buttonSetDefault);
            this.groupBoxSettings.Controls.Add(this.buttonToggleForced);
            this.groupBoxSettings.Controls.Add(this.buttonClear);
            this.groupBoxSettings.Controls.Add(this.ButtonRemoveSubtitles);
            this.groupBoxSettings.Controls.Add(this.ButtonMoveSubDown);
            this.groupBoxSettings.Controls.Add(this.ButtonMoveSubUp);
            this.groupBoxSettings.Controls.Add(this.buttonAddSubtitles);
            this.groupBoxSettings.Controls.Add(this.labelSubtitles);
            this.groupBoxSettings.Controls.Add(this.listViewSubtitles);
            this.groupBoxSettings.Controls.Add(this.buttonOpenVideoFile);
            this.groupBoxSettings.Controls.Add(this.textBoxInputFileName);
            this.groupBoxSettings.Controls.Add(this.labelInputVideoFile);
            this.groupBoxSettings.Location = new System.Drawing.Point(12, 13);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(936, 422);
            this.groupBoxSettings.TabIndex = 0;
            this.groupBoxSettings.TabStop = false;
            // 
            // contextMenuStripMain
            // 
            this.contextMenuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setSuffixToolStripMenuItem});
            this.contextMenuStripMain.Name = "contextMenuStripMain";
            this.contextMenuStripMain.Size = new System.Drawing.Size(132, 26);
            // 
            // setSuffixToolStripMenuItem
            // 
            this.setSuffixToolStripMenuItem.Name = "setSuffixToolStripMenuItem";
            this.setSuffixToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.setSuffixToolStripMenuItem.Text = "Set suffix...";
            this.setSuffixToolStripMenuItem.Click += new System.EventHandler(this.setSuffixToolStripMenuItem_Click);
            // 
            // labelNotSupported
            // 
            this.labelNotSupported.AutoSize = true;
            this.labelNotSupported.ForeColor = System.Drawing.Color.Red;
            this.labelNotSupported.Location = new System.Drawing.Point(19, 67);
            this.labelNotSupported.Name = "labelNotSupported";
            this.labelNotSupported.Size = new System.Drawing.Size(95, 13);
            this.labelNotSupported.TabIndex = 36;
            this.labelNotSupported.Text = "labelNotSupported";
            // 
            // buttonSetLanguage
            // 
            this.buttonSetLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSetLanguage.Location = new System.Drawing.Point(808, 311);
            this.buttonSetLanguage.Name = "buttonSetLanguage";
            this.buttonSetLanguage.Size = new System.Drawing.Size(122, 23);
            this.buttonSetLanguage.TabIndex = 35;
            this.buttonSetLanguage.Text = "Set language...";
            this.buttonSetLanguage.UseVisualStyleBackColor = true;
            this.buttonSetLanguage.Click += new System.EventHandler(this.buttonSetLanguage_Click);
            // 
            // buttonSetDefault
            // 
            this.buttonSetDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSetDefault.Location = new System.Drawing.Point(808, 282);
            this.buttonSetDefault.Name = "buttonSetDefault";
            this.buttonSetDefault.Size = new System.Drawing.Size(122, 23);
            this.buttonSetDefault.TabIndex = 34;
            this.buttonSetDefault.Text = "Toggle default";
            this.buttonSetDefault.UseVisualStyleBackColor = true;
            this.buttonSetDefault.Click += new System.EventHandler(this.buttonSetDefault_Click);
            // 
            // buttonToggleForced
            // 
            this.buttonToggleForced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonToggleForced.Location = new System.Drawing.Point(808, 253);
            this.buttonToggleForced.Name = "buttonToggleForced";
            this.buttonToggleForced.Size = new System.Drawing.Size(122, 23);
            this.buttonToggleForced.TabIndex = 33;
            this.buttonToggleForced.Text = "Toggle forced";
            this.buttonToggleForced.UseVisualStyleBackColor = true;
            this.buttonToggleForced.Click += new System.EventHandler(this.buttonToggleForced_Click);
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.Location = new System.Drawing.Point(808, 168);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(122, 23);
            this.buttonClear.TabIndex = 30;
            this.buttonClear.Text = "Remove all";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // ButtonRemoveSubtitles
            // 
            this.ButtonRemoveSubtitles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonRemoveSubtitles.Location = new System.Drawing.Point(808, 140);
            this.ButtonRemoveSubtitles.Name = "ButtonRemoveSubtitles";
            this.ButtonRemoveSubtitles.Size = new System.Drawing.Size(122, 23);
            this.ButtonRemoveSubtitles.TabIndex = 29;
            this.ButtonRemoveSubtitles.Text = "Remove";
            this.ButtonRemoveSubtitles.UseVisualStyleBackColor = true;
            this.ButtonRemoveSubtitles.Click += new System.EventHandler(this.ButtonRemoveSubtitles_Click);
            // 
            // ButtonMoveSubDown
            // 
            this.ButtonMoveSubDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonMoveSubDown.Location = new System.Drawing.Point(808, 224);
            this.ButtonMoveSubDown.Name = "ButtonMoveSubDown";
            this.ButtonMoveSubDown.Size = new System.Drawing.Size(122, 23);
            this.ButtonMoveSubDown.TabIndex = 32;
            this.ButtonMoveSubDown.Text = "Move down";
            this.ButtonMoveSubDown.UseVisualStyleBackColor = true;
            this.ButtonMoveSubDown.Click += new System.EventHandler(this.ButtonMoveSubDown_Click);
            // 
            // ButtonMoveSubUp
            // 
            this.ButtonMoveSubUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonMoveSubUp.Location = new System.Drawing.Point(808, 196);
            this.ButtonMoveSubUp.Name = "ButtonMoveSubUp";
            this.ButtonMoveSubUp.Size = new System.Drawing.Size(122, 23);
            this.ButtonMoveSubUp.TabIndex = 31;
            this.ButtonMoveSubUp.Text = "Move up";
            this.ButtonMoveSubUp.UseVisualStyleBackColor = true;
            this.ButtonMoveSubUp.Click += new System.EventHandler(this.ButtonMoveSubUp_Click);
            // 
            // buttonAddSubtitles
            // 
            this.buttonAddSubtitles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddSubtitles.Location = new System.Drawing.Point(808, 111);
            this.buttonAddSubtitles.Name = "buttonAddSubtitles";
            this.buttonAddSubtitles.Size = new System.Drawing.Size(122, 23);
            this.buttonAddSubtitles.TabIndex = 28;
            this.buttonAddSubtitles.Text = "Add...";
            this.buttonAddSubtitles.UseVisualStyleBackColor = true;
            this.buttonAddSubtitles.Click += new System.EventHandler(this.buttonAddSubtitles_Click);
            // 
            // labelSubtitles
            // 
            this.labelSubtitles.AutoSize = true;
            this.labelSubtitles.Location = new System.Drawing.Point(19, 92);
            this.labelSubtitles.Name = "labelSubtitles";
            this.labelSubtitles.Size = new System.Drawing.Size(47, 13);
            this.labelSubtitles.TabIndex = 26;
            this.labelSubtitles.Text = "Subtitles";
            // 
            // listViewSubtitles
            // 
            this.listViewSubtitles.AllowDrop = true;
            this.listViewSubtitles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewSubtitles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1Type,
            this.columnHeader2Language,
            this.columnHeader3Default,
            this.columnHeader4Forced,
            this.columnHeader5FileName});
            this.listViewSubtitles.ContextMenuStrip = this.contextMenuSubtitles;
            this.listViewSubtitles.FullRowSelect = true;
            this.listViewSubtitles.HideSelection = false;
            this.listViewSubtitles.Location = new System.Drawing.Point(22, 111);
            this.listViewSubtitles.Name = "listViewSubtitles";
            this.listViewSubtitles.Size = new System.Drawing.Size(780, 305);
            this.listViewSubtitles.TabIndex = 25;
            this.listViewSubtitles.UseCompatibleStateImageBehavior = false;
            this.listViewSubtitles.View = System.Windows.Forms.View.Details;
            this.listViewSubtitles.SelectedIndexChanged += new System.EventHandler(this.listViewSubtitles_SelectedIndexChanged);
            this.listViewSubtitles.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewSubtitles_DragDrop);
            this.listViewSubtitles.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewSubtitles_DragEnter);
            this.listViewSubtitles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewSubtitles_MouseDoubleClick);
            // 
            // columnHeader1Type
            // 
            this.columnHeader1Type.Text = "Type";
            this.columnHeader1Type.Width = 150;
            // 
            // columnHeader2Language
            // 
            this.columnHeader2Language.Text = "Language/title";
            this.columnHeader2Language.Width = 125;
            // 
            // columnHeader3Default
            // 
            this.columnHeader3Default.Text = "Default";
            // 
            // columnHeader4Forced
            // 
            this.columnHeader4Forced.Text = "Forced";
            // 
            // columnHeader5FileName
            // 
            this.columnHeader5FileName.Text = "File name";
            this.columnHeader5FileName.Width = 300;
            // 
            // contextMenuSubtitles
            // 
            this.contextMenuSubtitles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.toolStripSeparator1,
            this.toolStripMenuItemStorageRemove,
            this.toolStripMenuItemStorageRemoveAll,
            this.toolStripSeparator2,
            this.toolStripMenuItemStorageMoveUp,
            this.toolStripMenuItemStorageMoveDown,
            this.toggleForcedToolStripMenuItem,
            this.toggleDefaultToolStripMenuItem,
            this.setLanguageToolStripMenuItem,
            this.toolStripSeparator3,
            this.viewToolStripMenuItem});
            this.contextMenuSubtitles.Name = "contextMenuStrip1";
            this.contextMenuSubtitles.Size = new System.Drawing.Size(203, 220);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.addToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.addToolStripMenuItem.Text = "Add...";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.buttonAddSubtitles_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(199, 6);
            // 
            // toolStripMenuItemStorageRemove
            // 
            this.toolStripMenuItemStorageRemove.Name = "toolStripMenuItemStorageRemove";
            this.toolStripMenuItemStorageRemove.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.toolStripMenuItemStorageRemove.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemStorageRemove.Text = "Remove";
            this.toolStripMenuItemStorageRemove.Click += new System.EventHandler(this.ButtonRemoveSubtitles_Click);
            // 
            // toolStripMenuItemStorageRemoveAll
            // 
            this.toolStripMenuItemStorageRemoveAll.Name = "toolStripMenuItemStorageRemoveAll";
            this.toolStripMenuItemStorageRemoveAll.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemStorageRemoveAll.Text = "Remove all";
            this.toolStripMenuItemStorageRemoveAll.Click += new System.EventHandler(this.toolStripMenuItemStorageRemoveAll_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(199, 6);
            // 
            // toolStripMenuItemStorageMoveUp
            // 
            this.toolStripMenuItemStorageMoveUp.Name = "toolStripMenuItemStorageMoveUp";
            this.toolStripMenuItemStorageMoveUp.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.toolStripMenuItemStorageMoveUp.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemStorageMoveUp.Text = "Move up";
            this.toolStripMenuItemStorageMoveUp.Click += new System.EventHandler(this.ButtonMoveSubUp_Click);
            // 
            // toolStripMenuItemStorageMoveDown
            // 
            this.toolStripMenuItemStorageMoveDown.Name = "toolStripMenuItemStorageMoveDown";
            this.toolStripMenuItemStorageMoveDown.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.toolStripMenuItemStorageMoveDown.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemStorageMoveDown.Text = "Move down";
            this.toolStripMenuItemStorageMoveDown.Click += new System.EventHandler(this.ButtonMoveSubDown_Click);
            // 
            // toggleForcedToolStripMenuItem
            // 
            this.toggleForcedToolStripMenuItem.Name = "toggleForcedToolStripMenuItem";
            this.toggleForcedToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.toggleForcedToolStripMenuItem.Text = "Toggle forced";
            this.toggleForcedToolStripMenuItem.Click += new System.EventHandler(this.buttonToggleForced_Click);
            // 
            // toggleDefaultToolStripMenuItem
            // 
            this.toggleDefaultToolStripMenuItem.Name = "toggleDefaultToolStripMenuItem";
            this.toggleDefaultToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.toggleDefaultToolStripMenuItem.Text = "Toggle default";
            this.toggleDefaultToolStripMenuItem.DropDownOpening += new System.EventHandler(this.toggleDefaultToolStripMenuItem_DropDownOpening);
            this.toggleDefaultToolStripMenuItem.Click += new System.EventHandler(this.buttonSetDefault_Click);
            // 
            // setLanguageToolStripMenuItem
            // 
            this.setLanguageToolStripMenuItem.Name = "setLanguageToolStripMenuItem";
            this.setLanguageToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.setLanguageToolStripMenuItem.Text = "Set language...";
            this.setLanguageToolStripMenuItem.Click += new System.EventHandler(this.buttonSetLanguage_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(199, 6);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.viewToolStripMenuItem.Text = "View...";
            this.viewToolStripMenuItem.Click += new System.EventHandler(this.viewToolStripMenuItem_Click);
            // 
            // buttonOpenVideoFile
            // 
            this.buttonOpenVideoFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenVideoFile.Location = new System.Drawing.Point(888, 44);
            this.buttonOpenVideoFile.Name = "buttonOpenVideoFile";
            this.buttonOpenVideoFile.Size = new System.Drawing.Size(33, 23);
            this.buttonOpenVideoFile.TabIndex = 24;
            this.buttonOpenVideoFile.Text = "...";
            this.buttonOpenVideoFile.UseVisualStyleBackColor = true;
            this.buttonOpenVideoFile.Click += new System.EventHandler(this.buttonOpenVideoFile_Click);
            // 
            // textBoxInputFileName
            // 
            this.textBoxInputFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxInputFileName.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxInputFileName.Location = new System.Drawing.Point(22, 44);
            this.textBoxInputFileName.Name = "textBoxInputFileName";
            this.textBoxInputFileName.ReadOnly = true;
            this.textBoxInputFileName.Size = new System.Drawing.Size(860, 20);
            this.textBoxInputFileName.TabIndex = 16;
            // 
            // contextMenuStripRes
            // 
            this.contextMenuStripRes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.x2160ToolStripMenuItem,
            this.uHD3840x2160ToolStripMenuItem,
            this.k2048x1080ToolStripMenuItem,
            this.dCI2KScope2048x858ToolStripMenuItem,
            this.dCI2KFlat1998x1080ToolStripMenuItem,
            this.p1920x1080ToolStripMenuItem,
            this.x1080ToolStripMenuItem,
            this.p1280x720ToolStripMenuItem,
            this.x720ToolStripMenuItem,
            this.p848x480ToolStripMenuItem,
            this.pAL720x576ToolStripMenuItem,
            this.nTSC720x480ToolStripMenuItem,
            this.x352ToolStripMenuItem,
            this.x272ToolStripMenuItem});
            this.contextMenuStripRes.Name = "contextMenuStripRes";
            this.contextMenuStripRes.Size = new System.Drawing.Size(204, 312);
            // 
            // x2160ToolStripMenuItem
            // 
            this.x2160ToolStripMenuItem.Name = "x2160ToolStripMenuItem";
            this.x2160ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.x2160ToolStripMenuItem.Text = "4K (4096x2160)";
            // 
            // uHD3840x2160ToolStripMenuItem
            // 
            this.uHD3840x2160ToolStripMenuItem.Name = "uHD3840x2160ToolStripMenuItem";
            this.uHD3840x2160ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.uHD3840x2160ToolStripMenuItem.Text = "UHD (3840x2160)";
            // 
            // k2048x1080ToolStripMenuItem
            // 
            this.k2048x1080ToolStripMenuItem.Name = "k2048x1080ToolStripMenuItem";
            this.k2048x1080ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.k2048x1080ToolStripMenuItem.Text = "2K (2048x1080)";
            // 
            // dCI2KScope2048x858ToolStripMenuItem
            // 
            this.dCI2KScope2048x858ToolStripMenuItem.Name = "dCI2KScope2048x858ToolStripMenuItem";
            this.dCI2KScope2048x858ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.dCI2KScope2048x858ToolStripMenuItem.Text = "DCI 2K Scope (2048x858)";
            // 
            // dCI2KFlat1998x1080ToolStripMenuItem
            // 
            this.dCI2KFlat1998x1080ToolStripMenuItem.Name = "dCI2KFlat1998x1080ToolStripMenuItem";
            this.dCI2KFlat1998x1080ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.dCI2KFlat1998x1080ToolStripMenuItem.Text = "DCI 2K Flat (1998x1080)";
            // 
            // p1920x1080ToolStripMenuItem
            // 
            this.p1920x1080ToolStripMenuItem.Name = "p1920x1080ToolStripMenuItem";
            this.p1920x1080ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.p1920x1080ToolStripMenuItem.Text = "1080p (1920x1080)";
            // 
            // x1080ToolStripMenuItem
            // 
            this.x1080ToolStripMenuItem.Name = "x1080ToolStripMenuItem";
            this.x1080ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.x1080ToolStripMenuItem.Text = "1440x1080";
            // 
            // p1280x720ToolStripMenuItem
            // 
            this.p1280x720ToolStripMenuItem.Name = "p1280x720ToolStripMenuItem";
            this.p1280x720ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.p1280x720ToolStripMenuItem.Text = "720p (1280x720)";
            // 
            // x720ToolStripMenuItem
            // 
            this.x720ToolStripMenuItem.Name = "x720ToolStripMenuItem";
            this.x720ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.x720ToolStripMenuItem.Text = "960x720";
            // 
            // p848x480ToolStripMenuItem
            // 
            this.p848x480ToolStripMenuItem.Name = "p848x480ToolStripMenuItem";
            this.p848x480ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.p848x480ToolStripMenuItem.Text = "480p (848x480)";
            // 
            // pAL720x576ToolStripMenuItem
            // 
            this.pAL720x576ToolStripMenuItem.Name = "pAL720x576ToolStripMenuItem";
            this.pAL720x576ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.pAL720x576ToolStripMenuItem.Text = "PAL (720x576)";
            // 
            // nTSC720x480ToolStripMenuItem
            // 
            this.nTSC720x480ToolStripMenuItem.Name = "nTSC720x480ToolStripMenuItem";
            this.nTSC720x480ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.nTSC720x480ToolStripMenuItem.Text = "NTSC (720x480)";
            // 
            // x352ToolStripMenuItem
            // 
            this.x352ToolStripMenuItem.Name = "x352ToolStripMenuItem";
            this.x352ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.x352ToolStripMenuItem.Text = "640x352";
            // 
            // x272ToolStripMenuItem
            // 
            this.x272ToolStripMenuItem.Name = "x272ToolStripMenuItem";
            this.x272ToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.x272ToolStripMenuItem.Text = "640x272";
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(12, 438);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPleaseWait.TabIndex = 32;
            this.labelPleaseWait.Text = "Please wait...";
            // 
            // checkBoxDeleteInputVideoAfterGeneration
            // 
            this.checkBoxDeleteInputVideoAfterGeneration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxDeleteInputVideoAfterGeneration.AutoSize = true;
            this.checkBoxDeleteInputVideoAfterGeneration.Location = new System.Drawing.Point(531, 445);
            this.checkBoxDeleteInputVideoAfterGeneration.Name = "checkBoxDeleteInputVideoAfterGeneration";
            this.checkBoxDeleteInputVideoAfterGeneration.Size = new System.Drawing.Size(209, 17);
            this.checkBoxDeleteInputVideoAfterGeneration.TabIndex = 3;
            this.checkBoxDeleteInputVideoAfterGeneration.Text = "Delete input video file after \"Generate\"";
            this.checkBoxDeleteInputVideoAfterGeneration.UseVisualStyleBackColor = true;
            // 
            // textBoxLog
            // 
            this.textBoxLog.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxLog.Location = new System.Drawing.Point(12, 13);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.Size = new System.Drawing.Size(188, 26);
            this.textBoxLog.TabIndex = 31;
            // 
            // contextMenuStripMain2
            // 
            this.contextMenuStripMain2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSuffix2});
            this.contextMenuStripMain2.Name = "contextMenuStripMain";
            this.contextMenuStripMain2.Size = new System.Drawing.Size(132, 26);
            // 
            // toolStripMenuItemSuffix2
            // 
            this.toolStripMenuItemSuffix2.Name = "toolStripMenuItemSuffix2";
            this.toolStripMenuItemSuffix2.Size = new System.Drawing.Size(131, 22);
            this.toolStripMenuItemSuffix2.Text = "Set suffix...";
            this.toolStripMenuItemSuffix2.Click += new System.EventHandler(this.setSuffixToolStripMenuItem_Click);
            // 
            // GenerateVideoWithSoftSubs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 476);
            this.ContextMenuStrip = this.contextMenuStripMain2;
            this.Controls.Add(this.checkBoxDeleteInputVideoAfterGeneration);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.groupBoxSettings);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.textBoxLog);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 450);
            this.Name = "GenerateVideoWithSoftSubs";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generate video with soft subs";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GenerateVideoWithHardSubs_FormClosing);
            this.Shown += new System.EventHandler(this.GenerateVideoWithHardSubs_Shown);
            this.ResizeEnd += new System.EventHandler(this.GenerateVideoWithSoftSubs_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenerateVideoWithSoftSubs_KeyDown);
            this.contextMenuStripGenerate.ResumeLayout(false);
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            this.contextMenuStripMain.ResumeLayout(false);
            this.contextMenuSubtitles.ResumeLayout(false);
            this.contextMenuStripRes.ResumeLayout(false);
            this.contextMenuStripMain2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelInputVideoFile;
        private System.Windows.Forms.GroupBox groupBoxSettings;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxLog;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRes;
        private System.Windows.Forms.ToolStripMenuItem x2160ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uHD3840x2160ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem k2048x1080ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dCI2KScope2048x858ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dCI2KFlat1998x1080ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem p1920x1080ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x1080ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem p1280x720ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x720ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem p848x480ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pAL720x576ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nTSC720x480ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x352ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x272ToolStripMenuItem;
        private System.Windows.Forms.Label labelSubtitles;
        private System.Windows.Forms.ListView listViewSubtitles;
        private System.Windows.Forms.ColumnHeader columnHeader1Type;
        private System.Windows.Forms.ColumnHeader columnHeader2Language;
        private System.Windows.Forms.ColumnHeader columnHeader3Default;
        private System.Windows.Forms.ColumnHeader columnHeader4Forced;
        private System.Windows.Forms.ColumnHeader columnHeader5FileName;
        private System.Windows.Forms.Button buttonOpenVideoFile;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxInputFileName;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button ButtonRemoveSubtitles;
        private System.Windows.Forms.Button ButtonMoveSubDown;
        private System.Windows.Forms.Button ButtonMoveSubUp;
        private System.Windows.Forms.Button buttonAddSubtitles;
        private System.Windows.Forms.ContextMenuStrip contextMenuSubtitles;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageRemove;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageRemoveAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageMoveUp;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStorageMoveDown;
        private System.Windows.Forms.Button buttonToggleForced;
        private System.Windows.Forms.Button buttonSetDefault;
        private System.Windows.Forms.ToolStripMenuItem toggleForcedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleDefaultToolStripMenuItem;
        private System.Windows.Forms.Button buttonSetLanguage;
        private System.Windows.Forms.ToolStripMenuItem setLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripGenerate;
        private System.Windows.Forms.ToolStripMenuItem promptParameterBeforeGenerateToolStripMenuItem;
        private System.Windows.Forms.Label labelNotSupported;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.CheckBox checkBoxDeleteInputVideoAfterGeneration;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMain;
        private System.Windows.Forms.ToolStripMenuItem setSuffixToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMain2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSuffix2;
    }
}