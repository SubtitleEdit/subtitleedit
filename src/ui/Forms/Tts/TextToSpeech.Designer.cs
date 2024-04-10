namespace Nikse.SubtitleEdit.Forms.Tts
{
    partial class TextToSpeech
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
            this.components = new System.ComponentModel.Container();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelProgress = new System.Windows.Forms.Label();
            this.buttonGenerateTTS = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelEngine = new System.Windows.Forms.Label();
            this.groupBoxMsSettings = new System.Windows.Forms.GroupBox();
            this.labelMsVoice = new System.Windows.Forms.Label();
            this.nikseComboBoxVoice = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxAddToVideoFile = new System.Windows.Forms.CheckBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripActors = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.nikseComboBoxEngine = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.groupBoxMsSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(767, 456);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelProgress
            // 
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgress.AutoSize = true;
            this.labelProgress.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelProgress.Location = new System.Drawing.Point(12, 433);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(70, 13);
            this.labelProgress.TabIndex = 9;
            this.labelProgress.Text = "labelProgress";
            // 
            // buttonGenerateTTS
            // 
            this.buttonGenerateTTS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGenerateTTS.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenerateTTS.Location = new System.Drawing.Point(614, 423);
            this.buttonGenerateTTS.Name = "buttonGenerateTTS";
            this.buttonGenerateTTS.Size = new System.Drawing.Size(228, 23);
            this.buttonGenerateTTS.TabIndex = 11;
            this.buttonGenerateTTS.Text = "Generate speech from text";
            this.buttonGenerateTTS.UseVisualStyleBackColor = true;
            this.buttonGenerateTTS.Click += new System.EventHandler(this.ButtonGenerateTtsClick);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 456);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(749, 10);
            this.progressBar1.TabIndex = 12;
            // 
            // labelEngine
            // 
            this.labelEngine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelEngine.AutoSize = true;
            this.labelEngine.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelEngine.Location = new System.Drawing.Point(448, 12);
            this.labelEngine.Name = "labelEngine";
            this.labelEngine.Size = new System.Drawing.Size(40, 13);
            this.labelEngine.TabIndex = 14;
            this.labelEngine.Text = "Engine";
            // 
            // groupBoxMsSettings
            // 
            this.groupBoxMsSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMsSettings.Controls.Add(this.labelMsVoice);
            this.groupBoxMsSettings.Controls.Add(this.nikseComboBoxVoice);
            this.groupBoxMsSettings.Location = new System.Drawing.Point(451, 57);
            this.groupBoxMsSettings.Name = "groupBoxMsSettings";
            this.groupBoxMsSettings.Size = new System.Drawing.Size(391, 320);
            this.groupBoxMsSettings.TabIndex = 15;
            this.groupBoxMsSettings.TabStop = false;
            this.groupBoxMsSettings.Text = "Settings";
            // 
            // labelMsVoice
            // 
            this.labelMsVoice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMsVoice.AutoSize = true;
            this.labelMsVoice.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelMsVoice.Location = new System.Drawing.Point(14, 25);
            this.labelMsVoice.Name = "labelMsVoice";
            this.labelMsVoice.Size = new System.Drawing.Size(34, 13);
            this.labelMsVoice.TabIndex = 16;
            this.labelMsVoice.Text = "Voice";
            // 
            // nikseComboBoxVoice
            // 
            this.nikseComboBoxVoice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nikseComboBoxVoice.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxVoice.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxVoice.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxVoice.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxVoice.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxVoice.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxVoice.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxVoice.DropDownHeight = 400;
            this.nikseComboBoxVoice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nikseComboBoxVoice.DropDownWidth = 0;
            this.nikseComboBoxVoice.FormattingEnabled = false;
            this.nikseComboBoxVoice.Location = new System.Drawing.Point(17, 41);
            this.nikseComboBoxVoice.MaxLength = 32767;
            this.nikseComboBoxVoice.Name = "nikseComboBoxVoice";
            this.nikseComboBoxVoice.SelectedIndex = -1;
            this.nikseComboBoxVoice.SelectedItem = null;
            this.nikseComboBoxVoice.SelectedText = "";
            this.nikseComboBoxVoice.Size = new System.Drawing.Size(351, 23);
            this.nikseComboBoxVoice.TabIndex = 15;
            this.nikseComboBoxVoice.UsePopupWindow = false;
            // 
            // checkBoxAddToVideoFile
            // 
            this.checkBoxAddToVideoFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAddToVideoFile.AutoSize = true;
            this.checkBoxAddToVideoFile.Location = new System.Drawing.Point(614, 397);
            this.checkBoxAddToVideoFile.Name = "checkBoxAddToVideoFile";
            this.checkBoxAddToVideoFile.Size = new System.Drawing.Size(176, 17);
            this.checkBoxAddToVideoFile.TabIndex = 16;
            this.checkBoxAddToVideoFile.Text = "Add audio to video file (new file)";
            this.checkBoxAddToVideoFile.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.ContextMenuStrip = this.contextMenuStripActors;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 28);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(421, 349);
            this.listView1.TabIndex = 17;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Style";
            this.columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Voice";
            this.columnHeader2.Width = 200;
            // 
            // contextMenuStripActors
            // 
            this.contextMenuStripActors.Name = "contextMenuStripActors";
            this.contextMenuStripActors.Size = new System.Drawing.Size(61, 4);
            // 
            // nikseComboBoxEngine
            // 
            this.nikseComboBoxEngine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nikseComboBoxEngine.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxEngine.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxEngine.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxEngine.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxEngine.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxEngine.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxEngine.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxEngine.DropDownHeight = 400;
            this.nikseComboBoxEngine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.nikseComboBoxEngine.DropDownWidth = 391;
            this.nikseComboBoxEngine.FormattingEnabled = false;
            this.nikseComboBoxEngine.Location = new System.Drawing.Point(451, 28);
            this.nikseComboBoxEngine.MaxLength = 32767;
            this.nikseComboBoxEngine.Name = "nikseComboBoxEngine";
            this.nikseComboBoxEngine.SelectedIndex = -1;
            this.nikseComboBoxEngine.SelectedItem = null;
            this.nikseComboBoxEngine.SelectedText = "";
            this.nikseComboBoxEngine.Size = new System.Drawing.Size(391, 23);
            this.nikseComboBoxEngine.TabIndex = 13;
            this.nikseComboBoxEngine.TabStop = false;
            this.nikseComboBoxEngine.Text = "nikseComboBox1";
            this.nikseComboBoxEngine.UsePopupWindow = false;
            this.nikseComboBoxEngine.SelectedIndexChanged += new System.EventHandler(this.nikseComboBoxEngine_SelectedIndexChanged);
            // 
            // TextToSpeech
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(854, 491);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.checkBoxAddToVideoFile);
            this.Controls.Add(this.groupBoxMsSettings);
            this.Controls.Add(this.labelEngine);
            this.Controls.Add(this.nikseComboBoxEngine);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonGenerateTTS);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(827, 481);
            this.Name = "TextToSpeech";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Text to speach";
            this.Load += new System.EventHandler(this.TextToSpeech_Load);
            this.ResizeEnd += new System.EventHandler(this.TextToSpeech_ResizeEnd);
            this.SizeChanged += new System.EventHandler(this.TextToSpeech_SizeChanged);
            this.groupBoxMsSettings.ResumeLayout(false);
            this.groupBoxMsSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.Button buttonGenerateTTS;
        private System.Windows.Forms.ProgressBar progressBar1;
        private Controls.NikseComboBox nikseComboBoxEngine;
        private System.Windows.Forms.Label labelEngine;
        private System.Windows.Forms.GroupBox groupBoxMsSettings;
        private System.Windows.Forms.Label labelMsVoice;
        private Controls.NikseComboBox nikseComboBoxVoice;
        private System.Windows.Forms.CheckBox checkBoxAddToVideoFile;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripActors;
    }
}