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
            _downloadUrl = "https://github.com/SubtitleEdit/support-files/releases/download/ibmpv-2023-11-26/libmpv2-" + IntPtr.Size * 8 + ".zip";
            try
            {
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                Refresh();
                Cursor = Cursors.WaitCursor;
                var httpClient = DownloaderFactory.MakeHttpClient();
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
                "1aaf774b046f3776059fff14162bca73a65a06ff64ed5df53a0db7b53c34196de7325dadbfaf1017336a146c785db60d814eee61fef094bcb9fca739c63abc7b", // 32-bit
                "2b85050721d75cfe3447a7282bf717fadd8999a20217f6ed0ffa121573e1d07301bff6b86ca2896b59206b4af09a0b977f3dcc901ae9245a94941157a0398c00", // 64-bit
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
