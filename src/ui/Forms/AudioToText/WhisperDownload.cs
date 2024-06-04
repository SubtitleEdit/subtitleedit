using Nikse.SubtitleEdit.Core.AudioToText;
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

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    public sealed partial class WhisperDownload : Form
    {
        private const string DownloadUrl64Cpp = "https://github.com/ggerganov/whisper.cpp/releases/download/v1.5.4/whisper-blas-clblast-bin-x64.zip";
        private const string DownloadUrl32Cpp = "https://github.com/ggerganov/whisper.cpp/releases/download/v1.5.4/whisper-blas-bin-Win32.zip";
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly string _whisperChoice;

        private static readonly string[] Sha512HashesCpp =
        {
            "4da4e0c5ed15063ea784a1be9d3f1b791c4c380d9f536fb120b93c3d0cbed853e74f318c394480b620156ad73704d4d51bec82badac7ae52929e0a2d53cd5e1d", // v1.5.4/whisper-blas-clblast-bin-x64.zip
            "1fdcd4c57f19d09507f63c5a4ac4de93858433517c549c6f8b5fe3508f4e35d2535638e59fcddc9f6d36df3d87630870a12c87c80b442da3de6ddeaf859ef0c6", // v1.5.4/whisper-blas-bin-Win32.zip"
        };

        private static readonly string[] OldSha512HashesCpp =
        {
            "179fd78ce1691ab885118d0359cfaeb753a7aaae61d378cc47ea0c215fe01c929259858a081eff47ef2c2c07a3b5f6d0f30b894ce246aab0d6083ccc6fd517ab", // v1.5.3/whisper-blas-clblast-bin-x64.zip
            "cf0ebadb964701beb01ec1ac98eb4a85dae03e356609d65b2f7fb4f8b12aee00609369bfd4e1a40930eaeb95f3e0d208535a967dc6f4af03a564ae01f654d364", // v1.5.3/whisper-blas-bin-Win32.zip
            "1667a86007a6f6d36a94fae0c315c3321eb2572274be8ac540d141be198993d306554aabce1b5f34ac10ffdae09b4c227efba8a4f16978addd82836dc2156c34", // v1.5.2/whisper-blas-bin-x64.zip
            "647c727417bc6a7c90c7460100214426fc1b82fee1ce9924eaec71b46466920b1045c1a534a72782a0d6dcc31541a85a5ad62bfb635c815738c95fafea368cd4", // v1.5.2/whisper-blas-bin-Win32.zip
            "4dad22644af9770ecd05f1959adbe516e0948fb717d0bc33d5f987513f619162159aa2092b54a535e909846caca8dbf53f34c9060dadb43fc57b2c28e645dd73", // v1.5.1/whisper-blas-bin-x64.zip
            "00af057d6ba4005ac1758a713bbe21091796202a81ec6b7dcce5cd9e7680734730c430b75b88a6834b76378747bc4edbcf14a4ed7429b07ea9a394754f4e3368", // v1.5.1/whisper-blas-bin-Win32.zip
            "102cd250958c3158b96453284f101fadebbb0484762c78145309f7d7499aa2b9c9e01e5926a634bd423aee8701f65c7d851a19cb5468364697e624a2c53a325d", // v1.5.0/whisper-blas-bin-x64.zip
            "0bc8df7ca4fdd32a80a9f8e7568b8668221a4205ff8fc3d04963081c14677c6a97e4510e7bb12d7b110fc9a88553aeaa53eff558262ff2d725cef52b3100b149", // v1.5.0/whisper-blas-bin-Win32.zip
            "fc1878c3b7200d0531c376bbe52319a55575e3ceeeacecbee54a366116c30eb1aa3d0a34c742f9fd5a47ffb9f24cba75653d1498e95e4f6f86c00f6d5e593d2a", // v1.4.0/whisper-blas-bin-x64.zip
            "44cb0f326ece26c1b41bd0b20663bc946990a7c3b56150966eebefb783496089289b6002ce93d08f1862bf6600e9912ac62057c268698672397192c55eeb30a2", // v1.4.0/whisper-blas-bin-Win32.zip
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


        private const string DownloadUrl64CppCuBlas = "https://github.com/ggerganov/whisper.cpp/releases/download/v1.5.4/whisper-cublas-12.2.0-bin-x64.zip";

        private static readonly string[] Sha512HashesCppCuBlas =
        {
            "e0279cfc73473b3a9530f44906453c34d9d197cb1cdec860447ce226dd757cc13e3f5f2a22386b95553fc99961e56baf92b20ac1be4217c6a60e749bb5e95cc0", // v1.5.3/whisper-cublas-12.2.0-bin-x64.zip
            "9ca711e835075249a7ecbeb6188be2da2407f94ca04740ba56b984601e68df994e607f03c3816617d92562ed3820b170c48ec82840880efd524da6dfe5b70691", // v1.5.4/whisper-cublas-12.2.0-bin-x64.zip
            "9ca711e835075249a7ecbeb6188be2da2407f94ca04740ba56b984601e68df994e607f03c3816617d92562ed3820b170c48ec82840880efd524da6dfe5b70691", // v1.6.0/whisper-cublas-12.2.0-bin-x64.zip
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


        private const string DownloadUrlPurfviewFasterWhisper = "https://github.com/Purfview/whisper-standalone-win/releases/download/faster-whisper/Whisper-Faster_r192.3_windows.zip";

        private static readonly string[] Sha512HashesPurfviewFasterWhisper =
        {
            "7e289659ff5622cdf99144602729a714985a7f11ebdc988f3f0721fe433d72e7fb751afeece94029f602b304690ad34f8a35bf5980af5517b2718ba07c163279", // r192.3
        };

        private static readonly string[] OldSha512HashesPurfviewFasterWhisper =
        {
            "3b84e21134b7f24b81d7252fe895299d14087cccc1d8b1e7ae187c190b87b0bf7d84d3ce0a4a2fc5d2f9c436a33003663763fd6f62e7f87da1683df7fbd6d10b", // r192.2
            "94faae09146e33e0d55d9190d5b3ed79ea3bda0cf9e7f308dd47812827e4f0c24d2037718aa3c647881008eaaa04bbed033ea338aaa8333aae536bb2f2b83256", // r192.1
            "3dee9ece233be4e661bab7555a2b4e7d4c53d823bf2b4032bd75857554a14a04745c57112946e735dc5ab6f8ec832483444cb95a0921f18b5f736787dbbc515c", // r189.1
            "e78616511a92b21cb8ac82e23cdbd06f5b9310751e5f3fa940b5c48743b69bad130aaf6d629ae07c5388326f117be8f181b125ed04aacd23f1a80d8891be889b", // r186.1
            "a16e2b5460d7f4b0d45de3f0e07b231d58ad4c79d077ad6b9c84a4e2ced4bd1cd3a7d9f01689f1d847ec8ff59c8f81cb742fcf2b153291ed6f15ec8b27adb998", // r167.2
            "1995feca9dd971eccfb41f8dc330d418a531e615cee56eac7cc053fd343fe5200f9e64e2b4feafdde49b018ac518d1ee1b244aedd32dcb84e3fb69c1035b8a4f", // r160.7
            "10ac03f098f991fe9474430a7f44c6fe0574dfb88d37ea4a31b764c540337918c529c4eceaf0524e88975b11b771c61dd67501d2a59fe05008a10195d2768edf", // r160.6
            "9d65922c41a8848e70f04af8deed7279f827264e1fa305c165849e391917713f0336eee07320b2c2cbb6191167953f4d6d1e23a378bfa5a4273c6065a0eba5b3", // r160.5
            "ab2d9f0955a618474cb07141837f280192d6fe9198bab56a62c3e4e76c8bfd6a7a1b8e2d1ce106993e00e00a3305c24f17ec53d5829174dd51a69ad0f82e4b63", // r160.4
            "f66572f08bd93f684c91e40bf873c9c5207d3558ddbea2edaecd6e673300d0349e26ad41e084b7b8a4b74993fb1fd51acb4b9858f7a7c7e9ef1df4de00d07646", // r160.3
            "6a3a0e2e7ae69ec259a0d347bf0970cb276d1ce271a71e8785729fe4a453e71e807e31599223ce2d65f6d8eb8e52d6eee53c3d1d22c373e407155d7717a45ceb", // r160
            "d5d81f3450254f988537bba400b316983fba80142027dbae7ed5abcb06ef6ec367dd2f0699f4096458e783573e02b117d8489b4fa03294dc928b40178d316daa", // r153
            "c40229a04f67409bf92b58a5f959a9aed3cb9bf467ae6d7dd313a1dc6bff7ed6a2f583a3fa8913684178bf14df71d49bdb933cf9ef8bb6915e4794fc4a2dff08", // r149.1
            "22e07106f50301b9a7b818791b825a89c823df25e25e760efd45a7b3b1ea5a5d2048ed24e669729f8bd09dade9ea3902e6452564dd90b88d85cd046cc9eb6efc", // r146
            "fee96c9f8f3a9b67c2e1923fa0f5ef388d645aa3788b1b00c9f12392ef2db4b905d84f5c00ab743a284c8ba2750121e08e9fee1edc76d9c0f12ae51d61b1b12a", // r145.3.zip
            "b689f5ff7329f0ae8f08e3d42b1a2f71bcbe2717cf1231395791cf3b90e305ba4e92955a62ebe946a73c5ca83f61bc60b2e4cff1065cc0f49cfc1f3c665fa587", // r145.2 
            "75ba2bcee9fef0846e54ce367df3fb54f3b9f4cb0f8ac33f01bdde44dc313cd01b3263b43c899271af5901f765ef6257358dcf68c11024652299942405afe289", //  r145.1
            "5414c15bb1682efc2f737f3ab5f15c4350a70c30a6101b631297420bbc4cb077ef9b88cb6e5512f4adcdafbda85eb894ff92eae07bd70c66efa0b28a08361033", // Whisper-Faster r141.4
        };

        private const string DownloadUrlPurfviewFasterWhisperCuda = "https://github.com/Purfview/whisper-standalone-win/releases/download/libs/cuBLAS.and.cuDNN_CUDA11_win_v2.zip";

        private static readonly string[] Sha512HashesPurfviewFasterWhisperCuda =
        {
            "6f3f12162b4537cc8a6dadd51718dc72b3f36dccc27e6c4b5f56d0cfca06bcddaa6877a1984873c0d3ac22ab5fbf56c4cbb87e437d655363b55df4c632574291", // V2
        };

        private static readonly string[] OldSha512HashesPurfviewFasterWhisperCuda =
        {
            "8d3499298bf4ee227c2587ab7ad80a2a6cbac6b64592a2bb2a887821465d20e19ceec2a7d97a4473a9fb47b477cbbba8c69b8e615a42201a6f5509800056a73b",
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
        }

        private void WhisperDownload_Shown(object sender, EventArgs e)
        {
            if (_whisperChoice == WhisperChoice.PurfviewFasterWhisperXXL)
            {
                UiUtil.OpenUrl("https://github.com/Purfview/whisper-standalone-win/releases/download/Faster-Whisper-XXL/Faster-Whisper-XXL_r192.3.4_windows.7z");

                var folder = Path.Combine(Configuration.DataDirectory, "Whisper");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                folder = Path.Combine(folder, "Purfview-Whisper-Faster");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                UiUtil.OpenFolder(folder);

                MessageBox.Show(this, $"Unpack and copy files from \"Faster-Whisper-XXL\" to: \"{folder}\"");
                DialogResult = DialogResult.OK;
                return;
            }

            var downloadUrl = IntPtr.Size * 8 == 32 ? DownloadUrl32Cpp : DownloadUrl64Cpp;
            if (_whisperChoice == WhisperChoice.CppCuBlas)
            {
                downloadUrl = DownloadUrl64CppCuBlas;
            }
            else if (_whisperChoice == WhisperChoice.ConstMe)
            {
                downloadUrl = DownloadUrlConstMe;
            }
            else if (_whisperChoice == WhisperChoice.PurfviewFasterWhisper)
            {
                downloadUrl = DownloadUrlPurfviewFasterWhisper;
            }
            else if (_whisperChoice == WhisperChoice.PurfviewFasterWhisperCuda || _whisperChoice == WhisperChoice.CppCuBlasLib)
            {
                downloadUrl = DownloadUrlPurfviewFasterWhisperCuda;
            }

            try
            {
                using (var httpClient = DownloaderFactory.MakeHttpClient())
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
            if (_whisperChoice == WhisperChoice.CppCuBlas)
            {
                hashes = Sha512HashesCppCuBlas;
            }
            else if (_whisperChoice == WhisperChoice.ConstMe)
            {
                hashes = Sha512HashesConstMe;
            }
            else if (_whisperChoice == WhisperChoice.PurfviewFasterWhisper)
            {
                hashes = Sha512HashesPurfviewFasterWhisper;
            }
            else if (_whisperChoice == WhisperChoice.PurfviewFasterWhisperCuda || _whisperChoice == WhisperChoice.CppCuBlasLib)
            {
                hashes = Sha512HashesPurfviewFasterWhisperCuda;
            }
            else
            {
                hashes = Sha512HashesCpp;
            }

            if (!hashes.Contains(hash))
            {
                MessageBox.Show("Whisper SHA-512 hash does not match!"); ;
                DialogResult = DialogResult.Cancel;
                return;
            }

            var folder = Path.Combine(Configuration.DataDirectory, "Whisper");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (_whisperChoice == WhisperChoice.Cpp)
            {
                folder = Path.Combine(folder, "Cpp");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

            if (_whisperChoice == WhisperChoice.CppCuBlas)
            {
                folder = Path.Combine(folder, WhisperChoice.CppCuBlas);

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

            if (_whisperChoice == WhisperChoice.ConstMe)
            {
                folder = Path.Combine(folder, "Const-me");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                try
                {
                    File.WriteAllText(Path.Combine(folder, "models.txt"), "Whisper Const-me uses models from Whisper.cpp");
                }
                catch
                {
                    // ignore
                }
            }

            if (_whisperChoice == WhisperChoice.PurfviewFasterWhisper)
            {
                folder = Path.Combine(folder, "Purfview-Whisper-Faster");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                using (var zip = ZipExtractor.Open(downloadStream))
                {
                    var dir = zip.ReadCentralDir();
                    foreach (var entry in dir)
                    {
                        if (entry.FilenameInZip.EndsWith(WhisperHelper.GetExecutableFileName(WhisperChoice.PurfviewFasterWhisper)))
                        {
                            var path = Path.Combine(folder, Path.GetFileName(entry.FilenameInZip));
                            zip.ExtractFile(entry, path);
                        }
                    }
                }
            }
            else if (_whisperChoice == WhisperChoice.PurfviewFasterWhisperCuda ||
                     _whisperChoice == WhisperChoice.CppCuBlasLib)
            {

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas)
                {
                    folder = Path.Combine(folder, WhisperChoice.CppCuBlas);
                }
                else
                {
                    folder = Path.Combine(folder, "Purfview-Whisper-Faster");
                }

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                using (var zip = ZipExtractor.Open(downloadStream))
                {
                    var dir = zip.ReadCentralDir();
                    foreach (var entry in dir)
                    {
                        if (entry.FileSize > 0)
                        {
                            var path = Path.Combine(folder, Path.GetFileName(entry.FilenameInZip));
                            zip.ExtractFile(entry, path);
                        }
                    }
                }
            }
            else
            {
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

            if (whisperChoice == WhisperChoice.PurfviewFasterWhisper)
            {
                return OldSha512HashesPurfviewFasterWhisper.Contains(hash);
            }

            return OldSha512HashesCpp.Contains(hash);
        }

        public static bool IsOldVersion(string fullPath, string whisperChoice)
        {
            var hash = Utilities.GetSha512Hash(FileUtil.ReadAllBytesShared(fullPath));

            if (whisperChoice == WhisperChoice.ConstMe)
            {
                var hashVer111 = "1885a6818287a552e955e4df7876d26810a05acd979e8ca3920f78f434965fe0e2052419a6ea089d8d1fa8158a5f7251935e972bc8dcda76abbb0001782d9ec0";
                return hash == hashVer111;
            }

            if (whisperChoice == WhisperChoice.PurfviewFasterWhisper)
            {
                var oldHashes = new List<string>
                {
                    "5e5822bc2d7a5b0d7e50f35460cbbf5bd145eaef03fa7cb3001d4c43622b7796243692a5ce3261e831b9d935f2441bbf7edbe8f119ea92c53fe077885fd10708",
                    "104b85753ce74a81cdec13a2f8665d6af8c2974a3ebef8833cccad15624f311ae17a0e9b9325e3c25cf34edf024127f824c5f000069d7e52459123c9546e1266",
                    "88d1f0c620b0f7c0e87d41da0fd67a3f8b8fb8083e6bb6579d3ca3ee8f7ec919458518965f0071f49068384b86667871213aa90370bcd65c5d9334e7dd1b681e",
                    "6983c90c96e47f53fb1451c1f0a32151ef144fe2e549affc7319d0c7666ea44dcbb0d7dc87ccdaaf0b3d8b2abe92060440e151495109f2681b99940f0eec5ad0",
                    "f616a4fecfb40e74b3e096207f08fbe84a0d08ad872380cf2791eba8458ed854399de2d547be98bc35c65ce0b6959a149b981e745aa75876ffa8eb2fc6a8719e",
                    "0f6b5b0a8d3d169ca7947866552dec30ac43406cda6b7e748c273ed78574087e330571925d8a36d48e5a3ea197d450be0289277677fdbad069038ac0788ea82e",
                    "628dee27ab3030798c42983d0f544668f54e7c8d1c7a433b322b9c07286eedd10666d9b1f89764a75301b334cea9c7ad8bfbfeee00a98113b4730ee5cafe8812",
                    "56faadc85291049b1ad912de8c20fd262288f315d881e517085a15213690f2b242d80aedb2a4c213a7aa26b6ec43d2d26fe3674354a31f816d0e4bca07d002bc",
                    "d53002d273287bfcfcd678d3d9f1faabbbca533ac3fa11867be0e7e365d386bf8fddf591cad41345006406cac663868dd7214d680f36906abe0f7d851d989fa2",
                    "0f463526879a83b938c315d8ca865db89945beb8ba9fd44e74319ba567affb0fcf223d1ee662bf8be280e736e54f44beec2f1e33aac9d537d7d7ae9ba155b049",
                    "78365ba55f66ac018aa8ca405bf11bcb93ce0bff44a528e9304be14f99dc4f84f08ce1679c9cf3d135dd56ade79881318833d69397ca55caa062c6214a0d4cff",
                };

                return oldHashes.Contains(hash);
            }

            if (whisperChoice == WhisperChoice.PurfviewFasterWhisperXXL)
            {
                var oldHashes = new List<string>
                {
                    "x",
                };

                return oldHashes.Contains(hash);
            }

            if (whisperChoice == WhisperChoice.Cpp)
            {
                var oldHashes = new List<string>
                {
                    "9164d033ac8bb9a2f4694da570c9878d24dcaee0bd2eedd26692493a47f916973f3e555c688ba28b337a57dc7effda9a116c1ed5bb8a620ce2c7d5ce42148a64", // 130WhisperBlasBinX64 
                    "ddf75452afc283ada3d686c4cd3eb8bd79b98a4960549585916c9523dee3f9c1a1176a59fa04ffdb33e070e0eaac12b1a263f790afa1dfd4bcb806c02431469d", // 130WhisperBlasBinX32 
                    "c43fed38d1ae99e6fbbd8c842c2d550b4949081c0c7fba72cd2e2e8435ff05eac4f64e659efb09d597c3c062edf1e5026acc375d2a07290fa3c0fca9ac3bd7a2", // 140WhisperBlasBinX64 
                    "3d98347bd89f37dcfaa97ad6a69ee84435ae209d3c2c30b407122f94d5fd512d86fef6f7699f09f8e3adf6236bc3757860b8784406c666328da968964411ecc7", // 150WhisperBlasBinX64 
                    "708a9b98c474f3523b73fd6a41105feb76a07ccf23220b6f068ed5c7a5720ed9b8d851af55368d3a479f5fdf1ae055ac07d128cc951cca70ba4950daab79cc5f", // 150WhisperBlasBinX32 
                    "bcdc1716ddf3e3e08edae6762a6a6122e7ef158ee84bd183669d7ce696bc13253d8645912154bf73d914248b64989aaafb0aa504654d5a117a102303f59d210c", // 151WhisperBlasBinX64 
                    "921012e6de8f6801cc8240ffbbc655826bcd60c5e564e7483cb492082f77fcd8fdb84a46f0d4ea393f9f713863d11a0989e6b13f68bb87ecfd08366b215700e6", // 151WhisperBlasBinX32 
                    "4753cd35a19bad70289c9952a9bd1b37508fa7c88f97439de0540cdb2c147518de976d179f7aa8b7e17f819bc49a061f50ffadbd7076372b39d84e77ea48fc60", // 152WhisperBlasBinX64 
                    "03efdfaa1363a4f0dcf6ca1ff2c2f5ec8f6e6a8f412d5648b955c4e882f586dd3c5d58bfb43a2c51a09d3c745dbe4b1c59fc259cebead0ffb96f0484db9aa54b", // 152WhisperBlasBinX32 
                    "04250bebd8df314abf5b6cf22f96f1c3ba17a5e8308e12a5416291d12dce971eabc8e533dfbcb0214524ad23f5e1065898dc1fd0fafdf66655d0467f7a8205be", // 153WhisperX64        
                    "880d0858e26665d6366a7a11c3b5c07f5f5710672b57110c704a4e94c53fbd1a86d6272b50d5d2e86b267f27b5ef76becff82d9f130deb0b5ed261f401324cdb", // 153WhisperX32        
                    //"72c5a0bec7d55fef420e45253d8133e592d19eb2c9cd2da942a99df58f8a10ee6b14c90ce27a575509e358e0b6c943e0a729da9c17ce16778afd58c30455b0f5", // 154whisper-blas-bin-Win32 
                    //"98d32926e4fbdbe0850f996400204ee62188a88cb5dfe8b182d15c54915f57b7ee4f22c98581c3f45c457241254df041aed49461a7260b84786ba4205dc2a17e", // 154whisper-blas-clblast-bin-x64
                };

                return oldHashes.Contains(hash);
            }

            if (whisperChoice == WhisperChoice.CppCuBlas)
            {
                var oldHashes = new List<string>
                {
                    "b1a88508c07c61f3ed3998ca61b730e389f6a8dddbefbbeb84641dc3ba953fad5696b3e09900327fe3e74e3a7bd9ddacf2caed7e55baa1aa736af434aff73ac7", // 150WhisperCppCublass
                    "3d7f86d816785b980734ccffeb1209b0218bbfbc7cc4e34f6d5b7999d63cf99e36e253db3f88ace0dbfed19ac54c3d04d2fcbb37f39f3df3cc1c3ef1be8bae65", // 151WhisperCppCublass
                    "4f08846d302cbe4022a13c7d9c0fbd12899768cb64bafd52988169c4d6775f9991440a5ac142069b91277db4f782b3b8eeadf0abd7838712e174f861243d2776", // 152WhisperCppCublass
                    "129930b4c69c48855a8f21ace3588c2cec2515baf33b9e557ef4f096132c3f4674639fe5d40ef364fcec0a9f1e9d9916ac6d7b9a220f3c9b44a4c1cc6777bd35", // 153WhisperCppCublass
//                    "5bcda2b519193c137fd5b2a3d9c0289bf9afd18bb21239c5f9f8f7196a8fb57179da53b23924dfa06fbda32e19ace203177f87c62759b64cdf6f04c2514aec94", // 154WhisperCppCublass
                };

                return oldHashes.Contains(hash);
            }

            return false;
        }
    }
}
