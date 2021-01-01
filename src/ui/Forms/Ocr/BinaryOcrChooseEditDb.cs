using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using Nikse.SubtitleEdit.Logic.Ocr.Binary;
using System;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class BinaryOcrChooseEditDb : Form
    {
        public string ImageCompareDatabaseName { get; private set; }

        public BinaryOcrChooseEditDb(string binaryImageDb)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            buttonNewCharacterDatabase.Text = LanguageSettings.Current.VobSubOcr.New;
            buttonEditCharacterDatabase.Text = LanguageSettings.Current.VobSubOcr.Edit;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            labelImageDatabase.Text = LanguageSettings.Current.VobSubOcr.ImageDatabase;
            linkLabelOpenDictionaryFolder.Text = LanguageSettings.Current.GetDictionaries.OpenDictionariesFolder;
            Text = LanguageSettings.Current.VobSubOcr.ImageDatabase;

            var imageCompareDbName = string.Empty;
            var nOcrDbName = string.Empty;
            ImageCompareDatabaseName = binaryImageDb;
            if (!string.IsNullOrEmpty(binaryImageDb))
            {
                var parts = binaryImageDb.Split('+');
                if (parts.Length > 0)
                {
                    imageCompareDbName = parts[0];
                    if (parts.Length > 1)
                    {
                        nOcrDbName = parts[1];
                    }
                }
            }

            comboBoxNOcrLanguage.Items.Clear();
            comboBoxNOcrLanguage.Items.Add(string.Empty);
            foreach (string s in NOcrDb.GetDatabases())
            {
                comboBoxNOcrLanguage.Items.Add(s);
                if (s == nOcrDbName)
                {
                    comboBoxNOcrLanguage.SelectedIndex = comboBoxNOcrLanguage.Items.Count - 1;
                }
            }

            comboBoxCharacterDatabase.Items.Clear();
            foreach (string s in BinaryOcrDb.GetDatabases())
            {
                comboBoxCharacterDatabase.Items.Add(s);
                if (s == imageCompareDbName)
                {
                    comboBoxCharacterDatabase.SelectedIndex = comboBoxCharacterDatabase.Items.Count - 1;
                }
            }
        }

        private void BinaryOcrChooseEditDb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            ImageCompareDatabaseName = comboBoxCharacterDatabase.Text;
            if (string.IsNullOrEmpty(ImageCompareDatabaseName))
            {
                return;
            }

            var nOcrDatabase = comboBoxNOcrLanguage.Text;
            if (!string.IsNullOrEmpty(nOcrDatabase))
            {
                ImageCompareDatabaseName += "+" + nOcrDatabase;
            }

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonNewCharacterDatabase_Click(object sender, EventArgs e)
        {
            using (var newFolder = new VobSubOcrNewFolder(false))
            {
                if (newFolder.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        string fileName = Path.Combine(Configuration.OcrDirectory, newFolder.FolderName + ".db");
                        if (File.Exists(fileName))
                        {
                            MessageBox.Show("OCR db already exists!");
                            return;
                        }

                        comboBoxCharacterDatabase.Items.Add(newFolder.FolderName);
                        comboBoxCharacterDatabase.SelectedIndex = comboBoxCharacterDatabase.Items.Count - 1;
                        var binaryOcrDb = new BinaryOcrDb(fileName);
                        binaryOcrDb.Save();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }
        }

        private void buttonEditCharacterDatabase_Click(object sender, EventArgs e)
        {
            var fileName = Path.Combine(Configuration.OcrDirectory, comboBoxCharacterDatabase.SelectedItem + ".db");
            var binaryOcrDb = new BinaryOcrDb(fileName);
            binaryOcrDb.LoadCompareImages();
            using (var formVobSubEditCharacters = new VobSubEditCharacters(null, binaryOcrDb))
            {
                if (formVobSubEditCharacters.ShowDialog() == DialogResult.OK)
                {
                    binaryOcrDb.Save();
                }
            }
        }

        private void linkLabelOpenDictionaryFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string dictionaryFolder = Configuration.OcrDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            UiUtil.OpenFolder(dictionaryFolder);
        }
    }
}
