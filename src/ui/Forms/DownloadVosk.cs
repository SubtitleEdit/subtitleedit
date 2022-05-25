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

        public static readonly string[] VoskNewSha512Hashes =
        {
            "7baa07adeb613994cac43778627cda0df925ff12e3851d67ad1336e45ded51f2a07102d6df2685b296f533255bd5dcc6d36d635a26e8c1da935422f1d904205f", // 32-bit
            "e78db54a169f0b8ad859850f98621a54acb1902e2978043cea2787a618fd638ab690ef28694dd71b69f6aecad2292254c6cd073825268c3fcba23d8530668577", // 64-bit
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
                var url = "https://github.com/alphacep/vosk-api/releases/download/v0.3.38/vosk-win" + IntPtr.Size * 8 + "-0.3.38.zip?raw=true";
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
