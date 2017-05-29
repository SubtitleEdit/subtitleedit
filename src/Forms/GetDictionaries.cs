using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GetDictionaries : Form
    {
        private List<string> _dictionaryDownloadLinks = new List<string>();
        private List<string> _descriptions = new List<string>();
        private string _xmlName;
        private int _testAllIndex = -1;

        public GetDictionaries()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.GetDictionaries.Title;
            labelDescription1.Text = Configuration.Settings.Language.GetDictionaries.DescriptionLine1;
            labelDescription2.Text = Configuration.Settings.Language.GetDictionaries.DescriptionLine2;
            linkLabelOpenDictionaryFolder.Text = Configuration.Settings.Language.GetDictionaries.OpenDictionariesFolder;
            labelChooseLanguageAndClickDownload.Text = Configuration.Settings.Language.GetDictionaries.ChooseLanguageAndClickDownload;
            buttonDownload.Text = Configuration.Settings.Language.GetDictionaries.Download;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            labelPleaseWait.Text = string.Empty;

            FixLargeFonts();
#if DEBUG
            buttonDownloadAll.Visible = true; // For testing all download links
#endif

            // initialize combobox providers

            foreach (DictionaryProvider dicProvider in GetProviders())
            {
                comboBoxProviders.Items.Add(dicProvider);
            }
        }

        private static IList<DictionaryProvider> GetProviders()
        {
            return new List<DictionaryProvider>
            {
                new LibreOfficeProvider("Nikse.SubtitleEdit.Resources.HunspellDictionaries.xml.gz"),
                new GitHubProvider("Nikse.SubtitleEdit.Resources.HunspellDictionaries-github.xml.gz")
            };
        }

        private void FixLargeFonts()
        {
            if (labelDescription1.Left + labelDescription1.Width + 5 > Width)
                Width = labelDescription1.Left + labelDescription1.Width + 5;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void FormGetDictionaries_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#spellcheck");
                e.SuppressKeyPress = true;
            }
        }

        private void LinkLabel4LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
                Directory.CreateDirectory(dictionaryFolder);

            System.Diagnostics.Process.Start(dictionaryFolder);
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            try
            {
                labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait;
                buttonOK.Enabled = false;
                buttonDownload.Enabled = false;
                buttonDownloadAll.Enabled = false;
                comboBoxDictionaries.Enabled = false;
                this.Refresh();
                Cursor = Cursors.WaitCursor;

                int index = comboBoxDictionaries.SelectedIndex;
                string url = _dictionaryDownloadLinks[index];

                var wc = new WebClient { Proxy = Utilities.GetProxy() };
                wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                wc.DownloadDataAsync(new Uri(url));
            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                buttonDownloadAll.Enabled = true;
                comboBoxDictionaries.Enabled = true;
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
            }
        }

        private void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            Cursor = Cursors.Default;
            if (e.Error != null && _xmlName == "Nikse.SubtitleEdit.Resources.HunspellDictionaries.xml.gz")
            {
                MessageBox.Show("Unable to connect to extensions.services.openoffice.org... Switching host - please re-try!");
                // TODO: LoadDictionaryList("Nikse.SubtitleEdit.Resources.HunspellBackupDictionaries.xml.gz");
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                buttonDownloadAll.Enabled = true;
                comboBoxDictionaries.Enabled = true;
                Cursor = Cursors.Default;
                return;
            }
            else if (e.Error != null)
            {
                MessageBox.Show(Configuration.Settings.Language.GetTesseractDictionaries.DownloadFailed + Environment.NewLine +
                                Environment.NewLine +
                                e.Error.Message);
                DialogResult = DialogResult.Cancel;
                return;
            }

            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
                Directory.CreateDirectory(dictionaryFolder);

            int index = comboBoxDictionaries.SelectedIndex;

            using (var ms = new MemoryStream(e.Result))
            using (ZipExtractor zip = ZipExtractor.Open(ms))
            {
                List<ZipExtractor.ZipFileEntry> dir = zip.ReadCentralDir();
                // Extract dic/aff files in dictionary folder
                bool found = false;
                ExtractDic(dictionaryFolder, zip, dir, ref found);

                if (!found) // check zip inside zip
                {
                    foreach (ZipExtractor.ZipFileEntry entry in dir)
                    {
                        if (entry.FilenameInZip.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var innerMs = new MemoryStream())
                            {
                                zip.ExtractFile(entry, innerMs);
                                ZipExtractor innerZip = ZipExtractor.Open(innerMs);
                                List<ZipExtractor.ZipFileEntry> innerDir = innerZip.ReadCentralDir();
                                ExtractDic(dictionaryFolder, innerZip, innerDir, ref found);
                            }
                        }
                    }
                }
            }

            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            buttonOK.Enabled = true;
            buttonDownload.Enabled = true;
            buttonDownloadAll.Enabled = true;
            comboBoxDictionaries.Enabled = true;
            if (_testAllIndex >= 0)
            {
                DownloadNext();
                return;
            }
            MessageBox.Show(string.Format(Configuration.Settings.Language.GetDictionaries.XDownloaded, comboBoxDictionaries.Items[index]));
        }

        private static void ExtractDic(string dictionaryFolder, ZipExtractor zip, List<ZipExtractor.ZipFileEntry> dir, ref bool found)
        {
            foreach (ZipExtractor.ZipFileEntry entry in dir)
            {
                if (entry.FilenameInZip.EndsWith(".dic", StringComparison.OrdinalIgnoreCase) || entry.FilenameInZip.EndsWith(".aff", StringComparison.OrdinalIgnoreCase))
                {
                    string fileName = Path.GetFileName(entry.FilenameInZip);

                    // French fix
                    if (fileName.StartsWith("fr-moderne", StringComparison.Ordinal))
                        fileName = fileName.Replace("fr-moderne", "fr_FR");

                    // German fix
                    if (fileName.StartsWith("de_DE_frami", StringComparison.Ordinal))
                        fileName = fileName.Replace("de_DE_frami", "de_DE");

                    // Russian fix
                    if (fileName.StartsWith("russian-aot", StringComparison.Ordinal))
                        fileName = fileName.Replace("russian-aot", "ru_RU");

                    string path = Path.Combine(dictionaryFolder, fileName);
                    zip.ExtractFile(entry, path);

                    found = true;
                }
            }
        }

        private void comboBoxDictionaries_SelectedIndexChanged(object sender, EventArgs e)
        {
            var dicInfo = (DictionaryInfo)comboBoxDictionaries.SelectedItem;
            labelPleaseWait.Text = dicInfo.Description;
        }

        private void buttonDownloadAll_Click(object sender, EventArgs e)
        {
            _testAllIndex = -1;
            DownloadNext();
        }

        private void DownloadNext()
        {
            _testAllIndex++;
            if (_testAllIndex < comboBoxDictionaries.Items.Count)
            {
                comboBoxDictionaries.SelectedIndex = _testAllIndex;
                buttonDownload_Click(null, null);
            }
        }

        private void comboBoxProviders_SelectedIndexChanged(object sender, EventArgs e)
        {
            DictionaryProvider dicProvider = (DictionaryProvider)comboBoxProviders.SelectedItem;
            if (dicProvider == null)
            {
                throw new InvalidOperationException("Invalid provider selected!");
            }
            comboBoxDictionaries.Items.Clear();
            foreach (DictionaryInfo dicInfo in dicProvider.Dictionaries)
            {
                comboBoxDictionaries.Items.Add(dicInfo);
            }
            if (comboBoxDictionaries.Items.Count > 0)
            {
                comboBoxDictionaries.SelectedIndex = 0;
            }
        }
    }
}
