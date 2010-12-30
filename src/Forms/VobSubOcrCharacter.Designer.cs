namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class VobSubOcrCharacter
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelSubtitleImage = new System.Windows.Forms.Label();
            this.pictureBoxSubtitleImage = new System.Windows.Forms.PictureBox();
            this.labelCharactersAsText = new System.Windows.Forms.Label();
            this.textBoxCharacters = new System.Windows.Forms.TextBox();
            this.pictureBoxCharacter = new System.Windows.Forms.PictureBox();
            this.labelCharacters = new System.Windows.Forms.Label();
            this.buttonAbort = new System.Windows.Forms.Button();
            this.checkBoxItalic = new System.Windows.Forms.CheckBox();
            this.buttonExpandSelection = new System.Windows.Forms.Button();
            this.buttonShrinkSelection = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCharacter)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(325, 286);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(406, 286);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelSubtitleImage
            // 
            this.labelSubtitleImage.AutoSize = true;
            this.labelSubtitleImage.Location = new System.Drawing.Point(9, 13);
            this.labelSubtitleImage.Name = "labelSubtitleImage";
            this.labelSubtitleImage.Size = new System.Drawing.Size(93, 17);
            this.labelSubtitleImage.TabIndex = 13;
            this.labelSubtitleImage.Text = "Subtitle image";
            // 
            // pictureBoxSubtitleImage
            // 
            this.pictureBoxSubtitleImage.Location = new System.Drawing.Point(12, 29);
            this.pictureBoxSubtitleImage.Name = "pictureBoxSubtitleImage";
            this.pictureBoxSubtitleImage.Size = new System.Drawing.Size(550, 125);
            this.pictureBoxSubtitleImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxSubtitleImage.TabIndex = 12;
            this.pictureBoxSubtitleImage.TabStop = false;
            // 
            // labelCharactersAsText
            // 
            this.labelCharactersAsText.AutoSize = true;
            this.labelCharactersAsText.Location = new System.Drawing.Point(9, 242);
            this.labelCharactersAsText.Name = "labelCharactersAsText";
            this.labelCharactersAsText.Size = new System.Drawing.Size(130, 17);
            this.labelCharactersAsText.TabIndex = 17;
            this.labelCharactersAsText.Text = "Character(s) as text";
            // 
            // textBoxCharacters
            // 
            this.textBoxCharacters.Location = new System.Drawing.Point(12, 259);
            this.textBoxCharacters.Name = "textBoxCharacters";
            this.textBoxCharacters.Size = new System.Drawing.Size(225, 24);
            this.textBoxCharacters.TabIndex = 0;
            this.textBoxCharacters.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxCharactersKeyDown);
            // 
            // pictureBoxCharacter
            // 
            this.pictureBoxCharacter.Location = new System.Drawing.Point(12, 184);
            this.pictureBoxCharacter.Name = "pictureBoxCharacter";
            this.pictureBoxCharacter.Size = new System.Drawing.Size(99, 47);
            this.pictureBoxCharacter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxCharacter.TabIndex = 18;
            this.pictureBoxCharacter.TabStop = false;
            // 
            // labelCharacters
            // 
            this.labelCharacters.AutoSize = true;
            this.labelCharacters.Location = new System.Drawing.Point(14, 168);
            this.labelCharacters.Name = "labelCharacters";
            this.labelCharacters.Size = new System.Drawing.Size(84, 17);
            this.labelCharacters.TabIndex = 19;
            this.labelCharacters.Text = "Character(s)";
            // 
            // buttonAbort
            // 
            this.buttonAbort.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.buttonAbort.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAbort.Location = new System.Drawing.Point(487, 286);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.Size = new System.Drawing.Size(75, 21);
            this.buttonAbort.TabIndex = 4;
            this.buttonAbort.Text = "&Abort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            // 
            // checkBoxItalic
            // 
            this.checkBoxItalic.AutoSize = true;
            this.checkBoxItalic.Location = new System.Drawing.Point(243, 262);
            this.checkBoxItalic.Name = "checkBoxItalic";
            this.checkBoxItalic.Size = new System.Drawing.Size(57, 21);
            this.checkBoxItalic.TabIndex = 1;
            this.checkBoxItalic.Text = "&Italic";
            this.checkBoxItalic.UseVisualStyleBackColor = true;
            this.checkBoxItalic.CheckedChanged += new System.EventHandler(this.CheckBoxItalicCheckedChanged);
            // 
            // buttonExpandSelection
            // 
            this.buttonExpandSelection.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonExpandSelection.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonExpandSelection.Location = new System.Drawing.Point(449, 2);
            this.buttonExpandSelection.Name = "buttonExpandSelection";
            this.buttonExpandSelection.Size = new System.Drawing.Size(112, 21);
            this.buttonExpandSelection.TabIndex = 20;
            this.buttonExpandSelection.Text = "Expand selection";
            this.buttonExpandSelection.UseVisualStyleBackColor = true;
            this.buttonExpandSelection.Click += new System.EventHandler(this.ButtonExpandSelectionClick);
            // 
            // buttonShrinkSelection
            // 
            this.buttonShrinkSelection.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonShrinkSelection.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonShrinkSelection.Location = new System.Drawing.Point(331, 2);
            this.buttonShrinkSelection.Name = "buttonShrinkSelection";
            this.buttonShrinkSelection.Size = new System.Drawing.Size(112, 21);
            this.buttonShrinkSelection.TabIndex = 21;
            this.buttonShrinkSelection.Text = "Shrink selection";
            this.buttonShrinkSelection.UseVisualStyleBackColor = true;
            this.buttonShrinkSelection.Click += new System.EventHandler(this.ButtonShrinkSelectionClick);
            // 
            // VobSubOcrCharacter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(573, 317);
            this.Controls.Add(this.buttonShrinkSelection);
            this.Controls.Add(this.buttonExpandSelection);
            this.Controls.Add(this.checkBoxItalic);
            this.Controls.Add(this.buttonAbort);
            this.Controls.Add(this.labelCharacters);
            this.Controls.Add(this.pictureBoxCharacter);
            this.Controls.Add(this.labelCharactersAsText);
            this.Controls.Add(this.textBoxCharacters);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelSubtitleImage);
            this.Controls.Add(this.pictureBoxSubtitleImage);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VobSubOcrCharacter";
            this.Text = "VobSub - Manual image to text";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCharacter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelSubtitleImage;
        private System.Windows.Forms.PictureBox pictureBoxSubtitleImage;
        private System.Windows.Forms.Label labelCharactersAsText;
        private System.Windows.Forms.TextBox textBoxCharacters;
        private System.Windows.Forms.PictureBox pictureBoxCharacter;
        private System.Windows.Forms.Label labelCharacters;
        private System.Windows.Forms.Button buttonAbort;
        private System.Windows.Forms.CheckBox checkBoxItalic;
        private System.Windows.Forms.Button buttonExpandSelection;
        private System.Windows.Forms.Button buttonShrinkSelection;
    }
}