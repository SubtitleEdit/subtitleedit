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
using System.Xml;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class GetTesseract302Dictionaries : Form
    {
        private Dictionary<string, string> _dictionaryDownloadLinks = new Dictionary<string, string>();
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

        private void LoadDictionaryList(string xmlResourceName)
        {
            var languageFilter = new List<CultureInfo>();
            var useAllLanguages = string.IsNullOrEmpty(Configuration.Settings.General.DefaultLanguages);
            if (!useAllLanguages)
            {
                languageFilter = Utilities.GetSubtitleLanguageCultures(true).ToList();
            }

            _dictionaryDownloadLinks = new Dictionary<string, string>();
            _xmlName = xmlResourceName;
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream(_xmlName);
            var nameList = new List<string>();
            var nameListAll = new List<string>();
            if (stream != null)
            {
                comboBoxDictionaries.Items.Clear();
                var doc = new XmlDocument();
                using (var rdr = new StreamReader(stream))
                using (var zip = new GZipStream(rdr.BaseStream, CompressionMode.Decompress))
                {
                    var data = new byte[195000];
                    var bytesRead = zip.Read(data, 0, data.Length);
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
                    var englishName = node.SelectSingleNode("EnglishName").InnerText;
                    var downloadLink = node.SelectSingleNode("DownloadLink").InnerText;

                    if (!string.IsNullOrEmpty(downloadLink))
                    {
                        nameListAll.Add(englishName);

                        if (useAllLanguages || IsInLanguageFilter(englishName, languageFilter))
                        {
                            nameList.Add(englishName);
                        }

                        _dictionaryDownloadLinks.Add(englishName, downloadLink);
                    }
                }

                comboBoxDictionaries.Items.AddRange(nameList.Count == 0 ? nameListAll.ToArray<object>() : nameList.ToArray<object>());
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
            }
        }

        private static bool IsInLanguageFilter(string englishName, List<CultureInfo> languageFilter)
        {
            foreach (var cultureInfo in languageFilter)
            {
                if (!string.IsNullOrEmpty(englishName) &&
                    cultureInfo.EnglishName.Contains(englishName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(englishName) &&
                    cultureInfo.ThreeLetterISOLanguageName.Contains(englishName, StringComparison.OrdinalIgnoreCase))
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
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            buttonOK.Enabled = false;
            buttonDownload.Enabled = false;
            comboBoxDictionaries.Enabled = false;
            Refresh();
            Cursor = Cursors.WaitCursor;

            var language = comboBoxDictionaries.Items[comboBoxDictionaries.SelectedIndex].ToString();
            var url = _dictionaryDownloadLinks[language];
            ChosenLanguage = language;

            try
            {
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
            var dictionaryFolder = Configuration.Tesseract302DataDirectory;
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

                LoadDictionaryList("Nikse.SubtitleEdit.Resources.TesseractDictionaries.xml.gz");
            }
        }
    }
}
