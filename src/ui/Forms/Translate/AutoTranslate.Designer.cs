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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.labelTarget = new System.Windows.Forms.Label();
            this.labelSource = new System.Windows.Forms.Label();
            this.labelUrl = new System.Windows.Forms.Label();
            this.nikseComboBoxUrl = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxSource = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxTarget = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.subtitleListViewTarget = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.subtitleListViewSource = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(699, 27);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(192, 16);
            this.progressBar1.TabIndex = 100;
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(697, 11);
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
            this.buttonCancel.TabIndex = 102;
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
            this.buttonOK.TabIndex = 101;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.Location = new System.Drawing.Point(618, 22);
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
            this.labelTarget.Location = new System.Drawing.Point(462, 27);
            this.labelTarget.Name = "labelTarget";
            this.labelTarget.Size = new System.Drawing.Size(23, 13);
            this.labelTarget.TabIndex = 104;
            this.labelTarget.Text = "To:";
            // 
            // labelSource
            // 
            this.labelSource.AutoSize = true;
            this.labelSource.Location = new System.Drawing.Point(279, 27);
            this.labelSource.Name = "labelSource";
            this.labelSource.Size = new System.Drawing.Size(33, 13);
            this.labelSource.TabIndex = 103;
            this.labelSource.Text = "From:";
            // 
            // labelUrl
            // 
            this.labelUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelUrl.AutoSize = true;
            this.labelUrl.Location = new System.Drawing.Point(14, 532);
            this.labelUrl.Name = "labelUrl";
            this.labelUrl.Size = new System.Drawing.Size(23, 13);
            this.labelUrl.TabIndex = 106;
            this.labelUrl.Text = "Url:";
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
            this.nikseComboBoxUrl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nikseComboBoxUrl.DropDownWidth = 280;
            this.nikseComboBoxUrl.FormattingEnabled = true;
            this.nikseComboBoxUrl.Location = new System.Drawing.Point(56, 528);
            this.nikseComboBoxUrl.MaxLength = 32767;
            this.nikseComboBoxUrl.Name = "nikseComboBoxUrl";
            this.nikseComboBoxUrl.SelectedIndex = -1;
            this.nikseComboBoxUrl.SelectedItem = null;
            this.nikseComboBoxUrl.SelectedText = "";
            this.nikseComboBoxUrl.Size = new System.Drawing.Size(280, 21);
            this.nikseComboBoxUrl.TabIndex = 105;
            this.nikseComboBoxUrl.UsePopupWindow = false;
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
            this.comboBoxSource.DropDownWidth = 200;
            this.comboBoxSource.FormattingEnabled = true;
            this.comboBoxSource.Location = new System.Drawing.Point(321, 23);
            this.comboBoxSource.MaxLength = 32767;
            this.comboBoxSource.Name = "comboBoxSource";
            this.comboBoxSource.SelectedIndex = -1;
            this.comboBoxSource.SelectedItem = null;
            this.comboBoxSource.SelectedText = "";
            this.comboBoxSource.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSource.TabIndex = 94;
            this.comboBoxSource.UsePopupWindow = false;
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
            this.comboBoxTarget.DropDownWidth = 200;
            this.comboBoxTarget.FormattingEnabled = true;
            this.comboBoxTarget.Location = new System.Drawing.Point(491, 23);
            this.comboBoxTarget.MaxLength = 32767;
            this.comboBoxTarget.Name = "comboBoxTarget";
            this.comboBoxTarget.SelectedIndex = -1;
            this.comboBoxTarget.SelectedItem = null;
            this.comboBoxTarget.SelectedText = "";
            this.comboBoxTarget.Size = new System.Drawing.Size(121, 21);
            this.comboBoxTarget.TabIndex = 95;
            this.comboBoxTarget.UsePopupWindow = false;
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
            this.subtitleListViewTarget.Location = new System.Drawing.Point(465, 52);
            this.subtitleListViewTarget.Name = "subtitleListViewTarget";
            this.subtitleListViewTarget.OwnerDraw = true;
            this.subtitleListViewTarget.Size = new System.Drawing.Size(428, 459);
            this.subtitleListViewTarget.SubtitleFontBold = false;
            this.subtitleListViewTarget.SubtitleFontName = "Tahoma";
            this.subtitleListViewTarget.SubtitleFontSize = 8;
            this.subtitleListViewTarget.TabIndex = 98;
            this.subtitleListViewTarget.UseCompatibleStateImageBehavior = false;
            this.subtitleListViewTarget.UseSyntaxColoring = true;
            this.subtitleListViewTarget.View = System.Windows.Forms.View.Details;
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
            this.subtitleListViewSource.Location = new System.Drawing.Point(12, 52);
            this.subtitleListViewSource.Name = "subtitleListViewSource";
            this.subtitleListViewSource.OwnerDraw = true;
            this.subtitleListViewSource.Size = new System.Drawing.Size(430, 459);
            this.subtitleListViewSource.SubtitleFontBold = false;
            this.subtitleListViewSource.SubtitleFontName = "Tahoma";
            this.subtitleListViewSource.SubtitleFontSize = 8;
            this.subtitleListViewSource.TabIndex = 97;
            this.subtitleListViewSource.UseCompatibleStateImageBehavior = false;
            this.subtitleListViewSource.UseSyntaxColoring = true;
            this.subtitleListViewSource.View = System.Windows.Forms.View.Details;
            // 
            // AutoTranslate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1058, 563);
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
            this.ResizeEnd += new System.EventHandler(this.AutoTranslate_ResizeEnd);
            this.Resize += new System.EventHandler(this.AutoTranslate_Resize);
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
    }
}