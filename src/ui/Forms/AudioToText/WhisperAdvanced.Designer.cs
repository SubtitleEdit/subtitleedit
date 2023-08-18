using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    sealed partial class WhisperAdvanced
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WhisperAdvanced));
            this.labelWhisperExtraCmdLine = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelNote = new System.Windows.Forms.Label();
            this.tabControlCommandLineHelp = new System.Windows.Forms.TabControl();
            this.TabPageCPP = new System.Windows.Forms.TabPage();
            this.textBoxCpp = new System.Windows.Forms.TextBox();
            this.tabPageConstMe = new System.Windows.Forms.TabPage();
            this.textBoxConstMe = new System.Windows.Forms.TextBox();
            this.tabPageOpenAI = new System.Windows.Forms.TabPage();
            this.textBoxOpenAI = new System.Windows.Forms.TextBox();
            this.tabPageFasterWhisper = new System.Windows.Forms.TabPage();
            this.textBoxPurfviewFasterWhisper = new System.Windows.Forms.TextBox();
            this.comboBoxWhisperExtra = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.tabControlCommandLineHelp.SuspendLayout();
            this.TabPageCPP.SuspendLayout();
            this.tabPageConstMe.SuspendLayout();
            this.tabPageOpenAI.SuspendLayout();
            this.tabPageFasterWhisper.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelWhisperExtraCmdLine
            // 
            this.labelWhisperExtraCmdLine.AutoSize = true;
            this.labelWhisperExtraCmdLine.Location = new System.Drawing.Point(12, 22);
            this.labelWhisperExtraCmdLine.Name = "labelWhisperExtraCmdLine";
            this.labelWhisperExtraCmdLine.Size = new System.Drawing.Size(214, 13);
            this.labelWhisperExtraCmdLine.TabIndex = 217;
            this.labelWhisperExtraCmdLine.Text = "Extra parameters for Whisper command line:";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(778, 559);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 216;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(697, 559);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 215;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelNote
            // 
            this.labelNote.AutoSize = true;
            this.labelNote.Location = new System.Drawing.Point(12, 73);
            this.labelNote.Name = "labelNote";
            this.labelNote.Size = new System.Drawing.Size(390, 13);
            this.labelNote.TabIndex = 218;
            this.labelNote.Text = "Note: Different Whisper implementations have different command line parameters!\r\n" +
    "";
            // 
            // tabControlCommandLineHelp
            // 
            this.tabControlCommandLineHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlCommandLineHelp.Controls.Add(this.TabPageCPP);
            this.tabControlCommandLineHelp.Controls.Add(this.tabPageConstMe);
            this.tabControlCommandLineHelp.Controls.Add(this.tabPageOpenAI);
            this.tabControlCommandLineHelp.Controls.Add(this.tabPageFasterWhisper);
            this.tabControlCommandLineHelp.Location = new System.Drawing.Point(15, 108);
            this.tabControlCommandLineHelp.Name = "tabControlCommandLineHelp";
            this.tabControlCommandLineHelp.SelectedIndex = 0;
            this.tabControlCommandLineHelp.Size = new System.Drawing.Size(838, 445);
            this.tabControlCommandLineHelp.TabIndex = 219;
            // 
            // TabPageCPP
            // 
            this.TabPageCPP.Controls.Add(this.textBoxCpp);
            this.TabPageCPP.Location = new System.Drawing.Point(4, 22);
            this.TabPageCPP.Name = "TabPageCPP";
            this.TabPageCPP.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageCPP.Size = new System.Drawing.Size(830, 419);
            this.TabPageCPP.TabIndex = 0;
            this.TabPageCPP.Text = "CPP";
            this.TabPageCPP.UseVisualStyleBackColor = true;
            // 
            // textBoxCpp
            // 
            this.textBoxCpp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxCpp.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxCpp.Location = new System.Drawing.Point(3, 3);
            this.textBoxCpp.Multiline = true;
            this.textBoxCpp.Name = "textBoxCpp";
            this.textBoxCpp.ReadOnly = true;
            this.textBoxCpp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxCpp.Size = new System.Drawing.Size(824, 413);
            this.textBoxCpp.TabIndex = 0;
            this.textBoxCpp.Text = resources.GetString("textBoxCpp.Text");
            this.textBoxCpp.WordWrap = false;
            // 
            // tabPageConstMe
            // 
            this.tabPageConstMe.Controls.Add(this.textBoxConstMe);
            this.tabPageConstMe.Location = new System.Drawing.Point(4, 22);
            this.tabPageConstMe.Name = "tabPageConstMe";
            this.tabPageConstMe.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConstMe.Size = new System.Drawing.Size(830, 419);
            this.tabPageConstMe.TabIndex = 1;
            this.tabPageConstMe.Text = "Const-me";
            this.tabPageConstMe.UseVisualStyleBackColor = true;
            // 
            // textBoxConstMe
            // 
            this.textBoxConstMe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxConstMe.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxConstMe.Location = new System.Drawing.Point(3, 3);
            this.textBoxConstMe.Multiline = true;
            this.textBoxConstMe.Name = "textBoxConstMe";
            this.textBoxConstMe.ReadOnly = true;
            this.textBoxConstMe.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxConstMe.Size = new System.Drawing.Size(824, 413);
            this.textBoxConstMe.TabIndex = 1;
            this.textBoxConstMe.Text = resources.GetString("textBoxConstMe.Text");
            this.textBoxConstMe.WordWrap = false;
            // 
            // tabPageOpenAI
            // 
            this.tabPageOpenAI.Controls.Add(this.textBoxOpenAI);
            this.tabPageOpenAI.Location = new System.Drawing.Point(4, 22);
            this.tabPageOpenAI.Name = "tabPageOpenAI";
            this.tabPageOpenAI.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOpenAI.Size = new System.Drawing.Size(830, 419);
            this.tabPageOpenAI.TabIndex = 2;
            this.tabPageOpenAI.Text = "OpenAI";
            this.tabPageOpenAI.UseVisualStyleBackColor = true;
            // 
            // textBoxOpenAI
            // 
            this.textBoxOpenAI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxOpenAI.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxOpenAI.Location = new System.Drawing.Point(3, 3);
            this.textBoxOpenAI.Multiline = true;
            this.textBoxOpenAI.Name = "textBoxOpenAI";
            this.textBoxOpenAI.ReadOnly = true;
            this.textBoxOpenAI.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxOpenAI.Size = new System.Drawing.Size(824, 413);
            this.textBoxOpenAI.TabIndex = 2;
            this.textBoxOpenAI.Text = resources.GetString("textBoxOpenAI.Text");
            this.textBoxOpenAI.WordWrap = false;
            // 
            // tabPageFasterWhisper
            // 
            this.tabPageFasterWhisper.Controls.Add(this.textBoxPurfviewFasterWhisper);
            this.tabPageFasterWhisper.Location = new System.Drawing.Point(4, 22);
            this.tabPageFasterWhisper.Name = "tabPageFasterWhisper";
            this.tabPageFasterWhisper.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFasterWhisper.Size = new System.Drawing.Size(830, 419);
            this.tabPageFasterWhisper.TabIndex = 3;
            this.tabPageFasterWhisper.Text = "Faster Whisper";
            this.tabPageFasterWhisper.UseVisualStyleBackColor = true;
            // 
            // textBoxPurfviewFasterWhisper
            // 
            this.textBoxPurfviewFasterWhisper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxPurfviewFasterWhisper.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxPurfviewFasterWhisper.Location = new System.Drawing.Point(3, 3);
            this.textBoxPurfviewFasterWhisper.Multiline = true;
            this.textBoxPurfviewFasterWhisper.Name = "textBoxPurfviewFasterWhisper";
            this.textBoxPurfviewFasterWhisper.ReadOnly = true;
            this.textBoxPurfviewFasterWhisper.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxPurfviewFasterWhisper.Size = new System.Drawing.Size(824, 413);
            this.textBoxPurfviewFasterWhisper.TabIndex = 3;
            this.textBoxPurfviewFasterWhisper.Text = resources.GetString("textBoxPurfviewFasterWhisper.Text");
            this.textBoxPurfviewFasterWhisper.WordWrap = false;
            // 
            // comboBoxWhisperExtra
            // 
            this.comboBoxWhisperExtra.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxWhisperExtra.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxWhisperExtra.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxWhisperExtra.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxWhisperExtra.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxWhisperExtra.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxWhisperExtra.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxWhisperExtra.DropDownHeight = 400;
            this.comboBoxWhisperExtra.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxWhisperExtra.DropDownWidth = 0;
            this.comboBoxWhisperExtra.FormattingEnabled = true;
            this.comboBoxWhisperExtra.Location = new System.Drawing.Point(15, 38);
            this.comboBoxWhisperExtra.MaxLength = 32767;
            this.comboBoxWhisperExtra.Name = "comboBoxWhisperExtra";
            this.comboBoxWhisperExtra.SelectedIndex = -1;
            this.comboBoxWhisperExtra.SelectedItem = null;
            this.comboBoxWhisperExtra.SelectedText = "";
            this.comboBoxWhisperExtra.Size = new System.Drawing.Size(441, 21);
            this.comboBoxWhisperExtra.TabIndex = 214;
            this.comboBoxWhisperExtra.TabStop = false;
            this.comboBoxWhisperExtra.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBoxWhisperExtra_KeyDown);
            // 
            // WhisperAdvanced
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(865, 594);
            this.Controls.Add(this.tabControlCommandLineHelp);
            this.Controls.Add(this.labelNote);
            this.Controls.Add(this.labelWhisperExtraCmdLine);
            this.Controls.Add(this.comboBoxWhisperExtra);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "WhisperAdvanced";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WhisperAdvanced";
            this.Shown += new System.EventHandler(this.WhisperAdvanced_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.WhisperAdvanced_KeyDown);
            this.tabControlCommandLineHelp.ResumeLayout(false);
            this.TabPageCPP.ResumeLayout(false);
            this.TabPageCPP.PerformLayout();
            this.tabPageConstMe.ResumeLayout(false);
            this.tabPageConstMe.PerformLayout();
            this.tabPageOpenAI.ResumeLayout(false);
            this.tabPageOpenAI.PerformLayout();
            this.tabPageFasterWhisper.ResumeLayout(false);
            this.tabPageFasterWhisper.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelWhisperExtraCmdLine;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxWhisperExtra;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.TabControl tabControlCommandLineHelp;
        private System.Windows.Forms.TabPage TabPageCPP;
        private System.Windows.Forms.TextBox textBoxCpp;
        private System.Windows.Forms.TabPage tabPageConstMe;
        private System.Windows.Forms.TabPage tabPageOpenAI;
        private System.Windows.Forms.TextBox textBoxConstMe;
        private System.Windows.Forms.TextBox textBoxOpenAI;
        private System.Windows.Forms.TabPage tabPageFasterWhisper;
        private System.Windows.Forms.TextBox textBoxPurfviewFasterWhisper;
    }
}