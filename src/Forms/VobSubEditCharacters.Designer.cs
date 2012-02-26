namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class VobSubEditCharacters
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
            this.comboBoxTexts = new System.Windows.Forms.ComboBox();
            this.listBoxFileNames = new System.Windows.Forms.ListBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.labelChooseCharacters = new System.Windows.Forms.Label();
            this.labelImageCompareFiles = new System.Windows.Forms.Label();
            this.groupBoxCurrentCompareImage = new System.Windows.Forms.GroupBox();
            this.checkBoxItalic = new System.Windows.Forms.CheckBox();
            this.labelDoubleSize = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.labelTextAssociatedWithImage = new System.Windows.Forms.Label();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.labelImageInfo = new System.Windows.Forms.Label();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxCurrentCompareImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxTexts
            // 
            this.comboBoxTexts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTexts.FormattingEnabled = true;
            this.comboBoxTexts.Location = new System.Drawing.Point(12, 27);
            this.comboBoxTexts.Name = "comboBoxTexts";
            this.comboBoxTexts.Size = new System.Drawing.Size(240, 21);
            this.comboBoxTexts.TabIndex = 1;
            this.comboBoxTexts.SelectedIndexChanged += new System.EventHandler(this.ComboBoxTextsSelectedIndexChanged);
            // 
            // listBoxFileNames
            // 
            this.listBoxFileNames.FormattingEnabled = true;
            this.listBoxFileNames.Location = new System.Drawing.Point(12, 73);
            this.listBoxFileNames.Name = "listBoxFileNames";
            this.listBoxFileNames.Size = new System.Drawing.Size(240, 264);
            this.listBoxFileNames.TabIndex = 3;
            this.listBoxFileNames.SelectedIndexChanged += new System.EventHandler(this.ListBoxFileNamesSelectedIndexChanged);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(368, 341);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonDelete
            // 
            this.buttonDelete.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDelete.Location = new System.Drawing.Point(120, 62);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(88, 21);
            this.buttonDelete.TabIndex = 4;
            this.buttonDelete.Text = "Delete ";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.ButtonDeleteClick);
            // 
            // labelChooseCharacters
            // 
            this.labelChooseCharacters.AutoSize = true;
            this.labelChooseCharacters.Location = new System.Drawing.Point(12, 10);
            this.labelChooseCharacters.Name = "labelChooseCharacters";
            this.labelChooseCharacters.Size = new System.Drawing.Size(105, 13);
            this.labelChooseCharacters.TabIndex = 0;
            this.labelChooseCharacters.Text = "Choose character(s)";
            // 
            // labelImageCompareFiles
            // 
            this.labelImageCompareFiles.AutoSize = true;
            this.labelImageCompareFiles.Location = new System.Drawing.Point(12, 56);
            this.labelImageCompareFiles.Name = "labelImageCompareFiles";
            this.labelImageCompareFiles.Size = new System.Drawing.Size(103, 13);
            this.labelImageCompareFiles.TabIndex = 2;
            this.labelImageCompareFiles.Text = "Image compare files";
            // 
            // groupBoxCurrentCompareImage
            // 
            this.groupBoxCurrentCompareImage.Controls.Add(this.checkBoxItalic);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelDoubleSize);
            this.groupBoxCurrentCompareImage.Controls.Add(this.pictureBox2);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelTextAssociatedWithImage);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonDelete);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonUpdate);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelImageInfo);
            this.groupBoxCurrentCompareImage.Controls.Add(this.textBoxText);
            this.groupBoxCurrentCompareImage.Controls.Add(this.pictureBox1);
            this.groupBoxCurrentCompareImage.Location = new System.Drawing.Point(258, 67);
            this.groupBoxCurrentCompareImage.Name = "groupBoxCurrentCompareImage";
            this.groupBoxCurrentCompareImage.Size = new System.Drawing.Size(266, 268);
            this.groupBoxCurrentCompareImage.TabIndex = 6;
            this.groupBoxCurrentCompareImage.TabStop = false;
            this.groupBoxCurrentCompareImage.Text = "Current compare image";
            // 
            // checkBoxItalic
            // 
            this.checkBoxItalic.AutoSize = true;
            this.checkBoxItalic.Location = new System.Drawing.Point(15, 61);
            this.checkBoxItalic.Name = "checkBoxItalic";
            this.checkBoxItalic.Size = new System.Drawing.Size(58, 17);
            this.checkBoxItalic.TabIndex = 2;
            this.checkBoxItalic.Text = "Is &italic";
            this.checkBoxItalic.UseVisualStyleBackColor = true;
            // 
            // labelDoubleSize
            // 
            this.labelDoubleSize.AutoSize = true;
            this.labelDoubleSize.Location = new System.Drawing.Point(14, 172);
            this.labelDoubleSize.Name = "labelDoubleSize";
            this.labelDoubleSize.Size = new System.Drawing.Size(93, 13);
            this.labelDoubleSize.TabIndex = 6;
            this.labelDoubleSize.Text = "Image double size";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.Red;
            this.pictureBox2.Location = new System.Drawing.Point(17, 188);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(66, 73);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 27;
            this.pictureBox2.TabStop = false;
            // 
            // labelTextAssociatedWithImage
            // 
            this.labelTextAssociatedWithImage.AutoSize = true;
            this.labelTextAssociatedWithImage.Location = new System.Drawing.Point(14, 19);
            this.labelTextAssociatedWithImage.Name = "labelTextAssociatedWithImage";
            this.labelTextAssociatedWithImage.Size = new System.Drawing.Size(137, 13);
            this.labelTextAssociatedWithImage.TabIndex = 0;
            this.labelTextAssociatedWithImage.Text = "Text associated with image";
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonUpdate.Location = new System.Drawing.Point(120, 35);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(88, 21);
            this.buttonUpdate.TabIndex = 3;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.ButtonUpdateClick);
            // 
            // labelImageInfo
            // 
            this.labelImageInfo.AutoSize = true;
            this.labelImageInfo.Location = new System.Drawing.Point(12, 87);
            this.labelImageInfo.Name = "labelImageInfo";
            this.labelImageInfo.Size = new System.Drawing.Size(79, 13);
            this.labelImageInfo.TabIndex = 5;
            this.labelImageInfo.Text = "labelImageInfo";
            // 
            // textBoxText
            // 
            this.textBoxText.Location = new System.Drawing.Point(14, 35);
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.Size = new System.Drawing.Size(100, 21);
            this.textBoxText.TabIndex = 1;
            this.textBoxText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxText_KeyDown);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Red;
            this.pictureBox1.Location = new System.Drawing.Point(17, 104);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(52, 52);
            this.pictureBox1.TabIndex = 22;
            this.pictureBox1.TabStop = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(449, 341);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // VobSubEditCharacters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 370);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxCurrentCompareImage);
            this.Controls.Add(this.labelImageCompareFiles);
            this.Controls.Add(this.labelChooseCharacters);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listBoxFileNames);
            this.Controls.Add(this.comboBoxTexts);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VobSubEditCharacters";
            this.ShowInTaskbar = false;
            this.Text = "Edit image compare database";
            this.Shown += new System.EventHandler(this.VobSubEditCharacters_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VobSubEditCharacters_KeyDown);
            this.groupBoxCurrentCompareImage.ResumeLayout(false);
            this.groupBoxCurrentCompareImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxTexts;
        private System.Windows.Forms.ListBox listBoxFileNames;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Label labelChooseCharacters;
        private System.Windows.Forms.Label labelImageCompareFiles;
        private System.Windows.Forms.GroupBox groupBoxCurrentCompareImage;
        private System.Windows.Forms.Label labelDoubleSize;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label labelTextAssociatedWithImage;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Label labelImageInfo;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox checkBoxItalic;
        private System.Windows.Forms.Button buttonCancel;
    }
}