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
            var url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-v7-1/ffmpeg-7.1.zip";
            if (IntPtr.Size == 32)
            {
                url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpegwin32v5.1/ffmpeg-win32-n5.1.zip";
            }

            if (_title.Contains("ffprobe", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://github.com/SubtitleEdit/support-files/releases/download/ffmpeg-v7-1/ffprobe-7.1.zip";
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
                "42a42cbed3175165a71cc573960306f422373a3f30864cf81b476675bdb485aca81d904f1f5c67b0a3397b62a468ca4a4fcad6507103e7b8908e56ab63be6255", // ffmpeg
                "74db8a0b8226c3104c52a986f3bd000b6eb57ab4a017d44aa10c00fb9310ce06a634e581cce76c5d82410dfd7b6cddfaceb8b7bdca17ee6c39e7213843041ab4", // ffprobe
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
