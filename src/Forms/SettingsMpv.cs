using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SettingsMpv : Form
    {
        public SettingsMpv()
        {
            InitializeComponent();
            labelPleaseWait.Text = string.Empty;
            comboBoxVideoOutput.Text = Configuration.Settings.General.MpvVideoOutput;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            Text = Configuration.Settings.Language.SettingsMpv.Title;
            buttonDownload.Text = Configuration.Settings.Language.SettingsMpv.DownloadMpv;
        }

        private void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null )
            {
                MessageBox.Show(Configuration.Settings.Language.SettingsMpv.DownloadMpvFailed);  
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                Cursor = Cursors.Default;
                return;
            }

            string dictionaryFolder = Configuration.DataDirectory;
            using (var ms = new MemoryStream(e.Result))
            using (ZipExtractor zip = ZipExtractor.Open(ms))
            {
                List<ZipExtractor.ZipFileEntry> dir = zip.ReadCentralDir();
                foreach (ZipExtractor.ZipFileEntry entry in dir)
                {
                    if (entry.FilenameInZip.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        string fileName = Path.GetFileName(entry.FilenameInZip);
                        string path = Path.Combine(dictionaryFolder, fileName);
                        zip.ExtractFile(entry, path);
                    }
                }
            }

            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            buttonOK.Enabled = true;
            buttonDownload.Enabled = true;
            MessageBox.Show("mpv downloaded OK");
        }

        private void buttonDownload_Click_1(object sender, EventArgs e)
        {
            try
            {
                labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait;
                buttonOK.Enabled = false;
                buttonDownload.Enabled = false;
                Refresh();
                Cursor = Cursors.WaitCursor;

                string url = "https://github.com/SubtitleEdit/subtitleedit/raw/master/Other/mpv-dll-64.zip";
                if (IntPtr.Size * 8 == 32)
                {
                    url = "https://github.com/SubtitleEdit/subtitleedit/raw/master/Other/mpv-dll-32.zip";
                }

                var wc = new WebClient { Proxy = Utilities.GetProxy() };
                wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                wc.DownloadDataAsync(new Uri(url));
                Cursor = Cursors.Default;
            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(comboBoxVideoOutput.Text))
                Configuration.Settings.General.MpvVideoOutput = comboBoxVideoOutput.Text;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}
