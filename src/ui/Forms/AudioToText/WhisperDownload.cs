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
        private const string DownloadUrlAvx2 = "https://github.com/SubtitleEdit/support-files/raw/master/whisper/whisper.cpp-AVX2-2022-12-03.zip";
        private const string DownloadUrlSse2 = "https://github.com/SubtitleEdit/support-files/raw/master/whisper/whisper.cpp-SSE2-2022-12-03.zip";
        private const string DownloadUrl32BitAvx2 = "https://github.com/SubtitleEdit/support-files/raw/master/whisper/whisper.cpp-32-AVX2-2022-12-03.zip";
        private const string DownloadUrl32BitSse2 = "https://github.com/SubtitleEdit/support-files/raw/master/whisper/whisper.cpp-32-SSE2-2022-12-03.zip";
        private readonly CancellationTokenSource _cancellationTokenSource;
        private static readonly string[] Sha512Hashes =
        {
            "3c8a360d1e097d229500f3ccdd66a6dc30600fd8ea1b46405ed2ec03bb0b1c26c72cac983440b5793d24d6983c3d76482a17673251dd89f0a894a54a6d42d169", // AVX2 64-bit
            "58834559f7930c8c3dff6e20963cb86a89ca0228752d35b2486f907e59435a9adc5f5fb13c644f66bedea9ce0368c193d9632366719d344abbd3c0eb547e7110", // SSE2 64-bit
            "9719648662906ece821a0f3d96ee4cb1ba004fb6c9322b17f4e5f6797b4702ad8d266169d4b5367b56cb382481fb79e7e2e3ed2eed38e3df0a930e9b4121414f", // AVX2 32-bit
            "794b84c1a2b33576d201bc6622a647361855e9b12a2af2d7dc5aa6f8b82892d750ce3bfe5095fa7dcf090e263cb546a90250c986989a08b13a67f02751337aee", // SSE2 32-bit
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

            if (IntPtr.Size * 8 == 32)
            {
                downloadUrl = avx2 ? DownloadUrl32BitAvx2 : DownloadUrl32BitSse2;
            }

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
