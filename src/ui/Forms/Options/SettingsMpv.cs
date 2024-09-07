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
            _downloadUrl = "https://github.com/SubtitleEdit/support-files/releases/download/libmpv-2024-06-09/libmpv2-" + IntPtr.Size * 8 + ".zip";
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
                "a6d6ee7b2a1173a77ee07e046d6f9875842d88ee41a8a935ff5f906300553a704c0b39ae8a1a42eb7c7b1dc06b4e6e9d28d62a2d93384b70ce45de9d9ac8f8d7", // 32-bit
                "ae62aa72a7678199c8470cbec38f94e930469332f90a2da77dab1d89ef86e3ba71a0d5258cbf1fe8680e84736ea123ebdc5737a93384de1106fba6c0b78ea560", // 64-bit
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
