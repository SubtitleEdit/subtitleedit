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
            this.buttonCancel.Location = new System.Drawing.Point(374, 133);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(56, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(314, 133);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(56, 23);
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
            this.PanelDrive.Location = new System.Drawing.Point(82, 31);
            this.PanelDrive.Margin = new System.Windows.Forms.Padding(2);
            this.PanelDrive.Name = "PanelDrive";
            this.PanelDrive.Size = new System.Drawing.Size(332, 81);
            this.PanelDrive.TabIndex = 1;
            // 
            // comboBoxDrive
            // 
            this.comboBoxDrive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDrive.FormattingEnabled = true;
            this.comboBoxDrive.Location = new System.Drawing.Point(5, 24);
            this.comboBoxDrive.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxDrive.Name = "comboBoxDrive";
            this.comboBoxDrive.Size = new System.Drawing.Size(288, 21);
            this.comboBoxDrive.TabIndex = 1;
            // 
            // labelChooseDrive
            // 
            this.labelChooseDrive.AutoSize = true;
            this.labelChooseDrive.Location = new System.Drawing.Point(3, 8);
            this.labelChooseDrive.Name = "labelChooseDrive";
            this.labelChooseDrive.Size = new System.Drawing.Size(69, 13);
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
            this.PanelFolder.Location = new System.Drawing.Point(29, 76);
            this.PanelFolder.Margin = new System.Windows.Forms.Padding(2);
            this.PanelFolder.Name = "PanelFolder";
            this.PanelFolder.Size = new System.Drawing.Size(332, 81);
            this.PanelFolder.TabIndex = 2;
            // 
            // buttonChooseFolder
            // 
            this.buttonChooseFolder.Location = new System.Drawing.Point(298, 23);
            this.buttonChooseFolder.Name = "buttonChooseFolder";
            this.buttonChooseFolder.Size = new System.Drawing.Size(26, 23);
            this.buttonChooseFolder.TabIndex = 2;
            this.buttonChooseFolder.Text = "...";
            this.buttonChooseFolder.UseVisualStyleBackColor = true;
            this.buttonChooseFolder.Click += new System.EventHandler(this.buttonChooseFolder_Click);
            // 
            // labelChooseFolder
            // 
            this.labelChooseFolder.AutoSize = true;
            this.labelChooseFolder.Location = new System.Drawing.Point(3, 8);
            this.labelChooseFolder.Name = "labelChooseFolder";
            this.labelChooseFolder.Size = new System.Drawing.Size(72, 13);
            this.labelChooseFolder.TabIndex = 0;
            this.labelChooseFolder.Text = "Choose folder";
            // 
            // textBoxFolder
            // 
            this.textBoxFolder.Location = new System.Drawing.Point(5, 25);
            this.textBoxFolder.Name = "textBoxFolder";
            this.textBoxFolder.Size = new System.Drawing.Size(288, 20);
            this.textBoxFolder.TabIndex = 1;
            // 
            // radioButtonDisc
            // 
            this.radioButtonDisc.AutoSize = true;
            this.radioButtonDisc.Checked = true;
            this.radioButtonDisc.Location = new System.Drawing.Point(11, 31);
            this.radioButtonDisc.Margin = new System.Windows.Forms.Padding(2);
            this.radioButtonDisc.Name = "radioButtonDisc";
            this.radioButtonDisc.Size = new System.Drawing.Size(46, 17);
            this.radioButtonDisc.TabIndex = 0;
            this.radioButtonDisc.TabStop = true;
            this.radioButtonDisc.Text = "Disc";
            this.radioButtonDisc.UseVisualStyleBackColor = true;
            this.radioButtonDisc.CheckedChanged += new System.EventHandler(this.radioButtonDisc_CheckedChanged);
            // 
            // radioButtonFolder
            // 
            this.radioButtonFolder.AutoSize = true;
            this.radioButtonFolder.Location = new System.Drawing.Point(11, 54);
            this.radioButtonFolder.Margin = new System.Windows.Forms.Padding(2);
            this.radioButtonFolder.Name = "radioButtonFolder";
            this.radioButtonFolder.Size = new System.Drawing.Size(54, 17);
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
            this.groupBoxOpenDvdFrom.Location = new System.Drawing.Point(9, 10);
            this.groupBoxOpenDvdFrom.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxOpenDvdFrom.Name = "groupBoxOpenDvdFrom";
            this.groupBoxOpenDvdFrom.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxOpenDvdFrom.Size = new System.Drawing.Size(422, 118);
            this.groupBoxOpenDvdFrom.TabIndex = 0;
            this.groupBoxOpenDvdFrom.TabStop = false;
            this.groupBoxOpenDvdFrom.Text = "Open DVD from...";
            // 
            // OpenVideoDvd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 160);
            this.Controls.Add(this.groupBoxOpenDvdFrom);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OpenVideoDvd";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "OpenVideoDvd";
            this.Shown += new System.EventHandler(this.OpenVideoDvd_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OpenVideoDvd_KeyDown);
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