using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AudioToTextModelDownload : Form
    {
        public bool AutoClose { get; internal set; }

        public class DownloadModel
        {
            public string Url { get; set; }
            public string TwoLetterLanguageCode { get; set; }
            public string LanguageName { get; set; }
            public override string ToString()
            {
                return LanguageName;
            }
        }

        private readonly DownloadModel[] _voskModels = new[]
        {
            new DownloadModel
            {
                TwoLetterLanguageCode = "en",
                LanguageName = "English",
                Url = "https://alphacephei.com/vosk/models/vosk-model-en-us-0.22-lgraph.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "cn",
                LanguageName = "Chinese",
                Url = "https://alphacephei.com/vosk/models/vosk-model-cn-kaldi-multicn-2-lgraph.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "fr",
                LanguageName = "French",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-fr-pguyot-0.3.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "es",
                LanguageName = "Spanish",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-es-0.3.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "de",
                LanguageName = "German",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-de-0.15.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "pt",
                LanguageName = "Portuguese",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-pt-0.3.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "it",
                LanguageName = "Italian",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-it-0.4.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "nl",
                LanguageName = "Dutch",
                Url = "https://alphacephei.com/vosk/models/vosk-model-nl-spraakherkenning-0.6-lgraph.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "sv",
                LanguageName = "Swedish",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-sv-rhasspy-0.15.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "fa",
                LanguageName = "Farsi",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-fa-0.5.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "tr",
                LanguageName = "Turkish",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-tr-0.3.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "el",
                LanguageName = "Greek",
                Url = "https://alphacephei.com/vosk/models/vosk-model-el-gr-0.7.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "ar",
                LanguageName = "Arabic",
                Url = "https://alphacephei.com/vosk/models/vosk-model-ar-mgb2-0.4.zip",
            }
        };

        public AudioToTextModelDownload()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownload, "Vosk models");
            buttonDownload.Text = LanguageSettings.Current.GetDictionaries.Download;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonDownload);

            foreach (var downloadModel in _voskModels)
            {
                comboBoxModels.Items.Add(downloadModel);
            }

            comboBoxModels.SelectedIndex = 0;
            labelPleaseWait.Text = string.Empty;
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
                string url = downloadModel.Url;
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
            if (e.Error != null)
            {
                labelPleaseWait.Text = string.Format(LanguageSettings.Current.SettingsFfmpeg.XDownloadFailed, "Vosk model");
                buttonDownload.Enabled = true;
                Cursor = Cursors.Default;
                return;
            }

            string folder = Path.Combine(Configuration.DataDirectory, "Vosk");
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
