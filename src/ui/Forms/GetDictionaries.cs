using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GetDictionaries : Form
    {
        private class DictionaryItem
        {
            public string EnglishName { get; set; }
            public string NativeName { get; set; }
            public string Description { get; set; }
            public string DownloadLink { get; set; }
            public string DisplayText { get; set; }
            public override string ToString() => DisplayText;
        }

        private string _xmlName;
        private string _downloadLink;
        private int _testAllIndex = -1;
        private readonly CancellationTokenSource _cancellationTokenSource;

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
            _cancellationTokenSource = new CancellationTokenSource();
#if DEBUG
            buttonDownloadAll.Visible = true; // For testing all download links
#endif
        }

        private void LoadDictionaryList(string xmlResourceName)
        {
            _xmlName = xmlResourceName;
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream(_xmlName);
            if (stream != null)
            {
                var dictionaryItems = new List<DictionaryItem>();
                var doc = new XmlDocument();
                using (var zip = new GZipStream(stream, CompressionMode.Decompress))
                using (var reader = XmlReader.Create(zip, new XmlReaderSettings { IgnoreProcessingInstructions = true }))
                {
                    doc.Load(reader);
                }

                comboBoxDictionaries.BeginUpdate();
                comboBoxDictionaries.Items.Clear();
                foreach (XmlNode node in doc.DocumentElement.SelectNodes("Dictionary"))
                {
                    var englishName = node.SelectSingleNode("EnglishName").InnerText;
                    var nativeName = node.SelectSingleNode("NativeName").InnerText;
                    var downloadLink = node.SelectSingleNode("DownloadLink").InnerText;

                    var description = string.Empty;
                    if (node.SelectSingleNode("Description") != null)
                    {
                        description = node.SelectSingleNode("Description").InnerText;
                    }

                    if (!string.IsNullOrEmpty(downloadLink))
                    {
                        dictionaryItems.Add(new DictionaryItem
                        {
                            EnglishName = englishName,
                            NativeName = nativeName,
                            Description = description,
                            DownloadLink = downloadLink
                        });
                    }
                }

                // ensure ellipses suffix on text overlaps
                using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    double comboboxWidth = comboBoxDictionaries.DropDownWidth;
                    // format display value
                    foreach (var dictionaryItem in dictionaryItems)
                    {
                        var text = $"{dictionaryItem.EnglishName}{(string.IsNullOrEmpty(dictionaryItem.NativeName) ? "" : $" - {dictionaryItem.NativeName}")}";
                        var width = graphics.MeasureString(text, comboBoxDictionaries.Font).Width;
                        var displayText = text;
                        if (width > comboboxWidth)
                        {
                            double pixelChar = width / text.Length;
                            var charCount = (int)Math.Floor(comboboxWidth / pixelChar);
                            displayText = text.Substring(0, charCount - 3) + "...";
                        }

                        dictionaryItem.DisplayText = displayText;
                    }
                }

                // ReSharper disable once CoVariantArrayConversion
                comboBoxDictionaries.Items.AddRange(dictionaryItems.ToArray());
                comboBoxDictionaries.EndUpdate();
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
            var item = (DictionaryItem)comboBoxDictionaries.SelectedItem;
            _downloadLink = item.DownloadLink;
            SelectedEnglishName = item.EnglishName;
            try
            {
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                buttonOK.Enabled = false;
                buttonDownload.Enabled = false;
                buttonDownloadAll.Enabled = false;
                comboBoxDictionaries.Enabled = false;
                Refresh();
                Cursor = Cursors.WaitCursor;

                var httpClient = DownloaderFactory.MakeHttpClient();
                using (var downloadStream = new MemoryStream())
                {
                    var downloadTask = httpClient.DownloadAsync(_downloadLink, downloadStream, new Progress<float>((progress) =>
                    {
                        var pct = (int)Math.Round(progress * 100.0, MidpointRounding.AwayFromZero);
                        labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait + "  " + pct + "%";
                    }), _cancellationTokenSource.Token);

                    while (!downloadTask.IsCompleted && !downloadTask.IsCanceled)
                    {
                        Application.DoEvents();
                    }

                    if (downloadTask.IsCanceled)
                    {
                        DialogResult = DialogResult.Cancel;
                        labelPleaseWait.Refresh();
                        return;
                    }

                    CompleteDownload(downloadStream);
                }
            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                buttonDownloadAll.Enabled = true;
                comboBoxDictionaries.Enabled = true;
                Cursor = Cursors.Default;
                MessageBox.Show($"Unable to download {_downloadLink}!" + Environment.NewLine + Environment.NewLine +
                                exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
                DialogResult = DialogResult.Cancel;
            }
        }

        private void CompleteDownload(MemoryStream downloadStream)
        {
            if (downloadStream.Length == 0)
            {
                throw new Exception("No content downloaded - missing file or no internet connection!");
            }

            Cursor = Cursors.Default;

            var dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            var index = comboBoxDictionaries.SelectedIndex;

            using (var zip = ZipExtractor.Open(downloadStream))
            {
                var dir = zip.ReadCentralDir();
                // Extract dic/aff files in dictionary folder
                var found = false;
                ExtractDic(dictionaryFolder, zip, dir, ref found);

                if (!found) // check zip inside zip
                {
                    foreach (var entry in dir)
                    {
                        if (entry.FilenameInZip.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var innerMs = new MemoryStream())
                            {
                                zip.ExtractFile(entry, innerMs);
                                var innerZip = ZipExtractor.Open(innerMs);
                                var innerDir = innerZip.ReadCentralDir();
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
            foreach (var entry in dir)
            {
                if (entry.FilenameInZip.EndsWith(".dic", StringComparison.OrdinalIgnoreCase) || entry.FilenameInZip.EndsWith(".aff", StringComparison.OrdinalIgnoreCase))
                {
                    var fileName = Path.GetFileName(entry.FilenameInZip);

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

                    var path = Path.Combine(dictionaryFolder, fileName);
                    zip.ExtractFile(entry, path);

                    found = true;

                    LastDownload = fileName;
                }
            }
        }

        private void comboBoxDictionaries_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = (DictionaryItem)comboBoxDictionaries.SelectedItem;
            labelPleaseWait.Text = item.Description;
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
