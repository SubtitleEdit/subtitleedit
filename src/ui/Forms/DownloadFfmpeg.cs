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
            var url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-2024-05/ffmpeg-2024-05-23.zip";
            if (IntPtr.Size == 32)
            {
                url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpegwin32v5.1/ffmpeg-win32-n5.1.zip";
            }

            if (_title.Contains("ffprobe", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-2024-03-25/ffprobe-2024-03-25-git-ecdc94b97f.zip";
            }

            try
            {
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                Cursor = Cursors.WaitCursor;
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
                "c07de58f2006565d7864bea7f3f8e0648e3b0596b0bab94fb1d8ff5a18ea7626bacc8c5206049549c01cda5176350df311e469d6f1f2bf99d968d55817852d4d", // ffprobe-2024-03-25-git-ecdc94b97f
                "fe70d60e9fc0a59c24c0be15f0a8d8af33f1fb7d0027b6fcccce72b6dc41a4023ae276d3e033010b6f628ca6c8d34fc932052622e16a23b5258ec959d20daca9", // ffmpeg-2024-05-23
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
