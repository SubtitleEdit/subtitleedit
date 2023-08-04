using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class GetTesseractDictionaries : Form
    {
        private string _dictionaryFileName;
        internal string ChosenLanguage { get; private set; }
        private readonly List<TesseractDictionary> _dictionaries;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public GetTesseractDictionaries(bool first)
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
            FixLargeFonts();
            _dictionaries = TesseractDictionary.List().OrderBy(p=>p.Name).ToList();
            LoadDictionaryList(first);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void LoadDictionaryList(bool first)
        {
            comboBoxDictionaries.BeginUpdate();
            comboBoxDictionaries.Items.Clear();
            for (int i = 0; i < _dictionaries.Count; i++)
            {
                TesseractDictionary d = _dictionaries[i];
                if (!string.IsNullOrEmpty(d.Url))
                {
                    comboBoxDictionaries.Items.Add(d);
                    if (first && d.Name == "English")
                    {
                        comboBoxDictionaries.SelectedIndex = i;
                    }
                }
            }
            if (comboBoxDictionaries.SelectedIndex < 0)
            {
                comboBoxDictionaries.SelectedIndex = 0;
            }
            comboBoxDictionaries.EndUpdate();
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
            int index = comboBoxDictionaries.SelectedIndex;
            string url = _dictionaries[index].Url;
            try
            {
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                buttonOK.Enabled = false;
                buttonDownload.Enabled = false;
                comboBoxDictionaries.Enabled = false;
                Refresh();
                Cursor = Cursors.WaitCursor;

                ChosenLanguage = comboBoxDictionaries.Items[index].ToString();

                var httpClient = DownloaderFactory.MakeHttpClient();
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
            }
        }

        private void DownloadDataCompleted(Stream downloadStream)
        {
            if (downloadStream.Length == 0)
            {
                throw new Exception("No content downloaded - missing file or no internet connection!");
            }

            string dictionaryFolder = Configuration.TesseractDataDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            int index = comboBoxDictionaries.SelectedIndex;
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
                foreach (var th in tr.Files)
                {
                    string fn = Path.Combine(dictionaryFolder, Path.GetFileName(th.FileName.Trim()));
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

            string dictionaryFolder = Configuration.TesseractDataDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            int index = comboBoxDictionaries.SelectedIndex;

            using (var fs = new FileStream(Path.Combine(dictionaryFolder, _dictionaryFileName), FileMode.Create))
            {
                downloadStream.Position = 0;
                byte[] buffer = new byte[1024];
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
            MessageBox.Show(string.Format(LanguageSettings.Current.GetTesseractDictionaries.XDownloaded, comboBoxDictionaries.Items[index]));
        }

        private void linkLabelOpenDictionaryFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string dictionaryFolder = Configuration.TesseractDataDirectory;
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
