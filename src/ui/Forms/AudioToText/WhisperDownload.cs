using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    public sealed partial class WhisperDownload : Form
    {
        private const string DownloadUrlAvx2 = "https://github.com/SubtitleEdit/support-files/raw/master/whisper/whisper.cpp-AVX2-2022-11-16.zip";
        private const string DownloadUrlSse2 = "https://github.com/SubtitleEdit/support-files/raw/master/whisper/whisper.cpp-SSE2-2022-11-16.zip";
        private readonly CancellationTokenSource _cancellationTokenSource;
        private static readonly string[] Sha512Hashes =
        {
            "ebbcb81782f5be763203d4e9b7a55b23d04997b0d0689c1af9638b0b5701db763569cb29d39593db84245e499876f930f64f4e758914fbfe39ad87a61cda8581", // AVX2
            "83d216fa1c1874be141c41b5bbdb61df9c447b5182787d8ae3d73c3cb6cf21743b0c7bfa6c45076938ed9629134882898623ab4932c8fde2fdee4fd5718e4bf3", // SSE2
        };

        public WhisperDownload()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.GetTesseractDictionaries.Download + " whisper.cpp";
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            labelDescription1.Text = LanguageSettings.Current.GetTesseractDictionaries.Download + " whisper.cpp";
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void WhisperDownload_Shown(object sender, EventArgs e)
        {
            var avx2 = CpuInfo.HasAvx2();
            labelAVX2.Visible = avx2;
            var downloadUrl = avx2 ? DownloadUrlAvx2 : DownloadUrlSse2;
            try
            {
                var httpClient = HttpClientHelper.MakeHttpClient();
                using (var downloadStream = new MemoryStream())
                {
                    var downloadTask = httpClient.DownloadAsync(downloadUrl, downloadStream, new Progress<float>((progress) =>
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
                MessageBox.Show($"Unable to download {downloadUrl}!" + Environment.NewLine + Environment.NewLine +
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
            if (!Sha512Hashes.Contains(hash))
            {
                MessageBox.Show("Whisper SHA-512 hash does not match!");
                return;
            }

            var folder = Path.Combine(Configuration.DataDirectory, "Whisper");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var zip = ZipExtractor.Open(downloadStream))
            {
                var dir = zip.ReadCentralDir();
                foreach (var entry in dir)
                {
                    var path = Path.Combine(folder, entry.FilenameInZip);
                    zip.ExtractFile(entry, path);
                }
            }

            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            DialogResult = DialogResult.OK;
        }
    }
}
