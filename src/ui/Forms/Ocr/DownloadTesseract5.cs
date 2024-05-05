using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class DownloadTesseract5 : Form
    {
        public const string TesseractDownloadUrl = "https://github.com/SubtitleEdit/support-files/releases/download/Tesseract533-2023-10-05/Tesseract533.zip";
        private readonly CancellationTokenSource _cancellationTokenSource;

        public DownloadTesseract5(string version)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.GetTesseractDictionaries.Download + " Tesseract " + version;
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            labelDescription1.Text = LanguageSettings.Current.GetTesseractDictionaries.Download + " Tesseract OCR";
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void DownloadTesseract5_Shown(object sender, EventArgs e)
        {
            try
            {
                Utilities.SetSecurityProtocol();
                using (var httpClient = DownloaderFactory.MakeHttpClient())
                using (var downloadStream = new MemoryStream())
                {
                    var downloadTask = httpClient.DownloadAsync(TesseractDownloadUrl, downloadStream, new Progress<float>((progress) =>
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
                Cursor = Cursors.Default;
                MessageBox.Show($"Unable to download {TesseractDownloadUrl}!" + Environment.NewLine + Environment.NewLine +
                                exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
                DialogResult = DialogResult.Cancel;
            }
        }

        private void CompleteDownload(Stream downloadStream)
        {
            if (downloadStream.Length == 0)
            {
                throw new Exception("No content downloaded - missing file or no internet connection!" + Environment.NewLine  + 
                                    $"For more info see: {Path.Combine(Configuration.DataDirectory, "error_log.txt")}");
            }

            var dictionaryFolder = Configuration.TesseractDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            using (var zip = ZipExtractor.Open(downloadStream))
            {
                var dir = zip.ReadCentralDir();
                foreach (var entry in dir)
                {
                    var path = Path.Combine(dictionaryFolder, entry.FilenameInZip);
                    zip.ExtractFile(entry, path);
                }
            }

            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            Cursor = Cursors.Default;
            DialogResult = DialogResult.OK;
        }
    }
}
