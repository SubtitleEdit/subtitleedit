using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Http;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    public sealed partial class WhisperDownload : Form
    {
        private const string DownloadUrl64Cpp = "https://github.com/ggerganov/whisper.cpp/releases/download/v1.4.0/whisper-blas-bin-x64.zip";
        private const string DownloadUrl32Cpp = "https://github.com/ggerganov/whisper.cpp/releases/download/v1.4.0/whisper-blas-bin-Win32.zip";
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly string _whisperChoice;

        private static readonly string[] Sha512Hashes =
        {
            "fc1878c3b7200d0531c376bbe52319a55575e3ceeeacecbee54a366116c30eb1aa3d0a34c742f9fd5a47ffb9f24cba75653d1498e95e4f6f86c00f6d5e593d2a", // 64-bit OpenBLAS
            "44cb0f326ece26c1b41bd0b20663bc946990a7c3b56150966eebefb783496089289b6002ce93d08f1862bf6600e9912ac62057c268698672397192c55eeb30a2", // 32-bit OpenBLAS
        };

        private static readonly string[] OldSha512Hashes =
        {
            "e193e845380676b59deade82d3f1de70ac54da3b5ffa70b4eabb3a2a96ad312d8f197403604805cef182c85c3a3370dd74a2b0b7bccf2d95b1022c10ce8c7b79", // 64-bit OpenBLAS
            "4218423f79d856096cdc8d88aad2e361740940e706e0b1d07dc3455571022419ad14cfef717f63e8fc61a7a1ef67b6722cec8b3c4c25ad7f087a23b1b89c5d91", // 32-bit OpenBLAS
            "a6a75a5d63b933c3529a500b7dd8b330530894b09461bb0a715dbedb31bf2e3493238e86af6d7cc64f3af196a6d61d96bb23853f98d21c8172d5d53d7aad33d9", // 64-bit OpenBLAS
            "92f64f207c400c7c0f1fc27006bf2a1e4170fdc63d045dfdf0a0848b3d727f2763eccfb55e10b6e745e9d39892d24cb9b4c471594011d041458c1ff8722e1ffc", // 32-bit OpenBLAS
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

        private const string DownloadUrlConstMe = "https://github.com/Const-me/Whisper/releases/download/1.12.0/cli.zip";

        private static readonly string[] Sha512HashesConstMe =
        {
            "bc329789c58887f14cfcdb14d4ced4e6322b6f8c8c4625bddc40321a46e9bae7c45107f4a757793b02d0df456ac839d4ef66529e79d3144b1813a2ae49e7a1ca", // 1.12
        };

        private static readonly string[] OldSha512HashesConstMe =
        {
            "e7169149864f3385df2904aadff224f66e39e93dff57135b078c8d8c44947b07fdcd57ce10221533afd417e3e864b8562a4d605b008be4efd6e208bb7b43efcd",
            "18e5aab30946e27d7ee88a11d88c4582be980fe4e6a86db85609d81067778b0014a9de5bed6c82176aa6f3ba7b4e0c8f13b3c1b91f6883529161fa54f2a00e7e",
            "76b9004121fb152cc11641ce4afb33de4e503d549dcfb4f1e17b5f2655d5bb8e912120b4b273937693014cdb6bbb242210b0572b4de767d7bd0d7e0c4144f3c8",
            "a4681b139c93d7b4b6cefbb4d72de175b3980a4c6052499ca9db473e817659479d2ef8096dfd0c50876194671b09b25985f6db56450b6b5f8a4117851cfd9f1f",
        };


        private const string DownloadUrlPurfviewFasterWhisper = "https://github.com/Purfview/whisper-standalone-win/releases/download/faster-whisper/Whisper-Faster_r141.4.zip";

        private static readonly string[] Sha512HashesPurfviewFasterWhisper =
        {
            "5414c15bb1682efc2f737f3ab5f15c4350a70c30a6101b631297420bbc4cb077ef9b88cb6e5512f4adcdafbda85eb894ff92eae07bd70c66efa0b28a08361033", // Whisper-Faster r141.4
        };

        private static readonly string[] OldSha512HashesPurfviewFasterWhisper =
        {
        };


        public WhisperDownload(string whisperChoice)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.GetTesseractDictionaries.Download;
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            labelDescription1.Text = LanguageSettings.Current.GetTesseractDictionaries.Download + " " + whisperChoice;
            _cancellationTokenSource = new CancellationTokenSource();
            _whisperChoice = whisperChoice;
            labelWhisperChoice.Text = _whisperChoice;
            labelWhisperChoice.Left = Width - labelWhisperChoice.Width - 20;
        }

        private void WhisperDownload_Shown(object sender, EventArgs e)
        {
            var downloadUrl = IntPtr.Size * 8 == 32 ? DownloadUrl32Cpp : DownloadUrl64Cpp;
            if (_whisperChoice == WhisperChoice.ConstMe)
            {
                downloadUrl = DownloadUrlConstMe;
            }
            else if (_whisperChoice == WhisperChoice.PurfviewFasterWhisper)
            {
                downloadUrl = DownloadUrlPurfviewFasterWhisper;
            }

            try
            {
                var httpClient = DownloaderFactory.MakeHttpClient();
                using (var downloadStream = new MemoryStream())
                {
                    var downloadTask = httpClient.DownloadAsync(downloadUrl, downloadStream, new Progress<float>((progress) =>
                    {
                        var pct = (int)Math.Round(progress * 100.0, MidpointRounding.AwayFromZero);
                        labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait + "  " + pct + "%";
                        labelPleaseWait.Refresh();
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
            string[] hashes;
            if (_whisperChoice == WhisperChoice.ConstMe)
            {
                hashes = Sha512HashesConstMe;
            }
            else if (_whisperChoice == WhisperChoice.PurfviewFasterWhisper)
            {
                hashes = Sha512HashesPurfviewFasterWhisper;
            }
            else
            {
                hashes = Sha512Hashes;
            }

            if (!hashes.Contains(hash))
            {
                MessageBox.Show("Whisper SHA-512 hash does not match!");
                return;
            }

            var folder = Path.Combine(Configuration.DataDirectory, "Whisper");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (_whisperChoice == WhisperChoice.ConstMe)
            {
                folder = Path.Combine(folder, "Const-me");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

            if (_whisperChoice == WhisperChoice.PurfviewFasterWhisper)
            {
                folder = Path.Combine(folder, "Purfview");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
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

        public static bool IsOld(string fullPath, string whisperChoice)
        {
            var hash = Utilities.GetSha512Hash(FileUtil.ReadAllBytesShared(fullPath));

            if (whisperChoice == WhisperChoice.ConstMe)
            {
                return OldSha512HashesConstMe.Contains(hash);
            }
            else if (whisperChoice == WhisperChoice.PurfviewFasterWhisper)
            {
                return OldSha512HashesPurfviewFasterWhisper.Contains(hash);
            }

            return OldSha512Hashes.Contains(hash);
        }
    }
}
