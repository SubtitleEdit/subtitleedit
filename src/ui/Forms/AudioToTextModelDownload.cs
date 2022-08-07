using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AudioToTextModelDownload : Form
    {
        public bool AutoClose { get; internal set; }

        public AudioToTextModelDownload()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = string.Format(LanguageSettings.Current.Settings.DownloadX, "Vosk models");
            buttonDownload.Text = LanguageSettings.Current.GetDictionaries.Download;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonDownload);

            var selectedIndex = 0;
            foreach (var downloadModel in DownloadModel.VoskModels.OrderBy(p=>p.LanguageName))
            {
                comboBoxModels.Items.Add(downloadModel);
                if (selectedIndex == 0 && downloadModel.TwoLetterLanguageCode == "en")
                {
                    selectedIndex = comboBoxModels.Items.Count - 1;
                }
            }

            comboBoxModels.SelectedIndex = selectedIndex;
            labelPleaseWait.Text = string.Empty;

            textBoxError.Visible = false;
        }

        private void AudioToTextDownload_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (comboBoxModels.SelectedIndex < 0)
            {
                return;
            }

            var downloadModel = (DownloadModel)comboBoxModels.Items[comboBoxModels.SelectedIndex];
            try
            {
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                buttonDownload.Enabled = false;
                Refresh();
                Cursor = Cursors.WaitCursor;
                var url = downloadModel.Url;
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
                buttonDownload.Enabled = true;
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            var folder = Path.Combine(Configuration.DataDirectory, "Vosk");

            if (e.Error != null)
            {
                Width = 630;
                Height = 350;
                labelPleaseWait.Text = string.Empty;
                var errorMsg = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownloadFailed, "Vosk model");
                textBoxError.Visible = true;
                var downloadModel = (DownloadModel)comboBoxModels.Items[comboBoxModels.SelectedIndex];
                var dirName = Path.GetFileNameWithoutExtension(downloadModel.Url);
                textBoxError.Text = $"{errorMsg}{Environment.NewLine}{Environment.NewLine}You could manually download and unpack{Environment.NewLine}{downloadModel.Url}{Environment.NewLine}to{Environment.NewLine}{folder}{Path.DirectorySeparatorChar}{dirName}" + Environment.NewLine +
                                    Environment.NewLine +
                                    e.Error;
                Cursor = Cursors.Default;
                return;
            }
  
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var ms = new MemoryStream(e.Result))
            using (var zip = ZipExtractor.Open(ms))
            {
                var dir = zip.ReadCentralDir();
                foreach (var entry in dir)
                {
                    var path = Path.Combine(folder, entry.FilenameInZip);
                    zip.ExtractFile(entry, path);
                }
            }

            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;

            if (AutoClose)
            {
                DialogResult = DialogResult.OK;
                return;
            }

            buttonDownload.Enabled = true;
            labelPleaseWait.Text = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownloadOk, "Vosk model");
        }
    }
}
