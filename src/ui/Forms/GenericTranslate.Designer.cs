using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Forms
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
            this.comboBoxFrom = new System.Windows.Forms.ComboBox();
            this.labelFrom = new System.Windows.Forms.Label();
            this.labelTo = new System.Windows.Forms.Label();
            this.comboBoxTo = new System.Windows.Forms.ComboBox();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.subtitleListViewTo = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.subtitleListViewFrom = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.labelApiKeyNotFound = new System.Windows.Forms.Label();
            this.comboBoxTranslationServices = new System.Windows.Forms.ComboBox();
            this.labelTranslationService = new System.Windows.Forms.Label();
            this.labelParagraphHandling = new System.Windows.Forms.Label();
            this.comboBoxParagraphHandling = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // comboBoxFrom
            // 
            this.comboBoxFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFrom.FormattingEnabled = true;
            this.comboBoxFrom.Location = new System.Drawing.Point(327, 34);
            this.comboBoxFrom.Name = "comboBoxFrom";
            this.comboBoxFrom.Size = new System.Drawing.Size(121, 25);
            this.comboBoxFrom.TabIndex = 0;
            // 
            // labelFrom
            // 
            this.labelFrom.AutoSize = true;
            this.labelFrom.Location = new System.Drawing.Point(285, 41);
            this.labelFrom.Name = "labelFrom";
            this.labelFrom.Size = new System.Drawing.Size(45, 17);
            this.labelFrom.TabIndex = 1;
            this.labelFrom.Text = "From:";
            // 
            // labelTo
            // 
            this.labelTo.AutoSize = true;
            this.labelTo.Location = new System.Drawing.Point(468, 37);
            this.labelTo.Name = "labelTo";
            this.labelTo.Size = new System.Drawing.Size(29, 17);
            this.labelTo.TabIndex = 4;
            this.labelTo.Text = "To:";
            // 
            // comboBoxTo
            // 
            this.comboBoxTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTo.FormattingEnabled = true;
            this.comboBoxTo.Location = new System.Drawing.Point(497, 34);
            this.comboBoxTo.Name = "comboBoxTo";
            this.comboBoxTo.Size = new System.Drawing.Size(121, 25);
            this.comboBoxTo.TabIndex = 3;
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
            this.buttonOK.Location = new System.Drawing.Point(754, 529);
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
            this.buttonCancel.Location = new System.Drawing.Point(835, 529);
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
            this.labelPleaseWait.Size = new System.Drawing.Size(215, 17);
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
            // subtitleListViewTo
            // 
            this.subtitleListViewTo.AllowColumnReorder = true;
            this.subtitleListViewTo.FirstVisibleIndex = -1;
            this.subtitleListViewTo.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListViewTo.FullRowSelect = true;
            this.subtitleListViewTo.GridLines = true;
            this.subtitleListViewTo.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListViewTo.HideSelection = false;
            this.subtitleListViewTo.Location = new System.Drawing.Point(471, 64);
            this.subtitleListViewTo.Name = "subtitleListViewTo";
            this.subtitleListViewTo.OwnerDraw = true;
            this.subtitleListViewTo.Size = new System.Drawing.Size(428, 459);
            this.subtitleListViewTo.SubtitleFontBold = false;
            this.subtitleListViewTo.SubtitleFontName = "Tahoma";
            this.subtitleListViewTo.SubtitleFontSize = 8;
            this.subtitleListViewTo.TabIndex = 2;
            this.subtitleListViewTo.UseCompatibleStateImageBehavior = false;
            this.subtitleListViewTo.UseSyntaxColoring = true;
            this.subtitleListViewTo.View = System.Windows.Forms.View.Details;
            this.subtitleListViewTo.Click += new System.EventHandler(this.subtitleListViewTo_DoubleClick);
            this.subtitleListViewTo.DoubleClick += new System.EventHandler(this.subtitleListViewTo_DoubleClick);
            // 
            // subtitleListViewFrom
            // 
            this.subtitleListViewFrom.AllowColumnReorder = true;
            this.subtitleListViewFrom.FirstVisibleIndex = -1;
            this.subtitleListViewFrom.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtitleListViewFrom.FullRowSelect = true;
            this.subtitleListViewFrom.GridLines = true;
            this.subtitleListViewFrom.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.subtitleListViewFrom.HideSelection = false;
            this.subtitleListViewFrom.Location = new System.Drawing.Point(18, 64);
            this.subtitleListViewFrom.Name = "subtitleListViewFrom";
            this.subtitleListViewFrom.OwnerDraw = true;
            this.subtitleListViewFrom.Size = new System.Drawing.Size(430, 459);
            this.subtitleListViewFrom.SubtitleFontBold = false;
            this.subtitleListViewFrom.SubtitleFontName = "Tahoma";
            this.subtitleListViewFrom.SubtitleFontSize = 8;
            this.subtitleListViewFrom.TabIndex = 0;
            this.subtitleListViewFrom.UseCompatibleStateImageBehavior = false;
            this.subtitleListViewFrom.UseSyntaxColoring = true;
            this.subtitleListViewFrom.View = System.Windows.Forms.View.Details;
            this.subtitleListViewFrom.Click += new System.EventHandler(this.subtitleListViewFrom_DoubleClick);
            this.subtitleListViewFrom.DoubleClick += new System.EventHandler(this.subtitleListViewFrom_DoubleClick);
            // 
            // labelApiKeyNotFound
            // 
            this.labelApiKeyNotFound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelApiKeyNotFound.AutoSize = true;
            this.labelApiKeyNotFound.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.labelApiKeyNotFound.Location = new System.Drawing.Point(291, 529);
            this.labelApiKeyNotFound.Name = "labelApiKeyNotFound";
            this.labelApiKeyNotFound.Size = new System.Drawing.Size(184, 17);
            this.labelApiKeyNotFound.TabIndex = 12;
            this.labelApiKeyNotFound.Text = "Warning: API key not found!";
            // 
            // comboBoxTranslatoEngines
            // 
            this.comboBoxTranslationServices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTranslationServices.FormattingEnabled = true;
            this.comboBoxTranslationServices.Location = new System.Drawing.Point(84, 36);
            this.comboBoxTranslationServices.Name = "comboBoxTranslationServices";
            this.comboBoxTranslationServices.Size = new System.Drawing.Size(121, 25);
            this.comboBoxTranslationServices.TabIndex = 13;
            this.comboBoxTranslationServices.TextChanged += new System.EventHandler(this.ComboBoxTranslatorEngineChanged);
            // 
            // labelTranslationService
            // 
            this.labelTranslationService.AutoSize = true;
            this.labelTranslationService.Location = new System.Drawing.Point(15, 41);
            this.labelTranslationService.Name = "labelTranslationService";
            this.labelTranslationService.Size = new System.Drawing.Size(57, 17);
            this.labelTranslationService.TabIndex = 14;
            this.labelTranslationService.Text = "Service:";
            // 
            // labelParagraphHandling
            // 
            this.labelParagraphHandling.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelParagraphHandling.AutoSize = true;
            this.labelParagraphHandling.Location = new System.Drawing.Point(18, 530);
            this.labelParagraphHandling.Name = "labelParagraphHandling";
            this.labelParagraphHandling.Size = new System.Drawing.Size(132, 17);
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
            this.comboBoxParagraphHandling.Size = new System.Drawing.Size(121, 25);
            this.comboBoxParagraphHandling.TabIndex = 16;
            // 
            // GenericTranslate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(925, 558);
            this.Controls.Add(this.comboBoxParagraphHandling);
            this.Controls.Add(this.labelParagraphHandling);
            this.Controls.Add(this.labelTranslationService);
            this.Controls.Add(this.comboBoxTranslationServices);
            this.Controls.Add(this.labelApiKeyNotFound);
            this.Controls.Add(this.comboBoxFrom);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonTranslate);
            this.Controls.Add(this.labelTo);
            this.Controls.Add(this.comboBoxTo);
            this.Controls.Add(this.subtitleListViewTo);
            this.Controls.Add(this.subtitleListViewFrom);
            this.Controls.Add(this.labelFrom);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "GenericTranslate";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Translate";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormGoogleTranslate_KeyDown);
            this.Resize += new System.EventHandler(this.GoogleTranslate_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxFrom;
        private System.Windows.Forms.Label labelFrom;
        private SubtitleListView subtitleListViewFrom;
        private SubtitleListView subtitleListViewTo;
        private System.Windows.Forms.Label labelTo;
        private System.Windows.Forms.ComboBox comboBoxTo;
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