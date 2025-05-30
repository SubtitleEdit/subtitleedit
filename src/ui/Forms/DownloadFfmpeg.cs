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

        private static bool IsWindows7()
        {
            return Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1;
        }

        private void DownloadFfmpeg_Shown(object sender, EventArgs e)
        {
            var url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-2025-03-31/ffmpeg-2025-03-31.zip";
            if (IntPtr.Size == 32)
            {
                url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpegwin32v5.1/ffmpeg-win32-n5.1.zip";
            }
            else if (IsWindows7())
            {
                url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpegwin32v5.1/ffmpeg-2024-05-20-win7.zip";
            }

            if (_title.Contains("ffprobe", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-2025-03-31/ffprobe-2025-03-31.zip";
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
                "18d4d0d9a780292385a088965ddc7c773d9a5c524b1f9ecd6336287f03490b2507d59ce6fa552f1f316168747cd75f93f395de2dab7b2986351783814937d19c", // ffmpeg 32
                "e715d308a666b8f16cc6585a14316029d905e42e2af8a5ad0a543360d80badfbdf3748080d825149f9e727ae56274f62121f5625929dba3b884b86ecd3a2a139", // ffmpeg 64
                "72ee50ce0b529d550606c1b1193480a864046888826108249ca94503e892685ea354ea24d603a051fc4c3704895dda04a5e7a9b6f234ade5b29e6c85c1a73591", // ffprobe
                "b7fba2d49212660d67652f4ffb5760fdc5c7c4732fa08af630bd2674fd90a9dc5de05cad543204f3846813bb566c708addb8d72b8b5c6c9ff973522b2fe25037", // ffmpeg for Windows 7 
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
