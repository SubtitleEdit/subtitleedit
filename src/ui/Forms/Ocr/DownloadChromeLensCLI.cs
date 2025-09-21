using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Logic;
using SevenZipExtractor;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class DownloadChromeLensCLI : Form
    {
        public const string DownloadUrl = "https://github.com/timminator/chrome-lens-py/releases/download/v3.3.0/Chrome-Lens-CLI-v3.3.0.7z";
        private readonly CancellationTokenSource _cancellationTokenSource;

        private string _tempFileName;

        public DownloadChromeLensCLI()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.GetTesseractDictionaries.Download + " Chrome Lens CLI";
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            labelDescription1.Text = LanguageSettings.Current.GetTesseractDictionaries.Download + " Chrome Lens CLI";
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void DownloadChromeLensCLI_Shown(object sender, EventArgs e)
        {
            try
            {
                _tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".7z");
                using (var downloadStream = new FileStream(_tempFileName, FileMode.Create, FileAccess.Write))
                using (var httpClient = DownloaderFactory.MakeHttpClient())
                {
                    var downloadTask = httpClient.DownloadAsync(DownloadUrl, downloadStream, new Progress<float>((progress) =>
                    {
                        var pct = (int)Math.Round(progress * 100.0, MidpointRounding.AwayFromZero);
                        labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait + "  " + pct + "%";
                        labelPleaseWait.Refresh();
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

                }
                CompleteDownload(_tempFileName);
                return;
            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                Cursor = Cursors.Default;
                MessageBox.Show($"Unable to download {DownloadUrl}!" + Environment.NewLine + Environment.NewLine +
                                exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
                DialogResult = DialogResult.Cancel;
            }
        }

        private void CompleteDownload(string downloadStream)
        {
            if (downloadStream.Length == 0)
            {
                throw new Exception("No content downloaded - missing file or no internet connection!" + Environment.NewLine  + 
                                    $"For more info see: {Path.Combine(Configuration.DataDirectory, "error_log.txt")}");
            }

            var dictionaryFolder = Configuration.GoogleLensDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            Extract7Zip(downloadStream, dictionaryFolder, "Chrome-Lens-CLI-v3.3.0");
            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            Cursor = Cursors.Default;
            DialogResult = DialogResult.OK;
            File.Delete(_tempFileName);
        }

        private void Extract7Zip(string tempFileName, string dir, string skipFolderLevel)
        {
            Text = string.Format(LanguageSettings.Current.Settings.ExtractingX, string.Empty);
            labelDescription1.Text = string.Format(LanguageSettings.Current.Settings.ExtractingX, "Chrome Lens CLI");
            labelDescription1.Refresh();

            using (var archiveFile = new ArchiveFile(tempFileName))
            {
                archiveFile.Extract(entry =>
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        return null;
                    }

                    var entryFullName = entry.FileName;
                    if (!string.IsNullOrEmpty(skipFolderLevel) && entryFullName.StartsWith(skipFolderLevel))
                    {
                        entryFullName = entryFullName.Substring(skipFolderLevel.Length);
                    }

                    entryFullName = entryFullName.Replace('/', Path.DirectorySeparatorChar);
                    entryFullName = entryFullName.TrimStart(Path.DirectorySeparatorChar);

                    var fullFileName = Path.Combine(dir, entryFullName);

                    var fullPath = Path.GetDirectoryName(fullFileName);
                    if (fullPath == null)
                    {
                        return null;
                    }

                    var displayName = entryFullName;
                    if (displayName.Length > 30)
                    {
                        displayName = "..." + displayName.Remove(0, displayName.Length - 26).Trim();
                    }

                    labelPleaseWait.Text = string.Format(LanguageSettings.Current.Settings.ExtractingX, $"{displayName}");
                    labelPleaseWait.Refresh();

                    return fullFileName;
                });
            }
        }
    }
}
