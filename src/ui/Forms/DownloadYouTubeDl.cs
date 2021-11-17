using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class DownloadYouTubeDl : Form
    {
        public const string Url = "https://github.com/ytdl-org/youtube-dl/releases/download/2021.06.06/youtube-dl.exe";
        public const string Sha512Hash = "8f0fd27419ef340d64bff928567b0e4b03d47ca19094b409605ad71ec2b5293a79f814dd2cd4591719a7e386b4041973353fbe90124dbfd4d004b19ea31457ed";
        public bool AutoClose { get; internal set; }

        public DownloadYouTubeDl()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownload, "youtube-dl");
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void DownloadFfmpeg_KeyDown(object sender, KeyEventArgs e)
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

        private void DownloadFfmpeg_Shown(object sender, EventArgs e)
        {
            try
            {
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                buttonOK.Enabled = false;
                Refresh();
                Cursor = Cursors.WaitCursor;
                var wc = new WebClient { Proxy = Utilities.GetProxy() };

                wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                wc.DownloadProgressChanged += (o, args) =>
                {
                    labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait + "  " + args.ProgressPercentage + "%";
                };
                wc.DownloadDataAsync(new Uri(Url));
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
                labelPleaseWait.Text = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownloadFailed, "youtube-dl");
                buttonOK.Enabled = true;
                Cursor = Cursors.Default;
                return;
            }

            string folder = Path.Combine(Configuration.DataDirectory);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var hash = GetSha512Hash(e.Result);
            if (hash != Sha512Hash)
            {
                MessageBox.Show("youtube-dl SHA-512 hash does not match!");
                return;
            }

            File.WriteAllBytes(Path.Combine(folder, "youtube-dl.exe"), e.Result);

            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;

            if (AutoClose)
            {
                DialogResult = DialogResult.OK;
                return;
            }

            buttonOK.Enabled = true;
            labelPleaseWait.Text = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownloadOk, "youtube-dl");
        }

        private static string GetSha512Hash(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            using (var bs = new BufferedStream(ms))
            {
                using (var sha512 = new SHA512Managed())
                {
                    byte[] hash = sha512.ComputeHash(bs);
                    string hashString = string.Empty;
                    foreach (byte x in hash)
                    {
                        hashString += string.Format("{0:x2}", x);
                    }

                    return hashString.ToString().ToLower();
                }
            }
        }
    }
}
