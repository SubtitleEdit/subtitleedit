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
            if (Configuration.IsRunningOnLinux() && Configuration.Settings.General.MpvVideoOutput == "direct3d")
                comboBoxVideoOutput.Text = "vaapi";
            else
                comboBoxVideoOutput.Text = Configuration.Settings.General.MpvVideoOutput;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            Text = Configuration.Settings.Language.SettingsMpv.Title;
            if (!Configuration.IsRunningOnLinux())
                buttonDownload.Text = Configuration.Settings.Language.SettingsMpv.DownloadMpv;

            if (Configuration.IsRunningOnLinux())
            {
                comboBoxVideoOutput.Items.Clear();
                comboBoxVideoOutput.Items.AddRange(new object[] { "vaapi", "opengl", "sdl", "vdpau" });
                Controls.Remove(buttonDownload);
            }
        }

        private void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(Configuration.Settings.Language.SettingsMpv.DownloadMpvFailed);
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = !Configuration.IsRunningOnLinux();
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
                        if (File.Exists(path))
                            path = Path.Combine(dictionaryFolder, fileName + ".new-mpv");
                        zip.ExtractFile(entry, path);
                    }
                }
            }

            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            buttonOK.Enabled = true;
            if (!Configuration.IsRunningOnLinux())
                buttonDownload.Enabled = true;
            else
                buttonDownload.Enabled = false;
            MessageBox.Show(Configuration.Settings.Language.SettingsMpv.DownloadMpvOk);
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
                string url = "https://github.com/SubtitleEdit/support-files/blob/master/mpv/libmpv" + IntPtr.Size * 8 + ".zip?raw=true";
                var wc = new WebClient { Proxy = Utilities.GetProxy() };
                wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                wc.DownloadProgressChanged += (o, args) =>
                {
                    labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait + "  " + args.ProgressPercentage + "%";
                };
                wc.DownloadDataAsync(new Uri(url));
            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = !Configuration.IsRunningOnLinux();
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
