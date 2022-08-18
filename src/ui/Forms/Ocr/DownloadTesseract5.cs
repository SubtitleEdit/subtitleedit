using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class DownloadTesseract5 : Form
    {
        public const string TesseractDownloadUrl = "https://github.com/SubtitleEdit/support-files/raw/master/Tesseract520.tar.gz";
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
                var httpClient = HttpClientHelper.MakeHttpClient();
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
                throw new Exception("No content downloaded - missing file or no internet connection!");
            }

            var dictionaryFolder = Configuration.TesseractDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

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
                    string fn = Path.Combine(dictionaryFolder, th.FileName.Replace('/', Path.DirectorySeparatorChar));
                    if (th.IsFolder)
                    {
                        Directory.CreateDirectory(Path.Combine(dictionaryFolder, th.FileName.Replace('/', Path.DirectorySeparatorChar)));
                    }
                    else if (th.FileSizeInBytes > 0)
                    {
                        th.WriteData(fn);
                    }
                }
            }
            File.Delete(tempFileName);
            Cursor = Cursors.Default;
            DialogResult = DialogResult.OK;
        }
    }
}
