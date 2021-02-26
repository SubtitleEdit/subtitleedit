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
            this.comboBoxSource = new System.Windows.Forms.ComboBox();
            this.labelSource = new System.Windows.Forms.Label();
            this.labelTarget = new System.Windows.Forms.Label();
            this.comboBoxTarget = new System.Windows.Forms.ComboBox();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.subtitleListViewTarget = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.subtitleListViewSource = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.labelApiKeyNotFound = new System.Windows.Forms.Label();
            this.comboBoxTranslationServices = new System.Windows.Forms.ComboBox();
            this.labelTranslationService = new System.Windows.Forms.Label();
            this.labelParagraphHandling = new System.Windows.Forms.Label();
            this.comboBoxParagraphHandling = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // comboBoxSource
            // 
            this.comboBoxSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSource.FormattingEnabled = true;
            this.comboBoxSource.Location = new System.Drawing.Point(327, 34);
            this.comboBoxSource.Name = "comboBoxSource";
            this.comboBoxSource.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSource.TabIndex = 0;
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
            this.comboBoxTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTarget.FormattingEnabled = true;
            this.comboBoxTarget.Location = new System.Drawing.Point(497, 34);
            this.comboBoxTarget.Name = "comboBoxTarget";
            this.comboBoxTarget.Size = new System.Drawing.Size(121, 21);
            this.comboBoxTarget.TabIndex = 3;
            this.comboBoxTarget.TextChanged += new System.EventHandler(this.ComboBoxLanguageChanged);
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.Location = new System.Drawing.Point(624, 34);
            this.buttonTranslate.Name = "buttonTranslate";
            this.buttonTranslate.Size = new System.Drawing.Size(75, 23);
            this.buttonTranslate.TabIndex = 5;
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
            this.buttonOK.TabIndex = 6;
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
            this.buttonCancel.TabIndex = 7;
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
            this.subtitleListViewTarget.TabIndex = 2;
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
            this.subtitleListViewSource.TabIndex = 0;
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
            this.comboBoxTranslationServices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTranslationServices.FormattingEnabled = true;
            this.comboBoxTranslationServices.Location = new System.Drawing.Point(84, 34);
            this.comboBoxTranslationServices.Name = "comboBoxTranslationServices";
            this.comboBoxTranslationServices.Size = new System.Drawing.Size(152, 21);
            this.comboBoxTranslationServices.TabIndex = 13;
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
            this.comboBoxParagraphHandling.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxParagraphHandling.FormattingEnabled = true;
            this.comboBoxParagraphHandling.Location = new System.Drawing.Point(155, 527);
            this.comboBoxParagraphHandling.Name = "comboBoxParagraphHandling";
            this.comboBoxParagraphHandling.Size = new System.Drawing.Size(165, 21);
            this.comboBoxParagraphHandling.TabIndex = 16;
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
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormTranslate_KeyDown);
            this.Resize += new System.EventHandler(this.Translate_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxSource;
        private System.Windows.Forms.Label labelSource;
        private SubtitleListView subtitleListViewSource;
        private SubtitleListView subtitleListViewTarget;
        private System.Windows.Forms.Label labelTarget;
        private System.Windows.Forms.ComboBox comboBoxTarget;
        private System.Windows.Forms.Button buttonTranslate;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelApiKeyNotFound;
        private System.Windows.Forms.ComboBox comboBoxTranslationServices;
        private System.Windows.Forms.Label labelTranslationService;
        private System.Windows.Forms.Label labelParagraphHandling;
        private System.Windows.Forms.ComboBox comboBoxParagraphHandling;
    }
}