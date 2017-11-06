﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class VobSubOcrNewFolder : Form
    {
        public string FolderName { get; set; }
        private bool _vobSub = false;
        public VobSubOcrNewFolder(bool vobsub)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            FolderName = null;
            _vobSub = vobsub;

            Text = Configuration.Settings.Language.VobSubOcrNewFolder.Title;
            label1.Text = Configuration.Settings.Language.VobSubOcrNewFolder.Message;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonCancel);
        }

        private void FormVobSubOcrNewFolder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            string folderName = textBoxFolder.Text.Trim();
            if (folderName.Length == 0)
            {
                return;
            }

            if (folderName.Contains('?') || folderName.Contains('/') || folderName.Contains("\\"))
            {
                MessageBox.Show("Please correct invalid characters");
                textBoxFolder.Focus();
                textBoxFolder.SelectAll();
                return;
            }

            if (!_vobSub)
            {
                FolderName = folderName;
                DialogResult = DialogResult.OK;
                return;
            }

            if (folderName.Length >= 0 && _vobSub)
            {
                try
                {
                    string fullName = Configuration.VobSubCompareDirectory + folderName;
                    Directory.CreateDirectory(fullName);
                    FolderName = folderName;
                    DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void TextBoxFolderKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ButtonOkClick(null, null);
        }
    }
}
