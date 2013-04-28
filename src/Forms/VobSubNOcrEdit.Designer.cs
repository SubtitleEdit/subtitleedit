namespace Nikse.SubtitleEdit.Forms
{
    partial class VobSubNOcrEdit
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
            this.labelImageCompareFiles = new System.Windows.Forms.Label();
            this.labelChooseCharacters = new System.Windows.Forms.Label();
            this.listBoxFileNames = new System.Windows.Forms.ListBox();
            this.comboBoxTexts = new System.Windows.Forms.ComboBox();
            this.groupBoxCurrentCompareImage = new System.Windows.Forms.GroupBox();
            this.buttonZoomOut = new System.Windows.Forms.Button();
            this.buttonZoomIn = new System.Windows.Forms.Button();
            this.labelCharacters = new System.Windows.Forms.Label();
            this.pictureBoxCharacter = new System.Windows.Forms.PictureBox();
            this.buttonAddBetterMatch = new System.Windows.Forms.Button();
            this.checkBoxItalic = new System.Windows.Forms.CheckBox();
            this.labelTextAssociatedWithImage = new System.Windows.Forms.Label();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.buttonMakeItalic = new System.Windows.Forms.Button();
            this.groupBoxCurrentCompareImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCharacter)).BeginInit();
            this.SuspendLayout();
            // 
            // labelImageCompareFiles
            // 
            this.labelImageCompareFiles.AutoSize = true;
            this.labelImageCompareFiles.Location = new System.Drawing.Point(12, 55);
            this.labelImageCompareFiles.Name = "labelImageCompareFiles";
            this.labelImageCompareFiles.Size = new System.Drawing.Size(76, 13);
            this.labelImageCompareFiles.TabIndex = 6;
            this.labelImageCompareFiles.Text = "Compare items";
            // 
            // labelChooseCharacters
            // 
            this.labelChooseCharacters.AutoSize = true;
            this.labelChooseCharacters.Location = new System.Drawing.Point(12, 9);
            this.labelChooseCharacters.Name = "labelChooseCharacters";
            this.labelChooseCharacters.Size = new System.Drawing.Size(102, 13);
            this.labelChooseCharacters.TabIndex = 4;
            this.labelChooseCharacters.Text = "Choose character(s)";
            // 
            // listBoxFileNames
            // 
            this.listBoxFileNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxFileNames.FormattingEnabled = true;
            this.listBoxFileNames.Location = new System.Drawing.Point(12, 72);
            this.listBoxFileNames.Name = "listBoxFileNames";
            this.listBoxFileNames.Size = new System.Drawing.Size(240, 316);
            this.listBoxFileNames.TabIndex = 7;
            this.listBoxFileNames.SelectedIndexChanged += new System.EventHandler(this.listBoxFileNames_SelectedIndexChanged);
            // 
            // comboBoxTexts
            // 
            this.comboBoxTexts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTexts.FormattingEnabled = true;
            this.comboBoxTexts.Location = new System.Drawing.Point(12, 26);
            this.comboBoxTexts.Name = "comboBoxTexts";
            this.comboBoxTexts.Size = new System.Drawing.Size(240, 21);
            this.comboBoxTexts.TabIndex = 5;
            this.comboBoxTexts.SelectedIndexChanged += new System.EventHandler(this.comboBoxTexts_SelectedIndexChanged);
            // 
            // groupBoxCurrentCompareImage
            // 
            this.groupBoxCurrentCompareImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonMakeItalic);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonZoomOut);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonZoomIn);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelCharacters);
            this.groupBoxCurrentCompareImage.Controls.Add(this.pictureBoxCharacter);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonAddBetterMatch);
            this.groupBoxCurrentCompareImage.Controls.Add(this.checkBoxItalic);
            this.groupBoxCurrentCompareImage.Controls.Add(this.labelTextAssociatedWithImage);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonDelete);
            this.groupBoxCurrentCompareImage.Controls.Add(this.buttonUpdate);
            this.groupBoxCurrentCompareImage.Controls.Add(this.textBoxText);
            this.groupBoxCurrentCompareImage.Location = new System.Drawing.Point(258, 12);
            this.groupBoxCurrentCompareImage.Name = "groupBoxCurrentCompareImage";
            this.groupBoxCurrentCompareImage.Size = new System.Drawing.Size(442, 377);
            this.groupBoxCurrentCompareImage.TabIndex = 15;
            this.groupBoxCurrentCompareImage.TabStop = false;
            this.groupBoxCurrentCompareImage.Text = "Current compare image";
            // 
            // buttonZoomOut
            // 
            this.buttonZoomOut.Location = new System.Drawing.Point(89, 149);
            this.buttonZoomOut.Name = "buttonZoomOut";
            this.buttonZoomOut.Size = new System.Drawing.Size(25, 23);
            this.buttonZoomOut.TabIndex = 36;
            this.buttonZoomOut.Text = "-";
            this.buttonZoomOut.UseVisualStyleBackColor = true;
            this.buttonZoomOut.Click += new System.EventHandler(this.buttonZoomOut_Click);
            // 
            // buttonZoomIn
            // 
            this.buttonZoomIn.Location = new System.Drawing.Point(120, 149);
            this.buttonZoomIn.Name = "buttonZoomIn";
            this.buttonZoomIn.Size = new System.Drawing.Size(25, 23);
            this.buttonZoomIn.TabIndex = 35;
            this.buttonZoomIn.Text = "+";
            this.buttonZoomIn.UseVisualStyleBackColor = true;
            this.buttonZoomIn.Click += new System.EventHandler(this.buttonZoomIn_Click);
            // 
            // labelCharacters
            // 
            this.labelCharacters.AutoSize = true;
            this.labelCharacters.Location = new System.Drawing.Point(19, 160);
            this.labelCharacters.Name = "labelCharacters";
            this.labelCharacters.Size = new System.Drawing.Size(64, 13);
            this.labelCharacters.TabIndex = 34;
            this.labelCharacters.Text = "Character(s)";
            // 
            // pictureBoxCharacter
            // 
            this.pictureBoxCharacter.Location = new System.Drawing.Point(17, 181);
            this.pictureBoxCharacter.Name = "pictureBoxCharacter";
            this.pictureBoxCharacter.Size = new System.Drawing.Size(99, 47);
            this.pictureBoxCharacter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxCharacter.TabIndex = 33;
            this.pictureBoxCharacter.TabStop = false;
            this.pictureBoxCharacter.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxCharacter_Paint);
            // 
            // buttonAddBetterMatch
            // 
            this.buttonAddBetterMatch.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAddBetterMatch.Location = new System.Drawing.Point(120, 98);
            this.buttonAddBetterMatch.Name = "buttonAddBetterMatch";
            this.buttonAddBetterMatch.Size = new System.Drawing.Size(140, 21);
            this.buttonAddBetterMatch.TabIndex = 28;
            this.buttonAddBetterMatch.Text = "Add better match";
            this.buttonAddBetterMatch.UseVisualStyleBackColor = true;
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
            // labelTextAssociatedWithImage
            // 
            this.labelTextAssociatedWithImage.AutoSize = true;
            this.labelTextAssociatedWithImage.Location = new System.Drawing.Point(14, 19);
            this.labelTextAssociatedWithImage.Name = "labelTextAssociatedWithImage";
            this.labelTextAssociatedWithImage.Size = new System.Drawing.Size(135, 13);
            this.labelTextAssociatedWithImage.TabIndex = 0;
            this.labelTextAssociatedWithImage.Text = "Text associated with image";
            // 
            // buttonDelete
            // 
            this.buttonDelete.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDelete.Location = new System.Drawing.Point(120, 62);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(140, 21);
            this.buttonDelete.TabIndex = 4;
            this.buttonDelete.Text = "Delete ";
            this.buttonDelete.UseVisualStyleBackColor = true;
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonUpdate.Location = new System.Drawing.Point(120, 35);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(140, 21);
            this.buttonUpdate.TabIndex = 3;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            // 
            // textBoxText
            // 
            this.textBoxText.Location = new System.Drawing.Point(14, 35);
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.Size = new System.Drawing.Size(100, 20);
            this.textBoxText.TabIndex = 1;
            // 
            // buttonMakeItalic
            // 
            this.buttonMakeItalic.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonMakeItalic.Location = new System.Drawing.Point(151, 149);
            this.buttonMakeItalic.Name = "buttonMakeItalic";
            this.buttonMakeItalic.Size = new System.Drawing.Size(109, 23);
            this.buttonMakeItalic.TabIndex = 37;
            this.buttonMakeItalic.Text = "Make italic";
            this.buttonMakeItalic.UseVisualStyleBackColor = true;
            this.buttonMakeItalic.Click += new System.EventHandler(this.buttonMakeItalic_Click);
            // 
            // VobSubNOcrEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 426);
            this.Controls.Add(this.groupBoxCurrentCompareImage);
            this.Controls.Add(this.labelImageCompareFiles);
            this.Controls.Add(this.labelChooseCharacters);
            this.Controls.Add(this.listBoxFileNames);
            this.Controls.Add(this.comboBoxTexts);
            this.KeyPreview = true;
            this.Name = "VobSubNOcrEdit";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "VobSubNOcrEdit";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VobSubNOcrEdit_KeyDown);
            this.groupBoxCurrentCompareImage.ResumeLayout(false);
            this.groupBoxCurrentCompareImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCharacter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelImageCompareFiles;
        private System.Windows.Forms.Label labelChooseCharacters;
        private System.Windows.Forms.ListBox listBoxFileNames;
        private System.Windows.Forms.ComboBox comboBoxTexts;
        private System.Windows.Forms.GroupBox groupBoxCurrentCompareImage;
        private System.Windows.Forms.Button buttonZoomOut;
        private System.Windows.Forms.Button buttonZoomIn;
        private System.Windows.Forms.Label labelCharacters;
        private System.Windows.Forms.PictureBox pictureBoxCharacter;
        private System.Windows.Forms.Button buttonAddBetterMatch;
        private System.Windows.Forms.CheckBox checkBoxItalic;
        private System.Windows.Forms.Label labelTextAssociatedWithImage;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.Button buttonMakeItalic;
    }
}