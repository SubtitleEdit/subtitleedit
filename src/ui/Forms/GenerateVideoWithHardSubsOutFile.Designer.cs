namespace Nikse.SubtitleEdit.Forms
{
    partial class GenerateVideoWithHardSubsOutFile
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
            this.radioButtonSaveInOutputFolder = new System.Windows.Forms.RadioButton();
            this.linkLabelOpenOutputFolder = new System.Windows.Forms.LinkLabel();
            this.buttonChooseFolder = new System.Windows.Forms.Button();
            this.textBoxOutputFolder = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.radioButtonSaveInSourceFolder = new System.Windows.Forms.RadioButton();
            this.labelSuffix = new System.Windows.Forms.Label();
            this.textBoxSuffix = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // radioButtonSaveInOutputFolder
            // 
            this.radioButtonSaveInOutputFolder.AutoSize = true;
            this.radioButtonSaveInOutputFolder.Location = new System.Drawing.Point(21, 54);
            this.radioButtonSaveInOutputFolder.Name = "radioButtonSaveInOutputFolder";
            this.radioButtonSaveInOutputFolder.Size = new System.Drawing.Size(154, 17);
            this.radioButtonSaveInOutputFolder.TabIndex = 6;
            this.radioButtonSaveInOutputFolder.Text = "Save in output folder below";
            this.radioButtonSaveInOutputFolder.UseVisualStyleBackColor = true;
            this.radioButtonSaveInOutputFolder.CheckedChanged += new System.EventHandler(this.radioButtonSaveInOutputFolder_CheckedChanged);
            // 
            // linkLabelOpenOutputFolder
            // 
            this.linkLabelOpenOutputFolder.AutoSize = true;
            this.linkLabelOpenOutputFolder.Location = new System.Drawing.Point(368, 83);
            this.linkLabelOpenOutputFolder.Name = "linkLabelOpenOutputFolder";
            this.linkLabelOpenOutputFolder.Size = new System.Drawing.Size(42, 13);
            this.linkLabelOpenOutputFolder.TabIndex = 9;
            this.linkLabelOpenOutputFolder.TabStop = true;
            this.linkLabelOpenOutputFolder.Text = "Open...";
            this.linkLabelOpenOutputFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenOutputFolder_LinkClicked);
            // 
            // buttonChooseFolder
            // 
            this.buttonChooseFolder.Enabled = false;
            this.buttonChooseFolder.Location = new System.Drawing.Point(336, 78);
            this.buttonChooseFolder.Name = "buttonChooseFolder";
            this.buttonChooseFolder.Size = new System.Drawing.Size(26, 23);
            this.buttonChooseFolder.TabIndex = 8;
            this.buttonChooseFolder.Text = "...";
            this.buttonChooseFolder.UseVisualStyleBackColor = true;
            this.buttonChooseFolder.Click += new System.EventHandler(this.buttonChooseFolder_Click);
            // 
            // textBoxOutputFolder
            // 
            this.textBoxOutputFolder.Enabled = false;
            this.textBoxOutputFolder.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxOutputFolder.Location = new System.Drawing.Point(28, 81);
            this.textBoxOutputFolder.Name = "textBoxOutputFolder";
            this.textBoxOutputFolder.Size = new System.Drawing.Size(302, 20);
            this.textBoxOutputFolder.TabIndex = 7;
            // 
            // radioButtonSaveInSourceFolder
            // 
            this.radioButtonSaveInSourceFolder.AutoSize = true;
            this.radioButtonSaveInSourceFolder.Checked = true;
            this.radioButtonSaveInSourceFolder.Location = new System.Drawing.Point(21, 23);
            this.radioButtonSaveInSourceFolder.Name = "radioButtonSaveInSourceFolder";
            this.radioButtonSaveInSourceFolder.Size = new System.Drawing.Size(141, 17);
            this.radioButtonSaveInSourceFolder.TabIndex = 5;
            this.radioButtonSaveInSourceFolder.TabStop = true;
            this.radioButtonSaveInSourceFolder.Text = "Save in source file folder";
            this.radioButtonSaveInSourceFolder.UseVisualStyleBackColor = true;
            this.radioButtonSaveInSourceFolder.CheckedChanged += new System.EventHandler(this.radioButtonSaveInSourceFolder_CheckedChanged);
            // 
            // labelSuffix
            // 
            this.labelSuffix.AutoSize = true;
            this.labelSuffix.Location = new System.Drawing.Point(21, 135);
            this.labelSuffix.Name = "labelSuffix";
            this.labelSuffix.Size = new System.Drawing.Size(79, 13);
            this.labelSuffix.TabIndex = 11;
            this.labelSuffix.Text = "File name suffix";
            // 
            // textBoxSuffix
            // 
            this.textBoxSuffix.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxSuffix.Location = new System.Drawing.Point(21, 151);
            this.textBoxSuffix.Name = "textBoxSuffix";
            this.textBoxSuffix.Size = new System.Drawing.Size(309, 20);
            this.textBoxSuffix.TabIndex = 12;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(349, 214);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 105;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(268, 214);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 104;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // GenerateVideoWithHardSubsOutFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 249);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelSuffix);
            this.Controls.Add(this.textBoxSuffix);
            this.Controls.Add(this.radioButtonSaveInOutputFolder);
            this.Controls.Add(this.linkLabelOpenOutputFolder);
            this.Controls.Add(this.buttonChooseFolder);
            this.Controls.Add(this.textBoxOutputFolder);
            this.Controls.Add(this.radioButtonSaveInSourceFolder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GenerateVideoWithHardSubsOutFile";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GenerateVideoWithHardSubsOutFile";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GenerateVideoWithHardSubsOutFile_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonSaveInOutputFolder;
        private System.Windows.Forms.LinkLabel linkLabelOpenOutputFolder;
        private System.Windows.Forms.Button buttonChooseFolder;
        private Controls.NikseTextBox textBoxOutputFolder;
        private System.Windows.Forms.RadioButton radioButtonSaveInSourceFolder;
        private System.Windows.Forms.Label labelSuffix;
        private Controls.NikseTextBox textBoxSuffix;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}