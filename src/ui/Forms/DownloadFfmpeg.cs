using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class DownloadFfmpeg : Form
    {
        public string FFmpegPath { get; internal set; }
        public bool AutoClose { get; internal set; }
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly string _title;

        public DownloadFfmpeg(string title)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _title = title;
            Text = string.Format(LanguageSettings.Current.Settings.DownloadX, title);
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonCancel);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void DownloadFfmpeg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (buttonCancel.Text == LanguageSettings.Current.General.Ok)
            {
                DialogResult = DialogResult.OK;
                return;
            }

            _cancellationTokenSource.Cancel();
            DialogResult = DialogResult.Cancel;
        }

        private void DownloadFfmpeg_Shown(object sender, EventArgs e)
        {
            var url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-v6-1/ffmpeg61.zip";
            if (IntPtr.Size == 32)
            {
                url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpegwin32v5.1/ffmpeg-win32-n5.1.zip";
            }

            if (_title.Contains("ffprobe", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-v6-1/ffprobe61.zip";
            }

            try
            {
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                Cursor = Cursors.WaitCursor;
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

                    CompleteDownload(downloadStream);
                }
            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                Cursor = Cursors.Default;
                MessageBox.Show($"Unable to download {url}!" + Environment.NewLine + Environment.NewLine +
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

            var folder = Path.Combine(Configuration.DataDirectory, "ffmpeg");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var sha512Hashes = new[]
            {
                "08d1a88a6293ad9c66de2f11a029b648bafe7f2a0ebaca6bb863f11e7d80967f6804b8c2b86c2d8381a441857fd7fc52f2a9e95b340295e1b16ed124c6f776f3", // ffmpeg 6.1
                "d2ee1d3bfa6cfb8c7563cfb8cd641962e7274149458630191a69b689e9c0288608885f51c39f578008e68856ee71bc66a82ed4b3cba2f1411aec2b4da991b974", // ffprobe 6.1
            };
            var hash = Utilities.GetSha512Hash(downloadStream.ToArray());
            if (!sha512Hashes.Contains(hash))
            {
                MessageBox.Show("ffmpeg SHA 512 hash does not match - download aborted!"); ;
                DialogResult = DialogResult.Cancel;
                return;
            }

            downloadStream.Position = 0;
            using (var zip = ZipExtractor.Open(downloadStream))
            {
                var dir = zip.ReadCentralDir();
                foreach (var entry in dir)
                {
                    var fileName = Path.GetFileName(entry.FilenameInZip);
                    if (fileName != null)
                    {
                        var path = Path.Combine(folder, fileName);
                        if (fileName.EndsWith("ffmpeg.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            FFmpegPath = path;
                        }

                        zip.ExtractFile(entry, path);
                    }
                }
            }

            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;

            if (AutoClose)
            {
                DialogResult = DialogResult.OK;
                return;
            }

            buttonCancel.Text = LanguageSettings.Current.General.Ok;
            labelPleaseWait.Text = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownloadOk, _title);
        }
    }
}
