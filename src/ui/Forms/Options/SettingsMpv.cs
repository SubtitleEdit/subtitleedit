using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public sealed partial class SettingsMpv : Form
    {
        private string _downloadUrl;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public SettingsMpv()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            labelPleaseWait.Text = string.Empty;

            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            Text = LanguageSettings.Current.SettingsMpv.DownloadMpv;
            UiUtil.FixLargeFonts(this, buttonCancel);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void ButtonDownloadClick(object sender, EventArgs e)
        {
            _downloadUrl = "https://github.com/SubtitleEdit/support-files/releases/download/libmpv-2025-01-25/libmpv2-" + IntPtr.Size * 8 + ".zip";
            try
            {
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                Refresh();
                Cursor = Cursors.WaitCursor;
                using (var httpClient = DownloaderFactory.MakeHttpClient())
                using (var downloadStream = new MemoryStream())
                {
                    var downloadTask = httpClient.DownloadAsync(_downloadUrl, downloadStream, new Progress<float>((progress) =>
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
                MessageBox.Show($"Unable to download {_downloadUrl}!" + Environment.NewLine + Environment.NewLine +
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

            downloadStream.Position = 0;
            var hash = Utilities.GetSha512Hash(downloadStream.ToArray());
            string[] validHashes =
            {
                "e8bd85b3f536ed36e48d7ce7f0110ed21c020c22e9cf82567d128bb08a4c3d5957c408bed5300323420c0769276e5ae4fa131dbb0246b0853c3ce44e0ed6a203", // 32-bit
                "318cb5688f95e1f2e307594d570b402005c6a3874c09ea757bdf6b9a845dbffa826c7652471e7435ddf38bb1a5c4d6d0eed40038824b37ea31563d814a15d4cc", // 64-bit
            };
            if (!validHashes.Contains(hash))
            {
                MessageBox.Show("libmpv SHA-512 hash does not match!");
                DialogResult = DialogResult.Cancel;
                return;
            }

            downloadStream.Position = 0;
            var dictionaryFolder = Configuration.DataDirectory;
            using (ZipExtractor zip = ZipExtractor.Open(downloadStream))
            {
                List<ZipExtractor.ZipFileEntry> dir = zip.ReadCentralDir();
                foreach (ZipExtractor.ZipFileEntry entry in dir)
                {
                    if (entry.FilenameInZip.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        var fileName = Path.GetFileName(entry.FilenameInZip);
                        var path = Path.Combine(dictionaryFolder, fileName);

                        try
                        {
                            zip.ExtractFile(entry, path);
                        }
                        catch
                        {
                            path = Path.Combine(dictionaryFolder, fileName + ".new-mpv");
                            zip.ExtractFile(entry, path);
                        }
                    }
                }
            }

            Cursor = Cursors.Default;
            labelPleaseWait.Text = LanguageSettings.Current.SettingsMpv.DownloadMpvOk;
            buttonCancel.Text = LanguageSettings.Current.General.Ok;
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

        private void SettingsMpv_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
            if (Configuration.IsRunningOnWindows)
            {
                ButtonDownloadClick(null, null);
            }
        }
    }
}
