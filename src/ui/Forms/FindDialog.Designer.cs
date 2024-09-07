﻿namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class FindDialog
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
            this.textBoxFind = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.buttonFind = new System.Windows.Forms.Button();
            this.radioButtonNormal = new System.Windows.Forms.RadioButton();
            this.radioButtonCaseSensitive = new System.Windows.Forms.RadioButton();
            this.radioButtonRegEx = new System.Windows.Forms.RadioButton();
            this.comboBoxFind = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.buttonCount = new System.Windows.Forms.Button();
            this.labelCount = new System.Windows.Forms.Label();
            this.checkBoxWholeWord = new System.Windows.Forms.CheckBox();
            this.buttonFindPrev = new System.Windows.Forms.Button();
            this.labelFindWhat = new System.Windows.Forms.Label();
            this.contextMenuStripNormal = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripNormal.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxFind
            // 
            this.textBoxFind.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxFind.Location = new System.Drawing.Point(12, 28);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(232, 21);
            this.textBoxFind.TabIndex = 0;
            this.textBoxFind.TextChanged += new System.EventHandler(this.textBoxFind_TextChanged);
            this.textBoxFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxFind_KeyDown);
            // 
            // buttonFind
            // 
            this.buttonFind.Location = new System.Drawing.Point(260, 28);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(122, 23);
            this.buttonFind.TabIndex = 1;
            this.buttonFind.Text = "Find next";
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.ButtonFind_Click);
            // 
            // radioButtonNormal
            // 
            this.radioButtonNormal.AutoSize = true;
            this.radioButtonNormal.Checked = true;
            this.radioButtonNormal.Location = new System.Drawing.Point(12, 89);
            this.radioButtonNormal.Name = "radioButtonNormal";
            this.radioButtonNormal.Size = new System.Drawing.Size(58, 17);
            this.radioButtonNormal.TabIndex = 5;
            this.radioButtonNormal.TabStop = true;
            this.radioButtonNormal.Text = "Normal";
            this.radioButtonNormal.UseVisualStyleBackColor = true;
            this.radioButtonNormal.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // radioButtonCaseSensitive
            // 
            this.radioButtonCaseSensitive.AutoSize = true;
            this.radioButtonCaseSensitive.Location = new System.Drawing.Point(12, 112);
            this.radioButtonCaseSensitive.Name = "radioButtonCaseSensitive";
            this.radioButtonCaseSensitive.Size = new System.Drawing.Size(94, 17);
            this.radioButtonCaseSensitive.TabIndex = 7;
            this.radioButtonCaseSensitive.Text = "Case sensitive";
            this.radioButtonCaseSensitive.UseVisualStyleBackColor = true;
            this.radioButtonCaseSensitive.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // radioButtonRegEx
            // 
            this.radioButtonRegEx.AutoSize = true;
            this.radioButtonRegEx.Location = new System.Drawing.Point(12, 135);
            this.radioButtonRegEx.Name = "radioButtonRegEx";
            this.radioButtonRegEx.Size = new System.Drawing.Size(56, 17);
            this.radioButtonRegEx.TabIndex = 9;
            this.radioButtonRegEx.Text = "RegEx";
            this.radioButtonRegEx.UseVisualStyleBackColor = true;
            this.radioButtonRegEx.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // comboBoxFind
            // 
            this.comboBoxFind.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFind.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFind.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFind.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFind.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFind.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFind.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFind.DropDownHeight = 400;
            this.comboBoxFind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxFind.DropDownWidth = 232;
            this.comboBoxFind.FormattingEnabled = true;
            this.comboBoxFind.Location = new System.Drawing.Point(12, 28);
            this.comboBoxFind.MaxLength = 32767;
            this.comboBoxFind.Name = "comboBoxFind";
            this.comboBoxFind.SelectedIndex = -1;
            this.comboBoxFind.SelectedItem = null;
            this.comboBoxFind.SelectedText = "";
            this.comboBoxFind.Size = new System.Drawing.Size(232, 21);
            this.comboBoxFind.TabIndex = 0;
            this.comboBoxFind.TabStop = false;
            this.comboBoxFind.UsePopupWindow = false;
            this.comboBoxFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ComboBoxFind_KeyDown);
            this.comboBoxFind.TextChanged += new System.EventHandler(this.comboBoxFind_TextChanged);
            // 
            // buttonCount
            // 
            this.buttonCount.Location = new System.Drawing.Point(260, 86);
            this.buttonCount.Name = "buttonCount";
            this.buttonCount.Size = new System.Drawing.Size(122, 23);
            this.buttonCount.TabIndex = 3;
            this.buttonCount.Text = "Count";
            this.buttonCount.UseVisualStyleBackColor = true;
            this.buttonCount.Click += new System.EventHandler(this.buttonCount_Click);
            // 
            // labelCount
            // 
            this.labelCount.AutoSize = true;
            this.labelCount.Location = new System.Drawing.Point(259, 112);
            this.labelCount.Name = "labelCount";
            this.labelCount.Size = new System.Drawing.Size(40, 13);
            this.labelCount.TabIndex = 11;
            this.labelCount.Text = "Count:";
            // 
            // checkBoxWholeWord
            // 
            this.checkBoxWholeWord.AutoSize = true;
            this.checkBoxWholeWord.Location = new System.Drawing.Point(12, 61);
            this.checkBoxWholeWord.Name = "checkBoxWholeWord";
            this.checkBoxWholeWord.Size = new System.Drawing.Size(83, 17);
            this.checkBoxWholeWord.TabIndex = 4;
            this.checkBoxWholeWord.Text = "Whole word";
            this.checkBoxWholeWord.UseVisualStyleBackColor = true;
            this.checkBoxWholeWord.CheckedChanged += new System.EventHandler(this.checkBoxWholeWord_CheckedChanged);
            // 
            // buttonFindPrev
            // 
            this.buttonFindPrev.Location = new System.Drawing.Point(260, 57);
            this.buttonFindPrev.Name = "buttonFindPrev";
            this.buttonFindPrev.Size = new System.Drawing.Size(122, 23);
            this.buttonFindPrev.TabIndex = 2;
            this.buttonFindPrev.Text = "Find prevoius";
            this.buttonFindPrev.UseVisualStyleBackColor = true;
            this.buttonFindPrev.Click += new System.EventHandler(this.buttonFindPrev_Click);
            // 
            // labelFindWhat
            // 
            this.labelFindWhat.AutoSize = true;
            this.labelFindWhat.Location = new System.Drawing.Point(12, 12);
            this.labelFindWhat.Name = "labelFindWhat";
            this.labelFindWhat.Size = new System.Drawing.Size(58, 13);
            this.labelFindWhat.TabIndex = 12;
            this.labelFindWhat.Text = "Find what:";
            // 
            // contextMenuStripNormal
            // 
            this.contextMenuStripNormal.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenuStripNormal.Name = "contextMenuStripNormal";
            this.contextMenuStripNormal.Size = new System.Drawing.Size(145, 92);
            this.contextMenuStripNormal.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripNormal_Opening);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // FindDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 172);
            this.Controls.Add(this.labelFindWhat);
            this.Controls.Add(this.buttonFindPrev);
            this.Controls.Add(this.checkBoxWholeWord);
            this.Controls.Add(this.labelCount);
            this.Controls.Add(this.buttonCount);
            this.Controls.Add(this.radioButtonRegEx);
            this.Controls.Add(this.radioButtonCaseSensitive);
            this.Controls.Add(this.radioButtonNormal);
            this.Controls.Add(this.buttonFind);
            this.Controls.Add(this.comboBoxFind);
            this.Controls.Add(this.textBoxFind);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FindDialog_FormClosing);
            this.Shown += new System.EventHandler(this.FindDialog_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormFindDialog_KeyDown);
            this.contextMenuStripNormal.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxFind;
        private System.Windows.Forms.Button buttonFind;
        private System.Windows.Forms.RadioButton radioButtonNormal;
        private System.Windows.Forms.RadioButton radioButtonCaseSensitive;
        private System.Windows.Forms.RadioButton radioButtonRegEx;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFind;
        private System.Windows.Forms.Button buttonCount;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.CheckBox checkBoxWholeWord;
        private System.Windows.Forms.Button buttonFindPrev;
        private System.Windows.Forms.Label labelFindWhat;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripNormal;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
    }
}
