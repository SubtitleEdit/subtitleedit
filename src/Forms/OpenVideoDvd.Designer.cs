namespace Nikse.SubtitleEdit.Forms
{
    partial class OpenVideoDvd
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.PanelDrive = new System.Windows.Forms.Panel();
            this.comboBoxDrive = new System.Windows.Forms.ComboBox();
            this.labelChooseDrive = new System.Windows.Forms.Label();
            this.PanelFolder = new System.Windows.Forms.Panel();
            this.buttonChooseFolder = new System.Windows.Forms.Button();
            this.labelChooseFolder = new System.Windows.Forms.Label();
            this.textBoxFolder = new System.Windows.Forms.TextBox();
            this.radioButtonDisc = new System.Windows.Forms.RadioButton();
            this.radioButtonFolder = new System.Windows.Forms.RadioButton();
            this.groupBoxOpenDvdFrom = new System.Windows.Forms.GroupBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.PanelDrive.SuspendLayout();
            this.PanelFolder.SuspendLayout();
            this.groupBoxOpenDvdFrom.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(499, 164);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 24);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(418, 164);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 24);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // PanelDrive
            // 
            this.PanelDrive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelDrive.Controls.Add(this.comboBoxDrive);
            this.PanelDrive.Controls.Add(this.labelChooseDrive);
            this.PanelDrive.Location = new System.Drawing.Point(110, 38);
            this.PanelDrive.Name = "PanelDrive";
            this.PanelDrive.Size = new System.Drawing.Size(442, 100);
            this.PanelDrive.TabIndex = 1;
            // 
            // comboBoxDrive
            // 
            this.comboBoxDrive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDrive.FormattingEnabled = true;
            this.comboBoxDrive.Location = new System.Drawing.Point(7, 30);
            this.comboBoxDrive.Name = "comboBoxDrive";
            this.comboBoxDrive.Size = new System.Drawing.Size(383, 24);
            this.comboBoxDrive.TabIndex = 1;
            // 
            // labelChooseDrive
            // 
            this.labelChooseDrive.AutoSize = true;
            this.labelChooseDrive.Location = new System.Drawing.Point(4, 10);
            this.labelChooseDrive.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelChooseDrive.Name = "labelChooseDrive";
            this.labelChooseDrive.Size = new System.Drawing.Size(91, 17);
            this.labelChooseDrive.TabIndex = 0;
            this.labelChooseDrive.Text = "Choose drive";
            // 
            // PanelFolder
            // 
            this.PanelFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelFolder.Controls.Add(this.buttonChooseFolder);
            this.PanelFolder.Controls.Add(this.labelChooseFolder);
            this.PanelFolder.Controls.Add(this.textBoxFolder);
            this.PanelFolder.Location = new System.Drawing.Point(39, 93);
            this.PanelFolder.Name = "PanelFolder";
            this.PanelFolder.Size = new System.Drawing.Size(442, 100);
            this.PanelFolder.TabIndex = 2;
            // 
            // buttonChooseFolder
            // 
            this.buttonChooseFolder.Location = new System.Drawing.Point(398, 28);
            this.buttonChooseFolder.Margin = new System.Windows.Forms.Padding(4);
            this.buttonChooseFolder.Name = "buttonChooseFolder";
            this.buttonChooseFolder.Size = new System.Drawing.Size(35, 28);
            this.buttonChooseFolder.TabIndex = 2;
            this.buttonChooseFolder.Text = "...";
            this.buttonChooseFolder.UseVisualStyleBackColor = true;
            this.buttonChooseFolder.Click += new System.EventHandler(this.buttonChooseFolder_Click);
            // 
            // labelChooseFolder
            // 
            this.labelChooseFolder.AutoSize = true;
            this.labelChooseFolder.Location = new System.Drawing.Point(4, 10);
            this.labelChooseFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelChooseFolder.Name = "labelChooseFolder";
            this.labelChooseFolder.Size = new System.Drawing.Size(96, 17);
            this.labelChooseFolder.TabIndex = 0;
            this.labelChooseFolder.Text = "Choose folder";
            // 
            // textBoxFolder
            // 
            this.textBoxFolder.Location = new System.Drawing.Point(7, 31);
            this.textBoxFolder.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxFolder.Name = "textBoxFolder";
            this.textBoxFolder.Size = new System.Drawing.Size(383, 22);
            this.textBoxFolder.TabIndex = 1;
            // 
            // radioButtonDisc
            // 
            this.radioButtonDisc.AutoSize = true;
            this.radioButtonDisc.Checked = true;
            this.radioButtonDisc.Location = new System.Drawing.Point(15, 38);
            this.radioButtonDisc.Name = "radioButtonDisc";
            this.radioButtonDisc.Size = new System.Drawing.Size(56, 21);
            this.radioButtonDisc.TabIndex = 0;
            this.radioButtonDisc.TabStop = true;
            this.radioButtonDisc.Text = "Disc";
            this.radioButtonDisc.UseVisualStyleBackColor = true;
            this.radioButtonDisc.CheckedChanged += new System.EventHandler(this.radioButtonDisc_CheckedChanged);
            // 
            // radioButtonFolder
            // 
            this.radioButtonFolder.AutoSize = true;
            this.radioButtonFolder.Location = new System.Drawing.Point(15, 66);
            this.radioButtonFolder.Name = "radioButtonFolder";
            this.radioButtonFolder.Size = new System.Drawing.Size(69, 21);
            this.radioButtonFolder.TabIndex = 1;
            this.radioButtonFolder.Text = "Folder";
            this.radioButtonFolder.UseVisualStyleBackColor = true;
            // 
            // groupBoxOpenDvdFrom
            // 
            this.groupBoxOpenDvdFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOpenDvdFrom.Controls.Add(this.radioButtonDisc);
            this.groupBoxOpenDvdFrom.Controls.Add(this.radioButtonFolder);
            this.groupBoxOpenDvdFrom.Controls.Add(this.PanelFolder);
            this.groupBoxOpenDvdFrom.Controls.Add(this.PanelDrive);
            this.groupBoxOpenDvdFrom.Location = new System.Drawing.Point(12, 12);
            this.groupBoxOpenDvdFrom.Name = "groupBoxOpenDvdFrom";
            this.groupBoxOpenDvdFrom.Size = new System.Drawing.Size(562, 145);
            this.groupBoxOpenDvdFrom.TabIndex = 0;
            this.groupBoxOpenDvdFrom.TabStop = false;
            this.groupBoxOpenDvdFrom.Text = "Open DVD from...";
            // 
            // OpenVideoDvd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 197);
            this.Controls.Add(this.groupBoxOpenDvdFrom);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OpenVideoDvd";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "OpenVideoDvd";
            this.Shown += new System.EventHandler(this.OpenVideoDvd_Shown);
            this.PanelDrive.ResumeLayout(false);
            this.PanelDrive.PerformLayout();
            this.PanelFolder.ResumeLayout(false);
            this.PanelFolder.PerformLayout();
            this.groupBoxOpenDvdFrom.ResumeLayout(false);
            this.groupBoxOpenDvdFrom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Panel PanelDrive;
        private System.Windows.Forms.Panel PanelFolder;
        private System.Windows.Forms.Label labelChooseDrive;
        private System.Windows.Forms.Label labelChooseFolder;
        private System.Windows.Forms.TextBox textBoxFolder;
        private System.Windows.Forms.ComboBox comboBoxDrive;
        private System.Windows.Forms.RadioButton radioButtonDisc;
        private System.Windows.Forms.RadioButton radioButtonFolder;
        private System.Windows.Forms.GroupBox groupBoxOpenDvdFrom;
        private System.Windows.Forms.Button buttonChooseFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}