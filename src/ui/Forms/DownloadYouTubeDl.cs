using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class DownloadYouTubeDl : Form
    {
        public const string Url = "https://github.com/yt-dlp/yt-dlp/releases/download/2022.05.18/yt-dlp.exe";
        public const string Sha512Hash = "f8f55be89b8b4b5c9703bb594367ad10c245dd1e1de9182a8a3d8ae06220aeabe200ed9b797d6f176a5d10f99ff3bbb48176c4e52cd9f4cc3c1fac451df42dde";
        public bool AutoClose { get; internal set; }

        public DownloadYouTubeDl()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = string.Format(LanguageSettings.Current.Settings.DownloadX, "youtube-dl");
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

            var hash = Utilities.GetSha512Hash(e.Result);
            if (hash != Sha512Hash)
            {
                MessageBox.Show("yt-dlp.exe SHA-512 hash does not match!");
                return;
            }

            File.WriteAllBytes(Path.Combine(folder, "yt-dlp.exe"), e.Result);

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
    }
}
