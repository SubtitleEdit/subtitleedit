using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class DownloadVosk : Form
    {
        public bool AutoClose { get; internal set; }

        private static readonly string[] VoskNewSha512Hashes =
        {
            "edb7aa9bdc17573fc903561f1c97ea6dbdc5206550e3a15facea799c8448b38efbc9aca2f3465ae93f12585a8f887d1126c0ce44d7e14bfe1bd807ae5e9ce923", // 32-bit
            "5465884aae4509acbc27b81e4d0f99a9429228f8e77709e2bf07f7056015521df72ae5b2fb6de26efc42b595b9144c417e1b73ebce84595e5cc3bac7c388fe0c", // 64-bit
        };

        public DownloadVosk()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = string.Format(LanguageSettings.Current.Settings.DownloadX, "Vosk");
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void DownloadVosk_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void DownloadVosk_Shown(object sender, EventArgs e)
        {
            try
            {
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                buttonOK.Enabled = false;
                Refresh();
                Cursor = Cursors.WaitCursor;
                var url = "https://github.com/alphacep/vosk-api/releases/download/v0.3.42/vosk-win" + IntPtr.Size * 8 + "-0.3.42.zip?raw=true";
                var wc = new WebClient { Proxy = Utilities.GetProxy() };

                wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                wc.DownloadProgressChanged += (o, args) =>
                {
                    labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait + "  " + args.ProgressPercentage + "%";
                };
                wc.DownloadDataAsync(new Uri(url));
            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
            }
        }

        private void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                labelPleaseWait.Text = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownloadFailed, "Vosk");
                buttonOK.Enabled = true;
                Cursor = Cursors.Default;
                return;
            }

            var hash = Utilities.GetSha512Hash(e.Result);
            if (!VoskNewSha512Hashes.Contains(hash))
            {
                MessageBox.Show("Vosk SHA-512 hash does not match!");
                return;
            }

            var folder = Path.Combine(Configuration.DataDirectory, "Vosk");
            using (var ms = new MemoryStream(e.Result))
            using (var zip = ZipExtractor.Open(ms))
            {
                var dir = zip.ReadCentralDir();
                foreach (var entry in dir)
                {
                    var name = entry.FilenameInZip;
                    var idxOfSlash = name.IndexOf('/');
                    if (idxOfSlash > 0)
                    {
                        name = name.Substring(idxOfSlash).TrimStart('/');
                        if (name.Length > 0)
                        {
                            var path = Path.Combine(folder, name);
                            zip.ExtractFile(entry, path);
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

            buttonOK.Enabled = true;
            labelPleaseWait.Text = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownloadOk, "Vosk");
        }
    }
}
