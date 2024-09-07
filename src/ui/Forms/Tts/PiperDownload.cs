using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Logic;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.Tts
{
    public sealed partial class PiperDownload : Form
    {
        public bool AutoClose { get; internal set; }
        public string ModelUrl { get; internal set; }
        public string ModelFileName { get; internal set; }
        public string PiperPath { get; set; }

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly string _title;

        public PiperDownload(string title)
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
            var url = "https://github.com/SubtitleEdit/support-files/releases/download/piper-2023.11.14-2/piper2023.11.14-2.zip";
            if (!string.IsNullOrEmpty(ModelUrl))
            {
                url = ModelUrl;
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

            var sha512Hashes = new[]
            {
                "b09e87706e5194b320f28a13298e92e7c2e9acd6eecc8f22ba268d93b554e5c83373e8648b88dcb53756271a71571fd2ae689b3a09596c5313ad955bcea6bbfb", // Piper2023.11.14-2
            };
            var hash = Utilities.GetSha512Hash(downloadStream.ToArray());
            if (string.IsNullOrEmpty(ModelUrl) && !sha512Hashes.Contains(hash))
            {
                MessageBox.Show("piper SHA 512 hash does not match - download aborted!"); ;
                DialogResult = DialogResult.Cancel;
                return;
            }

            if (!Directory.Exists(PiperPath))
            {
                Directory.CreateDirectory(PiperPath);
            }

            downloadStream.Position = 0;
            if (!string.IsNullOrEmpty(ModelFileName))
            {
                using (var fileStream = File.Create(ModelFileName))
                {
                    downloadStream.CopyTo(fileStream);
                }
            }
            else
            {
                using (var zip = ZipExtractor.Open(downloadStream))
                {
                    var dir = zip.ReadCentralDir();
                    foreach (var entry in dir)
                    {
                        var fileName = Path.GetFileName(entry.FilenameInZip);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            var name = entry.FilenameInZip;
                            var path = Path.Combine(PiperPath, name.Replace('/', Path.DirectorySeparatorChar));
                            zip.ExtractFile(entry, path);
                        }
                        else
                        {
                            var p = Path.Combine(PiperPath, entry.FilenameInZip.TrimEnd('/'));
                            if (!Directory.Exists(p))
                            {
                                Directory.CreateDirectory(p);
                            }
                        }
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
