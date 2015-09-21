using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class GoogleTranslate
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
            comboBoxFrom = new System.Windows.Forms.ComboBox();
            labelFrom = new System.Windows.Forms.Label();
            labelTo = new System.Windows.Forms.Label();
            comboBoxTo = new System.Windows.Forms.ComboBox();
            buttonTranslate = new System.Windows.Forms.Button();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            linkLabelPoweredByGoogleTranslate = new System.Windows.Forms.LinkLabel();
            labelPleaseWait = new System.Windows.Forms.Label();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            subtitleListViewTo = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            subtitleListViewFrom = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            SuspendLayout();
            // 
            // comboBoxFrom
            // 
            comboBoxFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxFrom.FormattingEnabled = true;
            comboBoxFrom.Location = new System.Drawing.Point(327, 34);
            comboBoxFrom.Name = "comboBoxFrom";
            comboBoxFrom.Size = new System.Drawing.Size(121, 21);
            comboBoxFrom.TabIndex = 0;
            // 
            // labelFrom
            // 
            labelFrom.AutoSize = true;
            labelFrom.Location = new System.Drawing.Point(285, 41);
            labelFrom.Name = "labelFrom";
            labelFrom.Size = new System.Drawing.Size(35, 13);
            labelFrom.TabIndex = 1;
            labelFrom.Text = "From:";
            // 
            // labelTo
            // 
            labelTo.AutoSize = true;
            labelTo.Location = new System.Drawing.Point(468, 37);
            labelTo.Name = "labelTo";
            labelTo.Size = new System.Drawing.Size(23, 13);
            labelTo.TabIndex = 4;
            labelTo.Text = "To:";
            // 
            // comboBoxTo
            // 
            comboBoxTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxTo.FormattingEnabled = true;
            comboBoxTo.Location = new System.Drawing.Point(497, 34);
            comboBoxTo.Name = "comboBoxTo";
            comboBoxTo.Size = new System.Drawing.Size(121, 21);
            comboBoxTo.TabIndex = 3;
            // 
            // buttonTranslate
            // 
            buttonTranslate.Location = new System.Drawing.Point(624, 34);
            buttonTranslate.Name = "buttonTranslate";
            buttonTranslate.Size = new System.Drawing.Size(75, 21);
            buttonTranslate.TabIndex = 5;
            buttonTranslate.Text = "Translate";
            buttonTranslate.UseVisualStyleBackColor = true;
            buttonTranslate.Click += new System.EventHandler(buttonTranslate_Click);
            // 
            // buttonOK
            // 
            buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            buttonOK.Location = new System.Drawing.Point(754, 529);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(75, 21);
            buttonOK.TabIndex = 6;
            buttonOK.Text = "&OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += new System.EventHandler(ButtonOkClick);
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(835, 529);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(75, 21);
            buttonCancel.TabIndex = 7;
            buttonCancel.Text = "C&ancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // linkLabelPoweredByGoogleTranslate
            // 
            linkLabelPoweredByGoogleTranslate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            linkLabelPoweredByGoogleTranslate.AutoSize = true;
            linkLabelPoweredByGoogleTranslate.Location = new System.Drawing.Point(15, 529);
            linkLabelPoweredByGoogleTranslate.Name = "linkLabelPoweredByGoogleTranslate";
            linkLabelPoweredByGoogleTranslate.Size = new System.Drawing.Size(146, 13);
            linkLabelPoweredByGoogleTranslate.TabIndex = 8;
            linkLabelPoweredByGoogleTranslate.TabStop = true;
            linkLabelPoweredByGoogleTranslate.Text = "Powered by Google translate";
            linkLabelPoweredByGoogleTranslate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(LinkLabel1LinkClicked);
            // 
            // labelPleaseWait
            // 
            labelPleaseWait.AutoSize = true;
            labelPleaseWait.Location = new System.Drawing.Point(703, 23);
            labelPleaseWait.Name = "labelPleaseWait";
            labelPleaseWait.Size = new System.Drawing.Size(176, 13);
            labelPleaseWait.TabIndex = 10;
            labelPleaseWait.Text = "Please wait... this may take a while";
            // 
            // progressBar1
            // 
            progressBar1.Location = new System.Drawing.Point(705, 39);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(192, 16);
            progressBar1.TabIndex = 11;
            // 
            // subtitleListViewTo
            // 
            subtitleListViewTo.DisplayExtraFromExtra = false;
            subtitleListViewTo.FirstVisibleIndex = -1;
            subtitleListViewTo.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            subtitleListViewTo.FullRowSelect = true;
            subtitleListViewTo.GridLines = true;
            subtitleListViewTo.HideSelection = false;
            subtitleListViewTo.Location = new System.Drawing.Point(471, 64);
            subtitleListViewTo.Name = "subtitleListViewTo";
            subtitleListViewTo.OwnerDraw = true;
            subtitleListViewTo.Size = new System.Drawing.Size(428, 459);
            subtitleListViewTo.TabIndex = 2;
            subtitleListViewTo.UseCompatibleStateImageBehavior = false;
            subtitleListViewTo.View = System.Windows.Forms.View.Details;
            subtitleListViewTo.Click += new System.EventHandler(subtitleListViewTo_DoubleClick);
            subtitleListViewTo.DoubleClick += new System.EventHandler(subtitleListViewTo_DoubleClick);
            // 
            // subtitleListViewFrom
            // 
            subtitleListViewFrom.DisplayExtraFromExtra = false;
            subtitleListViewFrom.FirstVisibleIndex = -1;
            subtitleListViewFrom.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            subtitleListViewFrom.FullRowSelect = true;
            subtitleListViewFrom.GridLines = true;
            subtitleListViewFrom.HideSelection = false;
            subtitleListViewFrom.Location = new System.Drawing.Point(18, 64);
            subtitleListViewFrom.Name = "subtitleListViewFrom";
            subtitleListViewFrom.OwnerDraw = true;
            subtitleListViewFrom.Size = new System.Drawing.Size(430, 459);
            subtitleListViewFrom.TabIndex = 0;
            subtitleListViewFrom.UseCompatibleStateImageBehavior = false;
            subtitleListViewFrom.View = System.Windows.Forms.View.Details;
            subtitleListViewFrom.Click += new System.EventHandler(subtitleListViewFrom_DoubleClick);
            subtitleListViewFrom.DoubleClick += new System.EventHandler(subtitleListViewFrom_DoubleClick);
            // 
            // GoogleTranslate
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(925, 558);
            Controls.Add(comboBoxFrom);
            Controls.Add(progressBar1);
            Controls.Add(labelPleaseWait);
            Controls.Add(linkLabelPoweredByGoogleTranslate);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(buttonTranslate);
            Controls.Add(labelTo);
            Controls.Add(comboBoxTo);
            Controls.Add(subtitleListViewTo);
            Controls.Add(subtitleListViewFrom);
            Controls.Add(labelFrom);
            Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(900, 500);
            Name = "GoogleTranslate";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Google translate";
            KeyDown += new System.Windows.Forms.KeyEventHandler(FormGoogleTranslate_KeyDown);
            Resize += new System.EventHandler(GoogleTranslate_Resize);
            ResumeLayout(false);
            PerformLayout();

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
        private System.Windows.Forms.LinkLabel linkLabelPoweredByGoogleTranslate;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}