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
            var url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-v8/ffmpeg.zip";
            if (IntPtr.Size == 32)
            {
                url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-v8/ffmpeg-win32.zip";
            }
            else if (IsWindows7())
            {
                url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpegwin32v5.1/ffmpeg-2024-05-20-win7.zip";
            }

            if (_title.Contains("ffprobe", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-v8/ffprobe.zip";
                if (IntPtr.Size == 32)
                {
                    url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-v8/ffprobe-win32.zip";
                }
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
                "3e24d632de0bc0ee01a064354dd334377ea88880ffc5a6801252361db869d413c103cfd459beb4a64aa0b3bea4154e828f111320d9a5b3becf35c3ebba89d451", // ffmpeg 32
                "5aa9e9bbd88d30408c3a97e05adcc3a3c682d8a53b5fa011a65566bcf10249afa752529c475ce1b7b287f1fee815df4e83a191259de23cfae22f216c9e973084", // ffmpeg 64
                "41db945aea3a16b2d9f2ec967831cb5da4d22a3567e7be3110a5f64bfd40a84f33be48f5162403c968769101c0a1a5329dabba32968697e927fc5213a37701ff", // ffprobe 32
                "d2fda9b2abab69b4bbfe57e135dcc38312368c62ce3419476738ac2d96bcecf9e0ec3147b3c131199155ef6bc237b70b9eebe7546e4e5d13b414f20aa54c2a6d", // ffprobe 64
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
