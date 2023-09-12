﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class GetTesseract302Dictionaries : Form
    {
        private List<string> _dictionaryDownloadLinks = new List<string>();
        private string _xmlName;
        private string _dictionaryFileName;
        internal string ChosenLanguage { get; private set; }
        private readonly CancellationTokenSource _cancellationTokenSource;

        public GetTesseract302Dictionaries()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.GetTesseractDictionaries.Title;
            labelDescription1.Text = LanguageSettings.Current.GetTesseractDictionaries.DescriptionLine1;
            linkLabelOpenDictionaryFolder.Text = LanguageSettings.Current.GetTesseractDictionaries.OpenDictionariesFolder;
            labelChooseLanguageAndClickDownload.Text = LanguageSettings.Current.GetTesseractDictionaries.ChooseLanguageAndClickDownload;
            buttonDownload.Text = LanguageSettings.Current.GetTesseractDictionaries.Download;
            labelPleaseWait.Text = string.Empty;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            LoadDictionaryList("Nikse.SubtitleEdit.Resources.TesseractDictionaries.xml.gz");
            FixLargeFonts();
            _cancellationTokenSource = new CancellationTokenSource();
            comboBoxDictionaries.UsePopupWindow = true;
        }

        private void LoadDictionaryList(string xmlRessourceName)
        {
            _dictionaryDownloadLinks = new List<string>();
            _xmlName = xmlRessourceName;
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            Stream strm = asm.GetManifestResourceStream(_xmlName);
            if (strm != null)
            {
                comboBoxDictionaries.Items.Clear();
                XmlDocument doc = new XmlDocument();
                using (var rdr = new StreamReader(strm))
                using (var zip = new GZipStream(rdr.BaseStream, CompressionMode.Decompress))
                {
                    byte[] data = new byte[195000];
                    int bytesRead = zip.Read(data, 0, data.Length);
                    var s = System.Text.Encoding.UTF8.GetString(data, 0, bytesRead).Trim();
                    try
                    {
                        doc.LoadXml(s);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }

                foreach (XmlNode node in doc.DocumentElement.SelectNodes("Dictionary"))
                {
                    string englishName = node.SelectSingleNode("EnglishName").InnerText;
                    string downloadLink = node.SelectSingleNode("DownloadLink").InnerText;
                    if (!string.IsNullOrEmpty(downloadLink))
                    {
                        string name = englishName;

                        comboBoxDictionaries.Items.Add(name);
                        _dictionaryDownloadLinks.Add(downloadLink);
                    }
                    comboBoxDictionaries.SelectedIndex = 0;
                }
            }
        }

        private void FixLargeFonts()
        {
            if (labelDescription1.Left + labelDescription1.Width + 5 > Width)
            {
                Width = labelDescription1.Left + labelDescription1.Width + 5;
            }

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            buttonOK.Enabled = false;
            buttonDownload.Enabled = false;
            comboBoxDictionaries.Enabled = false;
            Refresh();
            Cursor = Cursors.WaitCursor;

            int index = comboBoxDictionaries.SelectedIndex;
            string url = _dictionaryDownloadLinks[index];
            ChosenLanguage = comboBoxDictionaries.Items[index].ToString();

            try
            {
                var httpClient = HttpClientFactory.MakeHttpClient();
                using (var downloadStream = new MemoryStream())
                {
                    var downloadTask = httpClient.DownloadAsync(url, downloadStream, new Progress<float>((progress) =>
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

                    if (url.EndsWith(".traineddata", StringComparison.OrdinalIgnoreCase) || url.EndsWith(".traineddata?raw=true", StringComparison.OrdinalIgnoreCase))
                    {
                        _dictionaryFileName = Path.GetFileName(url);
                        DownloadTrainedDataCompleted(downloadStream);
                    }
                    else
                    {
                        DownloadDataCompleted(downloadStream);
                    }
                }
            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                comboBoxDictionaries.Enabled = true;
                Cursor = Cursors.Default;
                MessageBox.Show($"Unable to download {url}!" + Environment.NewLine + Environment.NewLine +
                                exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
                DialogResult = DialogResult.Cancel;
            }
        }

        private void DownloadDataCompleted(Stream downloadStream)
        {
            if (downloadStream.Length == 0)
            {
                throw new Exception("No content downloaded - missing file or no internet connection!");
            }

            var dictionaryFolder = Configuration.Tesseract302DataDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            var index = comboBoxDictionaries.SelectedIndex;

            downloadStream.Position = 0;
            var tempFileName = FileUtil.GetTempFileName(".tar");
            using (var fs = new FileStream(tempFileName, FileMode.Create))
            using (var zip = new GZipStream(downloadStream, CompressionMode.Decompress))
            {
                byte[] buffer = new byte[1024];
                int nRead;
                while ((nRead = zip.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, nRead);
                }
            }

            using (var tr = new TarReader(tempFileName))
            {
                foreach (var th in tr.Files.Where(p => !p.IsFolder && p.FileSizeInBytes > 0))
                {
                    var fn = Path.Combine(dictionaryFolder, Path.GetFileName(th.FileName.Trim()));
                    th.WriteData(fn);
                }
            }
            File.Delete(tempFileName);

            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            buttonOK.Enabled = true;
            buttonDownload.Enabled = true;
            comboBoxDictionaries.Enabled = true;
            MessageBox.Show(string.Format(LanguageSettings.Current.GetDictionaries.XDownloaded, comboBoxDictionaries.Items[index]));
        }

        private void DownloadTrainedDataCompleted(Stream downloadStream)
        {
            if (downloadStream.Length == 0)
            {
                throw new Exception("No content downloaded - missing file or no internet connection!");
            }

            var dictionaryFolder = Configuration.Tesseract302DataDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            int index = comboBoxDictionaries.SelectedIndex;
            using (var fs = new FileStream(Path.Combine(dictionaryFolder, _dictionaryFileName), FileMode.Create))
            {
                downloadStream.Position = 0;
                var buffer = new byte[1024];
                int nRead;
                while ((nRead = downloadStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, nRead);
                }
            }
            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            buttonOK.Enabled = true;
            buttonDownload.Enabled = true;
            comboBoxDictionaries.Enabled = true;
            MessageBox.Show(string.Format(LanguageSettings.Current.GetDictionaries.XDownloaded, comboBoxDictionaries.Items[index]));
        }

        private void linkLabelOpenDictionaryFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string dictionaryFolder = Configuration.Tesseract302DataDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            UiUtil.OpenFolder(dictionaryFolder);
        }

        private void GetTesseractDictionaries_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#importvobsub");
                e.SuppressKeyPress = true;
            }
        }
    }
}
