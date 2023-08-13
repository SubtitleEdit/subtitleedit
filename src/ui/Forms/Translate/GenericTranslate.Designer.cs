using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    sealed partial class GenericTranslate
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
            this.comboBoxSource = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelSource = new System.Windows.Forms.Label();
            this.labelTarget = new System.Windows.Forms.Label();
            this.comboBoxTarget = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.subtitleListViewTarget = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.subtitleListViewSource = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.labelApiKeyNotFound = new System.Windows.Forms.Label();
            this.comboBoxTranslationServices = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelTranslationService = new System.Windows.Forms.Label();
            this.labelParagraphHandling = new System.Windows.Forms.Label();
            this.comboBoxParagraphHandling = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.SuspendLayout();
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
            this.comboBoxSource.DropDownWidth = 121;
            this.comboBoxSource.FormattingEnabled = true;
            this.comboBoxSource.Location = new System.Drawing.Point(327, 34);
            this.comboBoxSource.MaxLength = 32767;
            this.comboBoxSource.Name = "comboBoxSource";
            this.comboBoxSource.SelectedIndex = -1;
            this.comboBoxSource.SelectedItem = null;
            this.comboBoxSource.SelectedText = "";
            this.comboBoxSource.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSource.TabIndex = 1;
            this.comboBoxSource.TextChanged += new System.EventHandler(this.ComboBoxLanguageChanged);
            // 
            // labelSource
            // 
            this.labelSource.AutoSize = true;
            this.labelSource.Location = new System.Drawing.Point(285, 41);
            this.labelSource.Name = "labelSource";
            this.labelSource.Size = new System.Drawing.Size(35, 13);
            this.labelSource.TabIndex = 1;
            this.labelSource.Text = "From:";
            // 
            // labelTarget
            // 
            this.labelTarget.AutoSize = true;
            this.labelTarget.Location = new System.Drawing.Point(468, 37);
            this.labelTarget.Name = "labelTarget";
            this.labelTarget.Size = new System.Drawing.Size(23, 13);
            this.labelTarget.TabIndex = 4;
            this.labelTarget.Text = "To:";
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
            this.comboBoxTarget.DropDownWidth = 121;
            this.comboBoxTarget.FormattingEnabled = true;
            this.comboBoxTarget.Location = new System.Drawing.Point(497, 34);
            this.comboBoxTarget.MaxLength = 32767;
            this.comboBoxTarget.Name = "comboBoxTarget";
            this.comboBoxTarget.SelectedIndex = -1;
            this.comboBoxTarget.SelectedItem = null;
            this.comboBoxTarget.SelectedText = "";
            this.comboBoxTarget.Size = new System.Drawing.Size(121, 21);
            this.comboBoxTarget.TabIndex = 2;
            this.comboBoxTarget.TextChanged += new System.EventHandler(this.ComboBoxLanguageChanged);
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.Location = new System.Drawing.Point(624, 34);
            this.buttonTranslate.Name = "buttonTranslate";
            this.buttonTranslate.Size = new System.Drawing.Size(75, 23);
            this.buttonTranslate.TabIndex = 3;
            this.buttonTranslate.Text = "Translate";
            this.buttonTranslate.UseVisualStyleBackColor = true;
            this.buttonTranslate.Click += new System.EventHandler(this.buttonTranslate_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(882, 529);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 92;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(963, 529);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 93;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(703, 23);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(176, 13);
            this.labelPleaseWait.TabIndex = 10;
            this.labelPleaseWait.Text = "Please wait... this may take a while";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(705, 39);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(192, 16);
            this.progressBar1.TabIndex = 11;
            // 
            // subtitleListViewTarget
            // 
            this.subtitleListViewTarget.AllowColumnReorder = true;
            this.subtitleListViewTarget.FirstVisibleIndex = -1;
            this.subtitleListViewTarget.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListViewTarget.FullRowSelect = true;
            this.subtitleListViewTarget.GridLines = true;
            this.subtitleListViewTarget.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListViewTarget.HideSelection = false;
            this.subtitleListViewTarget.Location = new System.Drawing.Point(471, 64);
            this.subtitleListViewTarget.Name = "subtitleListViewTarget";
            this.subtitleListViewTarget.OwnerDraw = true;
            this.subtitleListViewTarget.Size = new System.Drawing.Size(428, 459);
            this.subtitleListViewTarget.SubtitleFontBold = false;
            this.subtitleListViewTarget.SubtitleFontName = "Tahoma";
            this.subtitleListViewTarget.SubtitleFontSize = 8;
            this.subtitleListViewTarget.TabIndex = 5;
            this.subtitleListViewTarget.UseCompatibleStateImageBehavior = false;
            this.subtitleListViewTarget.UseSyntaxColoring = true;
            this.subtitleListViewTarget.View = System.Windows.Forms.View.Details;
            this.subtitleListViewTarget.Click += new System.EventHandler(this.subtitleListViewTarget_DoubleClick);
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
            this.subtitleListViewSource.Location = new System.Drawing.Point(18, 64);
            this.subtitleListViewSource.Name = "subtitleListViewSource";
            this.subtitleListViewSource.OwnerDraw = true;
            this.subtitleListViewSource.Size = new System.Drawing.Size(430, 459);
            this.subtitleListViewSource.SubtitleFontBold = false;
            this.subtitleListViewSource.SubtitleFontName = "Tahoma";
            this.subtitleListViewSource.SubtitleFontSize = 8;
            this.subtitleListViewSource.TabIndex = 4;
            this.subtitleListViewSource.UseCompatibleStateImageBehavior = false;
            this.subtitleListViewSource.UseSyntaxColoring = true;
            this.subtitleListViewSource.View = System.Windows.Forms.View.Details;
            this.subtitleListViewSource.Click += new System.EventHandler(this.subtitleListViewSource_DoubleClick);
            this.subtitleListViewSource.DoubleClick += new System.EventHandler(this.subtitleListViewSource_DoubleClick);
            // 
            // labelApiKeyNotFound
            // 
            this.labelApiKeyNotFound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelApiKeyNotFound.AutoSize = true;
            this.labelApiKeyNotFound.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.labelApiKeyNotFound.Location = new System.Drawing.Point(388, 530);
            this.labelApiKeyNotFound.Name = "labelApiKeyNotFound";
            this.labelApiKeyNotFound.Size = new System.Drawing.Size(145, 13);
            this.labelApiKeyNotFound.TabIndex = 12;
            this.labelApiKeyNotFound.Text = "Warning: API key not found!";
            // 
            // comboBoxTranslationServices
            // 
            this.comboBoxTranslationServices.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxTranslationServices.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxTranslationServices.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxTranslationServices.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxTranslationServices.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxTranslationServices.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxTranslationServices.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxTranslationServices.DropDownHeight = 400;
            this.comboBoxTranslationServices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTranslationServices.DropDownWidth = 152;
            this.comboBoxTranslationServices.FormattingEnabled = true;
            this.comboBoxTranslationServices.Location = new System.Drawing.Point(84, 34);
            this.comboBoxTranslationServices.MaxLength = 32767;
            this.comboBoxTranslationServices.Name = "comboBoxTranslationServices";
            this.comboBoxTranslationServices.SelectedIndex = -1;
            this.comboBoxTranslationServices.SelectedItem = null;
            this.comboBoxTranslationServices.SelectedText = "";
            this.comboBoxTranslationServices.Size = new System.Drawing.Size(152, 21);
            this.comboBoxTranslationServices.TabIndex = 0;
            this.comboBoxTranslationServices.SelectedIndexChanged += new System.EventHandler(this.comboBoxTranslationServices_SelectedIndexChanged);
            // 
            // labelTranslationService
            // 
            this.labelTranslationService.AutoSize = true;
            this.labelTranslationService.Location = new System.Drawing.Point(15, 41);
            this.labelTranslationService.Name = "labelTranslationService";
            this.labelTranslationService.Size = new System.Drawing.Size(46, 13);
            this.labelTranslationService.TabIndex = 14;
            this.labelTranslationService.Text = "Service:";
            // 
            // labelParagraphHandling
            // 
            this.labelParagraphHandling.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelParagraphHandling.AutoSize = true;
            this.labelParagraphHandling.Location = new System.Drawing.Point(18, 530);
            this.labelParagraphHandling.Name = "labelParagraphHandling";
            this.labelParagraphHandling.Size = new System.Drawing.Size(105, 13);
            this.labelParagraphHandling.TabIndex = 15;
            this.labelParagraphHandling.Text = "Paragraph Handling:";
            // 
            // comboBoxParagraphHandling
            // 
            this.comboBoxParagraphHandling.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxParagraphHandling.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxParagraphHandling.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxParagraphHandling.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxParagraphHandling.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxParagraphHandling.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxParagraphHandling.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxParagraphHandling.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxParagraphHandling.DropDownHeight = 400;
            this.comboBoxParagraphHandling.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxParagraphHandling.DropDownWidth = 165;
            this.comboBoxParagraphHandling.FormattingEnabled = true;
            this.comboBoxParagraphHandling.Location = new System.Drawing.Point(155, 527);
            this.comboBoxParagraphHandling.MaxLength = 32767;
            this.comboBoxParagraphHandling.Name = "comboBoxParagraphHandling";
            this.comboBoxParagraphHandling.SelectedIndex = -1;
            this.comboBoxParagraphHandling.SelectedItem = null;
            this.comboBoxParagraphHandling.SelectedText = "";
            this.comboBoxParagraphHandling.Size = new System.Drawing.Size(165, 21);
            this.comboBoxParagraphHandling.TabIndex = 90;
            // 
            // GenericTranslate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1053, 558);
            this.Controls.Add(this.comboBoxParagraphHandling);
            this.Controls.Add(this.labelParagraphHandling);
            this.Controls.Add(this.labelTranslationService);
            this.Controls.Add(this.comboBoxTranslationServices);
            this.Controls.Add(this.labelApiKeyNotFound);
            this.Controls.Add(this.comboBoxSource);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonTranslate);
            this.Controls.Add(this.labelTarget);
            this.Controls.Add(this.comboBoxTarget);
            this.Controls.Add(this.subtitleListViewTarget);
            this.Controls.Add(this.subtitleListViewSource);
            this.Controls.Add(this.labelSource);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "GenericTranslate";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Translate";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GenericTranslate_FormClosing);
            this.Shown += new System.EventHandler(this.GenericTranslate_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormTranslate_KeyDown);
            this.Resize += new System.EventHandler(this.Translate_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSource;
        private System.Windows.Forms.Label labelSource;
        private SubtitleListView subtitleListViewSource;
        private SubtitleListView subtitleListViewTarget;
        private System.Windows.Forms.Label labelTarget;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxTarget;
        private System.Windows.Forms.Button buttonTranslate;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelApiKeyNotFound;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxTranslationServices;
        private System.Windows.Forms.Label labelTranslationService;
        private System.Windows.Forms.Label labelParagraphHandling;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxParagraphHandling;
    }
}