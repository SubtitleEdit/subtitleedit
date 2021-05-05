using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
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
        private List<string> _englishNames = new List<string>();
        private string _xmlName;
        private string _downloadLink;
        private int _testAllIndex = -1;

        public string SelectedEnglishName { get; private set; }
        public string LastDownload { get; private set; }

        public GetDictionaries()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.GetDictionaries.Title;
            labelDescription1.Text = LanguageSettings.Current.GetDictionaries.DescriptionLine1;
            labelDescription2.Text = LanguageSettings.Current.GetDictionaries.DescriptionLine2;
            linkLabelOpenDictionaryFolder.Text = LanguageSettings.Current.GetDictionaries.OpenDictionariesFolder;
            labelChooseLanguageAndClickDownload.Text = LanguageSettings.Current.GetDictionaries.ChooseLanguageAndClickDownload;
            buttonDownload.Text = LanguageSettings.Current.GetDictionaries.Download;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            labelPleaseWait.Text = string.Empty;

            LoadDictionaryList("Nikse.SubtitleEdit.Resources.HunspellDictionaries.xml.gz");
            FixLargeFonts();
#if DEBUG
            buttonDownloadAll.Visible = true; // For testing all download links
#endif
        }

        private void LoadDictionaryList(string xmlResourceName)
        {
            _dictionaryDownloadLinks = new List<string>();
            _descriptions = new List<string>();
            _englishNames = new List<string>();
            _xmlName = xmlResourceName;
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream(_xmlName);
            if (stream != null)
            {
                comboBoxDictionaries.Items.Clear();
                var doc = new XmlDocument();
                using (var rdr = new StreamReader(stream))
                using (var zip = new GZipStream(rdr.BaseStream, CompressionMode.Decompress))
                using (var reader = XmlReader.Create(zip, new XmlReaderSettings { IgnoreProcessingInstructions = true }))
                {
                    doc.Load(reader);
                }

                foreach (XmlNode node in doc.DocumentElement.SelectNodes("Dictionary"))
                {
                    string englishName = node.SelectSingleNode("EnglishName").InnerText;
                    string nativeName = node.SelectSingleNode("NativeName").InnerText;
                    string downloadLink = node.SelectSingleNode("DownloadLink").InnerText;

                    string description = string.Empty;
                    if (node.SelectSingleNode("Description") != null)
                    {
                        description = node.SelectSingleNode("Description").InnerText;
                    }

                    if (!string.IsNullOrEmpty(downloadLink))
                    {
                        string name = englishName;
                        if (!string.IsNullOrEmpty(nativeName))
                        {
                            name += " - " + nativeName;
                        }

                        comboBoxDictionaries.Items.Add(name);
                        _dictionaryDownloadLinks.Add(downloadLink);
                        _descriptions.Add(description);
                        _englishNames.Add(englishName);
                    }
                }
                comboBoxDictionaries.SelectedIndex = 0;
            }
            comboBoxDictionaries.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBoxDictionaries.AutoCompleteMode = AutoCompleteMode.Append;
        }

        private void FixLargeFonts()
        {
            if (labelDescription1.Left + labelDescription1.Width + 5 > Width)
            {
                Width = labelDescription1.Left + labelDescription1.Width + 5;
            }

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void FormGetDictionaries_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#spellcheck");
                e.SuppressKeyPress = true;
            }
        }

        private void LinkLabel4LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            UiUtil.OpenFolder(dictionaryFolder);
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            try
            {
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                buttonOK.Enabled = false;
                buttonDownload.Enabled = false;
                buttonDownloadAll.Enabled = false;
                comboBoxDictionaries.Enabled = false;
                Refresh();
                Cursor = Cursors.WaitCursor;

                int index = comboBoxDictionaries.SelectedIndex;
                _downloadLink = _dictionaryDownloadLinks[index];
                string url = _dictionaryDownloadLinks[index];
                SelectedEnglishName = _englishNames[index];

                var wc = new WebClient { Proxy = Utilities.GetProxy() };
                wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                wc.DownloadProgressChanged += (o, args) =>
                {
                    labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait + "  " + args.ProgressPercentage + "%";
                };
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
                MessageBox.Show("Unable to download " + _downloadLink + Environment.NewLine +
                                "Switching host - please re-try!");
                LoadDictionaryList("Nikse.SubtitleEdit.Resources.HunspellBackupDictionaries.xml.gz");
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                buttonDownloadAll.Enabled = true;
                comboBoxDictionaries.Enabled = true;
                Cursor = Cursors.Default;
                return;
            }

            if (e.Error != null)
            {
                MessageBox.Show(LanguageSettings.Current.GetTesseractDictionaries.DownloadFailed + Environment.NewLine +
                                Environment.NewLine +
                                e.Error.Message);
                DialogResult = DialogResult.Cancel;
                return;
            }

            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

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
            MessageBox.Show(string.Format(LanguageSettings.Current.GetDictionaries.XDownloaded, comboBoxDictionaries.Items[index]));
        }

        private void ExtractDic(string dictionaryFolder, ZipExtractor zip, List<ZipExtractor.ZipFileEntry> dir, ref bool found)
        {
            foreach (ZipExtractor.ZipFileEntry entry in dir)
            {
                if (entry.FilenameInZip.EndsWith(".dic", StringComparison.OrdinalIgnoreCase) || entry.FilenameInZip.EndsWith(".aff", StringComparison.OrdinalIgnoreCase))
                {
                    string fileName = Path.GetFileName(entry.FilenameInZip);

                    // French fix
                    if (fileName.StartsWith("fr-moderne", StringComparison.Ordinal))
                    {
                        fileName = fileName.Replace("fr-moderne", "fr_FR");
                    }

                    // German fix
                    if (fileName.StartsWith("de_DE_frami", StringComparison.Ordinal))
                    {
                        fileName = fileName.Replace("de_DE_frami", "de_DE");
                    }

                    // Russian fix
                    if (fileName.StartsWith("russian-aot", StringComparison.Ordinal))
                    {
                        fileName = fileName.Replace("russian-aot", "ru_RU");
                    }

                    string path = Path.Combine(dictionaryFolder, fileName);
                    zip.ExtractFile(entry, path);

                    found = true;

                    LastDownload = fileName;
                }
            }
        }

        private void comboBoxDictionaries_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = comboBoxDictionaries.SelectedIndex;
            labelPleaseWait.Text = _descriptions[index];
        }

        private void buttonDownloadAll_Click(object sender, EventArgs e)
        {
            _testAllIndex = comboBoxDictionaries.SelectedIndex - 1;
            if (_testAllIndex < -1)
            {
                _testAllIndex = -1;
            }

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
            else
            {
                LoadDictionaryList("Nikse.SubtitleEdit.Resources.HunspellBackupDictionaries.xml.gz");
            }
        }
    }
}
