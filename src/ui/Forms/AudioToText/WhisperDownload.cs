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
        private const string DownloadUrlAvx2 = "https://github.com/ggerganov/whisper.cpp/releases/download/v1.2.0/whisper-blas-bin-x64.zip";
        private const string DownloadUrlSse2 = "https://github.com/ggerganov/whisper.cpp/releases/download/v1.2.0/whisper-blas-bin-x64.zip";
        private const string DownloadUrl32BitAvx2 = "https://github.com/ggerganov/whisper.cpp/releases/download/v1.2.0/whisper-blas-bin-Win32.zip";
        private const string DownloadUrl32BitSse2 = "https://github.com/ggerganov/whisper.cpp/releases/download/v1.2.0/whisper-blas-bin-Win32.zip";
        private readonly CancellationTokenSource _cancellationTokenSource;
        private static readonly string[] Sha512Hashes =
        {
            "a6a75a5d63b933c3529a500b7dd8b330530894b09461bb0a715dbedb31bf2e3493238e86af6d7cc64f3af196a6d61d96bb23853f98d21c8172d5d53d7aad33d9", // 64-bit OpenBLAS
            "92f64f207c400c7c0f1fc27006bf2a1e4170fdc63d045dfdf0a0848b3d727f2763eccfb55e10b6e745e9d39892d24cb9b4c471594011d041458c1ff8722e1ffc", // 32-bit OpenBLAS
        };

        private static readonly string[] OldSha512Hashes =
        {
            "f2073d5ce928e59f7717a82f0330e4d628c81e6cb421b934b4792ac16fb9a33fb9482812874f39d4c7ca02a47a9739d5dd46ddc2e0abc0eb1097dc60bb0616b2", // AVX2 64-bit
            "0d65839e3b05274b3edeef7bbb123c9a6ba04b89729011b758210a80f74563a1e58ca7da0a25e63e42229a2d3dd57a2cb6ce993474b13381871f343b75c3140f", // SSE2 64-bit
            "9f9ce1b39610109bc597b296cb4c1573fa61d33eeaef2a38af44bb2d696fa7c1da297520630ada2470d740edb18a17fe3cca922ad12a307476e27862906450e5", // AVX2 32-bit
            "aab8e7349a7051fb35f2294da3c4993731f47ce2d45ba4c6d4b2b106d0e3236a0082b68e67eb612fec1540e60ae9994183bd41f7fee31c23ec192cbd4155e3c2", // SSE2 32-bit
            "b69bd16bd4d11191b7b1980157d09cb1e489c804219cd336cd0b58182d357b5fff491b29ab8796d1991a9c8f6c8537f475592400b7f4e1244fdfdda8c970a80c", // AVX2 64-bit
            "8e45e147397b688e0ff814f6ef87fd6703748a4f9170fa6498b9428db47bbf9140c7479d016b8e201340ac6627e3f9632c70aa36e7a883355b9abf30e6796ae9", // SSE2 64-bit
            "87799433a5a29b3beaa5a58dfc22471e2c1eb1c9821b6a337b40d8e3c1b4dae9118e10d3a278664fe0f36ba6543ac554108593045640f62711b95f4c2a113b74", // SSE2 32-bit
            "58834559f7930c8c3dff6e20963cb86a89ca0228752d35b2486f907e59435a9adc5f5fb13c644f66bedea9ce0368c193d9632366719d344abbd3c0eb547e7110", // SSE2 64-bit
            "999863541ffbbce142df271c7577f31fef3f386e3ccbb0c436cb21bb13c7557a28602a2f2c25d6a32a6bca7953a21e086a4af3fbdc84b295e994d3452d3af5bc",
            "3c8a360d1e097d229500f3ccdd66a6dc30600fd8ea1b46405ed2ec03bb0b1c26c72cac983440b5793d24d6983c3d76482a17673251dd89f0a894a54a6d42d169", // AVX2 64-bit
            "96f8e6c073afc75062d230200c9406c38551d8ce65f609e433b35fb204dc297b415eb01181adb6b1087436ae82c4e582b20e97f6b204acf446189bde157187b7", // AVX2 32-bit
            "2a9e10f746a1ebe05dffa86e9f66cd20848faa6e849f3300c2281051c1a17b0fc35c60dc435f07f5974aa1191000aaf2866a4f03a5fe35ecffd4ae0919778e63", // SSE2 32-bit
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

            var skipFileNames = new[] { "command.exe", "stream.exe", "talk.exe", "bench.exe" };
            using (var zip = ZipExtractor.Open(downloadStream))
            {
                var dir = zip.ReadCentralDir();
                foreach (var entry in dir)
                {
                    var path = Path.Combine(folder, entry.FilenameInZip);
                    if (!skipFileNames.Contains(entry.FilenameInZip))
                    {
                        zip.ExtractFile(entry, path);
                    }
                }
            }

            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            DialogResult = DialogResult.OK;
        }

        public static bool IsOld(string fullPath)
        {
            var hash = Utilities.GetSha512Hash(FileUtil.ReadAllBytesShared(fullPath));
            return OldSha512Hashes.Contains(hash);
        }
    }
}
