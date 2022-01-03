using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AudioToTextModelDownload : Form
    {
        public string FFmpegPath { get; internal set; }
        public bool AutoClose { get; internal set; }

        public class DownloadModel
        {

        }

        private string[] _modelLinks = new []
        {
            "https://alphacephei.com/vosk/models/vosk-model-en-us-0.22-lgraph.zip",
            "https://alphacephei.com/vosk/models/vosk-model-cn-kaldi-multicn-2-lgraph.zip",
            "https://alphacephei.com/vosk/models/vosk-model-small-fr-pguyot-0.3.zip",
            "https://alphacephei.com/vosk/models/vosk-model-small-es-0.3.zip",
            "https://alphacephei.com/vosk/models/vosk-model-small-de-0.15.zip",
            "https://alphacephei.com/vosk/models/vosk-model-small-pt-0.3.zip",
            "https://alphacephei.com/vosk/models/vosk-model-small-it-0.4.zip",
            "https://alphacephei.com/vosk/models/vosk-model-nl-spraakherkenning-0.6-lgraph.zip",
            "https://alphacephei.com/vosk/models/vosk-model-small-sv-rhasspy-0.15.zip",
            "https://alphacephei.com/vosk/models/vosk-model-small-fa-0.5.zip",
            "https://alphacephei.com/vosk/models/vosk-model-small-tr-0.3.zip",
            "https://alphacephei.com/vosk/models/vosk-model-el-gr-0.7.zip",
            "https://alphacephei.com/vosk/models/vosk-model-ar-mgb2-0.4.zip",
        };

        public AudioToTextModelDownload()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownload, "FFmpeg");
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
                string url = "https://github.com/SubtitleEdit/support-files/raw/master/ffpmeg/ffmpeg-" + IntPtr.Size * 8 + ".zip";
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
                labelPleaseWait.Text = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownloadFailed, "ffmpeg");
                buttonOK.Enabled = true;
                Cursor = Cursors.Default;
                return;
            }

            string folder = Path.Combine(Configuration.DataDirectory, "ffmpeg");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var ms = new MemoryStream(e.Result))
            using (ZipExtractor zip = ZipExtractor.Open(ms))
            {
                List<ZipExtractor.ZipFileEntry> dir = zip.ReadCentralDir();
                foreach (ZipExtractor.ZipFileEntry entry in dir)
                {
                    string fileName = Path.GetFileName(entry.FilenameInZip);
                    if (fileName != null)
                    {
                        string path = Path.Combine(folder, fileName);
                        if (fileName.EndsWith("ffmpeg.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            FFmpegPath = path;
                        }

                        zip.ExtractFile(entry, path);
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
            labelPleaseWait.Text = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownloadOk, "ffmpeg");
        }
    }
}
