namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ExportCustomTextFormat
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
            this.groupBoxTemplate = new System.Windows.Forms.GroupBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.labelName = new System.Windows.Forms.Label();
            this.comboBoxNewLine = new System.Windows.Forms.ComboBox();
            this.comboBoxTimeCode = new System.Windows.Forms.ComboBox();
            this.labelNewLine = new System.Windows.Forms.Label();
            this.labelTimeCode = new System.Windows.Forms.Label();
            this.textBoxFooter = new System.Windows.Forms.TextBox();
            this.contextMenuStripFooter = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.labelFooter = new System.Windows.Forms.Label();
            this.labelTextLine = new System.Windows.Forms.Label();
            this.textBoxParagraph = new System.Windows.Forms.TextBox();
            this.contextMenuStripParagraph = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.insertHHMMSSMSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertnumberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertdurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.translationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemActor = new System.Windows.Forms.ToolStripMenuItem();
            this.tabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.textline2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelHeader = new System.Windows.Forms.Label();
            this.textBoxHeader = new System.Windows.Forms.TextBox();
            this.contextMenuStripHeader = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.textBoxPreview = new System.Windows.Forms.TextBox();
            this.cpsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textlengthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cpsperiodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxTemplate.SuspendLayout();
            this.contextMenuStripFooter.SuspendLayout();
            this.contextMenuStripParagraph.SuspendLayout();
            this.contextMenuStripHeader.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxTemplate
            // 
            this.groupBoxTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxTemplate.Controls.Add(this.textBoxName);
            this.groupBoxTemplate.Controls.Add(this.labelName);
            this.groupBoxTemplate.Controls.Add(this.comboBoxNewLine);
            this.groupBoxTemplate.Controls.Add(this.comboBoxTimeCode);
            this.groupBoxTemplate.Controls.Add(this.labelNewLine);
            this.groupBoxTemplate.Controls.Add(this.labelTimeCode);
            this.groupBoxTemplate.Controls.Add(this.textBoxFooter);
            this.groupBoxTemplate.Controls.Add(this.labelFooter);
            this.groupBoxTemplate.Controls.Add(this.labelTextLine);
            this.groupBoxTemplate.Controls.Add(this.textBoxParagraph);
            this.groupBoxTemplate.Controls.Add(this.labelHeader);
            this.groupBoxTemplate.Controls.Add(this.textBoxHeader);
            this.groupBoxTemplate.Location = new System.Drawing.Point(12, 12);
            this.groupBoxTemplate.Name = "groupBoxTemplate";
            this.groupBoxTemplate.Size = new System.Drawing.Size(346, 393);
            this.groupBoxTemplate.TabIndex = 0;
            this.groupBoxTemplate.TabStop = false;
            this.groupBoxTemplate.Text = "Current template";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(49, 30);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(291, 20);
            this.textBoxName.TabIndex = 1;
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(8, 33);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(35, 13);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name";
            // 
            // comboBoxNewLine
            // 
            this.comboBoxNewLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxNewLine.FormattingEnabled = true;
            this.comboBoxNewLine.Items.AddRange(new object[] {
            "[Do not modify]",
            "||",
            "[Only newline (hex char 0xd)]"});
            this.comboBoxNewLine.Location = new System.Drawing.Point(241, 266);
            this.comboBoxNewLine.Name = "comboBoxNewLine";
            this.comboBoxNewLine.Size = new System.Drawing.Size(99, 21);
            this.comboBoxNewLine.TabIndex = 9;
            this.comboBoxNewLine.SelectedIndexChanged += new System.EventHandler(this.comboBoxNewLine_SelectedIndexChanged);
            this.comboBoxNewLine.TextChanged += new System.EventHandler(this.comboBoxNewLine_TextChanged);
            // 
            // comboBoxTimeCode
            // 
            this.comboBoxTimeCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxTimeCode.FormattingEnabled = true;
            this.comboBoxTimeCode.Items.AddRange(new object[] {
            "hh:mm:ss,zzz",
            "hh:mm:ss.ff",
            "h:mm:ss,zzz",
            "h:mm:ss,zz",
            "ff",
            "zzz",
            "ss"});
            this.comboBoxTimeCode.Location = new System.Drawing.Point(69, 266);
            this.comboBoxTimeCode.Name = "comboBoxTimeCode";
            this.comboBoxTimeCode.Size = new System.Drawing.Size(92, 21);
            this.comboBoxTimeCode.TabIndex = 7;
            this.comboBoxTimeCode.SelectedIndexChanged += new System.EventHandler(this.comboBoxTimeCode_SelectedIndexChanged);
            this.comboBoxTimeCode.TextChanged += new System.EventHandler(this.comboBoxTimeCode_TextChanged);
            // 
            // labelNewLine
            // 
            this.labelNewLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelNewLine.AutoSize = true;
            this.labelNewLine.Location = new System.Drawing.Point(185, 267);
            this.labelNewLine.Name = "labelNewLine";
            this.labelNewLine.Size = new System.Drawing.Size(48, 13);
            this.labelNewLine.TabIndex = 8;
            this.labelNewLine.Text = "New line";
            // 
            // labelTimeCode
            // 
            this.labelTimeCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTimeCode.AutoSize = true;
            this.labelTimeCode.Location = new System.Drawing.Point(6, 267);
            this.labelTimeCode.Name = "labelTimeCode";
            this.labelTimeCode.Size = new System.Drawing.Size(57, 13);
            this.labelTimeCode.TabIndex = 6;
            this.labelTimeCode.Text = "Time code";
            // 
            // textBoxFooter
            // 
            this.textBoxFooter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxFooter.ContextMenuStrip = this.contextMenuStripFooter;
            this.textBoxFooter.Location = new System.Drawing.Point(6, 313);
            this.textBoxFooter.Multiline = true;
            this.textBoxFooter.Name = "textBoxFooter";
            this.textBoxFooter.Size = new System.Drawing.Size(334, 70);
            this.textBoxFooter.TabIndex = 11;
            this.textBoxFooter.TextChanged += new System.EventHandler(this.TextBoxParagraphTextChanged);
            // 
            // contextMenuStripFooter
            // 
            this.contextMenuStripFooter.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5});
            this.contextMenuStripFooter.Name = "contextMenuStrip1";
            this.contextMenuStripFooter.Size = new System.Drawing.Size(114, 70);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(113, 22);
            this.toolStripMenuItem3.Text = "{title}";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.InsertTagFooter);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(113, 22);
            this.toolStripMenuItem4.Text = "{#lines}";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.InsertTagFooter);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(113, 22);
            this.toolStripMenuItem5.Text = "{tab}";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.InsertTagFooter);
            // 
            // labelFooter
            // 
            this.labelFooter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFooter.AutoSize = true;
            this.labelFooter.Location = new System.Drawing.Point(6, 297);
            this.labelFooter.Name = "labelFooter";
            this.labelFooter.Size = new System.Drawing.Size(37, 13);
            this.labelFooter.TabIndex = 10;
            this.labelFooter.Text = "Footer";
            // 
            // labelTextLine
            // 
            this.labelTextLine.AutoSize = true;
            this.labelTextLine.Location = new System.Drawing.Point(6, 159);
            this.labelTextLine.Name = "labelTextLine";
            this.labelTextLine.Size = new System.Drawing.Size(104, 13);
            this.labelTextLine.TabIndex = 4;
            this.labelTextLine.Text = "Text line (paragraph)";
            // 
            // textBoxParagraph
            // 
            this.textBoxParagraph.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxParagraph.ContextMenuStrip = this.contextMenuStripParagraph;
            this.textBoxParagraph.Location = new System.Drawing.Point(6, 175);
            this.textBoxParagraph.Multiline = true;
            this.textBoxParagraph.Name = "textBoxParagraph";
            this.textBoxParagraph.Size = new System.Drawing.Size(334, 85);
            this.textBoxParagraph.TabIndex = 5;
            this.textBoxParagraph.Text = "{number}\r\n{start} --> {end}\r\n{text}\r\n\r\n";
            this.textBoxParagraph.TextChanged += new System.EventHandler(this.TextBoxParagraphTextChanged);
            // 
            // contextMenuStripParagraph
            // 
            this.contextMenuStripParagraph.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertHHMMSSMSToolStripMenuItem,
            this.insertendToolStripMenuItem,
            this.insertnumberToolStripMenuItem,
            this.insertdurationToolStripMenuItem,
            this.textToolStripMenuItem,
            this.translationToolStripMenuItem,
            this.toolStripMenuItemActor,
            this.tabToolStripMenuItem,
            this.textToolStripMenuItem1,
            this.textline2ToolStripMenuItem,
            this.cpsperiodToolStripMenuItem,
            this.cpsToolStripMenuItem,
            this.textlengthToolStripMenuItem});
            this.contextMenuStripParagraph.Name = "contextMenuStrip1";
            this.contextMenuStripParagraph.Size = new System.Drawing.Size(147, 290);
            // 
            // insertHHMMSSMSToolStripMenuItem
            // 
            this.insertHHMMSSMSToolStripMenuItem.Name = "insertHHMMSSMSToolStripMenuItem";
            this.insertHHMMSSMSToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.insertHHMMSSMSToolStripMenuItem.Text = "{start}";
            this.insertHHMMSSMSToolStripMenuItem.Click += new System.EventHandler(this.InsertTag);
            // 
            // insertendToolStripMenuItem
            // 
            this.insertendToolStripMenuItem.Name = "insertendToolStripMenuItem";
            this.insertendToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.insertendToolStripMenuItem.Text = "{end}";
            this.insertendToolStripMenuItem.Click += new System.EventHandler(this.InsertTag);
            // 
            // insertnumberToolStripMenuItem
            // 
            this.insertnumberToolStripMenuItem.Name = "insertnumberToolStripMenuItem";
            this.insertnumberToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.insertnumberToolStripMenuItem.Text = "{number}";
            this.insertnumberToolStripMenuItem.Click += new System.EventHandler(this.InsertTag);
            // 
            // insertdurationToolStripMenuItem
            // 
            this.insertdurationToolStripMenuItem.Name = "insertdurationToolStripMenuItem";
            this.insertdurationToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.insertdurationToolStripMenuItem.Text = "{duration}";
            this.insertdurationToolStripMenuItem.Click += new System.EventHandler(this.InsertTag);
            // 
            // textToolStripMenuItem
            // 
            this.textToolStripMenuItem.Name = "textToolStripMenuItem";
            this.textToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.textToolStripMenuItem.Text = "{text}";
            this.textToolStripMenuItem.Click += new System.EventHandler(this.InsertTag);
            // 
            // translationToolStripMenuItem
            // 
            this.translationToolStripMenuItem.Name = "translationToolStripMenuItem";
            this.translationToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.translationToolStripMenuItem.Text = "{translation}";
            this.translationToolStripMenuItem.Click += new System.EventHandler(this.InsertTag);
            // 
            // toolStripMenuItemActor
            // 
            this.toolStripMenuItemActor.Name = "toolStripMenuItemActor";
            this.toolStripMenuItemActor.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItemActor.Text = "{actor}";
            this.toolStripMenuItemActor.Click += new System.EventHandler(this.InsertTag);
            // 
            // tabToolStripMenuItem
            // 
            this.tabToolStripMenuItem.Name = "tabToolStripMenuItem";
            this.tabToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.tabToolStripMenuItem.Text = "{tab}";
            this.tabToolStripMenuItem.Click += new System.EventHandler(this.InsertTag);
            // 
            // textToolStripMenuItem1
            // 
            this.textToolStripMenuItem1.Name = "textToolStripMenuItem1";
            this.textToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.textToolStripMenuItem1.Text = "{text-line-1}";
            this.textToolStripMenuItem1.Click += new System.EventHandler(this.InsertTag);
            // 
            // textline2ToolStripMenuItem
            // 
            this.textline2ToolStripMenuItem.Name = "textline2ToolStripMenuItem";
            this.textline2ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.textline2ToolStripMenuItem.Text = "{text-line-2}";
            this.textline2ToolStripMenuItem.Click += new System.EventHandler(this.InsertTag);
            // 
            // labelHeader
            // 
            this.labelHeader.AutoSize = true;
            this.labelHeader.Location = new System.Drawing.Point(6, 64);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Size = new System.Drawing.Size(42, 13);
            this.labelHeader.TabIndex = 2;
            this.labelHeader.Text = "Header";
            // 
            // textBoxHeader
            // 
            this.textBoxHeader.ContextMenuStrip = this.contextMenuStripHeader;
            this.textBoxHeader.Location = new System.Drawing.Point(6, 80);
            this.textBoxHeader.Multiline = true;
            this.textBoxHeader.Name = "textBoxHeader";
            this.textBoxHeader.Size = new System.Drawing.Size(334, 64);
            this.textBoxHeader.TabIndex = 3;
            this.textBoxHeader.TextChanged += new System.EventHandler(this.TextBoxParagraphTextChanged);
            // 
            // contextMenuStripHeader
            // 
            this.contextMenuStripHeader.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripMenuItem7});
            this.contextMenuStripHeader.Name = "contextMenuStrip1";
            this.contextMenuStripHeader.Size = new System.Drawing.Size(114, 70);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(113, 22);
            this.toolStripMenuItem1.Text = "{title}";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.InsertTagHeader);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(113, 22);
            this.toolStripMenuItem2.Text = "{#lines}";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.InsertTagHeader);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(113, 22);
            this.toolStripMenuItem7.Text = "{tab}";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.InsertTagHeader);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(628, 412);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(547, 412);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.textBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(364, 12);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(346, 393);
            this.groupBoxPreview.TabIndex = 1;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // textBoxPreview
            // 
            this.textBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxPreview.Location = new System.Drawing.Point(3, 16);
            this.textBoxPreview.Multiline = true;
            this.textBoxPreview.Name = "textBoxPreview";
            this.textBoxPreview.ReadOnly = true;
            this.textBoxPreview.Size = new System.Drawing.Size(340, 374);
            this.textBoxPreview.TabIndex = 0;
            // 
            // cpsToolStripMenuItem
            // 
            this.cpsToolStripMenuItem.Name = "cpsToolStripMenuItem";
            this.cpsToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.cpsToolStripMenuItem.Text = "{cps-comma}";
            this.cpsToolStripMenuItem.Click += new System.EventHandler(this.InsertTag);
            // 
            // textlengthToolStripMenuItem
            // 
            this.textlengthToolStripMenuItem.Name = "textlengthToolStripMenuItem";
            this.textlengthToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.textlengthToolStripMenuItem.Text = "{text-length}";
            this.textlengthToolStripMenuItem.Click += new System.EventHandler(this.InsertTag);
            // 
            // cpsperiodToolStripMenuItem
            // 
            this.cpsperiodToolStripMenuItem.Name = "cpsperiodToolStripMenuItem";
            this.cpsperiodToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.cpsperiodToolStripMenuItem.Text = "{cps-period}";
            this.cpsperiodToolStripMenuItem.Click += new System.EventHandler(this.InsertTag);
            // 
            // ExportCustomTextFormat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 445);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxTemplate);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 450);
            this.Name = "ExportCustomTextFormat";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Custom text format";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExportCustomTextFormatKeyDown);
            this.groupBoxTemplate.ResumeLayout(false);
            this.groupBoxTemplate.PerformLayout();
            this.contextMenuStripFooter.ResumeLayout(false);
            this.contextMenuStripParagraph.ResumeLayout(false);
            this.contextMenuStripHeader.ResumeLayout(false);
            this.groupBoxPreview.ResumeLayout(false);
            this.groupBoxPreview.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxTemplate;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.TextBox textBoxHeader;
        private System.Windows.Forms.Label labelFooter;
        private System.Windows.Forms.Label labelTextLine;
        private System.Windows.Forms.TextBox textBoxParagraph;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TextBox textBoxFooter;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripParagraph;
        private System.Windows.Forms.ToolStripMenuItem insertHHMMSSMSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertendToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertnumberToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertdurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tabToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBoxNewLine;
        private System.Windows.Forms.ComboBox comboBoxTimeCode;
        private System.Windows.Forms.Label labelNewLine;
        private System.Windows.Forms.Label labelTimeCode;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.TextBox textBoxPreview;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.ToolStripMenuItem translationToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripHeader;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFooter;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemActor;
        private System.Windows.Forms.ToolStripMenuItem textToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem textline2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cpsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textlengthToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cpsperiodToolStripMenuItem;
    }
}