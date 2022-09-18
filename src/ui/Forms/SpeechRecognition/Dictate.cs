using NAudio.Wave;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Vosk;

namespace Nikse.SubtitleEdit.Forms.SpeechRecognition
{
    public partial class Dictate : Form
    {
        private static WaveFileWriter _waveFile;
        private static Model _model;
        private WaveInEvent _waveSource;
        public string WaveFileName { get; set; }
        public static bool DataRecorded { get; set; }

        public Dictate()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            groupBoxModels.Text = LanguageSettings.Current.AudioToText.Models;
            labelModel.Text = LanguageSettings.Current.AudioToText.ChooseModel;
            linkLabelOpenModelsFolder.Text = LanguageSettings.Current.AudioToText.OpenModelsFolder;
            checkBoxUsePostProcessing.Text = LanguageSettings.Current.AudioToText.UsePostProcessing;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
            AudioToText.FillModels(comboBoxModels, string.Empty);
            checkBoxUsePostProcessing.Checked = Configuration.Settings.Tools.VoskPostProcessing;
        }

        public void Record()
        {
            _waveSource = new WaveInEvent();
            _waveSource.WaveFormat = new WaveFormat(16000, 1);
            _waveSource.DataAvailable += WaveSourceDataAvailable;
            WaveFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
            _waveFile = new WaveFileWriter(WaveFileName, _waveSource.WaveFormat);
            DataRecorded = false;
            _waveSource.StartRecording();
        }

        public string RecordingToText()
        {
            _waveSource.StopRecording();
            _waveSource.Dispose();
            _waveFile.Close();
            if (!DataRecorded || !File.Exists(WaveFileName))
            {
                return string.Empty;
            }

            var voskFolder = Path.Combine(Configuration.DataDirectory, "Vosk");
            Directory.SetCurrentDirectory(voskFolder);
            Vosk.Vosk.SetLogLevel(0);
            if (_model == null)
            {
                var modelFileName = Path.Combine(voskFolder, comboBoxModels.Text);
                _model = new Model(modelFileName);
            }

            var rec = new VoskRecognizer(_model, 16000.0f);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);
            var list = new List<ResultText>();
            var buffer = new byte[4096];
            using (var source = File.OpenRead(WaveFileName))
            {
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (rec.AcceptWaveform(buffer, bytesRead))
                    {
                        var res = rec.Result();
                        var results = AudioToText.ParseJsonToResult(res);
                        list.AddRange(results);
                    }
                    else
                    {
                        var res = rec.PartialResult();
                    }
                }
            }

            var finalResult = rec.FinalResult();
            var finalResults = AudioToText.ParseJsonToResult(finalResult);
            list.AddRange(finalResults);

            try
            {
                File.Delete(WaveFileName);
                WaveFileName = null;
            }
            catch
            {
                // ignore
            }

            return ResultListToText(list);
        }

        private static string ResultListToText(List<ResultText> list)
        {
            var sb = new StringBuilder();
            foreach (var resultText in list)
            {
                sb.Append(resultText.Text);
                sb.Append(" ");
            }

            return sb.ToString().Trim();
        }

        private static void WaveSourceDataAvailable(object sender, WaveInEventArgs e)
        {
            _waveFile.Write(e.Buffer, 0, e.BytesRecorded);
            DataRecorded = true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void Dictate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            using (var form = new AudioToTextModelDownload { AutoClose = true })
            {
                form.ShowDialog(this);
                AudioToText.FillModels(comboBoxModels, form.LastDownloadedModel);
            }
        }

        private void linkLabelOpenModelsFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var voskFolder = Path.Combine(Configuration.DataDirectory, "Vosk");
            UiUtil.OpenFolder(voskFolder);
        }

        private void Dictate_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.VoskModel = comboBoxModels.Text;
            Configuration.Settings.Tools.VoskPostProcessing = checkBoxUsePostProcessing.Checked;
        }
    }
}
