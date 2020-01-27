using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class GetTesseractDictionaries : Form
    {
        private string _dictionaryFileName;
        internal string ChosenLanguage { get; private set; }
        private readonly List<TesseractDictionary> _dictionaries;

        public GetTesseractDictionaries(bool first)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = Configuration.Settings.Language.GetTesseractDictionaries.Title;
            labelDescription1.Text = Configuration.Settings.Language.GetTesseractDictionaries.DescriptionLine1;
            linkLabelOpenDictionaryFolder.Text = Configuration.Settings.Language.GetTesseractDictionaries.OpenDictionariesFolder;
            labelChooseLanguageAndClickDownload.Text = Configuration.Settings.Language.GetTesseractDictionaries.ChooseLanguageAndClickDownload;
            buttonDownload.Text = Configuration.Settings.Language.GetTesseractDictionaries.Download;
            labelPleaseWait.Text = string.Empty;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            FixLargeFonts();
            _dictionaries = TesseractDictionary.List();
            LoadDictionaryList(first);
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

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            try
            {
                labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait;
                buttonOK.Enabled = false;
                buttonDownload.Enabled = false;
                comboBoxDictionaries.Enabled = false;
                Refresh();
                Cursor = Cursors.WaitCursor;

                int index = comboBoxDictionaries.SelectedIndex;
                string url = _dictionaries[index].Url;
                ChosenLanguage = comboBoxDictionaries.Items[index].ToString();

                var wc = new WebClient { Proxy = Utilities.GetProxy() };
                if (url.EndsWith(".traineddata", StringComparison.OrdinalIgnoreCase) || url.EndsWith(".traineddata?raw=true", StringComparison.OrdinalIgnoreCase))
                {
                    _dictionaryFileName = Path.GetFileName(url);
                    wc.DownloadDataCompleted += wc_DownloadTrainedDataCompleted;
                }
                else
                {
                    wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                }
                wc.DownloadProgressChanged += (o, args) =>
                {
                    labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait + "  " + args.ProgressPercentage + "%";
                };
                wc.DownloadDataAsync(new Uri(url));
                Cursor = Cursors.Default;
            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                comboBoxDictionaries.Enabled = true;
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
            }
        }

        private void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(Configuration.Settings.Language.GetTesseractDictionaries.DownloadFailed);
                DialogResult = DialogResult.Cancel;
                return;
            }

            string dictionaryFolder = Configuration.TesseractDataDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            int index = comboBoxDictionaries.SelectedIndex;

            var tempFileName = FileUtil.GetTempFileName(".tar");
            using (var ms = new MemoryStream(e.Result))
            using (var fs = new FileStream(tempFileName, FileMode.Create))
            using (var zip = new GZipStream(ms, CompressionMode.Decompress))
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
            MessageBox.Show(string.Format(Configuration.Settings.Language.GetDictionaries.XDownloaded, comboBoxDictionaries.Items[index]));
        }

        private void wc_DownloadTrainedDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(Configuration.Settings.Language.GetTesseractDictionaries.DownloadFailed);
                DialogResult = DialogResult.Cancel;
                return;
            }

            string dictionaryFolder = Configuration.TesseractDataDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            int index = comboBoxDictionaries.SelectedIndex;

            using (var ms = new MemoryStream(e.Result))
            using (var fs = new FileStream(Path.Combine(dictionaryFolder, _dictionaryFileName), FileMode.Create))
            {
                ms.Position = 0;
                byte[] buffer = new byte[1024];
                int nRead;
                while ((nRead = ms.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, nRead);
                }
            }
            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            buttonOK.Enabled = true;
            buttonDownload.Enabled = true;
            comboBoxDictionaries.Enabled = true;
            MessageBox.Show(string.Format(Configuration.Settings.Language.GetTesseractDictionaries.XDownloaded, comboBoxDictionaries.Items[index]));
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
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#importvobsub");
                e.SuppressKeyPress = true;
            }
        }
    }
}
