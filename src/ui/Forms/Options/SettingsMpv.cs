using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
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
        private readonly bool _justDownload;
        private string _downloadUrl;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public SettingsMpv(bool justDownload)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _justDownload = justDownload;
            labelPleaseWait.Text = string.Empty;

            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            Text = LanguageSettings.Current.SettingsMpv.DownloadMpv;
            if (!Configuration.IsRunningOnLinux)
            {
                buttonDownload.Text = LanguageSettings.Current.SettingsMpv.DownloadMpv;
            }

            UiUtil.FixLargeFonts(this, buttonOK);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void ButtonDownloadClick(object sender, EventArgs e)
        {
            _downloadUrl = "https://github.com/SubtitleEdit/support-files/releases/download/libmpv-2023-08-13/libmpv2-" + IntPtr.Size * 8 + ".zip";
            try
            {
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                buttonOK.Enabled = false;
                buttonDownload.Enabled = false;
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
                buttonOK.Enabled = true;
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
                "ea31a9599a5f01951debfc8c52b6c961e98a56632fa08ec83e2cbc7194c3e43560a5d889b524a4c930f33028bdf9ef8797dc0df5b2660cd6eb7b19bb125546ca",
                "5b91fa3b40e4b216dab1719a11dffc897a5110415e3cc9ddfcf9e2d0ae677bbfc5e3c360e09415b7c266ad1e65a601d951a8efcbcb863d63dfabe4f6b5fdd716", // 64-bit
            };
            if (!validHashes.Contains(hash))
            {
                MessageBox.Show("Whisper SHA-512 hash does not match!"); ;
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
            labelPleaseWait.Text = string.Empty;
            buttonOK.Enabled = true;
            buttonDownload.Enabled = !Configuration.IsRunningOnLinux;

            MessageBox.Show(LanguageSettings.Current.SettingsMpv.DownloadMpvOk);
            DialogResult = DialogResult.OK;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
            DialogResult = DialogResult.Cancel;
        }

        private void SettingsMpv_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
            if (Configuration.IsRunningOnWindows && (!LibMpvDynamic.IsInstalled || _justDownload))
            {
                ButtonDownloadClick(null, null);
            }
        }
    }
}
