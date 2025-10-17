using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Logic;
using SevenZipExtractor;
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
        private const string DownloadUrl64Cpp = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-182/whisper-blas-bin-x64.zip";
        private const string DownloadUrl32Cpp = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-182/whisper-blas-bin-win32.zip";
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly string _whisperChoice;
        private string _tempFileName;

        private static readonly string[] Sha512HashesCpp =
        {
            "913eadc1fc778dec52f42d16f914246b513f3583c45d01b7fc83b547c81bf2b4fcba32f8cc7c12271ab28d572e6fefad1f2bb8b6c7db988bea09c217cbea9f16", // v182/whisper-blas-bin-x64.zip
            "6c48ea5551a22347522784527cefda6e550abc79e3f430357e9f18c756928a45cb8a040e71e1ae694f52c946c31f45a0d6504cce7d4ba0ba054ec9d8e1b3a47a", // v182/whisper-blas-bin-Win32.zip
        };

        private static readonly string[] OldSha512HashesCpp =
        {
            "8cffbb1b95723624ed5b89fd747a9a8a2c6d96e34deb34261e76a6c8f2455d22edeb0a2292648cf6ef3cbe637428b3ce911d0fffd76bf0e48a179922decfe854", // v180/whisper-blas-bin-x64.zip
            "f7df083c8fa0d1723899c5a0bc31a6db161114ddeffa366a77ed7d9474b340609ef7912f8185f70ff491ae7606aff60b16d6e0167863e7294544ba7adf7e25fa", // v180/whisper-blas-bin-Win32.zip
            "55a951e17d72f62e76d8c97ef6348e74e427f3bde7affab7caf6c2b421e172092eb5f43323117273a41609bd0e5c23e712a0845ed46d18ee48d6f37cdbbaba30", // v176/whisper-blas-bin-x64.zip
            "84a0b520b2fa9b9e15353c91a14bffb36a5e752a2abadc9c1d4b6412ad3fad9c6b28a07e3d83c8ebde2d8c418052f56299715ea6a0e1e218717430a744774c43", // v176/whisper-blas-bin-Win32.zip
            "055f632cdb2da4ab1c04bc9d0a7ecf76b43999eed66e99d327064dea960ba7105728556c2d276f9703d3940587a38447fe9dbb546372cfbab9b769b790def113", // v175/whisper-blas-bin-x64.zip
            "6c54435370961650d1896eac6b66b2fa42631cf4d27c22d9da2e425c513df7882d7f824290e93397c4cf52fbe96765da913cb14c70b26ff599e0472afcae2774", // v175/whisper-blas-bin-Win32.zip
            "db15572e89c034754022e7542637b2e57f1e8b54484f05fa842bddf943cf41e95bb43a305f30c7a9f1cdd164ba4deca6178eadcb513861f3d470a508b9b385cc", // v173/whisper-blas-bin-x64.zip
            "a6770e8d9d90a2c39f74f53bda06a79012738d4ea9b83c53377776dd0e360b0634464447a87197303cea104ba5412b320d1b4742a9cd1de1c7d888091a65b189", // v173/whisper-blas-bin-Win32.zip
            "66c97cff5514827c1f9496d69df7a86ba786dc94b6ba78350ba5e43ce28af5267de9b859232243c2dfd020fb33a7a8c80739b9169fdbaad330cde417e4afff08", // v172/whisper-blas-bin-x64.zip
            "3833976a5278deac3983050944723963b5a2e1721734376bc25d64cc3ec87ff912dc3b12842d2e11ca85ce923b0a54f0c77d71f161fe97627bdc33ee6dcf64e2", // v172/whisper-blas-bin-Win32.zip
            "4da4e0c5ed15063ea784a1be9d3f1b791c4c380d9f536fb120b93c3d0cbed853e74f318c394480b620156ad73704d4d51bec82badac7ae52929e0a2d53cd5e1d", // v1.5.4/whisper-blas-clblast-bin-x64.zip
            "1fdcd4c57f19d09507f63c5a4ac4de93858433517c549c6f8b5fe3508f4e35d2535638e59fcddc9f6d36df3d87630870a12c87c80b442da3de6ddeaf859ef0c6", // v1.5.4/whisper-blas-bin-Win32.zip"
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
            "2f6ab662aecd09ad5d06690ad01981d155630462da077072301b624efed702559616bf368a640864d44d8f50927d56d252345117084ef6e795b67964f6303fe4", // v171/whisper-blas-bin-x64.zip
            "3ff1490ca2c0fa829ab0c4e5b485c4e93ed253adc30557ef4f369920925a246ffc459c122e0f3c0b166ef94b50e7f1e7e48c29c43ca551c3d8905f5ef3d8004c", // v171/whisper-blas-bin-Win32.zip
        };


        private const string DownloadUrl64CppCuBlas = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-180/whisper-cublas-11.8.0-bin-x64.7z";

        private static readonly string[] Sha512HashesCppCuBlas =
        {
            "eb41413c28627302c0335b3ea74d57c3eb9a2458c8a3f9745cb9eca7c6ad097c5e5c668e5971d17289ce2c018f4f839c14b86a34cf446ed6b21c264077b5a4f2", // v182/whisper-cublas-11.8.0-bin-x64.7z
            "d3a95aeb7b1607a0f7ab5298b8091fd016947601a2b1ed233d593e35195e8d598e7d897d2aa45a8114795615e2fb2ad7c3e27169a787f0785279c2d5c0e923f5", // v180/whisper-cublas-11.8.0-bin-x64.7z
            "754e110bdb5f4a30e67a683b196e135c968096de68f528ea107fe33d1fd70043cd9011a333e6fbdb322554d811edf301a35667928a9d7d0014bfe5d7fea51afa", // v176/whisper176-cublas-11-8-0-x64.7z
            "f901750abab46791ba91a3a6575f67f368fa01a903028bb35c3f6e347a6d0fd3b36dc6c4b81644250ec12c4ff8e7a4dff605eb87d14aedc3908e1335d8a34194", // v175/whisper-cublas-12.2.0-bin-x64.zip
            "1b105d38702a01ab8e98b31a690040ca54861b5e55773fff9242f33ba7b0718a6e9f25231ed107a7db0292d8349d508b18300f6c95a8e4234faef27cb05887aa", // v172/whisper-cublas-12.2.0-bin-x64.zip
            "37c77ce10739b67588fdc1ca06ac8ff3c578d230974af6c5d90cf80f0d85af1a28f6827447b7b63699c21a5fddfeedeb3bd6cf8a64dd859598e94faef2b9ba3e", // v171/whisper-cublas-12.2.0-bin-x64.zip
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

        private const string DownloadUrlPurfviewFasterWhisperXxl = "https://github.com/Purfview/whisper-standalone-win/releases/download/Faster-Whisper-XXL/Faster-Whisper-XXL_r245.4_windows.7z";
        private const string DownloadUrlPurfviewFasterWhisperXxlWin7 = "https://github.com/Purfview/whisper-standalone-win/releases/download/Faster-Whisper-XXL/Faster-Whisper-XXL_r192.3.4_windows.7z";
        //          private const string DownloadUrlPurfviewFasterWhisperXxl = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-172/test.7z";

        private static readonly string[] Sha512HashesPurfviewFasterWhisperXxl =
        {
            "", // 
        };

        private static readonly string[] OldSha512HashesPurfviewFasterWhisperXxl =
        {
            "",
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
            if (whisperChoice == WhisperChoice.PurfviewFasterWhisperXxl)
            {
                labelDescription1.Text += " (1.4 GB)";
            }
            _cancellationTokenSource = new CancellationTokenSource();
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            _whisperChoice = whisperChoice;
        }

        private static bool IsWindows7()
        {
            return Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1;
        }

        private void WhisperDownload_Shown(object sender, EventArgs e)
        {
            if (_whisperChoice == WhisperChoice.PurfviewFasterWhisperXxl)
            {
                _tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".7z");
                using (var downloadStream = new FileStream(_tempFileName, FileMode.Create, FileAccess.Write))
                using (var httpClient = DownloaderFactory.MakeHttpClient())
                {
                    var url = IsWindows7() ? DownloadUrlPurfviewFasterWhisperXxlWin7 : DownloadUrlPurfviewFasterWhisperXxl;
                    var downloadTask = httpClient.DownloadAsync(url, downloadStream, new Progress<float>((progress) =>
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

                }
                CompleteDownloadFasterWhisperXxl(_tempFileName);
                return;
            }

            if (_whisperChoice == WhisperChoice.CppCuBlas)
            {
                _tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".7z");
                using (var downloadStream = new FileStream(_tempFileName, FileMode.Create, FileAccess.Write))
                using (var httpClient = DownloaderFactory.MakeHttpClient())
                {
                    var url = DownloadUrl64CppCuBlas;
                    var downloadTask = httpClient.DownloadAsync(url, downloadStream, new Progress<float>((progress) =>
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

                }
                CompleteDownloadFasterWhisperCppCublas(_tempFileName);
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
            else if (_whisperChoice == WhisperChoice.CppCuBlasLib)
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
            else if (_whisperChoice == WhisperChoice.PurfviewFasterWhisperXxl)
            {
                hashes = Sha512HashesPurfviewFasterWhisperXxl;
            }
            else if (_whisperChoice == WhisperChoice.CppCuBlasLib)
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

            if (_whisperChoice == WhisperChoice.CppCuBlasLib)
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

        private void CompleteDownloadFasterWhisperXxl(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            if (fileInfo.Length == 0)
            {
                throw new Exception("No content downloaded - missing file or no internet connection!");
            }

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

            Extract7Zip(_tempFileName, folder, "Faster-Whisper-XXL");
            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            DialogResult = DialogResult.OK;
            File.Delete(_tempFileName);
        }

        private void CompleteDownloadFasterWhisperCppCublas(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            if (fileInfo.Length == 0)
            {
                throw new Exception("No content downloaded - missing file or no internet connection!");
            }

            var folder = Path.Combine(Configuration.DataDirectory, "Whisper");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            folder = Path.Combine(folder, WhisperChoice.CppCuBlas);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            Extract7Zip(_tempFileName, folder, string.Empty);
            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            DialogResult = DialogResult.OK;
            File.Delete(_tempFileName);
        }

        private void Extract7Zip(string tempFileName, string dir, string skipFolderLevel)
        {
            Text = string.Format(LanguageSettings.Current.Settings.ExtractingX, string.Empty);
            labelDescription1.Text = string.Format(LanguageSettings.Current.Settings.ExtractingX, _whisperChoice);
            labelDescription1.Refresh();

            using (var archiveFile = new ArchiveFile(tempFileName))
            {
                archiveFile.Extract(entry =>
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        return null;
                    }

                    var entryFullName = entry.FileName;
                    if (!string.IsNullOrEmpty(skipFolderLevel) && entryFullName.StartsWith(skipFolderLevel))
                    {
                        entryFullName = entryFullName.Substring(skipFolderLevel.Length);
                    }

                    entryFullName = entryFullName.Replace('/', Path.DirectorySeparatorChar);
                    entryFullName = entryFullName.TrimStart(Path.DirectorySeparatorChar);

                    var fullFileName = Path.Combine(dir, entryFullName);

                    var fullPath = Path.GetDirectoryName(fullFileName);
                    if (fullPath == null)
                    {
                        return null;
                    }

                    var displayName = entryFullName;
                    if (displayName.Length > 30)
                    {
                        displayName = "..." + displayName.Remove(0, displayName.Length - 26).Trim();
                    }

                    labelPleaseWait.Text = string.Format(LanguageSettings.Current.Settings.ExtractingX, $"{displayName}");
                    labelPleaseWait.Refresh();

                    return fullFileName;
                });
            }
        }

        public static bool IsOld(string fullPath, string whisperChoice)
        {
            var hash = Utilities.GetSha512Hash(FileUtil.ReadAllBytesShared(fullPath));

            if (whisperChoice == WhisperChoice.ConstMe)
            {
                return OldSha512HashesConstMe.Contains(hash);
            }

            if (whisperChoice == WhisperChoice.PurfviewFasterWhisperXxl)
            {
                return OldSha512HashesPurfviewFasterWhisperXxl.Contains(hash);
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

            if (whisperChoice == WhisperChoice.PurfviewFasterWhisperXxl)
            {
                var oldHashes = new List<string>
                {
                    "9266f55949596a75060bb696f41c5242375861a92bed474b39052499c2dc31b63c7d807662458469dc8dcc2d1a2b1e8897cf23b0a54fab9f55e1c3d1f93cc252",
                    "f2f1567245dce34552ba630a22e17d1c05e8eb2d3f952392d4befcde18723905caa94303cfc86e4de7b6ee2f5e4bb1eb60dd2154afbb02239ef7a6f6fc38aedd",
                    "780463ef74240955523a468c716280c3001de68db7f508c3379831ab9fffb9dec2735e8cdb5642c3f8260a311b94cfab2a4da6be9a2ec58538927f3e1d3b40f1",
                    "95256af7e82845b789a3fcc7d52412bccb5baf62dc31b5b9cce97d8e2247282bf89580f35f71b4ecd836a4752a5ce2540ac871ca9b502fa75332947e5852cc78",
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
                    "72c5a0bec7d55fef420e45253d8133e592d19eb2c9cd2da942a99df58f8a10ee6b14c90ce27a575509e358e0b6c943e0a729da9c17ce16778afd58c30455b0f5", // 154whisper-blas-bin-Win32 
                    "98d32926e4fbdbe0850f996400204ee62188a88cb5dfe8b182d15c54915f57b7ee4f22c98581c3f45c457241254df041aed49461a7260b84786ba4205dc2a17e", // 154whisper-blas-clblast-bin-x64
                    "acd220872f25e9ba9555ede278b7260fdc65578ff9a933d6cd5aaa31fcfb80ae86a833f2ca84ce3730c30d17cb5311b5de3d4a5215ebcc53a30cb9d184ccd469", // 1.7.2
                    "e49952ecb35c53f0d0dd42d5197962019af088ba48a0a7a10a42d413fa8417f208c486173885a4749ce26f7afb36e1dc2350ff6018486ae4bffa374554e3f41f", // 1.7.3
                    "212e901061962d208479919ae150b28aa29b73d974c73af1eb47b4c6729e87d20101d1c2117fcb091e520d59b5b090ce994f0a75e8276aea78f52faae9df294b", // 1.7.5
                    "562240d059f8c13d1b4d9be03d54581e3f2844234445a7e604ec88d8b36c6dd282e8648ee87cd326aa86b453c2b3072e87e03acbbf2e99994fe09c1f5475525d", // 1.7.6
                    "371deb177ebeda9377da4aab82a82f2553b0f771651d1c00ddd78ee52a12be3b659d084adb9a72e34947ea994f85cb8b9016c88c60f523a0386b98fd2b01f529", // 1.8.0
                    //"3c5efc12044f5e06a0ec7fbc82b0efaf65519dacf2d49dbcf2e3bd1a509d1a47db77d91c10ea90381edce63a86eb868a834cbc98648dd720bb3ce9fcafd94521", // 1.8.2
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
                    "5bcda2b519193c137fd5b2a3d9c0289bf9afd18bb21239c5f9f8f7196a8fb57179da53b23924dfa06fbda32e19ace203177f87c62759b64cdf6f04c2514aec94", // 154WhisperCppCublass
                    "8a78561e4a18d8cc8528028789395b575e952ede6ee36736fa0a15a69b137997c13eb324a2438cbb7e4a6b67c30843a27f9519cb7606ab758710bc33954869bf", // 1.7.3
                    "5e77cbc42e72dd602ae44af2d043721d061306e7b0d12fd3c52f2ce84152ca5f1972c2aa39ac4ec61cbda1db4b648643481aba1affc3afff06afefe708abbf8a", // 1.7.6
                    "b0df91d47684523282032a54534f4be24df6a579dcde73ac35812970c53ff5768bb1215e5bc0cb16fb2ee411a7e638209184b7502b33446c0c090cc6737a8200", // 1.8.0
                    //"a7ea02e1e524f57432d8076b6154cf0d96bff5ff49206452b77bb5a54da1c216dcf34079498641247f9acc71a30bf7c5f1e3b3d831cccc724e9a0c3dc77ece08", // 1.8.2
                };

                return oldHashes.Contains(hash);
            }

            return false;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
