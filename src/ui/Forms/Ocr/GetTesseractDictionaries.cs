using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Forms.Options;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class GetTesseractDictionaries : Form
    {
        private string _dictionaryFileName;
        internal string ChosenLanguage { get; private set; }
        private readonly List<TesseractDictionary> _dictionaries;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public GetTesseractDictionaries()
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
            _dictionaries = TesseractDictionary.List().OrderBy(p => p.Name).ToList();
            LoadDictionaryList();
            comboBoxDictionaries.UsePopupWindow = true;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void LoadDictionaryList()
        {
            var languageFilter = new List<CultureInfo>();
            var useAllLanguages = string.IsNullOrEmpty(Configuration.Settings.General.DefaultLanguages);
            if (!useAllLanguages)
            {
                languageFilter = Utilities.GetSubtitleLanguageCultures(true).ToList();
            }

            var dictionaries = new List<TesseractDictionary>();
            comboBoxDictionaries.BeginUpdate();
            comboBoxDictionaries.Items.Clear();
            for (var i = 0; i < _dictionaries.Count; i++)
            {
                var d = _dictionaries[i];
                if (!string.IsNullOrEmpty(d.Url))
                {
                    if (useAllLanguages || IsInLanguageFilter(d.Name, d.Code, languageFilter))
                    {
                        dictionaries.Add(d);
                    }
                }
            }

            comboBoxDictionaries.Items.AddRange(dictionaries.Count == 0 ? _dictionaries.ToArray<object>() : dictionaries.ToArray<object>());
            if (comboBoxDictionaries.Items.Count > 0)
            {
                comboBoxDictionaries.Items.Add(LanguageSettings.Current.General.ChangeLanguageFilter);
            }

            for (var i = 0; i < comboBoxDictionaries.Items.Count; i++)
            {
                if (comboBoxDictionaries.Items[i] is string n && n == "English")
                {
                    comboBoxDictionaries.SelectedIndex = i;
                    break;
                }
            }

            if (comboBoxDictionaries.SelectedIndex < 0)
            {
                comboBoxDictionaries.SelectedIndex = 0;
            }

            comboBoxDictionaries.EndUpdate();
        }

        private static bool IsInLanguageFilter(string name, string code, List<CultureInfo> languageFilter)
        {
            foreach (var cultureInfo in languageFilter)
            {
                if (!string.IsNullOrEmpty(name) &&
                    cultureInfo.EnglishName.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(code) &&
                    cultureInfo.ThreeLetterISOLanguageName.Contains(code, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
            var dictionary = comboBoxDictionaries.Items[comboBoxDictionaries.SelectedIndex] as TesseractDictionary;
            if (dictionary == null)
            {
                return;
            }

            var url = dictionary.Url;
            try
            {
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                buttonOK.Enabled = false;
                buttonDownload.Enabled = false;
                comboBoxDictionaries.Enabled = false;
                Refresh();
                Cursor = Cursors.WaitCursor;

                ChosenLanguage = dictionary.ToString();

                using (var httpClient = DownloaderFactory.MakeHttpClient())
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

            var dictionaryFolder = Configuration.TesseractDataDirectory;
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
                var buffer = new byte[1024];
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

            var dictionaryFolder = Configuration.TesseractDataDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            var index = comboBoxDictionaries.SelectedIndex;
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

        private void comboBoxDictionaries_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxDictionaries.SelectedIndex >= 0 && comboBoxDictionaries.Text == LanguageSettings.Current.General.ChangeLanguageFilter)
            {
                using (var form = new DefaultLanguagesChooser(Configuration.Settings.General.DefaultLanguages))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        Configuration.Settings.General.DefaultLanguages = form.DefaultLanguages;
                    }
                }

                LoadDictionaryList();
            }
        }
    }
}
